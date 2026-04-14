using aqua_api.Shared.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Repositories
{
    public class NetOperationRepository : INetOperationRepository
    {
        private readonly AquaDbContext _db;

        public NetOperationRepository(AquaDbContext db)
        {
            _db = db;
        }

        public Task<NetOperation?> GetForPost(long id)
        {
            return _db.NetOperations
                .Include(x => x.Lines)
                .ThenInclude(x => x.FishBatch)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }
    }
}
