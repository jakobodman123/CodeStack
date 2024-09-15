using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.IO;
using System.Collections.Generic;

public partial class VpnTlsService : ServiceBase
{
    private List<Process> openVpnProcesses = new List<Process>(); // Store all OpenVPN processes
    private string configFolderPath = @"C:\path\to\config\folder"; // Path to folder containing .ovpn files
    private string[] filterStrings = { "connected", "disconnected", "error" }; // Strings to filter for in logs

    public VpnTlsService()
    {
        this.ServiceName = "VpnTlsService";
        this.EventLog.Source = this.ServiceName;
        this.CanStop = true;
        this.CanPauseAndContinue = false;
        this.AutoLog = true;

        if (!EventLog.SourceExists(this.ServiceName))
        {
            EventLog.CreateEventSource(this.ServiceName, "Application");
        }
    }

    protected override void OnStart(string[] args)
    {
        base.OnStart(args);
        EventLog.WriteEntry("Service started.", EventLogEntryType.Information);

        if (!Directory.Exists(configFolderPath))
        {
            EventLog.WriteEntry($"Config folder not found at {configFolderPath}.", EventLogEntryType.Error);
            this.Stop();
            return;
        }

        // Find all .ovpn files in the config folder
        string[] ovpnFiles = Directory.GetFiles(configFolderPath, "*.ovpn");
        if (ovpnFiles.Length == 0)
        {
            EventLog.WriteEntry("No .ovpn files found.", EventLogEntryType.Error);
            this.Stop();
            return;
        }

        // Start OpenVPN process for each .ovpn file
        foreach (var ovpnFile in ovpnFiles)
        {
            StartOpenVpnProcess(ovpnFile);
        }
    }

    protected override void OnStop()
    {
        EventLog.WriteEntry("Service stopped.", EventLogEntryType.Information);

        // Stop all running OpenVPN processes
        foreach (var process in openVpnProcesses)
        {
            if (process != null && !process.HasExited)
            {
                process.Kill();
            }
        }

        base.OnStop();
    }

    private void StartOpenVpnProcess(string ovpnFile)
    {
        string openVpnExePath = FindOpenVpnExecutable();
        if (openVpnExePath == null)
        {
            EventLog.WriteEntry("OpenVPN executable not found.", EventLogEntryType.Error);
            return;
        }

        // Set up OpenVPN process for each .ovpn file
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = openVpnExePath,
                Arguments = $"--config \"{ovpnFile}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Path.GetDirectoryName(ovpnFile) // Set CWD to the folder of the .ovpn file
            }
        };

        process.OutputDataReceived += new DataReceivedEventHandler(OpenVpnOutputHandler);
        process.ErrorDataReceived += new DataReceivedEventHandler(OpenVpnOutputHandler);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        openVpnProcesses.Add(process); // Keep track of the process
    }

    private void OpenVpnOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (!string.IsNullOrEmpty(outLine.Data))
        {
            foreach (var filter in filterStrings)
            {
                if (outLine.Data.Contains(filter))
                {
                    EventLog.WriteEntry($"Filtered OpenVPN event: {outLine.Data}", EventLogEntryType.Information);
                    break;
                }
            }
        }
    }

    private string FindOpenVpnExecutable()
    {
        string[] openVpnExePaths = { 
            @"C:\Program Files\OpenVPN\bin\openvpn.exe", 
            @"C:\Program Files (x86)\OpenVPN\bin\openvpn.exe",
            @"D:\OpenVPN\bin\openvpn.exe",
            @"E:\Programs\OpenVPN\openvpn.exe"
        };

        foreach (var path in openVpnExePaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }
        return null;
    }

    public static void Main()
    {
        ServiceBase.Run(new VpnTlsService());
    }
}
