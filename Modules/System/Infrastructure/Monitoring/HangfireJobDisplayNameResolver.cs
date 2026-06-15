namespace aqua_api.Modules.System.Infrastructure.Monitoring
{
    public static class HangfireJobDisplayNameResolver
    {
        private static readonly IReadOnlyDictionary<string, HangfireJobDisplayInfo> RecurringJobs =
            new Dictionary<string, HangfireJobDisplayInfo>(StringComparer.OrdinalIgnoreCase)
            {
                ["erp-stock-sync-job"] = new(
                    "ERP stok aynalama",
                    "Netsis stok kartlarını Aqua stok mirror tablosuna aktarır ve mevcut stok bilgilerini günceller.",
                    "ERP Mirror"),
                ["erp-warehouse-sync-job"] = new(
                    "ERP depo aynalama",
                    "Netsis depo/kafes kodlarını Aqua depo mirror tablosuna aktarır ve mevcut depo bilgilerini günceller.",
                    "ERP Mirror"),
                ["erp-receipt-shipment-movement-sync-job"] = new(
                    "ERP mal kabul ve sevkiyat hareketleri",
                    "Netsis mal kabul, yem girişi ve sevkiyat hareketlerini okuyup Aqua operasyon kayıtlarıyla eşleştirir.",
                    "ERP Operasyon"),
            };

        private static readonly IReadOnlyDictionary<string, HangfireJobDisplayInfo> TypeNames =
            new Dictionary<string, HangfireJobDisplayInfo>(StringComparer.OrdinalIgnoreCase)
            {
                ["StockSyncJob"] = RecurringJobs["erp-stock-sync-job"],
                ["WarehouseSyncJob"] = RecurringJobs["erp-warehouse-sync-job"],
                ["ErpReceiptShipmentMovementSyncJob"] = RecurringJobs["erp-receipt-shipment-movement-sync-job"],
                ["MailJob"] = new("E-posta gönderimi", "Sistem e-posta bildirimlerini arka planda gönderir.", "Sistem"),
                ["HangfireDeadLetterJob"] = new("Başarısız job arşivleme", "Kritik seviyede başarısız olan işleri dead-letter kuyruğuna taşır.", "Sistem"),
            };

        private static readonly IReadOnlyDictionary<string, HangfireJobDisplayInfo> MethodNames =
            new Dictionary<string, HangfireJobDisplayInfo>(StringComparer.OrdinalIgnoreCase)
            {
                ["ExecuteAsync"] = new("Senkronizasyon işi", "Arka plan senkronizasyon işlemini çalıştırır.", "Sistem"),
                ["SendEmailAsync"] = TypeNames["MailJob"],
                ["ProcessAsync"] = TypeNames["HangfireDeadLetterJob"],
            };

        public static HangfireJobDisplayInfo ResolveInfo(string? recurringJobId, Type? jobType = null, string? methodName = null, string? fallback = null)
        {
            if (!string.IsNullOrWhiteSpace(recurringJobId) &&
                RecurringJobs.TryGetValue(recurringJobId, out var recurringInfo))
            {
                return recurringInfo;
            }

            var typeName = jobType?.Name;
            if (!string.IsNullOrWhiteSpace(typeName) &&
                TypeNames.TryGetValue(typeName, out var typeInfo))
            {
                return typeInfo;
            }

            if (!string.IsNullOrWhiteSpace(methodName) &&
                MethodNames.TryGetValue(methodName, out var methodInfo))
            {
                return methodInfo;
            }

            return ResolveInfo(recurringJobId, fallback);
        }

        public static HangfireJobDisplayInfo ResolveInfo(string? recurringJobId, string? rawJobName)
        {
            if (!string.IsNullOrWhiteSpace(recurringJobId) &&
                RecurringJobs.TryGetValue(recurringJobId, out var recurringInfo))
            {
                return recurringInfo;
            }

            if (!string.IsNullOrWhiteSpace(rawJobName))
            {
                foreach (var type in TypeNames)
                {
                    if (rawJobName.Contains(type.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        return type.Value;
                    }
                }

                foreach (var method in MethodNames)
                {
                    if (rawJobName.Contains(method.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        return method.Value;
                    }
                }
            }

            return new HangfireJobDisplayInfo(
                string.IsNullOrWhiteSpace(rawJobName) ? "Bilinmeyen job" : rawJobName,
                "Bu iş için açıklama tanımlı değil. Teknik job adını kontrol edin.",
                "Sistem");
        }

        public static string Resolve(string? recurringJobId, Type? jobType = null, string? methodName = null, string? fallback = null)
        {
            return ResolveInfo(recurringJobId, jobType, methodName, fallback).Name;
        }

        public static string Resolve(string? recurringJobId, string? rawJobName)
        {
            return ResolveInfo(recurringJobId, rawJobName).Name;
        }
    }

    public sealed record HangfireJobDisplayInfo(string Name, string Description, string Category);
}
