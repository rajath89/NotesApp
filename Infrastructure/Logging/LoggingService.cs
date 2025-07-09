namespace Infrastructure.Logging;

using Serilog;
using Serilog.Events;

public static class LoggingService 
{
    public static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.File("logs/note-taking-app-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }
}