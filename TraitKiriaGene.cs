using UnityEngine;

public class TraitKiriaGene : TraitGene
{
    
    public override void OnCreate(int lv)
    {
        Debug.LogWarning("KiriaDLC:: Gene On Create invoked");
        DNA dna = new DNA();
        dna.id = "android";
        dna.type = DNA.Type.Superior;
        dna.cost = 8;
        dna.lv = 35;
        dna.seed = 1;
        dna.vals = [1410, 1, 1652, 1];
        dna.CalcCost();
        //dna.vals = this.owner.elements.list;
        this.owner.c_DNA = dna;
        owner.ChangeMaterial(dna.GetMaterialId(dna.type));
    }
    
}