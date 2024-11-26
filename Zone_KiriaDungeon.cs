
using System.IO;
using System.Reflection;

namespace Mod_KiriaDLC; 

public class Zone_KiriaDungeon : Zone_Dungeon
{

    public int LvBasement => -3;
    public int LvBoss => LvBasement + 1;
    public override string idExport
    {
        get
        {
            if (this.lv == this.LvBasement)
            {
                return "kiria_lab";
            }
            else if (this.lv == this.LvBoss)
            {
                return "kiria_boss";
            }
            return this.source.id;
        }
    }

    //When the game tries to load a map file for map generation, it'll refer to this for it's location
    //We use Assembly.GetExecutingAssembly() to point at our mod's folder
    public override string pathExport => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                $"Map/{this.idExport}.z");

    public override bool LockExit => this.lv == this.LvBoss;
    
    public override bool CanUnlockExit => this.Boss == null || this.Boss.isDead;
    
    public override bool UseFog => this.lv != LvBasement;

    public override bool RevealRoom => this.lv == LvBasement;

    public override float PrespawnRate => this.lv == this.LvBasement ? 0 : base.PrespawnRate;

    public override string GetNewZoneID(int level)
    {
        if (level == this.LvBoss)
        {
            return "kiria_boss";
        }
        if (level == this.LvBasement)
        {
            return "kiria_lab";
        }
        return this.source.id;
    }
    
    // public override void OnGenerateMap()
    // {
    //     this._dangerLv = 50;
    //     if (this.IsBossLV)
    //     {
    //         this.Boss = this.SpawnMob(setting: SpawnSetting.Boss(this.DangerLv));
    //         this.Boss.hostility = this.Boss.c_originalHostility = Hostility.Enemy;
    //         foreach (Chara chara in EClass._map.charas)
    //         {
    //             if (chara.IsHostile())
    //                 chara.enemy = EClass.pc.party.members.RandomItem<Chara>();
    //         }
    //     }
    //     base.OnGenerateMap();
    // }
}


}