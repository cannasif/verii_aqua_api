
namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Repositories
{
    public interface ITransferRepository
    {
        Task<Transfer?> GetForPost(long id);
    }
}
