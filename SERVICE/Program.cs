using SERVICE;

using Serilog;
using Serilog.Events;

int Retention = 365;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()

    .WriteTo.Console(
        outputTemplate:
            "[{Level:u3}] {Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Message}{NewLine}")

    .WriteTo.File(@"log\.log", rollingInterval:RollingInterval.Minute, retainedFileTimeLimit: TimeSpan.FromHours(Retention),
        outputTemplate: "[{Level:u3}] {Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Message}{NewLine}")

    .WriteTo.Logger(LC => LC
        .Filter.ByIncludingOnly(Event => Event.Level == LogEventLevel.Debug)
        .WriteTo.Map<string>(
            File => "Debug",
                 (Folder, WT) =>
                    WT.File(@$"log\{Folder}\.log", rollingInterval: RollingInterval.Minute, retainedFileTimeLimit: TimeSpan.FromHours(Retention), shared: true,
                    outputTemplate: "[{Level:u3}] {Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Message}{NewLine}"
                 )
        )
    )

    .WriteTo.Logger(LC => LC
        .Filter.ByIncludingOnly(Event => Event.Level == LogEventLevel.Verbose)
        .WriteTo.Map<string>(
            File => "Verbose",
                 (Folder, WT) =>
                    WT.File(@$"log\{Folder}\.log", rollingInterval: RollingInterval.Minute, retainedFileTimeLimit: TimeSpan.FromHours(Retention), shared: true,
                    outputTemplate: "[{Level:u3}] {Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Message}{NewLine}"
                 )
        )
    )

    .WriteTo.Logger(LC => LC
        .Filter.ByIncludingOnly(Event => Event.Level == LogEventLevel.Warning)
        .WriteTo.Map<string>(
            File => "Warning",
                 (Folder, WT) =>
                    WT.File(@$"log\{Folder}\.log", rollingInterval: RollingInterval.Minute, retainedFileTimeLimit: TimeSpan.FromHours(Retention), shared: true,
                    outputTemplate: "[{Level:u3}] {Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Message}{NewLine}"
                 )
        )
    )

    .WriteTo.Logger(LC => LC
        .Filter.ByIncludingOnly(Event => Event.Level == LogEventLevel.Information)
        .WriteTo.Map<string>(
            File => "Information",
                 (Folder, WT) =>
                    WT.File(@$"log\{Folder}\.log", rollingInterval: RollingInterval.Minute, retainedFileTimeLimit: TimeSpan.FromHours(Retention), shared: true,
                    outputTemplate: "[{Level:u3}] {Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Message}{NewLine}"
                 )
        )
    )

    .WriteTo.Logger(LC => LC
        .Filter.ByIncludingOnly(Event => Event.Level == LogEventLevel.Error)
        .WriteTo.Map<string>(
            File => "Error",
                 (Folder, WT) =>
                    WT.File(@$"log\{Folder}\.log", rollingInterval: RollingInterval.Minute, retainedFileTimeLimit: TimeSpan.FromHours(Retention), shared: true,
                    outputTemplate: "[{Level:u3}] {Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Message}{NewLine}"
                 )
        )
    )

    .WriteTo.Logger(LC => LC
        .Filter.ByIncludingOnly(Event => Event.Level == LogEventLevel.Fatal)
        .WriteTo.Map<string>(
            File => "Fatal",
                 (Folder, WT) =>
                    WT.File(@$"log\{Folder}\.log", rollingInterval: RollingInterval.Minute, retainedFileTimeLimit: TimeSpan.FromHours(Retention), shared: true,
                    outputTemplate: "[{Level:u3}] {Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Message}{NewLine}"
                 )
        )
    )

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
