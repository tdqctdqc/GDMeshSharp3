
using System.Collections.Generic;

public class SetFrontsModule : LogicModule
{
    private Context _context;
    public SetFrontsModule(Context context)
    {
        _context = context;
    }
    public override LogicResults Calculate(List<RegimeTurnOrders> orders, 
        Data data)
    {
        var res = new LogicResults();
        _context.CalculateWaypointsAndForceBalances(data);
        var trim = new TrimFrontsProcedure();
        res.Messages.Add(trim);
        return res;
    }
    
    
    private void TrimFronts(Alliance alliance, Data data)
    {
        var members = alliance.Members.Items(data);
        foreach (var member in members)
        {
            
        }
    }
}