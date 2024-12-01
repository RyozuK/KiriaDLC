using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Mod_KiriaDLC;

public class Zone_KiriaDungeon : Zone_Dungeon
{
    public int LvBasement => -6;
    public int LvBoss => LvBasement + 1;
    public Chara[] bosses = null;

    public new Chara Boss => bosses.First(ch => !ch.isDead);

    public override bool IsReturnLocation => false;
    
    public override string IDPlayList => this.lv != this.LvBoss ? base.IDPlayList : "Dungeon_Boss";

    //Note, base Zone_Dungeon will skip ore/big daddy/evolved/shrine generation if this is not empty
    public override string idExport => this.lv == this.LvBasement ? "kiria_lab" : ""; //this.source.id;


    //When the game tries to load a map file for map generation, it'll refer to this for its location
    //We use Assembly.GetExecutingAssembly() to point at our mod's folder
    public override string pathExport => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
        $"Map/{this.idExport}.z");

    public override bool LockExit => this.lv == this.LvBoss && //If it's the boss level 
                                     (this.bosses == null || //And the bosses list is instantiated
                                     this.bosses.Any(b => !b.isDead)); //And any of the bosses is alive

    public override bool CanUnlockExit => this.bosses == null || this.bosses.All(b => b.isDead);

    public override bool UseFog => this.lv != LvBasement;

    public override bool RevealRoom => this.lv == LvBasement;

    public override float PrespawnRate => this.lv == this.LvBasement ? 0.0f : base.PrespawnRate;

    public override string GetNewZoneID(int level)
    {
        return level == this.LvBasement ? "kiria_lab" : this.source.id;
    }

    public override void OnGenerateMap()
    {
        Debug.LogWarning("KiriaDLC::OnGenerateMap()");
        if (this.lv == this.LvBoss && this.bosses == null)
        {
            this._dangerLv = 150;
            Debug.LogWarning("\tAdding Kirias to map");
            Thing exit = EClass._map.props.installed.Find<TraitStairsLocked>();
            if (exit == null)
            {
                Debug.LogError("\tDID NOT FIND EXIT");
                base.OnGenerateMap();
                return;
            }
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
            foreach (var chara in bosses)
            {
                chara.hostility = Hostility.Enemy;
                chara.enemy = EClass.pc.party.members.RandomItem<Chara>();
                // chara.rarity = Rarity.Legendary;
                chara.c_bossType = BossType.Boss;
            }
        }
        base.OnGenerateMap();
    }
}