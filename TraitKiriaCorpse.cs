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
        Thing gene = ThingGen.Create("gene_kiria");
        EClass._map.SetObj(this.owner.pos.x, this.owner.pos.z, 82,1,this.owner.dir);
        EClass.pc.Pick(gene);
        this.owner.Destroy();
        return true;
    }
}