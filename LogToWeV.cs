using System;
using System.IO;
using System.ServiceProcess;
using System.Diagnostics;

public class OpenVpnService : ServiceBase
{
    private string configFilePath;
    private FileSystemWatcher logWatcher;
    private string[] filterStrings = { "connected", "disconnected", "error" }; // Strings to filter for

    public OpenVpnService(string configFile)
    {
        this.ServiceName = "OpenVpnService";
        this.EventLog.Source = this.ServiceName;
        this.CanStop = true;
        this.CanPauseAndContinue = false;
        this.AutoLog = true;
        this.configFilePath = configFile;

        // Create the event source if it doesn't exist
        if (!EventLog.SourceExists(this.ServiceName))
        {
            EventLog.CreateEventSource(this.ServiceName, "Application");
        }
    }

    protected override void OnStart(string[] args)
    {
        base.OnStart(args);

        // Log service start
        EventLog.WriteEntry("Service started.", EventLogEntryType.Information);

        // Start watching the log file (you can replace this with actual OpenVPN log process if required)
        string logFilePath = Path.Combine(Path.GetDirectoryName(configFilePath), "openvpn.log");
        logWatcher = new FileSystemWatcher(Path.GetDirectoryName(logFilePath))
        {
            Filter = Path.GetFileName(logFilePath),
            NotifyFilter = NotifyFilters.LastWrite
        };

        logWatcher.Changed += OnLogFileChanged;
        logWatcher.EnableRaisingEvents = true;
    }

    protected override void OnStop()
    {
        // Log service stop
        EventLog.WriteEntry("Service stopped.", EventLogEntryType.Information);

        logWatcher.EnableRaisingEvents = false;
        logWatcher.Dispose();
        base.OnStop();
    }

    private void OnLogFileChanged(object sender, FileSystemEventArgs e)
    {
        // Read the log file and filter for specific strings
        string[] logLines = File.ReadAllLines(e.FullPath);
        foreach (var line in logLines)
        {
            foreach (var filter in filterStrings)
            {
                if (line.Contains(filter))
                {
                    EventLog.WriteEntry($"Filtered log event: {line}", EventLogEntryType.Information);
                    break;
                }
            }
        }
    }

    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Please provide the OpenVPN config file path as an argument.");
            return;
        }

        // Create and run the service
        ServiceBase.Run(new OpenVpnService(args[0]));
    }
}
