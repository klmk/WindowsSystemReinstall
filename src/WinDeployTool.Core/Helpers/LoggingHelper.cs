using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace WinDeployTool.Core.Helpers
{
    /// <summary>
    /// 日志配置助手
    /// </summary>
    public static class LoggingHelper
    {
        private static ILogger? _logger;

        /// <summary>
        /// 配置日志
        /// </summary>
        public static void ConfigureLogging(string logDirectory)
        {
            Directory.CreateDirectory(logDirectory);

            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                    theme: AnsiConsoleTheme.Code)
                .WriteTo.File(
                    path: Path.Combine(logDirectory, "Deploy-.log"),
                    rollingInterval: Serilog.RollingInterval.Day,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: Path.Combine(logDirectory, "Master.log"),
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{Phase}] [{Module}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Logger = _logger;
        }

        /// <summary>
        /// 获取Logger
        /// </summary>
        public static ILogger GetLogger<T>()
        {
            return _logger?.ForContext<T>() ?? Log.Logger;
        }

        /// <summary>
        /// 获取带上下文的Logger
        /// </summary>
        public static ILogger GetLogger(string context)
        {
            return _logger?.ForContext("SourceContext", context) ?? Log.Logger;
        }

        /// <summary>
        /// 关闭日志
        /// </summary>
        public static void Close()
        {
            Log.CloseAndFlush();
        }
    }
}
