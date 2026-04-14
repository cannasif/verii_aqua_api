using AutoMapper;
using aqua_api.Shared.Infrastructure.Time;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Application.Services
{
    public class AquaSettingsService : IAquaSettingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AquaSettingsService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<AquaSettingsDto>> GetAsync()
        {
            try
            {
                var entity = await EnsureEntityAsync();
                var dto = _mapper.Map<AquaSettingsDto>(entity);
                return ApiResponse<AquaSettingsDto>.SuccessResult(dto, "Aqua ayarlari getirildi.");
            }
            catch (Exception ex)
            {
                return ApiResponse<AquaSettingsDto>.ErrorResult(
                    "Aqua ayarlari alinamadi.",
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<AquaSettingsDto>> UpdateAsync(UpdateAquaSettingsDto dto, long userId)
        {
            try
            {
                var entity = await EnsureEntityAsync();
                _mapper.Map(dto, entity);
                entity.UpdatedBy = userId;
                entity.UpdatedDate = DateTimeProvider.Now;

                await _unitOfWork.AquaSettings.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<AquaSettingsDto>(entity);
                return ApiResponse<AquaSettingsDto>.SuccessResult(result, "Aqua ayarlari kaydedildi.");
            }
            catch (Exception ex)
            {
                return ApiResponse<AquaSettingsDto>.ErrorResult(
                    "Aqua ayarlari kaydedilemedi.",
                    ex.Message,
                    StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<AquaSetting> EnsureEntityAsync()
        {
            var entity = await _unitOfWork.AquaSettings
                .Query()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();

            if (entity != null)
            {
                return entity;
            }

            entity = new AquaSetting
            {
                RequireFullTransfer = true,
                AllowProjectMerge = false,
                PartialTransferOccupiedCageMode = 0,
                CreatedDate = DateTimeProvider.Now,
                UpdatedDate = DateTimeProvider.Now,
                IsDeleted = false
            };

            await _unitOfWork.AquaSettings.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return entity;
        }
    }
}
