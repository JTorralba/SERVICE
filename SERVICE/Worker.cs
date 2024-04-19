using Serilog;

namespace SERVICE
{
    public class Worker : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //try
                //{
                //    int a = 10;
                //    int b = 0;
                //    int c = a / b;
                //}
                //catch (Exception E)
                //{
                //    Log.Error($"{E.Message}");
                //}

                Log.Verbose("Verbose");
                Log.Debug("Debug");
                Log.Warning("Warning");
                Log.Information("Information");
                Log.Error("Error");
                Log.Fatal("Fatal");

                Log.ForContext("Folder", "Data").Information("HelloWorld");

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
