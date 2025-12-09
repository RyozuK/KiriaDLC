using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace Mod_KiriaDLC;

public class Zone_KiriaDungeon : Zone_QuestDungeon
{
    public static int LvBasement => -6;
    
    public static int LvBoss => -5;
    
    public override int MinLv => -6;
    
    [JsonProperty]
    public List<int> Bosses;
    
    public override bool RestrictBuild => this.lv == LvBasement; //Don't let the PC build here.

    public override bool IsReturnLocation => false;
    
    public override string IDPlayList =>
        this.lv == LvBoss && EClass.game.quests.GetPhase<QuestKiria>() <= QuestKiria.PHASE_BOSS
            ? "Dungeon_Boss"
            : base.IDPlayList;

    public override string idExport => this.lv != LvBasement ? base.idExport : "kiria_lab";


    //When the game tries to load a map file for map generation, it'll refer to this for its location
    //We use Assembly.GetExecutingAssembly() to point at our mod's folder
    //Now handled by Fresh Toast Loader
    // public override string pathExport => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
    //     $"Map/{this.idExport}.z");
    
    public override bool LockExit => this.lv == LvBoss && (EClass.game.quests.GetPhase<QuestKiria>() < QuestKiria.PHASE_BOSS_DEAD);

    public override bool CanUnlockExit => EClass.game.quests.GetPhase<QuestKiria>() >= QuestKiria.PHASE_BOSS_DEAD 
                                          && EClass.pc.party.Find("adv_kiria") != null;

    public override bool UseFog => this.lv != LvBasement;

    public override bool RevealRoom => this.lv == LvBasement;
    
    public override bool HasLaw => this.lv == LvBasement;

    public override float PrespawnRate => this.lv == LvBasement ? 0.0f : base.PrespawnRate;

    public override string GetNewZoneID(int level) => level == LvBasement ? "kiria_lab" : this.source.id;
    
    public override void OnGenerateMap()
    {
        QuestKiria quest = EClass.game.quests.Get<QuestKiria>();
        if (quest is not null)
        {
            if (this.lv == LvBoss && quest.phase == QuestKiria.PHASE_MAP)
            {
                quest.NextPhase();
            }
            if (this.lv == LvBoss && (this.Bosses == null || Bosses.Count == 0) && quest.phase == QuestKiria.PHASE_BOSS)
            {
                this.events.Add(new ZoneEventKiria());
                KiriaDLCPlugin.LogWarning("ZoneKiria.OnBeforeSimulate","\tAdding Kirias to map");
                Point point = EClass._map.FindThing<TraitStairsLocked>().owner.pos.GetNearestPoint(allowChara: false, allowInstalled: true, ignoreCenter: true, minRadius: 1) ?? EClass._map.GetCenterPos();

                //The quest keeps track of how many of each type of Kiria remains, we get the spawn list based on that
                List<string> spawnList = (quest.task as QuestTaskBosses)?.GetSpawnList();
                this.Bosses = [];
                foreach (string bossID in spawnList!)
                {
                    Chara boss = CharaGen.Create(bossID).ScaleByPrincipal();
                    this.AddCard((Card)boss, point.GetNearestPoint(allowChara: false, allowInstalled: true) ?? point);
                    this.Bosses.Add(boss.uid);
                    KiriaDLCPlugin.LogWarning("ZoneKiriaDungeon::Spawning", bossID + " " + boss.uid);
                }
            }
        }
        base.OnGenerateMap();
    }
}