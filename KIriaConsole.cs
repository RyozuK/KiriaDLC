using ReflexCLI.Attributes;

[ConsoleCommandClassCustomizer("")]
public class KIriaConsole
{
    
    [ConsoleCommand("ResetKiria")]
    public static string ResetKiria()
    {
        Mod_KiriaDLC.QuestKiria quest = EClass.game.quests.Get<Mod_KiriaDLC.QuestKiria>();
        if (quest != null) quest.phase = 1;
        return "Reset Kiria Quest to first stage";
    }
}