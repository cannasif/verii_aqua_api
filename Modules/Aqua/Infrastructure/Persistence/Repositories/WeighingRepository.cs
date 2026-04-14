using aqua_api.Shared.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Repositories
{
    public class WeighingRepository : IWeighingRepository
    {
        private readonly AquaDbContext _db;

        public WeighingRepository(AquaDbContext db)
        {
            _db = db;
        }

        public Task<Weighing?> GetForPost(long id)
        {
            return _db.Weighings
                .Include(x => x.Lines)
                .ThenInclude(x => x.FishBatch)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }
    }
}
