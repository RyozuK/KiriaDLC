
using System.IO;
using System.Reflection;

namespace Mod_KiriaDLC; 

public class Zone_KiriaDungeon : Zone_Dungeon
{
    public override string idExport => this.lv == this.LvBasement ? "kiria_lab" : this.source.id;

    public override string pathExport => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                $"Map/{this.idExport}.z");

    public int LvBasement => -2;

    public override bool UseFog => this.lv != LvBasement;

    public override bool RevealRoom => this.lv == LvBasement;

    public override float PrespawnRate
    {
        get => this.lv == this.LvBasement ? 0 : base.PrespawnRate;
    }

    public override string GetNewZoneID(int level)
    {
        if (level == this.LvBasement)
        {
            return "kiria_lab";
        }
        return this.source.id;
    }

    public override bool LockExit => false;

}