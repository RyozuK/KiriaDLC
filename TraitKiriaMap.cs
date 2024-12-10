
using Mod_KiriaDLC;
using UnityEngine;
public class TraitKiriaMap : TraitScrollMapTreasure
{
    //This is here because of CSharp binding shenanigans, and since it's a quest item, as funny as it would be
    //for it to spawn cursed and ruin the quest, let's not.
    public override bool OnUse(Chara c)
    {
        if (this.owner.refVal == 0)
            this.owner.refVal = EClass.rnd(30000) + 1;
        if (this.GetDest() == null || EClass._map.IsIndoor)
        {
            Msg.Say("nothingHappens");
            return false;
        }

        EClass.ui.layerFloat.ToggleLayer<LayerTreasureMap>()?.SetMap(this);
        return false;
    }
    
     
    public new Point GetDest(bool fix = false)
    {
        Point point = new Point();
        int num = this.owner.GetInt(104);
        if (num == 0)
        {
            Rand.SetSeed(this.owner.refVal);
            for (int index = 0; index < 10000; ++index)
            {
                point.x = EClass.scene.elomap.minX + EClass.rnd(200);
                point.z = EClass.scene.elomap.minY + EClass.rnd(200);
                //Changed this so that we validate for nefia spawning instead of treasure spawning
                if (EClass.scene.elomap.CanBuildSite(point.x, point.z, 1, ElomapSiteType.Nefia))
                {
                    Rand.SetSeed();
                    num = (point.x + 500) * 1000 + (point.z + 500);
                    this.owner.SetInt(104, num);
                    break;
                }
            }
            Rand.SetSeed();
            if (num == 0)
                return (Point) null;
        }
        Point dest = new Point(num / 1000 - 500, num % 1000 - 500);
        if (fix)
        {
            dest.x -= EClass.scene.elomap.minX;
            dest.z -= EClass.scene.elomap.minY;
        }
        return dest;
    }

    //This spawns the nefia, it's called by the Prefix patch for digging
    public Zone SpawnNefia()
    {
        //We create the site this way because the CreateSite with given position is private
        //Radius 1 basically means within 1 tile of the PC's current location
        Zone site = EClass.world.region.CreateRandomSite(EClass._zone, 1, "kiria_dungeon");
        if (site != null)
        {
            site.isKnown = true;
            Msg.Say("discoverZone", site.NameWithDangerLevel);
            KiriaDLCPlugin.LogWarning("TraitKiriaMap.SpawnNefia","Map created " + site.NameWithDangerLevel);
            //I wanted to close the no-longer-valid map but this doesn't seem to do it.
            // EClass.ui.layerFloat.GetLayer<LayerTreasureMap>()?.CloseLayers();
            return site;
        }
        else
        {
            Debug.LogError("Failed to create nefia");
        }

        return null;
    }
}