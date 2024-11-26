using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace Mod_KiriaDLC;

[BepInPlugin("net.ryozu.kiriadlc", "Kiria DLC", "1.0.0.0")]
public class Plugin : BaseUnityPlugin
{
    private void Start()
    {
        Debug.Log("KIRIADLC: Mod Start()");
        var harmony = new Harmony("net.ryozu.kiriadlc");
        harmony.PatchAll();
    }
    public void OnStartCore() {
        // Console.WriteLine("KIRIADLC::OnStartCore() invoked");
        // string folder = Info.Location;
        KiriaEntries.OnStartCore();
    }
}

// TODO
// Add the books/notes
// Make the map
// Translate it all
// Add consequences? (IE give choice to implant gene into Kiria or have her get upset)
// Make the last floor locked/Boss guarded like Void?



// [HarmonyPatch(typeof(Game))]
// [HarmonyPatch(nameof(Game.StartNewGame))]
// class StartPatch : EClass
// {
//     static void Postfix(Game __instance)
//     {
//         CharaGen.Create("adv_kiria").SetGlobal(EClass.game.StartZone, EClass.game.Prologue.posFiama.x,
//             EClass.game.Prologue.posAsh.y);
//     }
// }


[HarmonyPatch(typeof(Zone))]
[HarmonyPatch(nameof(Zone.Activate))]
class ZonePatch : EClass {
    static void Prefix(Zone __instance)
    {
        Debug.LogWarning("Zone.Activate() called:");
        Debug.LogWarning("\t" + __instance.NameWithDangerLevel);
        Debug.LogWarning("\t" + __instance.pathExport);
    }
    static void Postfix(Zone __instance) {
        Debug.LogWarning("Now entering " + __instance.source.id);
        //If they've already gotten the quest, or the quest is finished, we don't want to add it again
        //This is a one and done quest
        if (!EClass.game.quests.IsCompleted("kiria_map_quest") && !EClass.game.quests.IsStarted<QuestKiria>())
        {
            //Quest must have a client, we find Kiria to be the client
            Chara c = EClass.game.cards.globalCharas.Find("adv_kiria");
            //If Kiria is recruited and has enough affinity, add the quest 
            if (c != null) // && c.IsPCFaction && c.affinity.value >= 85) //Pre marriage, post recruit
            {
                //Putting it on the global quest list and setting the client will make the quest
                //Appear on the quest board
                Debug.Log("KiriaDLC:: Adding quest to global list");
                EClass.game.quests.globalList.Add(Quest.Create("kiria_map_quest").SetClient(c, false));
            }
            
        }
    }
}

[HarmonyPatch(typeof(TaskDig))]
[HarmonyPatch(nameof(TaskDig.OnProgressComplete))]
class DigPatch : TaskDig
{
    static bool Prefix(TaskDig __instance)
    {
        //Can only dig up Nefia when in the world map
        if (EClass._zone.IsRegion)
        {
            //Find if the player holds a map for the current location
            Thing map = GetNefiaMap(__instance);
            if (map != null)
            {
                //We're putting the spawn logic inside the trait to make it easier to customize
                (map.trait as TraitKiriaMap).SpawnNefia(); 
                //We're done with this map
                map.Destroy();
                EClass.player.willAutoSave = true;
                //We'll change this to false in case this doesn't avoid digging up treasure too
                return true;
            }
        }
        return true;
    }
    //Helper to find specifically our map, an edit of TaskDig's GetTreasureMap
    public static Thing GetNefiaMap(TaskDig __instance)
    {
        foreach (Thing nefiaMap in EClass.pc.things.List((Func<Thing, bool>) (t => t.trait is TraitKiriaMap)))
        {
            if (__instance.pos.Equals((object) (nefiaMap.trait as TraitKiriaMap).GetDest(true)))
                return nefiaMap;
        }
        return (Thing) null;
    }
}

[HarmonyPatch(typeof(TraitBook))]
[HarmonyPatch(nameof(TraitBook.OnRead))]
class BookReadPatch : TraitBook
{
    static void Prefix(TraitBook __instance)
    {
        BookList.Item item = __instance.Item;
        Debug.LogWarning("KiriaDLC::TaitBook::OnRead");
        Debug.LogWarning("\t" + (__instance.IsParchment ? "LayerParchment" : "LayerBook"));
        Debug.LogWarning("\t" + (__instance.IsParchment ? "Scroll/" : "Book/") + item.id);
    }
}


/* ******************************
 * The following is all purely for testing/debugging.  leaving it in for posterity
 ********************************/

//
// //This dig into the quest system here
//
// [HarmonyPatch(typeof(Quest))]
// [HarmonyPatch(nameof(Quest.OnClickQuest))]
// class QuestPatch : Quest
// {
//     public static void Prefix(Quest __instance)
//     {
//         Debug.LogWarning("QUEST::Information::Quest.id is [" + __instance.id + "]");
//         Debug.LogWarning("QUEST::Information::Quest.drama is [" + String.Join(", ", __instance.source.drama) + "]");
//     }
// }
//
// //No really, who's talking?
//
// [HarmonyPatch(typeof(LayerDrama))]
// [HarmonyPatch(nameof(LayerDrama.Activate))]
// class LayerDramaPatch : LayerDrama
// {
//     static void Prefix(string book, string idSheet, string idStep,
//         Chara target = null, Card ref1 = null, string tag = "")
//     {
//         Debug.LogWarning("LayerDrama::Activate: Book    : " + book);
//         Debug.LogWarning("LayerDrama::Activate: idSheet : " + idSheet);
//         Debug.LogWarning("LayerDrama::Activate: idStep  : " + idStep);
//         Debug.LogWarning("LayerDrama::Activate: target  : " + target);
//         
//     }
// }
//
// [HarmonyPatch(typeof(DramaManager))]
// [HarmonyPatch(nameof(DramaManager.ParseLine))]
// class DramaManagerPatch : DramaManager
// {
//     static void Prefix(Dictionary<string, string> item)
//     {
//         bool notEmpty = false;
//         string output = "ParseLIne: ";
//         foreach (var i in item)
//         {
//             if (!i.Value.Equals(""))
//             {
//                 notEmpty = true;
//                 output += "\n\t\tKey: " + i.Key + " | Value: " + i.Value;
//             }
//         }
//
//         if (notEmpty)
//         {
//             Debug.LogWarning(output);
//         }
//     }
// }
//
// [HarmonyPatch(typeof(Map))]
// [HarmonyPatch(nameof(Map.TryLoadFile))]
// class MapTryPatch : Map
// {
//     static void Prefix(string path, string s, int size)
//     {
//         Debug.LogWarning("MapPatch::TryLoadFile called with: path/s/size=[" + path + "][" + s + "][" + size + "]");
//     }
// }
//
// [HarmonyPatch(typeof(Map))]
// [HarmonyPatch(nameof(Map.Load))]
// class MapLoadPatch : Map
// {
//     static void Prefix(string path, bool import = false, PartialMap partial = null)
//     {
//         Debug.LogWarning("MapPatch::Load called with: path/import/partial=[" + path + "][" + import + "][" + partial + "]");
//     }
// }
//
