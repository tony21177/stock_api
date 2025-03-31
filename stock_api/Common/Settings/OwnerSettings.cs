namespace stock_api.Common.Settings
{
    public class OwnerSettings
    {



        private readonly IConfiguration Configuration;

        public OwnerSettings(IConfiguration configuration)
        {
            Configuration = configuration;
            UnitId = Configuration.GetValue<string>("OnwerSettings:UnitId");
        }

        public string UnitId { get; set; } = null!;
        
    }
}
