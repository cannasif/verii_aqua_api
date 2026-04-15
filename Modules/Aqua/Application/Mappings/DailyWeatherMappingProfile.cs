using AutoMapper;

namespace aqua_api.Modules.Aqua.Application.Mappings
{
    public class DailyWeatherMappingProfile : Profile
    {
        public DailyWeatherMappingProfile()
        {
            CreateMap<DailyWeather, DailyWeatherDto>()
                .ForMember(dest => dest.ProjectCode, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectCode : null))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectName : null))
                .ForMember(dest => dest.WeatherTypeCode, opt => opt.MapFrom(src => src.WeatherType != null ? src.WeatherType.Code : null))
                .ForMember(dest => dest.WeatherTypeName, opt => opt.MapFrom(src => src.WeatherType != null ? src.WeatherType.Name : null))
                .ForMember(dest => dest.WeatherSeverityCode, opt => opt.MapFrom(src => src.WeatherSeverity != null ? src.WeatherSeverity.Code : null))
                .ForMember(dest => dest.WeatherSeverityName, opt => opt.MapFrom(src => src.WeatherSeverity != null ? src.WeatherSeverity.Name : null))
                .ForMember(dest => dest.WeatherSeverityScore, opt => opt.MapFrom(src => src.WeatherSeverity != null ? src.WeatherSeverity.Score : (int?)null))
                .ForMember(dest => dest.OperationalRiskScore, opt => opt.MapFrom(src => CalculateOperationalRiskScore(src)))
                .ForMember(dest => dest.OperationalRiskLevel, opt => opt.MapFrom(src => CalculateOperationalRiskLevel(src)))
                .ForMember(dest => dest.OperationalRiskFormula, opt => opt.MapFrom(src => BuildOperationalRiskFormula()));
            CreateMap<CreateDailyWeatherDto, DailyWeather>();
            CreateMap<UpdateDailyWeatherDto, DailyWeather>();
        }

        private static decimal CalculateOperationalRiskScore(DailyWeather source)
        {
            decimal score = Math.Clamp(source.WeatherSeverity?.Score ?? 0, 0, 100);

            if (source.DissolvedOxygenMgL.HasValue)
            {
                if (source.DissolvedOxygenMgL.Value < 4m) score += 35m;
                else if (source.DissolvedOxygenMgL.Value < 6m) score += 20m;
            }

            if (source.Ph.HasValue && (source.Ph.Value < 6.5m || source.Ph.Value > 8.5m)) score += 10m;
            if (source.CurrentSpeedKn.HasValue && source.CurrentSpeedKn.Value >= 2m) score += 10m;
            if (source.WaveHeightM.HasValue && source.WaveHeightM.Value >= 2m) score += 15m;
            if (source.TurbidityNtu.HasValue && source.TurbidityNtu.Value >= 25m) score += 10m;
            if (source.AmmoniaMgL.HasValue && source.AmmoniaMgL.Value >= 0.02m) score += 20m;
            if (source.NitriteMgL.HasValue && source.NitriteMgL.Value >= 0.1m) score += 15m;
            if (source.AlgalBloomIndex.HasValue && source.AlgalBloomIndex.Value >= 60m) score += 20m;
            if (source.SensorHealthScore.HasValue && source.SensorHealthScore.Value < 70m) score += 10m;

            return Math.Min(100m, score);
        }

        private static string CalculateOperationalRiskLevel(DailyWeather source)
        {
            var score = CalculateOperationalRiskScore(source);
            if (score >= 75m) return "Critical";
            if (score >= 50m) return "High";
            if (score >= 25m) return "Moderate";
            return "Low";
        }

        private static string BuildOperationalRiskFormula()
        {
            return "Base risk comes from user-defined weather severity score; measured oxygen, pH, current, wave, turbidity, ammonia, nitrite, algal bloom and sensor health add threshold-based risk.";
        }
    }
}
