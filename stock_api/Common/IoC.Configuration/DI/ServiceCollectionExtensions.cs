using Microsoft.AspNetCore.Authorization;
using stock_api.Auth;
using stock_api.Service;
using System.Reflection;

namespace stock_api.Common.IoC.Configuration.DI
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureBusinessServices(this IServiceCollection services, IConfiguration configuration)
        {
            if (services != null)
            {
                services.AddSingleton(configuration);
                services.AddScoped<AuthLayerService>();
                services.AddScoped<MemberService>();
                services.AddScoped<AuthHelpers>();
                services.AddScoped<AnnouncementService>();
                services.AddScoped<FileUploadService>();
                services.AddScoped<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();
                services.AddScoped<HandoverService>();
            }
        }

        public static void ConfigureMappings(this IServiceCollection services)
        {
            //Automap settings
            services?.AddAutoMapper(Assembly.GetExecutingAssembly());
        }
    }
}
