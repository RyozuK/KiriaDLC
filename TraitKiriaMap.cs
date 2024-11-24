
﻿using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
public class TraitKiriaMap : TraitScrollMapTreasure
{
    //This is here only because of CSharp binding shenanigans. Without it, we don't run our custom GetDest
    public override bool OnUse(Chara c)
    {
        if (this.owner.refVal == 0)
            this.owner.refVal = EClass.rnd(30000) + 1;
        if (this.GetDest() == null || EClass._map.IsIndoor)
        {
            Msg.Say("nothingHappens");
            return false;
        }
        Rand.SetSeed(this.owner.refVal);
        if (this.owner.blessedState <= BlessedState.Cursed && EClass.rnd(2) == 0)
        {
            Msg.Say("mapCrumble", this.owner);
            this.owner.Destroy();
            return false;
        }
        Rand.SetSeed();
        EClass.ui.layerFloat.ToggleLayer<LayerTreasureMap>()?.SetMap(this);
        return false;
    }
    
    public new Point GetDest(bool fix = false)
    {
        Debug.LogWarning("KiriaDLC:: Map Trait GetDest is being used here");
        Point point = new Point();
        int num = this.owner.GetInt(104);
        if (num == 0)
        {
            Rand.SetSeed(this.owner.refVal);
            for (int index = 0; index < 10000; ++index)
            {
                point.x = EClass.scene.elomap.minX + EClass.rnd(200);
                point.z = EClass.scene.elomap.minY + EClass.rnd(200);
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

    public void SpawnNefia()
    {
        //This spawns the nefia
        Debug.LogWarning("KiriaDLC:: Here is where I would spawn my Nefia, if I had any");
        //This works!  Now to actually SPAWN the nefia.
        Zone site = EClass.world.region.CreateRandomSite(EClass._zone, 1, "kiria_dungeon");
        if (site != null)
        {
            site.isKnown = true;
            Msg.Say("discoverZone", site.NameWithDangerLevel);
            Debug.LogWarning("Map created " + site.NameWithDangerLevel);
        }
        else
        {
            Debug.LogError("Failed to create nefia");
        }
    }
}