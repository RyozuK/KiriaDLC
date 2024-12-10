using System.Collections.Generic;

namespace Mod_KiriaDLC;

public class ZoneEventKIria : ZoneEvent
{
    public override float roundInterval => 2f;

    private List<Chara> bosses = [];

    private void PrintBossInfo(string loc, Chara boss, int bossUid)
    {
        KiriaDLCPlugin.LogWarning(loc, boss?.id + ":" + bossUid + 
                                                                 ":" + boss?.isDead +
                                                                 "/" + boss?.IsPCFaction +
                                                                 "/" + boss?.IsPCFactionMinion +
                                                                 "/" + boss?.IsPCPartyMinion);
    }

    public override void OnTickRound()
    {
        QuestKiria quest = EClass.game.quests.Get<QuestKiria>();
        if (quest is null || quest.phase != QuestKiria.PHASE_BOSS)
        {
            // KiriaDLCPlugin.LogWarning("ZoneEventKiria::OnTickRound", "Quest was null or not PHASE_BOSS");
            return;
        }
        var bossUids = (zone as Zone_KiriaDungeon)?.Bosses;
        KiriaDLCPlugin.LogWarning("-------\nZoneEventKiria::OnTickRound", "Checking " + bossUids?.Count + " uids.");
        if (bossUids != null && bossUids.Count > 0)
        {
            foreach (var bossUid in bossUids.Copy())
            {
                Chara boss = zone.FindChara(bossUid);
                PrintBossInfo("ZoneEventKiria::OnTickRound", boss, bossUid);
                if (boss == null)
                {
                    var possibleBoss = bosses.Find(chara => chara.uid == bossUid);
                    PrintBossInfo("\t\t", possibleBoss, bossUid);
                    //quest.OnSubdueChara(possibleBoss);
                    OnCharaDie(possibleBoss);
                    bosses.Remove(possibleBoss);
                    bossUids.Remove(bossUid);
                }
                else if (boss.isDead || boss.IsPCFaction || boss.IsPCFactionMinion || boss.IsPCPartyMinion)
                {
                    quest.OnSubdueChara(boss);
                    bosses.Remove(boss);
                    bossUids.Remove(bossUid);
                }
            }
        }

        base.OnTickRound();
    }
    
    public override void OnFirstTick()
    {
        KiriaDLCPlugin.LogWarning("ZoneEventKiria::OnFIrstTick", "Fetching kirias");
        var bossUids = (zone as Zone_KiriaDungeon)?.Bosses;
        if (bossUids != null)
            foreach (var bossUid in bossUids)
            {
                bosses.Add(zone.FindChara(bossUid));
            }
    }

    public override void OnCharaDie(Chara c)
    {
        KiriaDLCPlugin.LogWarning("ZoneEventKiria::OnCharaDie", "Killed " + c?.id + "/" + c?.uid);
        var bossUids = (zone as Zone_KiriaDungeon)?.Bosses;
        if (bossUids is null || c == null) return;
        if (bossUids.Contains(c.uid))
        {
            EClass.game.quests.Get<QuestKiria>()?.OnSubdueChara(c);
            bossUids.Remove(c.uid);
            bosses.Remove(c);
        }
        base.OnCharaDie(c);
    }
}