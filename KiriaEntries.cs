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
    public static void OnStartCore(string folder)
    {
        var sources = Core.Instance.sources;
        AddThings(sources, folder);
        AddQuest(sources);
        
    }

    private static void AddThings(SourceManager sources, string folder)
    {
        Console.WriteLine("KIRIADLC::AddThings:: Adding manual entry");

        SourceThing.Row treasure_map = sources.things.rows.Find(x => x.id == "map_treasure");
        sources.things.rows.Add(MakeKiriaMap(treasure_map));
    }

    private static SourceThing.Row MakeKiriaMap(SourceThing.Row treasureMap)
    {
        var map_kiria = CreateCopy(treasureMap);
        map_kiria.id = "map_kiria";
        map_kiria.name_JP = "キリアの地図";
        map_kiria.name = "Kiria's map";
        map_kiria.naming = "";
        map_kiria.unknown = "Oddly drawn map";
        map_kiria.tiles = new int[] { 1713 }; //.AddItem(1713);
        map_kiria.defMat = "paper";
        map_kiria.LV = 36;
        map_kiria.quality = 4;
        map_kiria.tag = new string[] { "noShop" };
        map_kiria.chance = 0;
        map_kiria.trait = new string[] {"KiriaMap"};
        map_kiria.detail_JP = "キリア提供の手描き地図";
        map_kiria.detail = "A hand drawn map provided by Kiria";
        
        // sources.things.rows.Add(map_kiria);
        return map_kiria;

    }

    private static void AddQuest(SourceManager sources)
    {
        var quest = sources.quests.CreateRow();
        //ClassCache.assemblies.Add("Mod_KiriaDLC");
        quest.id = "kiria_map_quest";
        quest.name = "Investigation request";
        quest.type = "Mod_KiriaDLC.QuestKiria";
        quest.detail =
            "#pc, can you look into something for me?";
        sources.quests.rows.Add(quest);
    }
    
    public static T CreateCopy<T>(T baseItem) where T : new() {
        System.Reflection.FieldInfo[] fields = baseItem.GetType().GetFields();
        T newItem = new T();
        foreach (System.Reflection.FieldInfo fieldInfo in fields) {
            newItem.SetField<object>(fieldInfo.Name, baseItem.GetField<object>(fieldInfo.Name));
        }
        return newItem;
    }
   
}
