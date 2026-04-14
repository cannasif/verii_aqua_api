using aqua_api.Shared.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Repositories
{
    public class DailyWeatherRepository : IDailyWeatherRepository
    {
        private readonly AquaDbContext _db;

        public DailyWeatherRepository(AquaDbContext db)
        {
            _db = db;
        }

        public Task<bool> ExistsByProjectAndDate(long projectId, DateTime date)
        {
            return _db.DailyWeathers.AnyAsync(x => x.ProjectId == projectId && x.WeatherDate == date.Date && !x.IsDeleted);
        }

        public Task Add(DailyWeather entity)
        {
            return _db.DailyWeathers.AddAsync(entity).AsTask();
        }
    }
}
