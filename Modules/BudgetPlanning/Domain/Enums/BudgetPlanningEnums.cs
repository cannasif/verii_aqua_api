namespace aqua_api.Modules.BudgetPlanning.Domain.Enums;

public enum BudgetPlanStatus : byte
{
    Draft = 0,
    LiveImported = 1,
    Adjusted = 2,
    GrowthCalculated = 3,
    SalesPlanned = 4,
    Calculated = 5,
    Approved = 6,
    Archived = 7
}

public enum BudgetPlanSourceType : byte
{
    Actual = 0,
    Virtual = 1
}

public enum BudgetPlanFishBatchAdjustmentType : byte
{
    Increase = 0,
    Decrease = 1,
    Transfer = 2,
    Correction = 3
}

public enum BudgetFishPriceType : byte
{
    Purchase = 0,
    Sales = 1
}

public enum BudgetMarketType : byte
{
    Domestic = 0,
    Foreign = 1
}
