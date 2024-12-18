using ReflexCLI.Attributes;

[ConsoleCommandClassCustomizer("")]
public class KIriaConsole
{
    
    [ConsoleCommand("ResetKiria")]
    public static string ResetKiria()
    {
        Mod_KiriaDLC.QuestKiria quest = EClass.game.quests.Get<Mod_KiriaDLC.QuestKiria>();
        if (quest != null) quest.phase = 1;
        Quest mapQuest = EClass.game.quests.GetGlobal("kiria_map_replace");
        if (mapQuest is null)
        {
            EClass.game.quests.globalList.Add(Quest.Create("kiria_map_replace").SetClient(quest?.person.chara, false));
        }
        return "Reset Kiria Quest to first stage";
    }
}