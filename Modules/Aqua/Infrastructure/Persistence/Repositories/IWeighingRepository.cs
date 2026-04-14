
namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Repositories
{
    public interface IWeighingRepository
    {
        Task<Weighing?> GetForPost(long id);
    }
}
