using System.Collections.Generic;

namespace aqua_api.Modules.WindDirection.Domain.Entities
{
    public class WindDirection : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public ICollection<WindDirectionMatch> Matches { get; set; } = new List<WindDirectionMatch>();
    }
}
