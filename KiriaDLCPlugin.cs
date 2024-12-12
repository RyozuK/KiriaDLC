using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using ReflexCLI;
using UnityEngine;


namespace Mod_KiriaDLC;



[BepInPlugin("net.ryozu.kiriadlc", "Kiria DLC", "1.0.0.0")]
public class KiriaDLCPlugin : BaseUnityPlugin
{
    public static readonly bool DEBUG_MODE = false;
    public static readonly bool DEBUG_OVERRIDE = false;
    public static readonly int NUM_FLOORS = DEBUG_MODE ? 3 : 6;
    
    private void Awake()
    {
        Assembly executingAssembly = Assembly.GetExecutingAssembly();
        CommandRegistry.assemblies.Add(executingAssembly);
    }
    
    public static void LogWarning(String loc, String msg)
    {
        if (DEBUG_MODE || DEBUG_OVERRIDE)
        {
            Debug.LogWarning("KiriaDLC::"+loc+":: " + msg);
        }
    }
    public static void printFields<T>(T baseItem) where T : new() {
        System.Reflection.FieldInfo[] fields = baseItem.GetType().GetFields();
        String baseText = baseItem.ToString();
        foreach (System.Reflection.FieldInfo fieldInfo in fields) {
            KiriaDLCPlugin.LogWarning(baseText, fieldInfo.Name + " : " + baseItem.GetField<object>(fieldInfo.Name));
        }
    }
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


[HarmonyPatch(typeof(Chara))]
[HarmonyPatch(nameof(Chara.GetTopicText))]
class CharaTextPatch : Chara
{   //We want to change Kiria's "CharaText" if the player uses the gene on her.  Since CharaText isn't an instance
    //variable, we'll have to interject in the GetTopicText
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        //Add this: key = CheckDna(key, this);
        //right after the first line that says "key ="
        return new CodeMatcher(instructions).MatchEndForward(
            new CodeMatch(OpCodes.Stloc_0)).Advance(1) //FInd the first "key =" and move past it
            .InsertAndAdvance(  //Insert the following function call
                new CodeInstruction(OpCodes.Ldloc_0), //key is the first argument
                new CodeInstruction(OpCodes.Ldarg_0), //'this' is the second argument
                Transpilers.EmitDelegate(CheckDna),          //Insert invocation of the delegate
                new CodeInstruction(OpCodes.Stloc_0)  //Store the resulting stack into key
        ).InstructionEnumeration();
    }

    private static string CheckDna(string key, Chara target)
    {
        if (key == "adv_kiria" && target.c_genes?.items.Find(gene => gene.id == "android_kiria") != null)
        {
            key = "adv_kiria2";
        }
        // KiriaDLCPlugin.LogWarning("CheckDNA", "returning key: " + key);
        return key;
    }
}

[HarmonyPatch(typeof(DNA))]
[HarmonyPatch(nameof(DNA.Apply), [typeof(Chara), typeof(bool)])]
class DNAApplyPatch : DNA
{
    static void Postfix(DNA __instance, Chara c, bool reverse)
    {
        if (__instance.id == "android_kiria")
        {
            Chara kiria = EClass.game.cards.globalCharas.Find("adv_kiria");
            if (c == kiria)
            {
                kiria.ShowDialog("kiriaDLC", "used_gene_kiria");
                kiria.affinity.Mod(50);
            }
            else
            {
                kiria.ShowDialog("kiriaDLC", "used_gene_other");
                
                //pc.DoHostileAction(kiria);
                kiria.hostility = Hostility.Enemy;
                kiria.DoHostileAction((Card) EMono.pc, true);
                kiria.enemy = pc;
                kiria.calmCheckTurn = 255;
            }
        }
    }
}


[HarmonyPatch(typeof(Zone))]
[HarmonyPatch(nameof(Zone.Activate))]
class ZonePatch : EClass {

    static void Postfix(Zone __instance) {
        KiriaDLCPlugin.LogWarning("Zone.Activate Postfix","Now entering " + __instance.source.id);
        //If they've already gotten the quest, or the quest is finished, we don't want to add it again
        //Also make sure it's the PC's zone and that it's not already in the list to avoid duplication
        //and issues with moongates.
        //This is a one and done quest
        if (!EClass.game.quests.IsCompleted("kiria_map_quest") 
            && !EClass.game.quests.IsStarted<QuestKiria>()
            && EClass._zone.IsPCFaction
            && EClass.game.quests.globalList.All(x => x.id != "kiria_map_quest")
            )
        {
            //Quest must have a client, we find Kiria to be the client
            //In theory, this will be the base Kiria
            Chara c = EClass.game.cards.globalCharas.Find("adv_kiria");
            //If Kiria is recruited and has enough affinity, add the quest 
            if
                (c != null &&
                 (KiriaDLCPlugin.DEBUG_MODE || 
                  (c.IsPCFaction && c.affinity.value >= 85))) //Pre marriage, post recruit
            {
                //Putting it on the global quest list and setting the client will make the quest
                //Appear on the quest board
                KiriaDLCPlugin.LogWarning("Zone.Activate Postfix","KiriaDLC:: Adding quest to global list");
                EClass.game.quests.globalList.Add(Quest.Create("kiria_map_quest").SetClient(c, false));
            }
            
        }
    }
}

[HarmonyPatch(typeof(Chara))]
[HarmonyPatch(nameof(Chara.Pick))]
class PickupPatch : Chara
{
    static void Postfix(Chara __instance, Thing t)
    {
        KiriaDLCPlugin.LogWarning("Chara", "Pick: " + t.id + " with uid of " + t.uid);
        //At least for this mod, we only care about the gene and letters
        if (t.id != "letter" && t.id != "gene_kiria")
        {
            return;
        }

        //Only the player or their pawns matter for picking up items here
        if (__instance.IsPC || __instance.IsPCFaction)
        {
            QuestKiria quest = EClass.game.quests.Get<QuestKiria>();
            //If they even have the quest
            if (quest is null)
            {
                return;
            }
            //Let the quest know the player picked up the item.
            quest.OnItemPickup(t);
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
                //We're done with this map
                QuestKiria quest = EClass.game.quests.Get<QuestKiria>();
                if (quest is not null && !quest.isComplete)
                {
                    quest.MapItem = null;
                    quest.KiriaZone = (map.trait as TraitKiriaMap)?.SpawnNefia(); 
                }

                while (map != null)
                {
                    map.Destroy();
                    map = GetNefiaMap(__instance);
                }

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
            if (__instance.pos.Equals((object) (nefiaMap.trait as TraitKiriaMap)?.GetDest(true)))
                return nefiaMap;
        }
        return (Thing) null;
    }
}

//
// [HarmonyPatch(typeof(LayerDrama))]
// [HarmonyPatch(nameof(LayerDrama.Activate))]
// class LayerDramaPatch : LayerDrama
// {
//     static void Prefix(string book, string idSheet, string idStep,
//         Chara target = null, Card ref1 = null, string tag = "")
//     {
//         // KiriaDLCPlugin.LogWarning("LayerDrama::Activate",": Book    : " + book);
//         // KiriaDLCPlugin.LogWarning("LayerDrama::Activate",": idSheet : " + idSheet);
//         // KiriaDLCPlugin.LogWarning("LayerDrama::Activate",": idStep  : " + idStep);
//         // KiriaDLCPlugin.LogWarning("LayerDrama::Activate",": target  : " + target);
//         if (target != null)
//         {
//             KiriaDLCPlugin.LogWarning("LayerDrama::Activate", "Target ID is " + target.id);
//             KiriaDLCPlugin.LogWarning("LayerDrama::Activate", "Target UID is " + target.uid);
//             if (target.c_genes != null)
//             {
//                 foreach (var item in target.c_genes.items)
//                 {
//                     KiriaDLCPlugin.LogWarning(item.ToString(), "-----------------");
//                     KiriaDLCPlugin.LogWarning(item.id, "-----------------");
//                     KiriaDLCPlugin.printFields(item);
//                 }
//             }
//         }
//         
//     }
//     
//
// }

