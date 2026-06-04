namespace aqua_api.Modules.CurrentDirection.Domain.Entities
{
    public class CurrentDirection : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<CurrentDirectionMatch> Matches { get; set; } = new List<CurrentDirectionMatch>();
    }
}
