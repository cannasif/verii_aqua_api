namespace aqua_api.Modules.Aqua.Domain.Enums
{
    public enum DocumentStatus : byte
    {
        Draft = 0,
        Posted = 1,
        Cancelled = 2
    }

    public enum GoodsReceiptItemType : byte
    {
        Feed = 0,
        Fish = 1
    }

    public enum BatchMovementType : byte
    {
        Transfer = 0,
        Mortality = 1,
        Weighing = 2,
        StockConvert = 3,
        Adjustment = 4,
        Stocking = 5,
        Shipment = 6,
        Feeding = 7,
        WarehouseTransfer = 8
    }

    public enum FeedingSlot : byte
    {
        Morning = 0,
        Evening = 1
    }

    public enum FeedingSourceType : byte
    {
        Manual = 0,
        Planned = 1,
        Auto = 2
    }

    public enum ProjectMergeSourceState : byte
    {
        Passive = 0,
        Archived = 1
    }
}
