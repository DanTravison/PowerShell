enum Platform
{
    Unknown = '0';
    Windows = '1';
    Linux = '2';
    OSX = '3';
}

function Get-Platform
{
    [Platform] $value = [Platform]::Unknown

    if ($PSHOME.EndsWith('\WindowsPowerShell\v1.0', [System.StringComparison]::OrdinalIgnoreCase))
    {
       $value = [Platform]::Windows
    }
    elseif ((Get-Variable -Name IsWindows -ErrorAction Ignore) -and $IsWindows)
    {
        $value = [Platform]::Windows
    }
    elseif ((Get-Variable -Name IsLinux -ErrorAction Ignore) -and $IsLinux)
    {
        $value = [Platform]::Linux
    }
    elseif ((Get-Variable -Name IsOSX -ErrorAction Ignore) -and $IsOSX)
    {
        $value = [Platform]::OSX
    }
    return $value
}

[Platform] $Platform = Get-Platform

# Define a type to encapsulate ShellExecuteEx with the 'runas' verb
# This is only used on Windows.
[string] $elevate = @"
using System;
using System.Runtime.InteropServices;
using System.Globalization;
using System.ComponentModel;

namespace Internal.Interop
{
    public static class Elevate
    {
        [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        class ShellExecuteInfo
        {
            public int cbSize = 0;
            public int nMask = 0;
            public IntPtr hwnd = IntPtr.Zero;
            public IntPtr lpVerb = IntPtr.Zero;
            public IntPtr lpFile = IntPtr.Zero;
            public IntPtr lpParameters = IntPtr.Zero;
            public IntPtr lpDirectory = IntPtr.Zero;
            public int nShow = 0;
            public IntPtr hInstApp = IntPtr.Zero;
            public IntPtr lpIDList = IntPtr.Zero;
            public IntPtr lpClass = IntPtr.Zero;
            public IntPtr hkeyClass = IntPtr.Zero;
            public int dwHotKey = 0;
            public IntPtr hIcon = IntPtr.Zero;
            public IntPtr hProcess = IntPtr.Zero;

            public ShellExecuteInfo()
            {
                cbSize = Marshal.SizeOf(this);
            }
        }

        public static void Run(string file, string directory, string parameters, bool showWindow)
        {
            ShellExecuteInfo info = new ShellExecuteInfo();
            info.lpFile = Marshal.StringToHGlobalUni(file);
            if (!string.IsNullOrEmpty(directory))
            {
                info.lpDirectory = Marshal.StringToHGlobalUni(directory);
            }

            if (!string.IsNullOrEmpty(parameters))
            {
                info.lpParameters = Marshal.StringToHGlobalUni(parameters);
            }
            info.lpVerb = Marshal.StringToHGlobalUni("runas");
            if (showWindow)
            {
                info.nShow = 1; // Normal
            }
            else
            {
                info.nShow = 0; // Hide
            }
            info.nMask = (int)0x00100000; // AsyncOK
            if (!ShellExecuteEx(info))
            {
                int errorCode = Marshal.GetLastWin32Error();
                string message = string.Format(CultureInfo.InvariantCulture, "Error launching program: {0} {1}", file, parameters);
                throw new Win32Exception(errorCode, message);
            }
        }

        [DllImport("Shell32", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool ShellExecuteEx(ShellExecuteInfo info);
    }
}
"@

if ($Platform -eq [Platform]::Windows)
{
    Add-Type -TypeDefinition $elevate
}

<#
.Synopsis
    Determines if the current process is elevated; on windows, or running from sudo, on Linux
.Example
    if (Test-Elevated)
    {
        # do something requiring elevated rights.
    }
#>
function Test-Elevated
{
    [CmdletBinding()]
    [OutputType([bool])]
    Param()

    if ($Platform -eq [Platform]::Windows)
    {
        # if the current Powershell session was called with administrator privileges,
        # the Administrator Group's well-known SID will show up in the Groups for the current identity.
        # Note that the SID won't show up unless the process is elevated.
        return (([Security.Principal.WindowsIdentity]::GetCurrent()).Groups -contains "S-1-5-32-544")
    }
    elseif($Platform -eq [Platform]::Linux)
    {
        return (Test-Path -Path "env:SUDO_USER")
    }

    throw "This function is not supported on this platform: $Platform"
}

function Invoke-WindowsElevated
{
    param
    (
        [string] $Command,
        [bool] $NoExit,
        [bool] $NoProfile,
        [bool] $ShowWindow,
        [bool] $isElevated
    )
    [System.Text.StringBuilder] $sb = [System.Text.StringBuilder]::new()
    if ($NoExit)
    {
        $null = $sb.Append(" -NoExit")
    }

    if ($NoProfile)
    {
        $null = $sb.Append(" -NoProfile")
    }

    if (-not [string]::IsNullOrEmpty($Command))
    {
        $null = $sb.Append(" -Command $Command")
    }
    else
    {
        $null = $sb.Append(" -NoExit")
    }

    if (-not $isElevated)
    {
        Write-Verbose -Message "Running $pshome\\powershell.exe $parameters"
        [bool] $showWindow = $ShowWindow.IsPresent -or $NoExit.IsPresent -or [string]::IsNullOrEmpty($Command)
        [Internal.Interop.Elevate]::Run("$pshome\\powershell.exe", $null, $parameters, $showWindow)
    }
    elseif ([string]::IsNullOrEmpty($parameters))
    {
        Start-Process -FilePath 'powershell'
    }
    else
    {
        Start-Process -FilePath 'powershell' -ArgumentList $parameters
    }
}

function Invoke-LinuxElevated
{
    param
    (
        [string] $Command,
        [bool] $NoProfile,
        [bool] $isElevated
    )
    if (-not $isElevated)
    {
        Write-Verbose -Message "Running sudo powershell $parameters"
        sudo powershell "$command"
    }
    elseif ([string]::IsNullOrEmpty($parameters))
    {
        Start-Process -FilePath 'powershell'
    }
    else
    {
        Start-Process -FilePath 'powershell' -ArgumentList $command
    }
}

<#
.Synopsis
    Invokes the specified command and parameters in an elevated PowerShell process.
.Description
    This command launches a PowerShell process elevated and executes the specified command as a script block.
    The command is always run in a second PowerShell process; even when the current process is elevated.
    Output from the command is not streamed back to the calling process.
    If the current process is already elevated, this command calls Start-Process directly.
.Parameter Command
    The optional command or expression to execute.
    The command should be placed within double quotes.
    On Windows, if no command is provided, a PowerShell process is launched elevated with -NoExit
.Parameter NoExit
    Causes the launched PowerShell process to not exit
    This parameter is only used on Windows
    On Windows, NoExit causes ShowWindow to be set.
.Parameter NoProfile
    Disables loading of the user's PowerShell profile.
.Parameter ShowWindow
    One Windows, displays a window for the launched process.
    This parameter should be used when the launched process requires interaction.
    This parameter is only used on Windows
.Example
    Start-Elevated -command "Remove-Item -Path c:\windows\*.dmp" -NoProfile
    Removes any .dmp files from the windows directory and supresses loading the user's PowerShell profile
.Example
    Start-Elevated -command "Get-Item c:\foo\*.*" -NoExit
    Launches PowerShell to list the contents of the c:\foo directory and does not exit the process when done.
.Example
    Start-Elevated
    Launches PowerShell as an elevated, interactive process.
#>
function Start-Elevated
{
    [CmdletBinding(DefaultParameterSetName='Command')]
    param
    (
        [Parameter(ParameterSetName='Command')]
        [string] $Command,

        [switch] $NoExit,
        [switch] $NoProfile,
        [switch] $ShowWindow
    )
    [bool] $isElevated = Test-Elevated
    if ($Platform -eq [Platform]::Windows)
    {
        Invoke-WindowsElevated -NoProfile $NoProfile.IsPresent -NoExit $NoExit.IsPresent -ShowWindow $ShowWindow.IsPresent -Command $command
    }
    elseif ($Platform -eq [Platform]::Linux)
    {
        Invoke-LinuxElevated -NoProfile $NoProfile.IsPresent -Command $command
    }
}

Export-ModuleMember -Function Test-Elevated, Start-Elevated
