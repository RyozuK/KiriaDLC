using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


// namespace Mod_KiriaDLC;

//Extending QuestDialog is suitable for *most* quests that involve Drama book
public class QuestKiria : QuestSequence
{
    public static int PHASE_START => 0;
    public static int PHASE_MAP => 1;
    public static int PHASE_BOSS => 2;
    public static int PHASE_BOSS_DEAD => 3;
    public static int PHASE_LETTERS => 4;
    public static int PHASE_REMAINS => 5;
    

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
            this.person.chara.ShowDialog("kiria", "after_battle");
            EClass._zone.RefreshBGM();
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
    
    public override bool RequireClientInSameZone => true;
    
    public override string idSource => this.id + (this.phase == 0 ? "" : this.phase.ToString() ?? "");

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
    }
    
    public void OnSubdueChara(string mobId)
    {
        if (phase == PHASE_BOSS)
        {
            (task as QuestTaskBosses)?.OnSubdueMobId(mobId);
        }
    }

    public override void OnStart()
    {
        KiriaDLCPlugin.LogWarning("Quest.OnStart", "KiriaDLC:: OnStart called, spawning map. Phase: " + this.phase);
        Thing mapItem = ThingGen.Create("map_kiria");
        EClass.pc.DropThing(mapItem);
        Msg.Say("get_quest_item");
        this.NextPhase();
    }

    public override void OnBeforeComplete()
    {
        this.person.chara.ShowDialog("kiria", "complete_quest");
    }

    //Called whenever the PC enters a zone, Of note: This is called after a Zone OnBeforeSimulate
    public override void OnEnterZone()
    {
        if (this.phase == PHASE_BOSS_DEAD && EClass._zone.idExport == "kiria_lab")
        {
            //User has found the lab, let's advance the quest and direct them to look for the letters
            KiriaDLCPlugin.LogWarning("QuestKiria.OnEnterZone", "Entered Lab, advancing quest");
            this.NextPhase();
        }
    }
}