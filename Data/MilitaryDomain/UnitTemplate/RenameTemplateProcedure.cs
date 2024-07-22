
using System;

public class RenameTemplateProcedure : Procedure
{
    public ERef<UnitTemplate> Template { get; private set; }
    public string NewName { get; private set; }
    
    public RenameTemplateProcedure(ERef<UnitTemplate> template, string newName) 
    {
        Template = template;
        NewName = newName;
    }


    public override void Enact(ProcedureWriteKey key)
    {
        Template.Get(key.Data).Rename(NewName, key);
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }

}