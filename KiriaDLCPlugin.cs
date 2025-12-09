using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using ReflexCLI;
using UnityEngine;


// namespace Mod_KiriaDLC;



[BepInPlugin("net.ryozu.kiriadlc", "Kiria DLC", "1.2.0.3")]
public class KiriaDLCPlugin : BaseUnityPlugin
{
    public static readonly int DEBUG_LEVEL = 2;
    
    public static readonly int NEFIA_LV = 42;
    public static readonly ZoneScaleType SCALE_TYPE = ZoneScaleType.Quest;
    
    private void Awake()
    {
        Assembly executingAssembly = Assembly.GetExecutingAssembly();
        CommandRegistry.assemblies.Add(executingAssembly);
    }
    
    public static void LogWarning(String loc, String msg)
    {
        if (DEBUG_LEVEL > 0)
        {
            Debug.LogWarning($"KiriaDLC::{loc}::{msg}");
        }
    }
    // public static void PrintFields<T>(T baseItem) where T : new() {
    //     System.Reflection.FieldInfo[] fields = baseItem.GetType().GetFields();
    //     String baseText = baseItem.ToString();
    //     foreach (System.Reflection.FieldInfo fieldInfo in fields) {
    //         KiriaDLCPlugin.LogWarning(baseText, fieldInfo.Name + " : " + baseItem.GetField<object>(fieldInfo.Name));
    //     }
    // }
    private void Start()
    {
        Debug.Log("KIRIADLC: Mod Start()");
        Harmony harmony = new Harmony("net.ryozu.kiriadlc");
        harmony.PatchAll();
    }
}


[HarmonyPatch(typeof(Chara))]
[HarmonyPatch(nameof(Chara.GetTopicText))]
class CharaTextPatch : Chara
{   //We want to change Kiria's "CharaText" if the player has used the gene on her.  Since CharaText isn't an instance
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
        // string key = this.source.idText.IsEmpty(this.id);
        // ^^^^^^^^^^^^^^^^^
        if (key == "adv_kiria" && target.c_genes?.items.Find(gene => gene.id == "android_kiria") != null)
        {
            key = "adv_kiria2";
        }
        // vvvvvvvvvvvvvvvvv
        // if (this.id == "littleOne" && EClass._zone is Zone_LittleGarden)
        return key;
    }
}

[HarmonyPatch(typeof(DNA))]
[HarmonyPatch(nameof(DNA.Apply), [typeof(Chara), typeof(bool)])]
class DNAApplyPatch : DNA
{
    static void Postfix(DNA __instance, Chara c, bool reverse)
    {
        //If the gene is being applied, let's add some spice
        if (__instance.id == "android_kiria" && !reverse)
        {
            KiriaDLCPlugin.LogWarning("DNAPatch", "Applying 'android_kiria' gene");
            if (c.id == "adv_kiria")
            {
                //This is a Kiria, she appreciates it
                c.ShowDialog("kiriaDLC", "used_gene_kiria");
                c.affinity.Mod(50);
            }
            else
            {
                //Used on something beside a Kiria, she does not approve
                Chara kiria = EClass.game.cards.globalCharas.Find("adv_kiria");
                kiria.ShowDialog("kiriaDLC", "used_gene_other");
                kiria.hostility = Hostility.Enemy;
                kiria.DoHostileAction((Card) EMono.pc, true);
                kiria.enemy = pc;
                kiria.calmCheckTurn = 255;
            }
        }
    }
}


[HarmonyPatch(typeof(DramaOutcome))]
[HarmonyPatch(nameof(DramaOutcome.upgrade_miscreation))]
class DramaOutcomePatch : DramaOutcome
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // Check if miscreation is being fixed on a Kiria, and give quest if it's not been done before
        return new CodeMatcher(instructions).MatchEndForward(
                new CodeMatch(OpCodes.Stloc_0)).Advance(1) //FInd the first "c1 =" and move past it
            .InsertAndAdvance(  //Insert the following function call
                new CodeInstruction(OpCodes.Ldloc_0), //c1 is the first argument
                Transpilers.EmitDelegate(CheckKiria)          //Insert invocation of the delegate
            ).InstructionEnumeration();
    }

    private static void CheckKiria(Chara c1)
    {
        // Chara c1 = EMono.pc.party.members.Find((Predicate<Chara>) (c => !c.IsPC && c.HasElement(1248)));
        // ^^^^^^^^^^^^^^^
        if (c1.id != "adv_kiria") return;
        if (!EClass.game.quests.IsCompleted("kiria_quest") 
            && !EClass.game.quests.IsStarted<QuestKiria>() 
            && EClass.game.quests.globalList.All(x => x.id != "kiria_quest"))
        {
            EClass.game.quests.globalList.Add(Quest.Create("kiria_quest").SetClient(c1, false));
            KiriaDLCPlugin.LogWarning("Zone.Activate Postfix","KiriaDLC:: Adding quest to global list");
        }
        // vvvvvvvvvvvvvvvv
        // int num = c1.Evalue(1248);
    }
}

[HarmonyPatch(typeof(Chara))]
[HarmonyPatch(nameof(Chara.Pick))]
class PickupPatch : Chara
{
    //Most item quests are delivery type quests, so just picking up the item doesn't advance
    static void Postfix(Chara __instance, Thing t)
    {
        KiriaDLCPlugin.LogWarning("Chara", "Pick: " + t.id + " with uid of " + t.uid);
        //At least for this mod, we only care about the letters
        if (t.id != "letter")
        {
            KiriaDLCPlugin.LogWarning("\t","Item was not of interest: " + t.ToString());
            return;
        }

        //Only the player or their pawns matter for picking up items here
        if (__instance.IsPC || __instance.IsPCFaction)
        {
            QuestKiria quest = EClass.game.quests.Get<QuestKiria>();
            //If they even have the quest
            if (quest is null)
            {
                KiriaDLCPlugin.LogWarning("\t","Quest was null: ");
                return;
            }
            //Let the quest know the player picked up the item.
            KiriaDLCPlugin.LogWarning("\t","Quest was informed: " + t.ToString());
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
                    (map.trait as TraitKiriaMap)?.SpawnNefia(); 
                }

                while (map != null)
                {
                    map.Destroy();
                    map = GetNefiaMap(__instance);
                }

                EClass.player.willAutoSave = true;
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
