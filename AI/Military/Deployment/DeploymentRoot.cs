
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class DeploymentRoot : CompoundDeploymentBranch
{
    public static DeploymentRoot Construct(Regime r, Data d)
    {
        return new DeploymentRoot(
            new HashSet<DeploymentBranch>(),
            r.MakeRef(),
            d.HostLogicData.DeploymentTreeIds.TakeId(d));
    }
    [SerializationConstructor] private DeploymentRoot(
        HashSet<DeploymentBranch> assignments,
        ERef<Regime> regime, 
        int id) 
        : base(assignments, regime, id)
    {
    }

    public void MakeTheaters(LogicWriteKey key)
    {
        var theaters = Assignments.OfType<Theater>().ToArray();
        var newTheaters = Blobber.Blob(
            theaters, Regime.Entity(key.Data), key);
        foreach (var theater in theaters)
        {
            theater.DissolveInto(theaters, key);
            theater.Disband(key);
        }
        foreach (var theater in newTheaters)
        {
            theater.SetParent(this, key);
            theater.MakeFronts(key);
        }
    }
    
    
    public override float GetPowerPointNeed(Data d)
    {
        return 0f;
    }

    public override PolyCell GetCharacteristicCell(Data d)
    {
        throw new Exception();
    }

    public override UnitGroup GetPossibleTransferGroup(LogicWriteKey key)
    {
        return null;
    }

    public override IEnumerable<IDeploymentNode> Children()
    {
        return Assignments;
    }

    public override void DissolveInto(IEnumerable<DeploymentBranch> into, LogicWriteKey key)
    {
        throw new Exception();
    }
}