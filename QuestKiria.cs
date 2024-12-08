﻿using System;
using System.Collections.Generic;
using System.Linq;


namespace Mod_KiriaDLC;

//Extending QuestDialog is suitable for *most* quests that involve Drama book
public class QuestKiria : QuestDialog
{

    public static int PHASE_START => 0;
    public static int PHASE_MAP => 1;
    public static int PHASE_BOSS => 2;
    public static int PHASE_BOSS_DEAD => 3;
    public static int PHASE_LETTERS => 4;
    public static int PHASE_REMAINS => 5;


    private Thing _mapItem = null;

    public Thing MapItem
    {
        get
        {
            if (_mapItem == null || _mapItem.isDestroyed)
            {
                KiriaZone?.Destroy();
                _mapItem = ThingGen.Create("map_kiria");
            }
            return _mapItem;
        }
        set => _mapItem = value;
    }

    public Zone KiriaZone { get; set; } = null;


    public override bool RequireClientInSameZone => !KiriaDLCPlugin.DEBUG_MODE;
    
    
    //QuestDialog -> QuestProgression -> QuestSequence, wherein idSource will append the progress
    //This means there is expected to be a SourceQuest row for each step of the quest, we don't want that.
    //(Or do we?)
    public override string idSource => this.id; // + (this.phase == 0 ? "" : this.phase.ToString() ?? "");

    //We want the player to have picked up all the letters to progress the quest
    public HashSet<string> letters = 
    [
        "kiria_dlc_1", "kiria_dlc_2", "kiria_dlc_3",
        "kiria_dlc_4", "kiria_dlc_5", "kiria_dlc_6", "kiria_dlc_7"
    ];

    public HashSet<string> found = [];

    public int MobKilLCount = 0;
    public List<Chara> bosses = [];
    
    private void UpdateBossKills() => MobKilLCount = this.bosses.Count(chara1 => chara1.isDead 
        || chara1.IsPCFaction
        || chara1.IsPCFactionMinion
        || chara1.IsPCPartyMinion);

    private void UpdateLetterCollections()
    {
        //Turns out this only works on the main inventory, and if the letters are picked up
        //into the sub inventory/bag, it won't detect.
        foreach (string letter in letters)
        {
            if (EClass.pc.things.Find(thing => thing.id == "letter" && thing.GetStr(53) == letter) != null)
            {
                found.Add(letter);
            }
        }
    }
    
    
    // private bool PlayerHasGene => EClass.pc.things.Any(thing => thing.id == "gene_kiria"); //Does not detect item in bag
    // private bool PlayerHasGene => EClass.pc.things.Find("gene_kiria") != null; //Detects items in bags
    private bool PlayerHasGene => EClass.pc.things.Find(gene => gene.id == "gene_kiria") != null; //Detects items in bags
    //Put the phases in the quest text so we use this to split it and choose the right one
    //I suppose if we did a row per quest in the source, each row would include a step.
    public override string GetDetail(bool onJournal = false)
    {
        string text = ((IList<string>) this.source.GetDetail().Split(['|'], StringSplitOptions.None)).TryGet<string>(onJournal ? this.phase : 0);
        string str = GetPhaseProgress(); //We don't need the old version, we'll use our private one here 
        if (!str.IsEmpty())
            str = "\n\n" + str;
        return GameLang.Convert(text) + str;
    }

    private string GetPhaseProgress()
    {
        //In theory, we'd use tasks for this, but tasks don't have the hooks we need.
        //(OnKillChara only counts your own kills)
        if (this.phase == PHASE_BOSS)
        {
            //Update the number here
            if (EClass._zone.id == "kiria_dungeon")
            {
                //If we're not even in the dungeon, we don't need to update
                UpdateBossKills();
                if (this.MobKilLCount >= 7)
                {
                    EClass._zone.SetBGM(114);
                    this.person.chara.ShowDialog("kiriaDLC", "after_battle");
                    NextPhase();
                }
            }

            return this.MobKilLCount + "/7";
        }
        if (this.phase == PHASE_LETTERS)
        {
            //Update found letters here
            UpdateLetterCollections();
            if (this.found.Count() >= 7)
            {
                //Found all the letters
                NextPhase();
            }
            return this.found.Count + "/7";
        }

        if (this.phase == PHASE_REMAINS)
        {
            if (PlayerHasGene)
            {
                this.Complete();
            }
        }
        return "";
    }

    public override void OnStart()
    {
        KiriaDLCPlugin.LogWarning("Quest.OnStart","KiriaDLC:: OnStart called, spawning map. Phase: " + this.phase);
        // MapItem = ThingGen.Create("map_kiria");
        Msg.Say("get_quest_item");
        this.NextPhase();
        EClass.pc.Pick(MapItem);
        //Give the player the map replacement quest.
        EClass.game.quests.globalList.Add(Quest.Create("kiria_map_replace").SetClient(this.person.chara, false));
    }
    

    public override void OnBeforeComplete()
    {
        this.person.chara.ShowDialog("kiriaDLC", "complete_quest");
        Quest mapReplaceQuest = EClass.game.quests.GetGlobal("kiria_map_replace");
        MapItem?.Destroy();
        KiriaDLCPlugin.LogWarning("QuestKiria::OnBeforeComplete", "Found quest " + mapReplaceQuest?.id);
        if (mapReplaceQuest is not null)
        {
            KiriaDLCPlugin.LogWarning("QuestKiria::OnBeforeComplete", "Removing it");
            EClass.game.quests.RemoveGlobal(mapReplaceQuest);
        }
    }

    //Called whenever the PC enters a zone, Of note: This is called after a Zone OnBeforeSimulate
    public override void OnEnterZone()
    {
        if (this.phase == PHASE_MAP && EClass._zone.id == "kiria_dungeon" && EClass._zone.lv == Zone_KiriaDungeon.LvBoss) //Oof.
        {
            KiriaDLCPlugin.LogWarning("OnEnterZone", "Going from PhaseMap to PhaseBoss");
            NextPhase();
        }
        
        if (this.phase == PHASE_BOSS_DEAD && EClass._zone.idExport == "kiria_lab")
        {
            //User has found the lab, let's advance the quest and direct them to look for the letters
            KiriaDLCPlugin.LogWarning("QuestKiria.OnEnterZone","Entered Lab, advancing quest");
            this.NextPhase();
        }
    }
}