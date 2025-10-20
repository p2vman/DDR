using NLog;
using NLog.Config;
using NLog.Targets;

namespace DDR;


public class Program
{
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    public static void Main(string[] args)
    {
        var config = new LoggingConfiguration();
        
        var consoleTarget = new ColoredConsoleTarget("console")
        {
            Layout = "[${date:format=HH\\:mm\\:ss}]:[${logger}]:[${thread}]:[${level:uppercase=true}]: ${message} ${exception:format=toString}"
        };
        
        var fileTarget = new FileTarget("file")
        {
            FileName = "logs/app-${shortdate}.log",
            Layout = "[${longdate}]:[${logger}]:[${thread}]:[${level:uppercase=true}]:${message}${onexception:${newline}${exception:format=tostring}}",
            ArchiveEvery = FileArchivePeriod.Day,
            MaxArchiveFiles = 7,
            KeepFileOpen = false
        };
        
        config.AddTarget(consoleTarget);
        config.AddTarget(fileTarget);
        
        config.AddRule(LogLevel.Debug, LogLevel.Fatal, consoleTarget);
        config.AddRule(LogLevel.Info, LogLevel.Fatal, fileTarget);
        
        LogManager.Configuration = config;
        
        Game game = new Game();
        
        
        game.Run();
    }
}