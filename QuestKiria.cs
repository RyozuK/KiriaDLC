using System;
using System.Transactions;
using UnityEngine;

namespace Mod_KiriaDLC;

//Extending QuestDialog is suitable for *most* quests that involve Drama book
public class QuestKiria : QuestDialog
{
    public override bool RequireClientInSameZone => false;

    public override void OnStart()
    {
        Debug.LogWarning("KiriaDLC:: OnStart called, spawning map");
        Thing mapItem = ThingGen.Create("map_kiria");
        Msg.Say("get_quest_item");
        EClass.pc.Pick(mapItem);
    }

    // public override void OnComplete()
    // {
    //     base.OnComplete();
    //     //Should I change CharaText for Kiria?
    //     Chara c = EClass.game.cards.globalCharas.Find("adv_kiria");
    // }

    public override void OnEnterZone()
    {
        if (EClass._zone.idExport == "kiria_lab")
        {
            Debug.LogWarning("KiriaDLC::Found lab, completing quest");
            this.Complete();
        }
    }
    
    // public override void OnGiveItem(Chara c, Thing t)
    // {
    //     Debug.LogWarning("KiriaDLC::Quest::OnGiveItem triggerred with Thing: " + t.ToString());
    // }
}