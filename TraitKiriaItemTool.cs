
public class TraitKiriaItemTool : TraitItem
{
    public override bool OnUse(Chara c)
    {
        Thing letter = ThingGen.Create("letter");
        letter.SetStr(53, "kiria_dlc_1");
        EClass.pc.Pick(letter);

        Thing book = ThingGen.Create("book");
        book.SetStr(53, "kiria_dlc_2");
        EClass.pc.Pick(book);
        
        return true;
    }
}