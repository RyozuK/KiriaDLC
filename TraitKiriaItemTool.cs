
using Mod_KiriaDLC;

public class TraitKiriaItemTool : TraitItem
{
    private static void AddNote(string note_id)
    {
        Thing note = ThingGen.Create("letter");
        note.SetStr(53, note_id);
        EClass.pc.Pick(note);
    }
    
    public override bool OnUse(Chara c)
    {
        if (KiriaDLCPlugin.DEBUG_MODE)
        {
            AddNote("kiria_dlc_1");
            AddNote("kiria_dlc_2");
            AddNote("kiria_dlc_3");
            AddNote("kiria_dlc_4");
            AddNote("kiria_dlc_5");
            AddNote("kiria_dlc_6");
            AddNote("kiria_dlc_7");

            Thing gene = ThingGen.Create("gene_kiria");
            EClass.pc.Pick(gene);

            EClass._map.SetObj(c.pos.x, c.pos.z, KiriaEntries.CorpseID);
        }
        return true;
    }

}