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
}