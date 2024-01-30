
using System.Collections.Generic;
using System.Linq;

public abstract class DeploymentLeaf : IDeploymentNode
{
    public int ParentId { get; }
    public int Id { get; }
    public ERef<Regime> Regime { get; private set; }
    public UnitGroupManager Groups { get; }
    
    protected DeploymentLeaf(int parentId, int id, ERef<Regime> regime, UnitGroupManager groups)
    {
        ParentId = parentId;
        Id = id;
        Regime = regime;
        Groups = groups;
    }
    
    public abstract void ClearGroupFromData(UnitGroup g, LogicWriteKey key);
    public abstract void AddGroupToData(UnitGroup g, LogicWriteKey key);
    public abstract float GetPowerPointNeed(Data d);
    public DeploymentBranch Parent(Data d)
    {
        return (DeploymentBranch)d.HostLogicData.RegimeAis[Regime.Entity(d)]
            .Military.Deployment.GetNode(ParentId);
    }
    IEnumerable<IDeploymentNode> IDeploymentNode.Children() => Enumerable.Empty<IDeploymentNode>();
    public float GetPowerPointsAssigned(Data data)
    {
        return Groups.Get(data).Sum(g => g.GetPowerPoints(data));
    }

    public void Disband(LogicWriteKey key)
    {
        
    }
    
    public abstract void AdjustWithin(LogicWriteKey key);
    public abstract void GiveOrders(LogicWriteKey key);
    public abstract UnitGroup GetPossibleTransferGroup(LogicWriteKey key);
    public abstract PolyCell GetCharacteristicCell(Data d);
}