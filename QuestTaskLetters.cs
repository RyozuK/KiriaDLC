using Newtonsoft.Json;

public class QuestTaskLetters : QuestTask
{
    [JsonProperty] public int HasLetter1;
    [JsonProperty] public int HasLetter2;
    [JsonProperty] public int HasLetter3;
    [JsonProperty] public int HasLetter4;
    [JsonProperty] public int HasLetter5;
    [JsonProperty] public int HasLetter6;
    [JsonProperty] public int HasLetter7;
    
    public override bool IsComplete() => NumLetters >= 7;

    private int NumLetters => HasLetter1 + HasLetter2 + HasLetter3 + HasLetter4 +
                              HasLetter5 + HasLetter6 + HasLetter7;

    public void OnPickThing(Thing thing)
    {
        if (thing.id == "letter" && thing.GetStr(53).StartsWith("kiria_dlc_"))
        {
            UpdateLetters(thing.GetStr(53));
        }
    }

    private void UpdateLetters(string letterID)
    {
        switch (letterID)
        {
            case "kiria_dlc_1": HasLetter1 = 1; break;
            case "kiria_dlc_2": HasLetter2 = 1; break;
            case "kiria_dlc_3": HasLetter3 = 1; break;
            case "kiria_dlc_4": HasLetter4 = 1; break;
            case "kiria_dlc_5": HasLetter5 = 1; break;
            case "kiria_dlc_6": HasLetter6 = 1; break;
            case "kiria_dlc_7": HasLetter7 = 1; break;
        }
    }
    
    public override void OnGetDetail(ref string detail, bool onJournal)
    {
        detail += $"\n\n{NumLetters}/7";
    }
    
}