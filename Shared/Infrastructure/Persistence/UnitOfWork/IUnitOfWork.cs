using aqua_api.Shared.Infrastructure.Persistence.Data;

namespace aqua_api.Shared.Infrastructure.Persistence.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        AquaDbContext Db { get; }

        IGenericRepository<User> Users { get; }
        IGenericRepository<UserAuthority> UserAuthorities { get; }
        IGenericRepository<UserDetail> UserDetails { get; }
        IGenericRepository<UserSession> UserSessions { get; }

        IGenericRepository<Stock> Stocks { get; }
        IGenericRepository<StockDetail> StockDetails { get; }
        IGenericRepository<StockImage> StockImages { get; }
        IGenericRepository<StockRelation> StockRelations { get; }

        IGenericRepository<SmtpSetting> SmtpSettings { get; }
        IGenericRepository<AquaSetting> AquaSettings { get; }
        IGenericRepository<PasswordResetRequest> PasswordResetRequests { get; }

        IGenericRepository<PermissionDefinition> PermissionDefinitions { get; }
        IGenericRepository<PermissionGroup> PermissionGroups { get; }
        IGenericRepository<PermissionGroupPermission> PermissionGroupPermissions { get; }
        IGenericRepository<UserPermissionGroup> UserPermissionGroups { get; }

        IGenericRepository<Project> Projects { get; }
        IGenericRepository<OpeningImportJob> OpeningImportJobs { get; }
        IGenericRepository<OpeningImportRow> OpeningImportRows { get; }
        IGenericRepository<Cage> Cages { get; }
        IGenericRepository<ProjectCage> ProjectCages { get; }
        IGenericRepository<FishBatch> FishBatches { get; }
        IGenericRepository<BatchCageBalance> BatchCageBalances { get; }
        IGenericRepository<BatchWarehouseBalance> BatchWarehouseBalances { get; }
        IGenericRepository<GoodsReceipt> GoodsReceipts { get; }
        IGenericRepository<GoodsReceiptLine> GoodsReceiptLines { get; }
        IGenericRepository<GoodsReceiptFishDistribution> GoodsReceiptFishDistributions { get; }
        IGenericRepository<Feeding> Feedings { get; }
        IGenericRepository<FeedingLine> FeedingLines { get; }
        IGenericRepository<FeedingDistribution> FeedingDistributions { get; }
        IGenericRepository<Mortality> Mortalities { get; }
        IGenericRepository<MortalityLine> MortalityLines { get; }
        IGenericRepository<Transfer> Transfers { get; }
        IGenericRepository<TransferLine> TransferLines { get; }
        IGenericRepository<WarehouseTransfer> WarehouseTransfers { get; }
        IGenericRepository<WarehouseTransferLine> WarehouseTransferLines { get; }
        IGenericRepository<CageWarehouseTransfer> CageWarehouseTransfers { get; }
        IGenericRepository<CageWarehouseTransferLine> CageWarehouseTransferLines { get; }
        IGenericRepository<WarehouseCageTransfer> WarehouseCageTransfers { get; }
        IGenericRepository<WarehouseCageTransferLine> WarehouseCageTransferLines { get; }
        IGenericRepository<Shipment> Shipments { get; }
        IGenericRepository<ShipmentLine> ShipmentLines { get; }
        IGenericRepository<Weighing> Weighings { get; }
        IGenericRepository<WeighingLine> WeighingLines { get; }
        IGenericRepository<StockConvert> StockConverts { get; }
        IGenericRepository<StockConvertLine> StockConvertLines { get; }
        IGenericRepository<BatchMovement> BatchMovements { get; }
        IGenericRepository<WeatherSeverity> WeatherSeverities { get; }
        IGenericRepository<WeatherType> WeatherTypes { get; }
        IGenericRepository<DailyWeather> DailyWeathers { get; }
        IGenericRepository<FishHealthEvent> FishHealthEvents { get; }
        IGenericRepository<FishTreatment> FishTreatments { get; }
        IGenericRepository<FishLabSample> FishLabSamples { get; }
        IGenericRepository<FishLabResult> FishLabResults { get; }
        IGenericRepository<WelfareAssessment> WelfareAssessments { get; }
        IGenericRepository<ComplianceAudit> ComplianceAudits { get; }
        IGenericRepository<ComplianceCorrectiveAction> ComplianceCorrectiveActions { get; }
        IGenericRepository<ProjectCageDailyKpiSnapshot> ProjectCageDailyKpiSnapshots { get; }
        IGenericRepository<NetOperationType> NetOperationTypes { get; }
        IGenericRepository<NetOperation> NetOperations { get; }
        IGenericRepository<NetOperationLine> NetOperationLines { get; }

        Task<int> SaveChanges();
        Task BeginTransaction();
        Task Commit();
        Task Rollback();

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        IGenericRepository<T> Repository<T>() where T : BaseEntity;
    }
}
