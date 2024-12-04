
using Mod_KiriaDLC;

public class TraitKiriaItemTool : TraitItem
{
    private int cycle = 0;
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
            Chara kiria = EClass.game.cards.globalCharas.Find("adv_kiria");
            if (cycle == -1)
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
            if (cycle == 0)
            {
                kiria.ShowDialog("kiriaDLC", "test_dialog");
            }

            if (cycle == 1)
            {
                kiria.ShowDialog("kiriaDLC", "test_dialog2");
            }

            ++cycle;
            cycle %= 2;
        }
        return true;
    }

}