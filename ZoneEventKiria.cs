using System;
using System.Collections.Generic;
using Newtonsoft.Json;

// namespace Mod_KiriaDLC;

public class ZoneEventKiria : ZoneEvent
{
    public override float roundInterval => 2f;

    //Just to keep a lookup table of the ids in case a boss is missing somehow (Captured, etc.)
    [JsonProperty] private Dictionary<int, string> _bosses;
    
    public override void OnTickRound()
    {
        QuestKiria quest = EClass.game.quests.Get<QuestKiria>();
        if (quest is null || quest.phase != QuestKiria.PHASE_BOSS)
        {
            return;
        }
        
        //The list of boss uids that the quest thinks need to be subdued
        List<int> bossUids = (zone as Zone_DungeonKiria)?.Bosses;
        
        if (bossUids != null)
        {
            foreach (var bossUid in bossUids?.Copy())
            {
                //Find the boss in the attached zone
                Chara boss = zone.FindChara(bossUid);
                string bossType = boss is not null ? boss.id : _bosses[bossUid];
                if (boss == null || boss.isDead || boss.IsPCFaction || boss.IsPCFactionMinion || boss.IsPCPartyMinion)
                {
                    //Boss wasn't found, must have been removed in some way, count it as subdued
                    quest.OnSubdueChara(bossType);
                    bossUids.Remove(bossUid);
                }
            }
        }

        base.OnTickRound();
    }
    
    public override void OnFirstTick()
    {
        KiriaDLCPlugin.LogWarning("ZoneEventKiria::OnFirstTick", "Fetching kirias");
        var bossUids = (zone as Zone_DungeonKiria)?.Bosses;
        if (bossUids == null) return;
        foreach (var bossUid in bossUids)
        {
            _bosses[bossUid] = zone.FindChara(bossUid).id;
        }
    }

    public override void OnCharaDie(Chara c)
    {
        KiriaDLCPlugin.LogWarning("ZoneEventKiria::OnCharaDie", "Killed " + c?.id + "/" + c?.uid);
        var bossUids = (zone as Zone_DungeonKiria)?.Bosses;
        if (bossUids is null || c == null) return;
        if (bossUids.Contains(c.uid))
        {
            EClass.game.quests.Get<QuestKiria>()?.OnSubdueChara(c.id);
            bossUids.Remove(c.uid);
        }
        base.OnCharaDie(c);
    }
}