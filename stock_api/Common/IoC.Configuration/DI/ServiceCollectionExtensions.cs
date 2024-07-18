using Microsoft.AspNetCore.Authorization;
using stock_api.Auth;
using stock_api.Common.Settings;
using stock_api.Service;
using stock_api.Utils;
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
                services.AddSingleton<SmtpSettings>();

                services.AddScoped<AuthLayerService>();
                services.AddScoped<MemberService>();
                services.AddScoped<AuthHelpers>();
                services.AddScoped<CompanyService>();
                services.AddScoped<GroupService>();
                services.AddScoped<FileUploadService>();
                services.AddScoped<ManufacturerService>();
                services.AddScoped<SupplierService>();
                services.AddScoped<WarehouseProductService>();
                services.AddScoped<PurchaseFlowSettingService>();
                services.AddScoped<PurchaseService>();
                services.AddScoped<StockInService>();
                services.AddScoped<StockOutService>();
                services.AddScoped<AdjustService>();
                services.AddScoped<SupplierTraceService>();
                services.AddScoped<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();

                // Configure EmailService
                services.AddSingleton<EmailService>();
            }
        }

        public static void ConfigureMappings(this IServiceCollection services)
        {
            //Automap settings
            services?.AddAutoMapper(Assembly.GetExecutingAssembly());
        }
    }
}
