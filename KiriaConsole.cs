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

    [ConsoleCommand("CleanKiria")]
    public static string CleanKiria(bool full = false)
    {
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
                EClass.game.quests.completedTypes.Remove("QuestKiria");
                EClass.game.quests.completedIDs.Remove("QuestKiria");
            }
        }
        catch (Exception e)
        {
            return "Failed to clean up quests: " + e.ToString();
        }

        return "Quests removed, immediately save and disable mod to avoid problems.";
    }
}