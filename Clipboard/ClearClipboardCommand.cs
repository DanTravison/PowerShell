using System.ComponentModel;
using System.Management.Automation;

namespace PowerShell.Clipboard
{
    /// <summary>
    /// Provides a PowerShell cmdlet for clearing the clipboard.
    /// </summary>
    [Cmdlet(VerbsCommon.Clear, "Clipboard")]
    public sealed class ClearClipboardCommand : ClipboardCommand
    {
        protected override void EndProcessing()
        {
            try
            {
                Clipboard.Clear();
            }
            catch (Win32Exception ex)
            {
                WriteError(ex, "Clipboard.Clear", ErrorCategory.NotSpecified);
            }
            base.EndProcessing();
        }
    }
}

