namespace aqua_api.Modules.Integrations.Infrastructure.Options
{
    public class NetsisOptions
    {
        public const string SectionName = "Netsis";

        public bool Enabled { get; set; }
        public NetsisRestOptions Rest { get; set; } = new();
    }
}
