using System;
// using Mod_KiriaDLC;
using ReflexCLI.Attributes;

[ConsoleCommandClassCustomizer("")]
public class KiriaConsole
{
    [ConsoleCommand("KiriaTest")]
    public static string KiriaTest()
    {
        //Step 1: Clean up the quests
        try
        {
            // 1.a: Get rid of completion flags
            EClass.game.quests.completedTypes.Remove("QuestKiria");
            EClass.game.quests.completedIDs.Remove("kiria_quest");
            // 1.b: Get rid of active quests (GlobalList)
            //The Map replacement quest shouldn't ever be completed, but just in case.

            Quest quest = EClass.game.quests.GetGlobal("kiria_quest");
            if (quest is not null)
            {
                EClass.game.quests.globalList.Remove(quest);
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
}