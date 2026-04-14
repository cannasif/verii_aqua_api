
namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Repositories
{
    public interface IStockConvertRepository
    {
        Task<StockConvert?> GetForPost(long id);
    }
}
