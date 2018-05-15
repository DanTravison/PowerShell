# PowerShell
Miscellaneous PowerShell modules and utilities.

# Elevate Module
This module is found in the Elevate directory.

The module provides the capability for starting a PowerShell process elevated.

On Windows, the process is started using ShellExecuteEx and causes a UAC prompt to be displayed.

On Linux, the process is started via calling sudo which prompts for a password before running powershell.

Note that Pipeline output is not returned to the calling PowerShell process.

## Windows Notes
By default, the launched PowerShell process is hidden and exits as soon as the passed in command completes. To change this behaviour use the following switches:

* -ShowWindow: makes the window visible.
This is useful for running commands that require interaction. The process will exit when the command completes.

* -NoExit: Places PowerShell in interactive mode after the command completes.
This switch also sets the -ShowWindow switch.

* Interactive only
To launch a PowerShell process for interactive use, do not provide a -command parameter. Doing so implicitly sets the -NoExit and -ShowWindow switches.

# Clipboard Module
This module supports access to the clipboard in PowerShell Core supporting some of the common, text-base clipboard formats including:

* Unicode
* Text
* HTML
* XmlSpreadSheet
* RTF
* CSF
* FileList (HDROP)

The Module defines the following commands:

| Command             | Description |
|---------------------|-------------|
| Clear-Clipboard     | Clears the clipboard
| Get-Clipboard       | Gets the clipboard in a specified format, if available. If no format is specified, the command attempts to choose the best format.
| Get-ClipboardFormat | Lists the formats supported by the current clipboard contents.
| Set-Clipboard       | Sets the contents of the clipboard to one of the above, supported formats.
