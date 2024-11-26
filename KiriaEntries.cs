using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System;
using System.IO;

namespace Mod_KiriaDLC;

[HarmonyPatch]
public class KiriaEntries
{
    public static void OnStartCore()
    {
        var sources = Core.Instance.sources;
        AddThings(sources);
        AddZones(sources);
        AddQuest(sources);
    }

    private static void AddZones(SourceManager sources)
    {
        SourceZone.Row zone_nymelle = sources.zones.rows.Find(x => x.id == "nymelle");
        sources.zones.rows.Add(MakeKiriaZoneDungeon(zone_nymelle));
        SourceZone.Row zone_nymell_boss = sources.zones.rows.Find(x => x.id == "nymelle_boss");
        sources.zones.rows.Add(MakeKiriaBoss(zone_nymell_boss));
        SourceZone.Row zone_nymelle_crystal = sources.zones.rows.Find(x => x.id == "nymelle_crystal");
        sources.zones.rows.Add(MakeKiriaZoneLab(zone_nymelle_crystal));

    }

    private static SourceZone.Row MakeKiriaBoss(SourceZone.Row zoneNymellBoss)
    {
        var zone_kiria_boss = CreateCopy(zoneNymellBoss);
        zone_kiria_boss.id = "kiria_boss";
        zone_kiria_boss.name = "Strange factory";
        zone_kiria_boss.name_JP = "..."; //TODO
        zone_kiria_boss.type = "Mod_KiriaDLC.Zone_KiriaDungeon";
        zone_kiria_boss.LV = 36;
        zone_kiria_boss.textFlavor = "You hear ominous beeping noises...";
        zone_kiria_boss.textFlavor_JP = "..."; //TODO

        return zone_kiria_boss;

    }

    private static SourceZone.Row MakeKiriaZoneLab(SourceZone.Row zoneNymelleCrystal)
    {
        var zone_kiria_lab = CreateCopy(zoneNymelleCrystal);
        zone_kiria_lab.id = "kiria_lab";
        zone_kiria_lab.name = "Fanatic's Laboratory";
        zone_kiria_lab.name_JP = "..."; //TODO
        zone_kiria_lab.type = "Mod_KiriaDLC.Zone_KiriaDungeon";
        zone_kiria_lab.LV = 1;
        zone_kiria_lab.textFlavor = "This area has a foreboding aura.";
        zone_kiria_lab.textFlavor_JP = "..."; //TODO
        
        return zone_kiria_lab;
    }

    private static SourceZone.Row MakeKiriaZoneDungeon(SourceZone.Row zoneNymelle)
    {
        var zone_kiria_dungeon = CreateCopy(zoneNymelle);
        zone_kiria_dungeon.id = "kiria_dungeon";
        zone_kiria_dungeon.name = "Strange factory";
        zone_kiria_dungeon.name_JP = "..."; //TODO
        zone_kiria_dungeon.type = "Mod_KiriaDLC.Zone_KiriaDungeon";
        zone_kiria_dungeon.LV = 35;
        zone_kiria_dungeon.idProfile = "DungeonFactory";
        zone_kiria_dungeon.idBiome = "Dungeon_Factory";
        zone_kiria_dungeon.textFlavor = "A strange factory full of broken machinery";
        zone_kiria_dungeon.textFlavor_JP = "..."; //TODO
        return zone_kiria_dungeon;
    }

    private static void AddThings(SourceManager sources)
    {
        Debug.Log("KIRIADLC::AddThings:: Adding manual entry");
        //Using Kuronekotei's example, we find an existing item to duplicate
        SourceThing.Row treasure_map = sources.things.rows.Find(x => x.id == "map_treasure");
        sources.things.rows.Add(MakeKiriaMap(treasure_map));
        
        SourceThing.Row tool_build = sources.things.rows.Find(x => x.id == "tool_build");
        sources.things.rows.Add(MakeBookTool(tool_build));
    }

    private static SourceThing.Row MakeBookTool(SourceThing.Row toolBuild)
    {
        var book_tool = CreateCopy(toolBuild);
        book_tool.id = "kiriaTool";
        book_tool.name = "Kiria Item Tool";
        book_tool.name_JP = "Kiria Item Tool";
        book_tool.trait = new string[] {"KiriaItemTool"};
        return book_tool;
    }

    private static SourceThing.Row MakeKiriaMap(SourceThing.Row treasureMap)
    {
        //Using a function to keep things clean
        var map_kiria = CreateCopy(treasureMap);
        map_kiria.id = "map_kiria";
        map_kiria.name_JP = "キリアの地図";
        map_kiria.name = "Kiria's map";
        map_kiria.naming = "";
        map_kiria.unknown = "Oddly drawn map";
        map_kiria.tiles = new int[] { 1713 };
        map_kiria.defMat = "paper";
        map_kiria.LV = 36;
        map_kiria.quality = 4;
        map_kiria.tag = new string[] { "noShop" };
        map_kiria.chance = 0;
        //Item Traits are expected to not be in a mod's namespace, and to be a class prefixed with Trait
        //See TraitKiriaMap.cs for this trait
        map_kiria.trait = new string[] {"KiriaMap"};
        map_kiria.detail_JP = "キリア提供の手描き地図";
        map_kiria.detail = "A hand drawn map provided by Kiria";
        
        // sources.things.rows.Add(map_kiria);
        return map_kiria;

    }

    private static void AddQuest(SourceManager sources)
    {
        //Sets up the quest's information so it can be instantiated later
        var quest = sources.quests.CreateRow();
        //ClassCache.assemblies.Add("Mod_KiriaDLC");
        quest.id = "kiria_map_quest";
        quest.name = "Investigation request";
        quest.name_JP = "..."; //TODO
        //If you have a quest lcass, specify it here, you can use a mod namespace, see
        //QuestKiria.cs
        quest.type = "Mod_KiriaDLC.QuestKiria";
        //The message on the quest board.
        //For multistep quests, each step's description is separated by pipes.
        quest.detail =
            "#pc, can you look into something for me?|You've recieved a map from Kiria, find out where it points.";
        quest.detail_JP =
            "#pc, ..."; //TODO
        quest.drama = new string[] { "kiriaDLC", "main" };
        sources.quests.rows.Add(quest);
    }
    
    //Utility, credit to kuronekotei https://github.com/kuronekotei/ElinMOD/
    public static T CreateCopy<T>(T baseItem) where T : new() {
        System.Reflection.FieldInfo[] fields = baseItem.GetType().GetFields();
        T newItem = new T();
        foreach (System.Reflection.FieldInfo fieldInfo in fields) {
            newItem.SetField<object>(fieldInfo.Name, baseItem.GetField<object>(fieldInfo.Name));
        }
        return newItem;
    }
}
