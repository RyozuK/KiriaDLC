using Mod_KiriaDLC;

public class TraitKiriaCorpse : TraitItem
{
    public override bool HoldAsDefaultInteraction => true;

    public override bool CanUse(Chara c)
    {
        return this.owner.IsInstalled;
    }

    public override bool OnUse(Chara c)
    {
        if (c != EClass.pc) return false;
        KiriaDLCPlugin.LogWarning("TraitCorpse","Used Corpse");
        //Deprecating old gene so that the gene won't vanish
        //Thing gene = ThingGen.Create("gene_kiria");

        DNA dna = new DNA();
        dna.id = "android_kiria";
        dna.type = DNA.Type.Superior;
        dna.cost = KiriaDLCPlugin.DEBUG_MODE ? 0 : 27;
        dna.lv = 35;
        dna.seed = 1;
        dna.vals = [1410, 1, 1652, 1];
        dna.slot = 0;
        dna.CalcCost();

        Thing gene = ThingGen.Create("gene");
        gene.c_DNA = dna;
        gene.ChangeMaterial(dna.GetMaterialId(dna.type));

        
        EClass._map.SetObj(this.owner.pos.x, this.owner.pos.z, 82,1,this.owner.dir);
        EClass.pc.Pick(gene);
        this.owner.Destroy();
        return true;
    }
}