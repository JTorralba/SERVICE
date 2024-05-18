using Serilog;
using SERVICE;

int Retention = 365;

var Configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        //.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
        .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(Configuration)
    .WriteTo.Map("Folder", "System", (Folder, WT) =>
        WT.File(@$"log\{Folder}\.log", rollingInterval: RollingInterval.Minute, retainedFileTimeLimit: TimeSpan.FromDays(Retention), shared: true,
        outputTemplate: "[{Level:u3}] {Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Message}{NewLine}"
        )
    )
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
