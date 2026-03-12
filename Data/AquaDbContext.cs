using Microsoft.EntityFrameworkCore;
using aqua_api.Models;
using aqua_api.Models.UserPermissions;
using depoWebAPI.Models;

namespace aqua_api.Data
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
        public DbSet<StockDetail> StockDetails { get; set; }
        public DbSet<StockImage> StockImages { get; set; }
        public DbSet<StockRelation> StockRelations { get; set; }

        public DbSet<SmtpSetting> SmtpSettings { get; set; }
        public DbSet<JobFailureLog> JobFailureLogs { get; set; }

        public DbSet<PermissionDefinition> PermissionDefinitions { get; set; }
        public DbSet<PermissionGroup> PermissionGroups { get; set; }
        public DbSet<PermissionGroupPermission> PermissionGroupPermissions { get; set; }
        public DbSet<UserPermissionGroup> UserPermissionGroups { get; set; }

        public DbSet<Project> Projects { get; set; }
        public DbSet<Cage> Cages { get; set; }
        public DbSet<ProjectCage> ProjectCages { get; set; }
        public DbSet<FishBatch> FishBatches { get; set; }
        public DbSet<BatchCageBalance> BatchCageBalances { get; set; }
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
            });

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AquaDbContext).Assembly);
        }
    }
}
