using System;

namespace aqua_api.Shared.Domain.Entities
{
    public interface IErpPostableHeader
    {
        bool IsERPIntegrated { get; set; }
        string? ERPReferenceNumber { get; set; }
        DateTime? ERPIntegrationDate { get; set; }
        string? ERPIntegrationStatus { get; set; }
        string? ERPErrorMessage { get; set; }
        int? CountTriedBy { get; set; }
    }
}
