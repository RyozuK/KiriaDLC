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
    
    public static int CorpseID;
    
    public static void OnStartCore()
    {
        var sources = Core.Instance.sources;
        AddThings(sources);
        AddObjs(sources);
        AddZones(sources);
        AddQuest(sources);
        AddKirias(sources);
    }

    private static void AddObjs(SourceManager sources)
    {
        SourceObj.Row remains = sources.objs.rows.Find(x => x.id == 82);
        //SourceThing.Row remains = sources.things.rows.Find(x => x.id == "870");
        sources.objs.rows.Add(MakeRemains(remains));
    }
    
    
    private static SourceObj.Row MakeRemains(SourceObj.Row remains)
    {
        var corpse = CreateCopy(remains);
        CorpseID = EClass.sources.objs.rows.Max(row => row.id) + 1;
        corpse.id = CorpseID;
        corpse.name = "remains of Strange Engineer 「Ryozu」";
        corpse.name_JP = "Strange Engineer 「Ryozu」"; //TODO: translate
        corpse.chance = 0;
        corpse.defMat = "bone";
        corpse.components = ["gene_kiria"];
        return corpse;
    }


    private static void AddKirias(SourceManager sources)
    {
        SourceChara.Row putty_metal = sources.charas.rows.Find(x => x.id == "putty_metal");
        sources.charas.rows.Add(MakeKiriaPutit(putty_metal));

        SourceChara.Row rock_thrower = sources.charas.rows.Find(x => x.id == "rock_thrower");
        sources.charas.rows.Add(MakeKiriaHeadless(rock_thrower));

        SourceChara.Row infantry = sources.charas.rows.Find(x => x.id == "mass_steel");
        sources.charas.rows.Add(MakeKiriaBroken(infantry));

        SourceChara.Row hot_guy = sources.charas.rows.Find(x => x.id == "hardgay");
        sources.charas.rows.Add(MakeKiriaBunny(hot_guy));

    }

    private static SourceChara.Row MakeKiriaBunny(SourceChara.Row hotGuy)
    {
        var kiria_bunny = CreateCopy(hotGuy);
        kiria_bunny.LV = 19;
        kiria_bunny.id = "kiria_bunny";
        kiria_bunny._idRenderData = "@chara";
        kiria_bunny.name = "Bunny Bomb Kiria";
        kiria_bunny.name_JP = "...";
        kiria_bunny.race = "machine";
        kiria_bunny.idText = "";
        kiria_bunny.tag = ["noPortrait"];
        kiria_bunny.trait = ["UniqueChara"];
        kiria_bunny.chance = 0;
        return kiria_bunny;

    }

    private static SourceChara.Row MakeKiriaBroken(SourceChara.Row infantry)
    {
        var kiria_broken = CreateCopy(infantry);
        kiria_broken.LV = 42;
        kiria_broken.id = "kiria_broken";
        kiria_broken._idRenderData = "@chara";
        kiria_broken.name = "Damaged Kiria";
        kiria_broken.name_JP = "...";
        kiria_broken.race = "machine";
        kiria_broken.idText = "";
        kiria_broken.tag = ["noPortrait"];
        kiria_broken.trait = ["UniqueChara"];
        kiria_broken.chance = 0;
        return kiria_broken;
    }

    private static SourceChara.Row MakeKiriaHeadless(SourceChara.Row rockThrower)
    {
        var kiria_headless = CreateCopy(rockThrower);
        kiria_headless.LV = 39;
        kiria_headless.id = "kiria_headless";
        kiria_headless._idRenderData = "@chara";
        kiria_headless.name = "Damaged Kiria";
        kiria_headless.name_JP = "...";
        kiria_headless.race = "machine";
        kiria_headless.idText = "";
        kiria_headless.tag = ["noPortrait"];
        kiria_headless.trait = ["UniqueChara"];
        kiria_headless.chance = 0;
        return kiria_headless;
    }

    private static SourceChara.Row MakeKiriaPutit(SourceChara.Row puttyMetal)
    {
        var kiria_putit = CreateCopy(puttyMetal);
        kiria_putit.LV = 39;
        kiria_putit.id = "kiria_putit";
        kiria_putit._idRenderData = "@chara";
        kiria_putit.name = "Putit Kiria";
        kiria_putit.name_JP = "...";
        kiria_putit.race = "machine";
        kiria_putit.idText = "";
        kiria_putit.tag = ["noPortrait"];
        kiria_putit.trait = ["UniqueChara"];
        kiria_putit.chance = 0;
        return kiria_putit;
    }

    private static void AddZones(SourceManager sources)
    {
        SourceZone.Row zone_nymelle = sources.zones.rows.Find(x => x.id == "nymelle");
        sources.zones.rows.Add(MakeKiriaZoneDungeon(zone_nymelle));
        SourceZone.Row zone_nymelle_crystal = sources.zones.rows.Find(x => x.id == "nymelle_crystal");
        sources.zones.rows.Add(MakeKiriaZoneLab(zone_nymelle_crystal));

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

        SourceThing.Row gene = sources.things.rows.Find(row => row.id == "gene");
        sources.things.rows.Add(MakePrizeGene(gene));

    }

    private static SourceThing.Row MakePrizeGene(SourceThing.Row gene)
    {
        var prizeGene = CreateCopy(gene);
        prizeGene.id = "gene_kiria";
        prizeGene.name = "Nanomachine Gene";
        prizeGene.trait = ["KiriaGene"];
        //prizeGene.elements = [1410, 1, 1652, 1];
        return prizeGene;
    }

    private static SourceThing.Row MakeBookTool(SourceThing.Row toolBuild)
    {
        var book_tool = CreateCopy(toolBuild);
        book_tool.id = "kiriaTool";
        book_tool.name = "Kiria Item Tool";
        book_tool.name_JP = "Kiria Item Tool";
        book_tool.trait = ["KiriaItemTool"];
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
        map_kiria.tiles = [1713];
        map_kiria.defMat = "paper";
        map_kiria.LV = 36;
        map_kiria.quality = 4;
        map_kiria.tag = ["noShop"];
        map_kiria.chance = 0;
        //Item Traits are expected to not be in a mod's namespace, and to be a class prefixed with Trait
        //See TraitKiriaMap.cs for this trait
        map_kiria.trait = ["KiriaMap"];
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
        quest.drama = ["kiriaDLC", "main"];
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
