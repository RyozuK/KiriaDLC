using System;
using Mod_KiriaDLC;
using ReflexCLI.Attributes;

[ConsoleCommandClassCustomizer("")]
public class KiriaConsole
{
    
    [ConsoleCommand("ResetKiria")]
    public static string ResetKiria()
    {
        try
        {
            Quest quest = EClass.game.quests.Get<QuestKiria>();
            if (quest is not null) quest.phase = 1;
            Quest mapQuest = EClass.game.quests.GetGlobal("kiria_map_replace");
            if (mapQuest is null)
            {
                EClass.game.quests.globalList.Add(Quest.Create("kiria_map_replace")
                    .SetClient(quest?.person.chara, false));
            }

            return "Reset Kiria Quest to first stage";
        }
        catch (Exception e)
        {
            return "Failed to reset, please make sure Custom Whatever Loader is updated: " + e.ToString();
        }
    }

    [ConsoleCommand("KiriaTest")]
    public static string KiriaTest()
    {
        //Step 1: Clean up the quests
        // 1.a: Get rid of completion flags
        // 1.b: Get rid of active quests (GlobalList)
        try
        {
            EClass.game.quests.completedTypes.Remove("Mod_KiriaDLC.QuestKiria");
            EClass.game.quests.completedIDs.Remove("kiria_map_quest");
            //The Map replacement quest shouldn't ever be completed, but just in case.
            EClass.game.quests.completedTypes.Remove("Mod_KiriaDLC.QuestMapReplace");
            EClass.game.quests.completedIDs.Remove("kiria_map_replace");

            Quest q1 = EClass.game.quests.GetGlobal("kiria_map_replace");
            if (q1 is not null)
            {
                EClass.game.quests.globalList.Remove(q1);
            }
            Quest q2 = EClass.game.quests.GetGlobal("kiria_map_quest");
            if (q2 is not null)
            {
                EClass.game.quests.globalList.Remove(q2);
            }
        }
        catch (Exception e)
        {
            KiriaDLCPlugin.LogWarning("Console", "Failed in removing quests somehow" + e);
        }
        //Step 2: Make sure the Kiria in the party has miscreation
        Chara c1 = EMono.pc.party.members.Find((Predicate<Chara>) (c => c.id == "adv_kiria"));
        if (c1 == null) return "No Kiria found in party";
        c1.SetFeat(1248, 2);
        //Step 3: Give the player a memory chip
        EClass.pc.Pick(ThingGen.Create("memory_chip"));
        return "Test setup process completed";
    }

    [ConsoleCommand("CleanKiria")]
    public static string CleanKiria(bool full = false)
    {
        //For doing a full cleanup, such as uninstalling the mod
        //In theory, this shouldn't be needed since mod object cleanup was added
        string extra_info = "";
        try
        {
            Quest q1 = EClass.game.quests.GetGlobal("kiria_map_replace");
            if (q1 is not null) {EClass.game.quests.globalList.Remove(q1);}
            Quest q2 = EClass.game.quests.GetGlobal("kiria_map_quest");
            if (q2 is not null) {EClass.game.quests.globalList.Remove(q2);}
            Quest q3 = EClass.game.quests.Get<QuestKiria>();
            if (q3 is not null) { EClass.game.quests.Remove(q3); }

            if (full)
            {
                extra_info = "Removed quest completion markers as well.";
                EClass.game.quests.completedTypes.Remove("Mod_KiriaDLC.QuestKiria");
                EClass.game.quests.completedIDs.Remove("kiria_map_quest");
            }
        }
        catch (Exception e)
        {
            return "Failed to clean up quests: " + e.ToString();
        }

        return "Quests removed.  If uninstalling, immediately save and disable mod to avoid problems. " + extra_info;
    }
}