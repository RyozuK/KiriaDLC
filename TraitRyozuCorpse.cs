using Mod_KiriaDLC;

public class TraitRyozuCorpse : TraitItem
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
        Thing gene = ThingGen.Create("gene");

        gene.c_DNA = new DNA
        {
            id = "android_kiria",
            type = DNA.Type.Superior,
            cost = 27,
            lv = 35,
            seed = 1,
            vals = [1410, 1, 1652, 1],
            slot = 0
        };
        gene.c_DNA.CalcCost();
        gene.ChangeMaterial(gene.c_DNA.GetMaterialId(gene.c_DNA.type));

        //Replace the usable corpse with a real/normal corpse
        EClass._map.SetObj(this.owner.pos.x, this.owner.pos.z, 82,1,this.owner.dir);
        EClass.player.DropReward(gene);
        this.owner.Destroy();
        return true;
    }
}
