using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

// namespace Mod_KiriaDLC;

public class QuestTaskBosses : QuestTask
{
    [JsonProperty] public int NumHeadlessNeeded;
    [JsonProperty] public int NumBunnyNeeded;
    [JsonProperty] public int NumPutitNeeded;
    [JsonProperty] public int NumDamagedNeeded;

    public int TotalLeft => NumDamagedNeeded + NumBunnyNeeded + NumHeadlessNeeded + NumPutitNeeded;
    
    public override bool IsComplete() => TotalLeft == 0;

    public override void OnInit()
    {
        KiriaDLCPlugin.LogWarning("TaskBosses", "OnInit invoked");
        base.OnInit();
    }

    public List<string> GetSpawnList()
    {
        KiriaDLCPlugin.LogWarning("TaskBosses", "GetSpawnList invoked");
        List<string> spawnList = [];
        if (NumHeadlessNeeded >= 1) spawnList.Add("kiria_headless");
        if (NumHeadlessNeeded >= 2) spawnList.Add("kiria_headless");
        if (NumBunnyNeeded >= 1) spawnList.Add("kiria_bunny");
        if (NumBunnyNeeded >= 2) spawnList.Add("kiria_bunny");
        if (NumPutitNeeded >= 1) spawnList.Add("kiria_putit");
        if (NumPutitNeeded >= 2) spawnList.Add("kiria_putit");
        if (NumDamagedNeeded >= 1) spawnList.Add("kiria_broken");
        return spawnList;
    }

    public void OnSubdueMobId(string mobId)
    {
        switch (mobId)
        {
            case "kiria_headless" when NumHeadlessNeeded > 0:
                NumHeadlessNeeded--;
                break;
            case "kiria_bunny" when NumBunnyNeeded > 0:
                NumBunnyNeeded--;
                break;
            case "kiria_putit" when NumPutitNeeded > 0:
                NumPutitNeeded--;
                break;
            case "kiria_broken" when NumDamagedNeeded > 0:
                NumDamagedNeeded--;
                break;
        }
        KiriaDLCPlugin.LogWarning("QuestTaskBosses::OnSubdueMob", "Left is now " + TotalLeft);
    }
    
    public override void OnGetDetail(ref string detail, bool onJournal)
    {
        detail += $"\n\n{7 - TotalLeft}/7";
    }
}