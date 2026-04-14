using AutoMapper;
using aqua_api.Shared.Infrastructure.Time;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;

namespace aqua_api.Modules.Integrations.Application.Services
{
    public class SmtpSettingsService : ISmtpSettingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;
        private readonly IMemoryCache _cache;
        private readonly IDataProtector _protector;
        private readonly IConfiguration _configuration;

        private const string CacheKey = "smtp_settings_runtime_v1";

        public SmtpSettingsService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILocalizationService localizationService,
            IMemoryCache cache,
            IDataProtectionProvider dataProtectionProvider,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
            _cache = cache;
            _protector = dataProtectionProvider.CreateProtector("smtp-settings-v1");
            _configuration = configuration;
        }

        public void InvalidateCache()
        {
            _cache.Remove(CacheKey);
        }

        public async Task<ApiResponse<SmtpSettingsDto>> GetAsync()
        {
            try
            {
                var entity = await EnsureSmtpSettingsEntityAsync();
                var mapped = _mapper.Map<SmtpSettingsDto>(entity);

                return ApiResponse<SmtpSettingsDto>.SuccessResult(
                    mapped,
                    _localizationService.GetLocalizedString("SmtpSettingsService.SmtpSettingsRetrieved"));
            }
            catch (Exception ex)
            {
                return ApiResponse<SmtpSettingsDto>.ErrorResult(
                    _localizationService.GetLocalizedString("SmtpSettingsService.InternalServerError"),
                    _localizationService.GetLocalizedString("SmtpSettingsService.GetExceptionMessage", ex.Message, StatusCodes.Status500InternalServerError));
            }
        }

        public async Task<ApiResponse<SmtpSettingsDto>> UpdateAsync(UpdateSmtpSettingsDto dto, long userId)
        {
            try
            {
                var entity = await _unitOfWork.SmtpSettings
                    .Query()
                    .Where(x => !x.IsDeleted)
                    .OrderBy(x => x.Id)
                    .FirstOrDefaultAsync();

                if (entity == null)
                {
                    entity = new SmtpSetting
                    {
                        IsDeleted = false,
                        CreatedDate = DateTimeProvider.Now,
                        CreatedBy = userId
                    };

                    // AutoMapper: dto -> entity (audit alanlar mapping profile'da ignore)
                    _mapper.Map(dto, entity);

                    // Password (varsa) şifrele
                    if (!string.IsNullOrWhiteSpace(dto.Password))
                        entity.PasswordEncrypted = _protector.Protect(dto.Password);

                    entity.UpdatedDate = DateTimeProvider.Now;
                    entity.UpdatedBy = userId;

                    await _unitOfWork.SmtpSettings.AddAsync(entity);
                    await _unitOfWork.SaveChangesAsync();

                    // cache invalidate (bu instance)
                    InvalidateCache();

                    var createdDto = _mapper.Map<SmtpSettingsDto>(entity);

                    return ApiResponse<SmtpSettingsDto>.SuccessResult(
                        createdDto,
                        _localizationService.GetLocalizedString("SmtpSettingsService.SmtpSettingsCreated"));
                }

                // mevcut kayıt update
                _mapper.Map(dto, entity);

                // Password boş ise eskisini koru
                if (!string.IsNullOrWhiteSpace(dto.Password))
                    entity.PasswordEncrypted = _protector.Protect(dto.Password);

                entity.UpdatedDate = DateTimeProvider.Now;
                entity.UpdatedBy = userId;

                await _unitOfWork.SmtpSettings.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                // cache invalidate (bu instance)
                InvalidateCache();

                var updatedDto = _mapper.Map<SmtpSettingsDto>(entity);

                return ApiResponse<SmtpSettingsDto>.SuccessResult(
                    updatedDto,
                    _localizationService.GetLocalizedString("SmtpSettingsService.SmtpSettingsUpdated"));
            }
            catch (Exception ex)
            {
                return ApiResponse<SmtpSettingsDto>.ErrorResult(
                    _localizationService.GetLocalizedString("SmtpSettingsService.InternalServerError"),
                    _localizationService.GetLocalizedString("SmtpSettingsService.UpdateExceptionMessage", ex.Message, StatusCodes.Status500InternalServerError));
            }
        }

        public async Task<SmtpSettingsRuntimeDto> GetRuntimeAsync()
        {
            if (_cache.TryGetValue(CacheKey, out SmtpSettingsRuntimeDto? cached) && cached != null)
                return cached;

            var entity = await EnsureSmtpSettingsEntityAsync();

            var password = string.IsNullOrWhiteSpace(entity.PasswordEncrypted)
                ? ""
                : _protector.Unprotect(entity.PasswordEncrypted);

            var runtime = new SmtpSettingsRuntimeDto
            {
                Host = entity.Host ?? "",
                Port = entity.Port,
                EnableSsl = entity.EnableSsl,
                Username = entity.Username ?? "",
                Password = password,
                FromEmail = entity.FromEmail ?? "",
                FromName = entity.FromName ?? "",
                Timeout = entity.Timeout
            };

            _cache.Set(CacheKey, runtime); // süresiz (instance-local)

            return runtime;
        }

        private async Task<SmtpSetting> EnsureSmtpSettingsEntityAsync()
        {
            var entity = await _unitOfWork.SmtpSettings
                .Query()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();

            if (entity != null)
                return entity;

            var section = _configuration.GetSection("SmtpDefaults");
            var host = section["Host"] ?? "smtp.gmail.com";
            var port = section.GetValue<int?>("Port") ?? 587;
            var enableSsl = section.GetValue<bool?>("EnableSsl") ?? true;
            var username = section["Username"] ?? "";
            var password = section["Password"] ?? "";
            var fromEmail = section["FromEmail"] ?? username;
            var fromName = section["FromName"] ?? "CRM AQUA SYSTEM";
            var timeout = section.GetValue<int?>("Timeout") ?? 30;

            entity = new SmtpSetting
            {
                IsDeleted = false,
                CreatedDate = DateTimeProvider.Now,
                UpdatedDate = DateTimeProvider.Now,
                Host = host,
                Port = port,
                EnableSsl = enableSsl,
                Username = username,
                PasswordEncrypted = string.IsNullOrWhiteSpace(password) ? "" : _protector.Protect(password),
                FromEmail = fromEmail,
                FromName = fromName,
                Timeout = timeout
            };

            await _unitOfWork.SmtpSettings.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            InvalidateCache();

            return entity;
        }
    }
}
