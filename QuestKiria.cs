using System;
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

    // private bool[] phase_dialog = [false, false, false, true, false, false, false];
    //
    // public override void OnChangePhase(int a)
    // {
    //     //Called AFTER the phase is changed,
    //     if (phase_dialog[a])
    //     {
    //         phase_dialog[a] = false;
    //         this.person.chara.ShowDialog("kiriaDLC", "phase_start_" + a);
    //     }
    // }

    public override bool RequireClientInSameZone => true;
    
    
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
        List<Thing> things = EClass.pc.things.FindAll(thing => thing.id == "letter" && letters.Contains(thing.GetStr(53)));
        foreach (Thing thing in things)
        {
            found.Add(thing.GetStr(53));
        }
    }
    
    private bool PlayerHasGene => EClass.pc.things.Any(thing => thing.id == "gene_kiria");
    
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
        Thing mapItem = ThingGen.Create("map_kiria");
        Msg.Say("get_quest_item");
        EClass.pc.Pick(mapItem);
        this.NextPhase();
    }


    public override void OnBeforeComplete()
    {
        this.person.chara.ShowDialog("kiriaDLC", "complete_quest");
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