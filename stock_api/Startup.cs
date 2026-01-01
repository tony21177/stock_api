using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Serilog;
using Serilog.Enrichers.CallerInfo;
using stock_api.Common.IoC.Configuration.DI;
using stock_api.Models;
using stock_api.Scheduler;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Transactions;

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

// [Response Compression] 1. 註冊 Response Compression 服務
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true; // 允許 HTTPS 使用壓縮
    options.Providers.Add<BrotliCompressionProvider>(); // 優先使用 Brotli (壓縮率更高)
    options.Providers.Add<GzipCompressionProvider>();   // 備用 Gzip
    // 指定要壓縮的 MIME 類型
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "application/json; charset=utf-8" });
});

// [Response Compression] 2. 設定 Brotli 壓縮層級
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    // Fastest: 速度最快 (CPU 負擔較小，適合 API)
    // Optimal: 平衡速度與壓縮比
    // SmallestSize: 壓縮比最高 (CPU 負擔較大)
    options.Level = CompressionLevel.Fastest;
});

// [Response Compression] 3. 設定 Gzip 壓縮層級 (備用)
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// 註冊 MemoryCache 服務
builder.Services.AddMemoryCache();

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
// Register DbContext. Set options lifetime to Singleton so IDbContextFactory (registered as singleton) can consume options.
builder.Services.AddDbContext<StockDbContext>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), serverVersion);
    options.UseLoggerFactory(serilogLoggerFactory)
           .EnableSensitiveDataLogging();
}, contextLifetime: ServiceLifetime.Scoped, optionsLifetime: ServiceLifetime.Singleton);

// Register IDbContextFactory to allow creating DbContext instances for parallel/async queries
builder.Services.AddDbContextFactory<StockDbContext>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), serverVersion);
    options.UseLoggerFactory(serilogLoggerFactory)
           .EnableSensitiveDataLogging();
});

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
        .WithCronSchedule("00 50 9 * * ?")
    );
});

// Add the Quartz Hosted Service
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();

// Log effective connection string (masked) to confirm what the app uses
try
{
    var effectiveConn = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
    string MaskConnectionString(string cs)
    {
        if (string.IsNullOrEmpty(cs)) return cs;
        try
        {
            var parts = cs.Split(';');
            for (int i = 0; i < parts.Length; i++)
            {
                var kvPos = parts[i].IndexOf('=');
                if (kvPos <= 0) continue;
                var key = parts[i].Substring(0, kvPos).Trim().ToLowerInvariant();
                if (key.Contains("password") || key == "pwd" || key == "user id" || key == "uid")
                {
                    var k = parts[i].Substring(0, kvPos + 1);
                    parts[i] = k + "***";
                }
            }
            return string.Join(';', parts);
        }
        catch
        {
            return "(unable to mask connstr)";
        }
    }

    Log.Information("Using DefaultConnection (masked): {conn}", MaskConnectionString(effectiveConn));
}
catch (Exception ex)
{
    Log.Warning(ex, "Failed to read or log connection string");
}

// Optional: warm-up connection pool to pre-create connections
try
{
    var warmupSize = builder.Configuration.GetValue<int?>("ConnectionPoolWarmup:Size") ?? 0;
    if (warmupSize > 0)
    {
        Log.Information("Starting DB connection pool warm-up, size={size}", warmupSize);
        using (var scope = app.Services.CreateScope())
        {
            var factory = scope.ServiceProvider.GetService<IDbContextFactory<stock_api.Models.StockDbContext>>();
            if (factory != null)
            {
                for (int i = 0; i < warmupSize; i++)
                {
                    try
                    {
                        using var ctx = factory.CreateDbContext();
                        ctx.Database.OpenConnection();
                        // quick small query to ensure connection is established and authenticated
                        using (var cmd = ctx.Database.GetDbConnection().CreateCommand())
                        {
                            cmd.CommandText = "SELECT 1";
                            cmd.ExecuteScalar();
                        }
                        ctx.Database.CloseConnection();
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Warm-up connection {i} failed", i);
                    }
                }
                Log.Information("DB connection pool warm-up completed");
            }
            else
            {
                Log.Information("IDbContextFactory<StockDbContext> not registered; skipping warm-up");
            }
        }
    }
}
catch (Exception ex)
{
    Log.Warning(ex, "Failed during DB warm-up procedure");
}

app.UseMiddleware<RequestLoggingMiddleware>();

// 記錄當前工作目錄
var currentDirectory = Directory.GetCurrentDirectory();
Log.Information($"目前工作目錄: {currentDirectory}");

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors();

// [Gzip 新增] 3. 啟用 Middleware
// 必須放在 UseCors 之後，但在 MapControllers 之前
app.UseResponseCompression();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

