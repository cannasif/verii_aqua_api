using System;

namespace aqua_api.Shared.Common.Dtos
{
    public class BaseEntityDto
    {
        public long Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public bool IsDeleted { get; set; }
        public string? CreatedByFullUser { get; set; }
        public string? UpdatedByFullUser { get; set; }
        public string? DeletedByFullUser { get; set; }
    }

    public class BaseHeaderEntityDto : BaseEntityDto
    {
        public string Year { get; set; } = string.Empty;
        public DateTime? CompletionDate { get; set; }
        public bool IsCompleted { get; set; } = false;
        public bool IsPendingApproval { get; set; } = false;
        public bool? ApprovalStatus { get; set; }
        public string? RejectedReason { get; set; }
        public long? ApprovedByUserId { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public bool IsERPIntegrated { get; set; } = false;
        public string? ERPIntegrationNumber { get; set; }
        public DateTime? LastSyncDate { get; set; }
        public int? CountTriedBy { get; set; } = 0;

    }

    /// <summary>
    /// Base class for all Create DTOs.
    /// Create DTOs should inherit from this class.
    /// Note: ERP integration fields should NOT be included in Create DTOs.
    /// </summary>
    public abstract class BaseCreateDto
    {
        // Base class for Create DTOs
        // Common properties can be added here in the future
    }

    /// <summary>
    /// Base class for all Update DTOs.
    /// Update DTOs should inherit from this class.
    /// Note: ERP integration fields should NOT be included in Update DTOs.
    /// </summary>
    public abstract class BaseUpdateDto
    {
        // Base class for Update DTOs
        // Common properties can be added here in the future
    }
}

