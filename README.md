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

* Interctive only
To launch a PowerShell process for interactive use, do not provide a -command parameter. Doing so implicitly sets the -NoExit and -ShowWindow switches. 


