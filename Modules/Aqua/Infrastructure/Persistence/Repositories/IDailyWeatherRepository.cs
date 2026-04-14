
namespace aqua_api.Modules.Aqua.Infrastructure.Persistence.Repositories
{
    public interface IDailyWeatherRepository
    {
        Task<bool> ExistsByProjectAndDate(long projectId, DateTime date);
        Task Add(DailyWeather entity);
    }
}
