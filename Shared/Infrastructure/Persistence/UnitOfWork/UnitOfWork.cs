using aqua_api.Shared.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Concurrent;

namespace aqua_api.Shared.Infrastructure.Persistence.UnitOfWork
{
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly AquaDbContext _context;
        private readonly ConcurrentDictionary<Type, object> _repositories;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IDbContextTransaction? _transaction;
        private bool _disposed;

        private IGenericRepository<User>? _users;
        private IGenericRepository<UserAuthority>? _userAuthorities;
        private IGenericRepository<UserDetail>? _userDetails;
        private IGenericRepository<UserSession>? _userSessions;

        private IGenericRepository<Stock>? _stocks;
        private IGenericRepository<StockDetail>? _stockDetails;
        private IGenericRepository<StockImage>? _stockImages;
        private IGenericRepository<StockRelation>? _stockRelations;

        private IGenericRepository<SmtpSetting>? _smtpSettings;
        private IGenericRepository<AquaSetting>? _aquaSettings;
        private IGenericRepository<PasswordResetRequest>? _passwordResetRequests;

        private IGenericRepository<PermissionDefinition>? _permissionDefinitions;
        private IGenericRepository<PermissionGroup>? _permissionGroups;
        private IGenericRepository<PermissionGroupPermission>? _permissionGroupPermissions;
        private IGenericRepository<UserPermissionGroup>? _userPermissionGroups;

        private IGenericRepository<Project>? _projects;
        private IGenericRepository<Cage>? _cages;
        private IGenericRepository<ProjectCage>? _projectCages;
        private IGenericRepository<FishBatch>? _fishBatches;
        private IGenericRepository<BatchCageBalance>? _batchCageBalances;
        private IGenericRepository<BatchWarehouseBalance>? _batchWarehouseBalances;
        private IGenericRepository<GoodsReceipt>? _goodsReceipts;
        private IGenericRepository<GoodsReceiptLine>? _goodsReceiptLines;
        private IGenericRepository<GoodsReceiptFishDistribution>? _goodsReceiptFishDistributions;
        private IGenericRepository<Feeding>? _feedings;
        private IGenericRepository<FeedingLine>? _feedingLines;
        private IGenericRepository<FeedingDistribution>? _feedingDistributions;
        private IGenericRepository<Mortality>? _mortalities;
        private IGenericRepository<MortalityLine>? _mortalityLines;
        private IGenericRepository<Transfer>? _transfers;
        private IGenericRepository<TransferLine>? _transferLines;
        private IGenericRepository<WarehouseTransfer>? _warehouseTransfers;
        private IGenericRepository<WarehouseTransferLine>? _warehouseTransferLines;
        private IGenericRepository<CageWarehouseTransfer>? _cageWarehouseTransfers;
        private IGenericRepository<CageWarehouseTransferLine>? _cageWarehouseTransferLines;
        private IGenericRepository<WarehouseCageTransfer>? _warehouseCageTransfers;
        private IGenericRepository<WarehouseCageTransferLine>? _warehouseCageTransferLines;
        private IGenericRepository<Shipment>? _shipments;
        private IGenericRepository<ShipmentLine>? _shipmentLines;
        private IGenericRepository<Weighing>? _weighings;
        private IGenericRepository<WeighingLine>? _weighingLines;
        private IGenericRepository<StockConvert>? _stockConverts;
        private IGenericRepository<StockConvertLine>? _stockConvertLines;
        private IGenericRepository<BatchMovement>? _batchMovements;
        private IGenericRepository<WeatherSeverity>? _weatherSeverities;
        private IGenericRepository<WeatherType>? _weatherTypes;
        private IGenericRepository<DailyWeather>? _dailyWeathers;
        private IGenericRepository<FishHealthEvent>? _fishHealthEvents;
        private IGenericRepository<FishTreatment>? _fishTreatments;
        private IGenericRepository<FishLabSample>? _fishLabSamples;
        private IGenericRepository<FishLabResult>? _fishLabResults;
        private IGenericRepository<WelfareAssessment>? _welfareAssessments;
        private IGenericRepository<ComplianceAudit>? _complianceAudits;
        private IGenericRepository<ComplianceCorrectiveAction>? _complianceCorrectiveActions;
        private IGenericRepository<ProjectCageDailyKpiSnapshot>? _projectCageDailyKpiSnapshots;
        private IGenericRepository<NetOperationType>? _netOperationTypes;
        private IGenericRepository<NetOperation>? _netOperations;
        private IGenericRepository<NetOperationLine>? _netOperationLines;

        public EfUnitOfWork(AquaDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _repositories = new ConcurrentDictionary<Type, object>();
        }

        public AquaDbContext Db => _context;

        public IGenericRepository<User> Users => _users ??= new GenericRepository<User>(_context, _httpContextAccessor);
        public IGenericRepository<UserAuthority> UserAuthorities => _userAuthorities ??= new GenericRepository<UserAuthority>(_context, _httpContextAccessor);
        public IGenericRepository<UserDetail> UserDetails => _userDetails ??= new GenericRepository<UserDetail>(_context, _httpContextAccessor);
        public IGenericRepository<UserSession> UserSessions => _userSessions ??= new GenericRepository<UserSession>(_context, _httpContextAccessor);

        public IGenericRepository<Stock> Stocks => _stocks ??= new GenericRepository<Stock>(_context, _httpContextAccessor);
        public IGenericRepository<StockDetail> StockDetails => _stockDetails ??= new GenericRepository<StockDetail>(_context, _httpContextAccessor);
        public IGenericRepository<StockImage> StockImages => _stockImages ??= new GenericRepository<StockImage>(_context, _httpContextAccessor);
        public IGenericRepository<StockRelation> StockRelations => _stockRelations ??= new GenericRepository<StockRelation>(_context, _httpContextAccessor);

        public IGenericRepository<SmtpSetting> SmtpSettings => _smtpSettings ??= new GenericRepository<SmtpSetting>(_context, _httpContextAccessor);
        public IGenericRepository<AquaSetting> AquaSettings => _aquaSettings ??= new GenericRepository<AquaSetting>(_context, _httpContextAccessor);
        public IGenericRepository<PasswordResetRequest> PasswordResetRequests => _passwordResetRequests ??= new GenericRepository<PasswordResetRequest>(_context, _httpContextAccessor);

        public IGenericRepository<PermissionDefinition> PermissionDefinitions => _permissionDefinitions ??= new GenericRepository<PermissionDefinition>(_context, _httpContextAccessor);
        public IGenericRepository<PermissionGroup> PermissionGroups => _permissionGroups ??= new GenericRepository<PermissionGroup>(_context, _httpContextAccessor);
        public IGenericRepository<PermissionGroupPermission> PermissionGroupPermissions => _permissionGroupPermissions ??= new GenericRepository<PermissionGroupPermission>(_context, _httpContextAccessor);
        public IGenericRepository<UserPermissionGroup> UserPermissionGroups => _userPermissionGroups ??= new GenericRepository<UserPermissionGroup>(_context, _httpContextAccessor);

        public IGenericRepository<Project> Projects => _projects ??= new GenericRepository<Project>(_context, _httpContextAccessor);
        public IGenericRepository<Cage> Cages => _cages ??= new GenericRepository<Cage>(_context, _httpContextAccessor);
        public IGenericRepository<ProjectCage> ProjectCages => _projectCages ??= new GenericRepository<ProjectCage>(_context, _httpContextAccessor);
        public IGenericRepository<FishBatch> FishBatches => _fishBatches ??= new GenericRepository<FishBatch>(_context, _httpContextAccessor);
        public IGenericRepository<BatchCageBalance> BatchCageBalances => _batchCageBalances ??= new GenericRepository<BatchCageBalance>(_context, _httpContextAccessor);
        public IGenericRepository<BatchWarehouseBalance> BatchWarehouseBalances => _batchWarehouseBalances ??= new GenericRepository<BatchWarehouseBalance>(_context, _httpContextAccessor);
        public IGenericRepository<GoodsReceipt> GoodsReceipts => _goodsReceipts ??= new GenericRepository<GoodsReceipt>(_context, _httpContextAccessor);
        public IGenericRepository<GoodsReceiptLine> GoodsReceiptLines => _goodsReceiptLines ??= new GenericRepository<GoodsReceiptLine>(_context, _httpContextAccessor);
        public IGenericRepository<GoodsReceiptFishDistribution> GoodsReceiptFishDistributions => _goodsReceiptFishDistributions ??= new GenericRepository<GoodsReceiptFishDistribution>(_context, _httpContextAccessor);
        public IGenericRepository<Feeding> Feedings => _feedings ??= new GenericRepository<Feeding>(_context, _httpContextAccessor);
        public IGenericRepository<FeedingLine> FeedingLines => _feedingLines ??= new GenericRepository<FeedingLine>(_context, _httpContextAccessor);
        public IGenericRepository<FeedingDistribution> FeedingDistributions => _feedingDistributions ??= new GenericRepository<FeedingDistribution>(_context, _httpContextAccessor);
        public IGenericRepository<Mortality> Mortalities => _mortalities ??= new GenericRepository<Mortality>(_context, _httpContextAccessor);
        public IGenericRepository<MortalityLine> MortalityLines => _mortalityLines ??= new GenericRepository<MortalityLine>(_context, _httpContextAccessor);
        public IGenericRepository<Transfer> Transfers => _transfers ??= new GenericRepository<Transfer>(_context, _httpContextAccessor);
        public IGenericRepository<TransferLine> TransferLines => _transferLines ??= new GenericRepository<TransferLine>(_context, _httpContextAccessor);
        public IGenericRepository<WarehouseTransfer> WarehouseTransfers => _warehouseTransfers ??= new GenericRepository<WarehouseTransfer>(_context, _httpContextAccessor);
        public IGenericRepository<WarehouseTransferLine> WarehouseTransferLines => _warehouseTransferLines ??= new GenericRepository<WarehouseTransferLine>(_context, _httpContextAccessor);
        public IGenericRepository<CageWarehouseTransfer> CageWarehouseTransfers => _cageWarehouseTransfers ??= new GenericRepository<CageWarehouseTransfer>(_context, _httpContextAccessor);
        public IGenericRepository<CageWarehouseTransferLine> CageWarehouseTransferLines => _cageWarehouseTransferLines ??= new GenericRepository<CageWarehouseTransferLine>(_context, _httpContextAccessor);
        public IGenericRepository<WarehouseCageTransfer> WarehouseCageTransfers => _warehouseCageTransfers ??= new GenericRepository<WarehouseCageTransfer>(_context, _httpContextAccessor);
        public IGenericRepository<WarehouseCageTransferLine> WarehouseCageTransferLines => _warehouseCageTransferLines ??= new GenericRepository<WarehouseCageTransferLine>(_context, _httpContextAccessor);
        public IGenericRepository<Shipment> Shipments => _shipments ??= new GenericRepository<Shipment>(_context, _httpContextAccessor);
        public IGenericRepository<ShipmentLine> ShipmentLines => _shipmentLines ??= new GenericRepository<ShipmentLine>(_context, _httpContextAccessor);
        public IGenericRepository<Weighing> Weighings => _weighings ??= new GenericRepository<Weighing>(_context, _httpContextAccessor);
        public IGenericRepository<WeighingLine> WeighingLines => _weighingLines ??= new GenericRepository<WeighingLine>(_context, _httpContextAccessor);
        public IGenericRepository<StockConvert> StockConverts => _stockConverts ??= new GenericRepository<StockConvert>(_context, _httpContextAccessor);
        public IGenericRepository<StockConvertLine> StockConvertLines => _stockConvertLines ??= new GenericRepository<StockConvertLine>(_context, _httpContextAccessor);
        public IGenericRepository<BatchMovement> BatchMovements => _batchMovements ??= new GenericRepository<BatchMovement>(_context, _httpContextAccessor);
        public IGenericRepository<WeatherSeverity> WeatherSeverities => _weatherSeverities ??= new GenericRepository<WeatherSeverity>(_context, _httpContextAccessor);
        public IGenericRepository<WeatherType> WeatherTypes => _weatherTypes ??= new GenericRepository<WeatherType>(_context, _httpContextAccessor);
        public IGenericRepository<DailyWeather> DailyWeathers => _dailyWeathers ??= new GenericRepository<DailyWeather>(_context, _httpContextAccessor);
        public IGenericRepository<FishHealthEvent> FishHealthEvents => _fishHealthEvents ??= new GenericRepository<FishHealthEvent>(_context, _httpContextAccessor);
        public IGenericRepository<FishTreatment> FishTreatments => _fishTreatments ??= new GenericRepository<FishTreatment>(_context, _httpContextAccessor);
        public IGenericRepository<FishLabSample> FishLabSamples => _fishLabSamples ??= new GenericRepository<FishLabSample>(_context, _httpContextAccessor);
        public IGenericRepository<FishLabResult> FishLabResults => _fishLabResults ??= new GenericRepository<FishLabResult>(_context, _httpContextAccessor);
        public IGenericRepository<WelfareAssessment> WelfareAssessments => _welfareAssessments ??= new GenericRepository<WelfareAssessment>(_context, _httpContextAccessor);
        public IGenericRepository<ComplianceAudit> ComplianceAudits => _complianceAudits ??= new GenericRepository<ComplianceAudit>(_context, _httpContextAccessor);
        public IGenericRepository<ComplianceCorrectiveAction> ComplianceCorrectiveActions => _complianceCorrectiveActions ??= new GenericRepository<ComplianceCorrectiveAction>(_context, _httpContextAccessor);
        public IGenericRepository<ProjectCageDailyKpiSnapshot> ProjectCageDailyKpiSnapshots => _projectCageDailyKpiSnapshots ??= new GenericRepository<ProjectCageDailyKpiSnapshot>(_context, _httpContextAccessor);
        public IGenericRepository<NetOperationType> NetOperationTypes => _netOperationTypes ??= new GenericRepository<NetOperationType>(_context, _httpContextAccessor);
        public IGenericRepository<NetOperation> NetOperations => _netOperations ??= new GenericRepository<NetOperation>(_context, _httpContextAccessor);
        public IGenericRepository<NetOperationLine> NetOperationLines => _netOperationLines ??= new GenericRepository<NetOperationLine>(_context, _httpContextAccessor);

        public IGenericRepository<T> Repository<T>() where T : BaseEntity
        {
            return (IGenericRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new GenericRepository<T>(_context, _httpContextAccessor));
        }

        public Task<int> SaveChanges() => _context.SaveChangesAsync();

        public async Task BeginTransaction()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException(LocalizationBootstrap.GetString("EfUnitOfWork.TransactionAlreadyInProgress"));
            }

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task Commit()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException(LocalizationBootstrap.GetString("EfUnitOfWork.TransactionNotInProgress"));
            }

            try
            {
                await _transaction.CommitAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task Rollback()
        {
            if (_transaction == null)
            {
                return;
            }

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public Task<int> SaveChangesAsync() => SaveChanges();
        public Task BeginTransactionAsync() => BeginTransaction();
        public Task CommitTransactionAsync() => Commit();
        public Task RollbackTransactionAsync() => Rollback();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
                _disposed = true;
            }
        }
    }

    public class UnitOfWork : EfUnitOfWork
    {
        public UnitOfWork(AquaDbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }
    }
}
