using Microsoft.EntityFrameworkCore;
using aqua_api.Modules.Integrations.Domain.Erp;

namespace aqua_api.Shared.Infrastructure.Persistence.Data
{
    public class AquaDbContext : DbContext
    {
        public AquaDbContext(DbContextOptions<AquaDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserAuthority> UserAuthorities { get; set; }
        public DbSet<UserDetail> UserDetails { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<PasswordResetRequest> PasswordResetRequests { get; set; }

        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<StockDetail> StockDetails { get; set; }
        public DbSet<StockImage> StockImages { get; set; }
        public DbSet<StockRelation> StockRelations { get; set; }

        public DbSet<SmtpSetting> SmtpSettings { get; set; }
        public DbSet<AquaSetting> AquaSettings { get; set; }
        public DbSet<JobFailureLog> JobFailureLogs { get; set; }
        public DbSet<RII_FN_CARI> RII_FN_CARI { get; set; }
        public DbSet<RII_VW_STOK> RII_VW_STOK { get; set; }
        public DbSet<RII_FN_DEPO> RII_FN_DEPO { get; set; }
        public DbSet<RII_FN_BRANCHES> Branches { get; set; }
        public DbSet<RII_FN_PROJECTCODE> RII_FN_PROJECTCODE { get; set; }

        public DbSet<PermissionDefinition> PermissionDefinitions { get; set; }
        public DbSet<PermissionGroup> PermissionGroups { get; set; }
        public DbSet<PermissionGroupPermission> PermissionGroupPermissions { get; set; }
        public DbSet<UserPermissionGroup> UserPermissionGroups { get; set; }

        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectMerge> ProjectMerges { get; set; }
        public DbSet<ProjectMergeSource> ProjectMergeSources { get; set; }
        public DbSet<ProjectMergeCage> ProjectMergeCages { get; set; }
        public DbSet<Cage> Cages { get; set; }
        public DbSet<ProjectCage> ProjectCages { get; set; }
        public DbSet<FishBatch> FishBatches { get; set; }
        public DbSet<BatchCageBalance> BatchCageBalances { get; set; }
        public DbSet<BatchWarehouseBalance> BatchWarehouseBalances { get; set; }
        public DbSet<GoodsReceipt> GoodsReceipts { get; set; }
        public DbSet<GoodsReceiptLine> GoodsReceiptLines { get; set; }
        public DbSet<GoodsReceiptFishDistribution> GoodsReceiptFishDistributions { get; set; }
        public DbSet<Feeding> Feedings { get; set; }
        public DbSet<FeedingLine> FeedingLines { get; set; }
        public DbSet<FeedingDistribution> FeedingDistributions { get; set; }
        public DbSet<Mortality> Mortalities { get; set; }
        public DbSet<MortalityLine> MortalityLines { get; set; }
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<TransferLine> TransferLines { get; set; }
        public DbSet<WarehouseTransfer> WarehouseTransfers { get; set; }
        public DbSet<WarehouseTransferLine> WarehouseTransferLines { get; set; }
        public DbSet<CageWarehouseTransfer> CageWarehouseTransfers { get; set; }
        public DbSet<CageWarehouseTransferLine> CageWarehouseTransferLines { get; set; }
        public DbSet<WarehouseCageTransfer> WarehouseCageTransfers { get; set; }
        public DbSet<WarehouseCageTransferLine> WarehouseCageTransferLines { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<ShipmentLine> ShipmentLines { get; set; }
        public DbSet<Weighing> Weighings { get; set; }
        public DbSet<WeighingLine> WeighingLines { get; set; }
        public DbSet<StockConvert> StockConverts { get; set; }
        public DbSet<StockConvertLine> StockConvertLines { get; set; }
        public DbSet<BatchMovement> BatchMovements { get; set; }
        public DbSet<WeatherSeverity> WeatherSeverities { get; set; }
        public DbSet<WeatherType> WeatherTypes { get; set; }
        public DbSet<DailyWeather> DailyWeathers { get; set; }
        public DbSet<FishHealthEvent> FishHealthEvents { get; set; }
        public DbSet<FishTreatment> FishTreatments { get; set; }
        public DbSet<FishLabSample> FishLabSamples { get; set; }
        public DbSet<FishLabResult> FishLabResults { get; set; }
        public DbSet<WelfareAssessment> WelfareAssessments { get; set; }
        public DbSet<ComplianceAudit> ComplianceAudits { get; set; }
        public DbSet<ComplianceCorrectiveAction> ComplianceCorrectiveActions { get; set; }
        public DbSet<ProjectCageDailyKpiSnapshot> ProjectCageDailyKpiSnapshots { get; set; }
        public DbSet<NetOperationType> NetOperationTypes { get; set; }
        public DbSet<NetOperation> NetOperations { get; set; }
        public DbSet<NetOperationLine> NetOperationLines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                    {
                        property.SetColumnType("decimal(18,6)");
                    }
                }
            }

            modelBuilder.Entity<RII_FN_KUR>(entity =>
            {
                entity.HasNoKey();
                entity.ToTable("__EFMigrationsHistory_FN_KUR", t => t.ExcludeFromMigrations());
                entity.ToFunction("RII_FN_KUR");
            });

            modelBuilder.Entity<RII_FN_2SHIPPING>(entity =>
            {
                entity.HasNoKey();
                entity.ToTable("__EFMigrationsHistory_FN_2SHIPPING", t => t.ExcludeFromMigrations());
                entity.ToFunction("RII_FN_2SHIPPING");
            });

            modelBuilder.Entity<RII_STGROUP>(entity =>
            {
                entity.HasNoKey();
                entity.ToTable("__EFMigrationsHistory_STGROUP", t => t.ExcludeFromMigrations());
                entity.ToFunction("RII_STGROUP");
            });

            modelBuilder.Entity<RII_FN_STOK>(entity =>
            {
                entity.HasNoKey();
                entity.ToTable("__EFMigrationsHistory_FN_STOK", t => t.ExcludeFromMigrations());
                entity.ToFunction("RII_FN_STOK");
                entity.Property(e => e.STOK_KODU).HasMaxLength(25);
                entity.Property(e => e.OLCU_BR1).IsRequired(false).HasMaxLength(10);
                entity.Property(e => e.URETICI_KODU).IsRequired(false).HasMaxLength(25);
                entity.Property(e => e.STOK_ADI).IsRequired(false).HasMaxLength(50);
                entity.Property(e => e.GRUP_KODU).IsRequired(false).HasMaxLength(8);
                entity.Property(e => e.GRUP_ISIM).IsRequired(false).HasMaxLength(50);
                entity.Property(e => e.KOD_1).IsRequired(false).HasMaxLength(20);
                entity.Property(e => e.KOD1_ADI).IsRequired(false).HasMaxLength(50);
                entity.Property(e => e.KOD_2).IsRequired(false).HasMaxLength(20);
                entity.Property(e => e.KOD2_ADI).IsRequired(false).HasMaxLength(50);
                entity.Property(e => e.KOD_3).IsRequired(false).HasMaxLength(20);
                entity.Property(e => e.KOD3_ADI).IsRequired(false).HasMaxLength(50);
                entity.Property(e => e.KOD_4).IsRequired(false).HasMaxLength(20);
                entity.Property(e => e.KOD4_ADI).IsRequired(false).HasMaxLength(50);
                entity.Property(e => e.KOD_5).IsRequired(false).HasMaxLength(20);
                entity.Property(e => e.KOD5_ADI).IsRequired(false).HasMaxLength(50);
                entity.Property(e => e.INGISIM).IsRequired(false).HasMaxLength(50);
            });

            modelBuilder.Entity<RII_FN_DEPO>(entity =>
            {
                entity.HasNoKey();
                entity.ToTable("__EFMigrationsHistory_FN_DEPO", t => t.ExcludeFromMigrations());
                entity.ToFunction("RII_FN_DEPO");
                entity.Property(e => e.DEPO_ISMI).HasMaxLength(100);
                entity.Property(e => e.CARI_KODU).HasMaxLength(25);
            });

            modelBuilder.Entity<RII_FN_CARI>(entity =>
            {
                entity.HasNoKey();
                entity.ToTable("__EFMigrationsHistory_FN_CARI", t => t.ExcludeFromMigrations());
                entity.ToFunction("RII_FN_CARI");
                entity.Property(e => e.CARI_KOD).HasMaxLength(25);
                entity.Property(e => e.CARI_ISIM).HasMaxLength(100);
                entity.Property(e => e.CARI_TEL).HasMaxLength(20);
                entity.Property(e => e.CARI_IL).HasMaxLength(50);
                entity.Property(e => e.CARI_ADRES).HasMaxLength(500);
            });

            modelBuilder.Entity<RII_VW_STOK>(entity =>
            {
                entity.HasNoKey();
                entity.ToTable("__EFMigrationsHistory_VW_STOK", t => t.ExcludeFromMigrations());
                entity.ToFunction("RII_VW_STOK");
                entity.Property(e => e.STOK_KODU).HasMaxLength(25);
                entity.Property(e => e.STOK_ADI).HasMaxLength(50);
                entity.Property(e => e.GRUP_KODU).HasMaxLength(10);
                entity.Property(e => e.URETICI_KODU).HasMaxLength(25);
            });

            modelBuilder.Entity<RII_FN_BRANCHES>(entity =>
            {
                entity.HasNoKey();
                entity.ToTable("__EFMigrationsHistory_FN_BRANCHES", t => t.ExcludeFromMigrations());
                entity.ToFunction("RII_FN_BRANCHES");
                entity.Property(e => e.UNVAN).HasMaxLength(150);
            });

            modelBuilder.Entity<RII_FN_PROJECTCODE>(entity =>
            {
                entity.HasNoKey();
                entity.ToTable("__EFMigrationsHistory_FN_PROJECTCODE", t => t.ExcludeFromMigrations());
                entity.ToFunction("RII_FN_PROJECTCODE");
                entity.Property(e => e.PROJE_KODU).HasMaxLength(15);
                entity.Property(e => e.PROJE_ACIKLAMA).HasMaxLength(50);
            });

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AquaDbContext).Assembly);
        }
    }
}
