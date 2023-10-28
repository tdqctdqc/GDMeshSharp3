
public class TurnEndState : TurnState
{
    public TurnEndState(Data data) : base(data)
    {
        _majorModules = new LogicModule[]
        {
            new DefaultLogicModule(() => new PrepareNewHistoriesProcedure()),
            new ProduceConstructModule(),
            new ConstructBuildingsModule(),
            new FoodAndPopGrowthModule(),
            new FinanceModule(),
            new TradeModule(),
            new ProposalsModule(),
            new FormUnitsAndGroupsModule(),
            new AllianceOrdersModule()
        };
        _minorModules = new LogicModule[]
        {
            new HandleUnitOrdersModule()
        };
    }
}