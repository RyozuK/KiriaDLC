using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace Mod_KiriaDLC;

[BepInPlugin("net.ryozu.kiriadlc", "Kiria DLC", "1.0.0.0")]
public class Plugin : BaseUnityPlugin
{
    private void Start()
    {
        // System.Console.WriteLine("KIRIADLC: Mod Start()");
        var harmony = new Harmony("net.ryozu.kiriadlc");
        harmony.PatchAll();
    }
    public void OnStartCore() {
        // Console.WriteLine("KIRIADLC::OnStartCore() invoked");
        string folder = Info.Location;
        KiriaEntries.OnStartCore(folder);
    }
}


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
    static void Postfix(Zone __instance) {
        System.Console.WriteLine("KIRIADLC: ZonePatch::Postfix called");

        
        if (!EClass.game.quests.IsCompleted("kiria_map_quest") && !EClass.game.quests.IsStarted<QuestKiria>())
        {
            Chara c = EClass.game.cards.globalCharas.Find("adv_kiria");
            if (true) //c.IsPCFaction && c.affinity.value >= 85) //Pre marriage, post recruit
            {
                // System.Console.WriteLine("KIRIADLC: Kiria Data");
                // Debug.LogWarning((object)c._affinity);
                EClass.game.quests.globalList.Add(Quest.Create("kiria_map_quest").SetClient(c, false));
            }
            
        }
    }
}

// [HarmonyPatch(typeof(ExcelParser))]
// [HarmonyPatch(nameof(ExcelParser.GetIntArray))]
// class ParserPatch : ExcelParser
// {
//     static void Prefix(ExcelParser __instance)
//     {
//         Debug.LogWarning((object)("ExcelParser::GetIntString::Parsing row " + row));
//     }
//     
// }