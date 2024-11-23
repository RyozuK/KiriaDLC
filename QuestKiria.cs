using System;
using System.Transactions;
using UnityEngine;

namespace Mod_KiriaDLC;

public class QuestKiria : Quest
{
    public override bool RequireClientInSameZone => false;
    
    public override void OnClickQuest()
    {
        Console.WriteLine("KIRIADLC::OnClickQuest::START");
        //this.DropReward("map_kiria");
        //this.DropReward("chicken_dagger");
        Thing mapItem = ThingGen.Create("map_kiria");
        Console.WriteLine(mapItem.ToString());
        this.DropReward(mapItem);
        this.chara.ShowDialog("kiriaDLC", "main", "");
    }

    public override void OnStart()
    {
        base.OnStart();
        
    }
}