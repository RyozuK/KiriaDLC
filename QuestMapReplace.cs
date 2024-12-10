namespace Mod_KiriaDLC;

public class QuestMapReplace : QuestDialog
{
    public override bool RequireClientInSameZone => false;

    // We don't want OnStart here actually
     public override void OnClickQuest()
     {
         //First, check if they have the map, if so, dialog that they don't need it
         if (EClass.pc.things.Find(thing => thing.id == "map_kiria") != null)
         {
             this.person.chara.ShowDialog("kiriaDLC", "already_has_map");
         }
         //Otherwise, look up the main quest, get the map from there, and give it to the player
         else
         {
             // Quest main = EClass.game.quests.GetGlobal("kiria_map_quest");
             Quest main = EClass.game.quests.Get<QuestKiria>();
             KiriaDLCPlugin.LogWarning("MapReplace", "Found quest: |" + main?.id + "|");
             this.person.chara.ShowDialog("kiriaDLC", "give_new_map");
             EClass.pc.Pick((main as QuestKiria).MapItem);
         }
     }
}