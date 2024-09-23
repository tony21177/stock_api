using stock_api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using stock_api.Common.IoC.Configuration.DI;
using System.Text;
using System.Text.Json;
using Serilog;
using Microsoft.AspNetCore.Hosting;
using Serilog.Enrichers.CallerInfo;
using System.Transactions;
using stock_api.Scheduler;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// 配置 Serilog
//Log.Logger = new LoggerConfiguration()
//    .MinimumLevel.Debug()
//    .WriteTo.Console()
//    .WriteTo.File("logs/stock_api_log.txt", rollingInterval: RollingInterval.Day)
//.CreateLogger();

// 配置 Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] [{Caller}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/stock_api_log.txt", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] [{Caller}] {Message:lj}{NewLine}{Exception}")
    .Enrich.FromLogContext()
    .Enrich.WithCallerInfo(
        includeFileInfo: true,
        allowedAssemblies: new List<string> { "Serilog.Enrichers.CallerInfo.Tests" },
        prefix: "stock_")// 添加这个以捕获调用者信息
    .CreateLogger();


// 設置 Serilog 作為 Logging Provider
builder.Host.UseSerilog();

// 創建一個 Serilog LoggerFactory
var serilogLoggerFactory = LoggerFactory.Create(loggingBuilder =>
{
    loggingBuilder.AddSerilog(); // 添加 Serilog 作為 Logger
});


//services cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        //policy.AllowAnyMethod();
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();

    });
});

//Hangfire Service
//builder.Services.AddHangfire(configuration => configuration
//.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
//.UseSimpleAssemblyNameTypeSerializer()
//.UseRecommendedSerializerSettings()
//.UseStorage(new MySqlStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new MySqlStorageOptions
//{
//    TransactionIsolationLevel = IsolationLevel.ReadCommitted,
//    QueuePollInterval = TimeSpan.FromSeconds(15),
//    JobExpirationCheckInterval = TimeSpan.FromHours(1),
//    CountersAggregateInterval = TimeSpan.FromMinutes(5),
//    PrepareSchemaIfNecessary = true,
//    DashboardJobListLimit = 50000,
//    TransactionTimeout = TimeSpan.FromMinutes(1),
//    TablesPrefix = "Hangfire"
//})));





// 配置 MySQL 和 Entity Framework
var serverVersion = new MySqlServerVersion(new Version(5, 7, 29));
builder.Services.AddDbContext<StockDbContext>(options =>

{
    //options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), serverVersion, mySqlOptions =>
    //{
    //    mySqlOptions.EnableRetryOnFailure(
    //        maxRetryCount: 5, // 最大重试次数
    //        maxRetryDelay: TimeSpan.FromSeconds(30), // 重试之间的最大延迟
    //        errorNumbersToAdd: null // 要添加的错误编号（可选）
    //    );
    //});
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), serverVersion);
    options.UseLoggerFactory(serilogLoggerFactory) // 使用 Serilog 的 LoggerFactory
           .EnableSensitiveDataLogging(); // 如果需要敏感數據記錄

}, ServiceLifetime.Scoped);


builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed. {context.Exception}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated.");
                return Task.CompletedTask;
            },
            // ... other event handlers
        };
        // 當驗證失敗時，回應標頭會包含 WWW-Authenticate 標頭，這裡會顯示失敗的詳細錯誤原因
        options.IncludeErrorDetails = true; // 預設值為 true，有時會特別關閉

        options.TokenValidationParameters = new TokenValidationParameters
        {
            // 透過這項宣告，就可以從 "sub" 取值並設定給 User.Identity.Name
            NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
            // 透過這項宣告，就可以從 "roles" 取值，並可讓 [Authorize] 判斷角色
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

            // 一般我們都會驗證 Issuer
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration.GetValue<string>("JwtSettings:Issuer"),

            // 通常不太需要驗證 Audience
            ValidateAudience = false,
            //ValidAudience = "JwtAuthDemo", // 不驗證就不需要填寫

            // 一般我們都會驗證 Token 的有效期間
            ValidateLifetime = true,

            // 如果 Token 中包含 key 才需要驗證，一般都只有簽章而已
            ValidateIssuerSigningKey = false,

            // "1234567890123456" 應該從 IConfiguration 取得
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtSettings:SignKey")))
        };
    });

builder.Services.AddAuthorization().AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>(); ;


// AutoMapper
builder.Services.ConfigureMappings();


builder.Services.AddControllers(options => { options.AllowEmptyInputInBodyModelBinding = true; }).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
}); ;
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.ConfigureBusinessServices(builder.Configuration);


// Configure Quartz
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    // Define a job and tie it to the ProductQuantityNotifyJob class
    q.AddJob<ProductQuantityNotifyJob>(opts => opts.WithIdentity("ProductQuantityNotifyJob"));
    q.AddJob<NearExpiredQuantityNotifyJob>(opts => opts.WithIdentity("NearExpiredQuantityNotifyJob"));
    q.AddJob<PurchaseNotifyJob>(opts => opts.WithIdentity("PurchaseNotifyJob"));
    q.AddJob<ApplyNewProductNotifyJob>(opts => opts.WithIdentity("ApplyNewProductNotifyJob"));

    // Create a trigger for the job
    q.AddTrigger(opts => opts
        .ForJob("ProductQuantityNotifyJob")
        .WithIdentity("ProductQuantityNotifyTrigger")
        .WithCronSchedule("0 30 6 * * ?")
    );
    q.AddTrigger(opts => opts
        .ForJob("NearExpiredQuantityNotifyJob")
        .WithIdentity("NearExpiredQuantityNotifyTrigger")
        .WithCronSchedule("00 30 6 * * ?")
    );
    q.AddTrigger(opts => opts
        .ForJob("PurchaseNotifyJob")
        .WithIdentity("PurchaseNotifyTrigger")
        .WithCronSchedule("00 30 6 * * ?")
    );
    q.AddTrigger(opts => opts
        .ForJob("ApplyNewProductNotifyJob")
        .WithIdentity("ApplyNewProductNotifyTrigger")
        .WithCronSchedule("00 30 6 * * ?")
    );
});

// Add the Quartz Hosted Service
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();

// 記錄當前工作目錄
var currentDirectory = Directory.GetCurrentDirectory();
Log.Information($"目前工作目錄: {currentDirectory}");

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

