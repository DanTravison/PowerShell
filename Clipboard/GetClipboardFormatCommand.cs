using System.Management.Automation;

namespace PowerShell.Clipboard
{
    /// <summary>
    /// Provides a PowerShell cmdlet for retrieving the supported formats of the clipboard contents.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "ClipboardFormat")]
    public sealed class GetClipboardFormatCommand : ClipboardCommand
    {
        protected override void EndProcessing()
        {
            foreach (ClipboardFormat format in Clipboard.GetFormats())
            {
                WriteObject(format);
            }
            base.EndProcessing();
        }
    }
}

