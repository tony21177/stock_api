namespace stock_api.Common.Settings
{
    public class SmtpSettings
    {



        private readonly IConfiguration Configuration;

        public SmtpSettings(IConfiguration configuration)
        {
            Configuration = configuration;
            Server = Configuration.GetValue<string>("SmtpSettings:Server");
            Port = Configuration.GetValue<int>("SmtpSettings:Port");
            User = Configuration.GetValue<string>("SmtpSettings:User");
            Password = Configuration.GetValue<string>("SmtpSettings:Password");
            Domain = Configuration.GetValue<string>("SmtpSettings:Domain");
        }

        public string Server { get; set; } = null!;
        public int Port { get; set; }
        public string User { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Domain { get; set; } = null!;
    }
}
