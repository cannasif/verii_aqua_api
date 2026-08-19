namespace aqua_api.Shared.Common.Mappings;

public static class AuditDtoMappingExtensions
{
    public static TDto WithAuditFrom<TDto>(this TDto dto, BaseEntity entity)
        where TDto : AuditDto
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(entity);

        dto.CreatedDate = entity.CreatedDate;
        dto.UpdatedDate = entity.UpdatedDate;
        dto.CreatedBy = entity.CreatedBy;
        dto.UpdatedBy = entity.UpdatedBy;
        dto.CreatedByFullUser = FormatUser(entity.CreatedByUser);
        dto.UpdatedByFullUser = FormatUser(entity.UpdatedByUser);
        return dto;
    }

    private static string? FormatUser(User? user)
    {
        if (user is null)
        {
            return null;
        }

        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? user.Username : fullName;
    }
}
