using AutoMapper;
using aqua_api.Shared.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Stock.Application.Services
{
    public class StockRelationService : IStockRelationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILocalizationService _localizationService;

        public StockRelationService(IUnitOfWork unitOfWork, IMapper mapper, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _localizationService = localizationService;
        }

        public async Task<ApiResponse<StockRelationDto>> CreateAsync(StockRelationCreateDto relationDto)
        {
            try
            {
                var existingRelation = await _unitOfWork.StockRelations
                    .Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => 
                        x.StockId == relationDto.StockId && 
                        x.RelatedStockId == relationDto.RelatedStockId && 
                        !x.IsDeleted);

                if (existingRelation != null)
                {
                    return ApiResponse<StockRelationDto>.ErrorResult(
                        _localizationService.GetLocalizedString("StockRelationService.DuplicateRelation"),
                        _localizationService.GetLocalizedString("StockRelationService.DuplicateRelation"),
                        StatusCodes.Status400BadRequest);
                }

                await _unitOfWork.BeginTransactionAsync();

                var relation = _mapper.Map<StockRelation>(relationDto);
                await _unitOfWork.StockRelations.AddAsync(relation);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var relationWithNav = await _unitOfWork.StockRelations
                    .Query()
                    .Include(x => x.Stock)
                    .Include(x => x.RelatedStock)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .Include(x => x.DeletedByUser)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == relation.Id && !x.IsDeleted);

                var dto = _mapper.Map<StockRelationDto>(relationWithNav);

                return ApiResponse<StockRelationDto>.SuccessResult(
                    dto,
                    _localizationService.GetLocalizedString("StockRelationService.RelationAdded"));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<StockRelationDto>.ErrorResult(
                    _localizationService.GetLocalizedString("StockRelationService.InternalServerError"),
                    _localizationService.GetLocalizedString("StockRelationService.CreateExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<List<StockRelationDto>>> GetByStockIdAsync(long stockId)
        {
            try
            {
                var relations = await _unitOfWork.StockRelations
                    .Query()
                    .Where(x => x.StockId == stockId && !x.IsDeleted)
                    .Include(x => x.Stock)
                    .Include(x => x.RelatedStock)
                    .Include(x => x.CreatedByUser)
                    .Include(x => x.UpdatedByUser)
                    .Include(x => x.DeletedByUser)
                    .AsNoTracking()
                    .ToListAsync();

                var dtos = relations.Select(x => _mapper.Map<StockRelationDto>(x)).ToList();

                return ApiResponse<List<StockRelationDto>>.SuccessResult(
                    dtos,
                    _localizationService.GetLocalizedString("StockRelationService.RelationsRetrieved"));
            }
            catch (Exception ex)
            {
                return ApiResponse<List<StockRelationDto>>.ErrorResult(
                    _localizationService.GetLocalizedString("StockRelationService.InternalServerError"),
                    _localizationService.GetLocalizedString("StockRelationService.GetByStockIdExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ApiResponse<object>> DeleteAsync(long id)
        {
            try
            {
                var relation = await _unitOfWork.StockRelations.GetByIdAsync(id);
                if (relation == null)
                {
                    return ApiResponse<object>.ErrorResult(
                        _localizationService.GetLocalizedString("StockRelationService.RelationNotFound"),
                        _localizationService.GetLocalizedString("StockRelationService.RelationNotFound"),
                        StatusCodes.Status404NotFound);
                }

                await _unitOfWork.StockRelations.SoftDeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<object>.SuccessResult(
                    null,
                    _localizationService.GetLocalizedString("StockRelationService.RelationRemoved"));
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.ErrorResult(
                    _localizationService.GetLocalizedString("StockRelationService.InternalServerError"),
                    _localizationService.GetLocalizedString("StockRelationService.DeleteExceptionMessage", ex.Message),
                    StatusCodes.Status500InternalServerError);
            }
        }
    }
}
