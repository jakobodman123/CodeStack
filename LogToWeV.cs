# Define the paths to search
$paths = @(
    "$env:ProgramFiles",
    "$env:ProgramFiles(x86)",
    "$env:ProgramData"
)

# Get the SID of TrustedInstaller
$trustedInstaller = New-Object System.Security.Principal.NTAccount("NT SERVICE\TrustedInstaller")
$trustedInstallerSid = $trustedInstaller.Translate([System.Security.Principal.SecurityIdentifier]).Value

foreach ($path in $paths) {
    # Ensure the path exists
    if (Test-Path $path) {
        Get-ChildItem -Path $path -Directory -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
            $folder = $_
            try {
                # Get the ACL of the folder
                $acl = Get-Acl -LiteralPath $folder.FullName
                # Get the SID of the owner
                $ownerSid = ($acl.Owner).Translate([System.Security.Principal.SecurityIdentifier]).Value
                # Check if the owner is TrustedInstaller
                if ($ownerSid -eq $trustedInstallerSid) {
                    Write-Output $folder.FullName
                }
            }
            catch {
                Write-Warning "Could not get owner for $($folder.FullName): $_"
            }
        }
    } else {
        Write-Warning "Path not found: $path"
    }
}


# Define critical groups that must retain access
$criticalGroups = @(
    "BUILTIN\Administrators",
    "NT AUTHORITY\SYSTEM",
    "NT AUTHORITY\LOCAL SERVICE",
    "NT SERVICE\TrustedInstaller",
    "NT SERVICE\OfficeClickToRun",
    "APPLICATION PACKAGE AUTHORITY\ALL APPLICATION PACKAGES",
    "APPLICATION PACKAGE AUTHORITY\ALL RESTRICTED APPLICATION PACKAGES"
    # Add any additional service accounts here
)

# Include 'CREATOR OWNER' if beneficial
$includeCreatorOwner = $true
if ($includeCreatorOwner) {
    $criticalGroups += "CREATOR OWNER"
}

# Directories to modify
$directories = @(
    "$env:ProgramData",
    "$env:ProgramFiles",
    "$env:ProgramFiles(x86)"
)

# Directory to save original ACLs
$aclBackupDir = "$env:USERPROFILE\ACL_Backups"
if (!(Test-Path $aclBackupDir)) {
    New-Item -ItemType Directory -Path $aclBackupDir | Out-Null
}

# Function to sanitize file names
function Sanitize-FileName {
    param([string]$filename)
    return $filename -replace '[:\\/*?"<>|]', '_'
}

# Function to process ACLs
function Remove-NonCriticalACLs {
    param (
        [string]$Path
    )

    Write-Host "Processing: $Path"

    try {
        # Backup current ACL
        $acl = Get-Acl -Path $Path
        $sanitizedPath = Sanitize-FileName($Path)
        $aclBackupPath = Join-Path -Path $aclBackupDir -ChildPath ($sanitizedPath + ".acl")
        $acl | Export-Clixml -Path $aclBackupPath

        # Filter out non-critical groups
        $newAcl = $acl.Access | Where-Object { $criticalGroups -contains $_.IdentityReference.Value }

        # Clear all existing access rules
        $acl.Access | ForEach-Object { $acl.RemoveAccessRule($_) }

        # Re-add critical group rules
        $newAcl | ForEach-Object { $acl.AddAccessRule($_) }

        # Apply new ACL
        Set-Acl -Path $Path -AclObject $acl

        Write-Host "Updated ACL for: $Path"
    } catch {
        Write-Warning "Failed to process $Path: $_"
    }
}

# Process directories recursively
foreach ($dir in $directories) {
    if (Test-Path $dir) {
        Get-ChildItem -Path $dir -Recurse -Force | ForEach-Object {
            if ($_ -is [System.IO.FileInfo] -or $_ -is [System.IO.DirectoryInfo]) {
                Remove-NonCriticalACLs -Path $_.FullName
            }
        }
    } else {
        Write-Warning "Directory not found: $dir"
    }
}

Write-Host "ACL cleanup complete."

# Instructions to restore original ACLs:
# To restore the original ACLs, run the following script:
# Get all backup files in the ACL_Backups directory and restore them to their respective paths.

# Restore-OriginalACLs.ps1
# --------------------------
# $aclBackupDir = "$env:USERPROFILE\ACL_Backups"
# Get-ChildItem -Path $aclBackupDir -Filter "*.acl" | ForEach-Object {
#     $backupFile = $_.FullName
#     $sanitizedPath = $_.BaseName
#     $originalPath = $sanitizedPath -replace '_', '\'
#     $acl = Import-Clixml -Path $backupFile
#     try {
#         Set-Acl -Path $originalPath -AclObject $acl
#         Write-Host "Restored ACL for: $originalPath"
#     } catch {
#         Write-Warning "Failed to restore ACL for $originalPath: $_"
#     }
# }
# --------------------------
# Save the above code as 'Restore-OriginalACLs.ps1' and run it to restore the ACLs.

# Path: Set-NetworkConfig.ps1

# Define Variables
$InterfaceAlias = "Ethernet" # Replace with your actual interface name
$LogFile = "C:\Scripts\NetworkConfig.log" # Ensure this directory exists or modify as needed

# Function to Log Messages
function Write-Log {
    param (
        [string]$Message
    )
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Add-Content -Path $LogFile -Value "$timestamp - $Message"
}

# Check if the Ethernet cable is connected
$netAdapter = Get-NetAdapter -Name $InterfaceAlias -ErrorAction SilentlyContinue

if ($null -eq $netAdapter) {
    Write-Log "Network adapter '$InterfaceAlias' not found."
    exit 1
}

if ($netAdapter.Status -eq "Up") {
    Write-Log "Ethernet is connected. No changes required."
    exit 0
} else {
    Write-Log "Ethernet is disconnected. Applying static IP configurations."

    try {
        # Remove existing IP addresses (optional: to ensure no conflicts)
        $currentIPs = Get-NetIPAddress -InterfaceAlias $InterfaceAlias -AddressFamily IPv4
        foreach ($ip in $currentIPs) {
            Remove-NetIPAddress -InterfaceAlias $InterfaceAlias -IPAddress $ip.IPAddress -Confirm:$false
            Write-Log "Removed existing IP address: $($ip.IPAddress)"
        }

        # Set Primary IP Address
        New-NetIPAddress -InterfaceAlias $InterfaceAlias -IPAddress "10.8.2.1" -PrefixLength 24 -DefaultGateway "10.8.2.254" -ErrorAction Stop
        Write-Log "Set Primary IP: 10.8.2.1/24 with Gateway: 10.8.2.254"

        # Add Secondary IP Address
        New-NetIPAddress -InterfaceAlias $InterfaceAlias -IPAddress "10.83.10.2" -PrefixLength 16 -ErrorAction Stop
        Write-Log "Added Secondary IP: 10.83.10.2/16"

        # (Optional) Set DNS Servers if required
        # Set-DnsClientServerAddress -InterfaceAlias $InterfaceAlias -ServerAddresses "8.8.8.8","8.8.4.4"
        # Write-Log "Configured DNS Servers: 8.8.8.8, 8.8.4.4"

        Write-Log "Static IP configurations applied successfully."
    }
    catch {
        Write-Log "Error applying network configurations: $_"
        exit 1
    }
}

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
