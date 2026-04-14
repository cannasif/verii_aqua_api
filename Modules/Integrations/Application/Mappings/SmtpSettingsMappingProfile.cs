using AutoMapper;

namespace aqua_api.Modules.Integrations.Application.Mappings
{
    public class SmtpSettingsMappingProfile : Profile
    {
        public SmtpSettingsMappingProfile()
        {
            // Entity -> DTO (GET response, password yok)
            CreateMap<SmtpSetting, SmtpSettingsDto>();

            // Update DTO -> Entity (PUT request)
            CreateMap<UpdateSmtpSettingsDto, SmtpSetting>()
                // Tek kayıt mantığı: Id serviste 1 üzerinden yönetilecek
                .ForMember(dest => dest.Id, opt => opt.Ignore())

                // PasswordEncrypted mapping'e dahil değil (service şifreleyecek)
                .ForMember(dest => dest.PasswordEncrypted, opt => opt.Ignore())

                // Sistem alanları (EntityBase) kontrol altında
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())

                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))

                .ForMember(dest => dest.DeletedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedBy, opt => opt.Ignore());
        }
    }
}
