using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace Mod_KiriaDLC;

public class Zone_KiriaDungeon : Zone_Dungeon
{
    public static int LvBasement => -KiriaDLCPlugin.NUM_FLOORS;
    public static int LvBoss => LvBasement + 1;
    
    [JsonProperty]
    public List<int> Bosses;
    
    public override bool ScaleMonsterLevel => !KiriaDLCPlugin.DEBUG_MODE;
    public override bool RestrictBuild => this.lv == LvBasement; //Don't let the PC build here.
    // public override bool AlwaysLowblock => false;
    public override bool IsReturnLocation => false;

    
    public override string IDPlayList =>
        this.lv == LvBoss && EClass.game.quests.GetPhase<QuestKiria>() == QuestKiria.PHASE_BOSS
            ? "Dungeon_Boss"
            : base.IDPlayList;

    //Note, base Zone_Dungeon will skip ore/big daddy/evolved/shrine generation if this is not empty
    public override string idExport => this.lv == LvBasement ? "kiria_lab" : ""; //this.source.id;


    //When the game tries to load a map file for map generation, it'll refer to this for its location
    //We use Assembly.GetExecutingAssembly() to point at our mod's folder
    public override string pathExport => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
        $"Map/{this.idExport}.z");
    
    public override bool LockExit => this.lv == LvBoss && (EClass.game.quests.GetPhase<QuestKiria>() < QuestKiria.PHASE_BOSS_DEAD);

    public override bool CanUnlockExit => EClass.game.quests.GetPhase<QuestKiria>() >= QuestKiria.PHASE_BOSS_DEAD;

    public override bool UseFog => this.lv != LvBasement;

    public override bool RevealRoom => this.lv == LvBasement;
    
    public override bool HasLaw => this.lv == LvBasement;

    public override float PrespawnRate => this.lv == LvBasement ? 0.0f : base.PrespawnRate;

    public override string GetNewZoneID(int level) => level == LvBasement ? "kiria_lab" : this.source.id;

    //Note: This is called before OnEnterZone
    public override void OnBeforeSimulate()
    {
        QuestKiria quest = EClass.game.quests.Get<QuestKiria>();
        if (quest is null)
        {
            base.OnBeforeSimulate();
            return;
        }

        if (this.lv == LvBoss && quest.phase == QuestKiria.PHASE_MAP)
        {
            quest.NextPhase();
        }
        if (this.lv == LvBoss && (this.Bosses == null || Bosses.Count == 0) && quest.phase == QuestKiria.PHASE_BOSS)
        {
            this.events.Add(new ZoneEventKIria());
            KiriaDLCPlugin.LogWarning("ZoneKiriaDungeon::OnBeforeSimulate", "Summong Kirias?");
            KiriaDLCPlugin.LogWarning("ZoneKiria.OnBeforeSimulate","\tAdding Kirias to map");
            Thing exit = EClass._map.props.installed.Find<TraitStairsLocked>();
            if (exit == null)
            {
                Debug.LogError("\tDID NOT FIND EXIT");
                base.OnGenerateMap();
                return;
            }

            List<string> spawnList = (quest.task as QuestTaskBosses)?.GetSpawnList();
            //For future reference, this should get a reasonable spawn location
            //EClass._map.bounds.GetRandomSpawnPos()
            this.Bosses = [];
            foreach (string bossID in spawnList)
            {
                Chara boss = (Chara)AddChara(bossID, exit.pos.x, exit.pos.z);
                boss.hostility = Hostility.Enemy;
                boss.enemy = EClass.pc.party.members.RandomItem<Chara>();
                //This turned out to be a bad idea... Custom mob cards/figures seem to be broken, and
                //Exploding legendaries are too brutal.
                // chara.rarity = Rarity.Legendary;
                boss.c_bossType = BossType.Boss;
                this.Bosses.Add(boss.uid);
                KiriaDLCPlugin.LogWarning("ZoneKiriaDungeon::Spawning", bossID + " " + boss.uid);
            }
        }
    }
}