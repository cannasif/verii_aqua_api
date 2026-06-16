namespace aqua_api.Modules.Integrations.Infrastructure.Options
{
    public class NetsisRestOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string LoginPath { get; set; } = "/api/v2/token";
        public string ItemSlipsPath { get; set; } = "/api/v2/ItemSlips";
        public string ItemsPath { get; set; } = "/api/v2/Items";
        public string ArpsPath { get; set; } = "/api/v2/ARPs";
        public int WarehouseTransferInDocumentType { get; set; }
        public int WarehouseTransferOutDocumentType { get; set; }
        public string FeedWarehouseTransferOutSeries { get; set; } = "YEM";
        public string MortalityWarehouseTransferOutSeries { get; set; } = "FIR";
        public string? WarehouseTransferOutExpenseCode { get; set; }
        public string? FeedWarehouseTransferOutExpenseCode { get; set; }
        public string? MortalityWarehouseTransferOutExpenseCode { get; set; }
        public bool UseRestGeneratedWarehouseTransferNumbers { get; set; } = true;
        public int? DefaultWarehouseCode { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
        public bool AllowInvalidSslCertificate { get; set; }
        public int DefaultTokenLifetimeMinutes { get; set; } = 60;
        public int TokenExpirySkewSeconds { get; set; } = 30;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string DbName { get; set; } = string.Empty;
        public string DbUser { get; set; } = string.Empty;
        public string DbPassword { get; set; } = string.Empty;
        public string DbType { get; set; } = string.Empty;
        public NetsisAuthOptions Auth { get; set; } = new();
    }
}
