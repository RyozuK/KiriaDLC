
using Mod_KiriaDLC;
using UnityEngine;

public class TraitKiriaItemTool : TraitItem
{
    public static void addNote(string note_id)
    {
        Thing note = ThingGen.Create("letter");
        note.SetStr(53, note_id);
        EClass.pc.Pick(note);
    }
    
    public override bool OnUse(Chara c)
    {
        addNote("kiria_dlc_1");
        addNote("kiria_dlc_2");
        addNote("kiria_dlc_3");
        addNote("kiria_dlc_4");
        addNote("kiria_dlc_5");
        addNote("kiria_dlc_6");
        addNote("kiria_dlc_7");

        Thing gene = ThingGen.Create("gene_kiria");
        EClass.pc.Pick(gene);
        
        //Thing remains = (Thing)EClass._zone.AddThing("ryo_corpse", EClass.pc.pos);
        // var remains = ThingGen.Create("ryo_corpse");
        // var remains2 = EClass._zone.AddCard(remains, c.pos).Install();
        
        EClass._map.SetObj(c.pos.x, c.pos.z, KiriaEntries.CorpseID);
        
        
        
        
        // remains.Install();
        // remains.SetPlaceState(PlaceState.installed, false);
        // EClass._zone.AddCard(remains, c.pos);
        
        //1410	featReboot	リブート	Reboot				0	0	100	1			0			Feat	FEAT	feat			class		50	50	1	1000	0	7	10	5	5				0							専門の職業に従事し、長い年月を経て獲得した特性。	A trait acquired over many years of working in a specialized profession.	あなたは自身を再起動できる。	You can reboot yourself.	死亡時に再起動	Reboot when dead
        
        //gene.rarity = Rarity.Artifact;
        //gene.c_genes = new CharaGenes()
        //remains.AddThing(gene);

        
        
        
        
        
        return true;
    }

}