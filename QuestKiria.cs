using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


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

    // public override bool CanAbandon => true;

    //Map related
    [JsonProperty]
    private Thing _mapItem;
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
    
    [JsonProperty]
    public Zone KiriaZone { get; set; }

    public override void OnChangePhase(int a)
    {
        base.OnChangePhase(a);
        if (a == PHASE_BOSS)
        {
            task = new QuestTaskBosses();
            ((QuestTaskBosses)task).NumHeadlessNeeded = 2;
            ((QuestTaskBosses)task).NumBunnyNeeded = 2;
            ((QuestTaskBosses)task).NumDamagedNeeded = 1;
            ((QuestTaskBosses)task).NumPutitNeeded = 2;
            task.SetOwner(this);
            
        }
        else if (a == PHASE_BOSS_DEAD)
        {
            EClass._zone.SetBGM(114);
            this.person.chara.ShowDialog("kiriaDLC", "after_battle");
        }
        else if (a == PHASE_LETTERS)
        {
            task = new QuestTaskLetters();
            task.SetOwner(this);

        }
        else
        {
            task = null;
        }
    }
    
    public override bool RequireClientInSameZone => !KiriaDLCPlugin.DEBUG_MODE;
    
    //QuestDialog -> QuestProgression -> QuestSequence, wherein idSource will append the progress
    //This means there is expected to be a SourceQuest row for each step of the quest, we don't want that.
    //(Or do we?)
    public override string idSource => this.id; // + (this.phase == 0 ? "" : this.phase.ToString() ?? "");
    

    public void OnItemPickup(Thing thing)
    {
        if (thing.id == "letter")
        {
            (task as QuestTaskLetters)?.OnPickThing(thing);
            if (task?.IsComplete() == true)
            {
                NextPhase();
            }
        }

        if (thing.id == "gene_kiria")
        {
            //We'll just complete the quest if they pick up the gene, the letters are for flavor
            //So it's up to the player to decide if that's important.
            this.Complete();
        }
    }

    public void OnSubdueChara(Chara c)
    {
        if (phase == PHASE_BOSS)
        {
            (task as QuestTaskBosses)?.OnSubdueMob(c);
        }
    }


    //Put the phases in the quest text so we use this to split it and choose the right one
    //I suppose if we did a row per quest in the source, each row would include a step.
    public override string GetDetail(bool onJournal = false)
    {
        string text =
            ((IList<string>)this.source.GetDetail().Split(['|'], StringSplitOptions.None)).TryGet<string>(
                onJournal ? this.phase : 0);
        task?.OnGetDetail(ref text, onJournal);
        return GameLang.Convert(text);
    }

    public override void OnStart()
    {
        KiriaDLCPlugin.LogWarning("Quest.OnStart", "KiriaDLC:: OnStart called, spawning map. Phase: " + this.phase);
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
        _mapItem?.Destroy();
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
        if (this.phase == PHASE_MAP && EClass._zone.id == "kiria_dungeon" &&
            EClass._zone.lv == Zone_KiriaDungeon.LvBoss) //Oof.
        {
            KiriaDLCPlugin.LogWarning("OnEnterZone", "Going from PhaseMap to PhaseBoss");
            NextPhase();
        }

        if (this.phase == PHASE_BOSS_DEAD && EClass._zone.idExport == "kiria_lab")
        {
            //User has found the lab, let's advance the quest and direct them to look for the letters
            KiriaDLCPlugin.LogWarning("QuestKiria.OnEnterZone", "Entered Lab, advancing quest");
            this.NextPhase();
        }
    }
}