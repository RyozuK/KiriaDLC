using System;
using Mod_KiriaDLC;
using ReflexCLI.Attributes;

[ConsoleCommandClassCustomizer("")]
public class KIriaConsole
{
    
    [ConsoleCommand("ResetKiria")]
    public static string ResetKiria()
    {
        try
        {
            Mod_KiriaDLC.QuestKiria quest = EClass.game.quests.Get<Mod_KiriaDLC.QuestKiria>();
            if (quest != null) quest.phase = 1;
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
    public static string CleanKiria()
    {
        try
        {
            EClass.game.quests.globalList.Remove(EClass.game.quests.GetGlobal("kiria_map_replace"));
            EClass.game.quests.globalList.Remove(EClass.game.quests.GetGlobal("kiria_map_quest"));
            Quest q = EClass.game.quests.Get<QuestKiria>();
            if (q is not null)
            {
                EClass.game.quests.Remove(q);
            }
        }
        catch (Exception e)
        {
            return "Failed to clean up quests: " + e.ToString();
        }

        return "Quests removed, immediately save and disable mod to avoid problems.";
    }
}