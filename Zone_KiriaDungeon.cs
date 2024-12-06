using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Mod_KiriaDLC;

public class Zone_KiriaDungeon : Zone_Dungeon
{
    public static int LvBasement => -KiriaDLCPlugin.NUM_FLOORS;
    public static int LvBoss => LvBasement + 1;
    public List<Chara> bosses;
    public bool BossesDead => bosses?.All(boss => boss.isDead) ?? false;

    public override bool RestrictBuild => this.lv == LvBasement; //Don't let the PC build here.
    // public override bool AlwaysLowblock => false;
    public override bool IsReturnLocation => false;

    public new Chara Boss
    {
        get => bosses != null && !BossesDead ? bosses.First(chara => !chara.isDead) : bosses?.TryGet(0);
        set => bosses.Add(value);
    }
    
    public override string IDPlayList
    {
        get
        {
            if (this.lv == LvBoss && (bosses == null || !BossesDead))
            {
                    KiriaDLCPlugin.LogWarning("ZoneKiriaDungeon.IDPlayList","\t\tPlaying Boss Music");
                    return "Dungeon_Boss";
            }
            return this.lv == LvBoss && BossesDead ? "Dungeon_Boss" : base.IDPlayList;
        }
    }

    //Note, base Zone_Dungeon will skip ore/big daddy/evolved/shrine generation if this is not empty
    public override string idExport => this.lv == LvBasement ? "kiria_lab" : ""; //this.source.id;


    //When the game tries to load a map file for map generation, it'll refer to this for its location
    //We use Assembly.GetExecutingAssembly() to point at our mod's folder
    public override string pathExport => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
        $"Map/{this.idExport}.z");

    
    public override bool LockExit => this.lv == LvBoss && //If it's the boss level 
                                     (this.bosses == null || //And the bosses list is instantiated
                                     !BossesDead); //And any of the bosses is alive

    public override bool CanUnlockExit => EClass.game.quests.GetPhase<QuestKiria>() >= QuestKiria.PHASE_BOSS_DEAD;

    public override bool UseFog => this.lv != LvBasement;

    public override bool RevealRoom => this.lv == LvBasement;
    
    public override bool HasLaw => this.lv == LvBasement;

    public override float PrespawnRate => this.lv == LvBasement ? 0.0f : base.PrespawnRate;

    public override string GetNewZoneID(int level)
    {
        return level == LvBasement ? "kiria_lab" : this.source.id;
    }
    
    // public override bool ScaleMonsterLevel => true;
    
    //Note: This is called before OnEnterZone
    public override void OnBeforeSimulate()
    {
        
        if (this.lv == LvBoss && this.bosses == null && EClass.game.quests.GetPhase<QuestKiria>() == QuestKiria.PHASE_BOSS - 1)
        {
            KiriaDLCPlugin.LogWarning("ZoneKiriaDungeon::OnBeforeSimulate", "Summong Kirias?");
            this._dangerLv = 100;
            KiriaDLCPlugin.LogWarning("ZoneKiria.OnBeforeSimulate","\tAdding Kirias to map");
            Thing exit = EClass._map.props.installed.Find<TraitStairsLocked>();
            if (exit == null)
            {
                Debug.LogError("\tDID NOT FIND EXIT");
                base.OnGenerateMap();
                return;
            }
            //For future reference, this should get a reasonable spawn location
            //EClass._map.bounds.GetRandomSpawnPos()
            this.bosses =
            [
                (Chara)AddChara("kiria_putit", exit.pos.x, exit.pos.z),
                (Chara)AddChara("kiria_putit", exit.pos.x, exit.pos.z),
                (Chara)AddChara("kiria_bunny", exit.pos.x, exit.pos.z),
                (Chara)AddChara("kiria_bunny", exit.pos.x, exit.pos.z),
                (Chara)AddChara("kiria_headless", exit.pos.x, exit.pos.z),
                (Chara)AddChara("kiria_headless", exit.pos.x, exit.pos.z),
                (Chara)AddChara("kiria_broken", exit.pos.x, exit.pos.z)
            ];
            QuestKiria qk = EClass.game.quests.Get<QuestKiria>();
            qk.bosses = this.bosses;
            foreach (var chara in bosses)
            {
                chara.hostility = Hostility.Enemy;
                chara.enemy = EClass.pc.party.members.RandomItem<Chara>();
                //This turned out to be a bad idea... Custom mob cards/figures seem to be broken, and
                //Exploding legendaries are too brutal.
                // chara.rarity = Rarity.Legendary;
                chara.c_bossType = BossType.Boss;
            }
            
        }
    }
}