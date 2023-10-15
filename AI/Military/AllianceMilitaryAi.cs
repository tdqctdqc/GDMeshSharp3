
using System.Linq;

public class AllianceMilitaryAi
{
    private Alliance _alliance;
    
    public AllianceMilitaryAi(Alliance alliance)
    {
        _alliance = alliance;
    }
    public void Calculate(Data data, AllianceTurnOrders orders)
    {
        var allianceWaypoints = _alliance
            .Members.Items(data)
            .SelectMany(r => r.GetPolys(data))
            .SelectMany(p => p.GetAssocWaypoints(data));
        
    }
}