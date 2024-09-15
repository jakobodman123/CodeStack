using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.IO;

public partial class VpnTlsService : ServiceBase
{
    private Process openVpnProcess;
    private string configFilePath = @"C:\path\to\your\config.ovpn"; // Hardcoded path to your .ovpn file
    private string[] filterStrings = { "connected", "disconnected", "error" }; // Strings to filter for

    public VpnTlsService()
    {
        this.ServiceName = "VpnTlsService";
        this.EventLog.Source = this.ServiceName;
        this.CanStop = true;
        this.CanPauseAndContinue = false;
        this.AutoLog = true;

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

        // Check if the .ovpn file exists
        if (!File.Exists(configFilePath))
        {
            EventLog.WriteEntry($"No .ovpn file found at {configFilePath}.", EventLogEntryType.Error);
            this.Stop();
            return;
        }

        // Start OpenVPN process with the provided .ovpn config
        StartOpenVpnProcess();
    }

    protected override void OnStop()
    {
        // Log service stop
        EventLog.WriteEntry("Service stopped.", EventLogEntryType.Information);

        // Stop OpenVPN process if it's running
        if (openVpnProcess != null && !openVpnProcess.HasExited)
        {
            openVpnProcess.Kill();
        }

        base.OnStop();
    }

    private void StartOpenVpnProcess()
    {
        // Set up the OpenVPN process
        openVpnProcess = new Process();
        openVpnProcess.StartInfo.FileName = "openvpn"; // Assumes OpenVPN is in PATH
        openVpnProcess.StartInfo.Arguments = $"--config \"{configFilePath}\"";
        openVpnProcess.StartInfo.UseShellExecute = false;
        openVpnProcess.StartInfo.RedirectStandardOutput = true;
        openVpnProcess.StartInfo.RedirectStandardError = true;
        openVpnProcess.OutputDataReceived += new DataReceivedEventHandler(OpenVpnOutputHandler);
        openVpnProcess.ErrorDataReceived += new DataReceivedEventHandler(OpenVpnOutputHandler);

        openVpnProcess.Start();

        // Start reading the output asynchronously
        openVpnProcess.BeginOutputReadLine();
        openVpnProcess.BeginErrorReadLine();
    }

    private void OpenVpnOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (!string.IsNullOrEmpty(outLine.Data))
        {
            // Filter the output based on the specified strings
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

    public static void Main()
    {
        // Create and run the service without needing a config file argument
        ServiceBase.Run(new VpnTlsService());
    }
}
