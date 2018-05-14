using System;
using System.Text;
using System.Management.Automation;
using System.ComponentModel;

namespace PowerShell.Clipboard
{
    /// <summary>
    /// Provides a PowerShell cmdlet for setting the clipboard content.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "Clipboard", DefaultParameterSetName = "Unicode")]
    public sealed class SetClipboardCommand : ClipboardFormatCommand
    {
        /// <summary>
        /// Gets or sets the value to set in the clipboard.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "Text", Position = 0)]
        [Parameter(Mandatory = true, ParameterSetName = "Unicode", Position = 0)]
        [Parameter(Mandatory = true, ParameterSetName = "HTML", Position = 0)]
        [Parameter(Mandatory = true, ParameterSetName = "XmlSpreadsheet", Position = 0)]
        [Parameter(Mandatory = true, ParameterSetName = "RTF", Position = 0)]
        [Parameter(Mandatory = true, ParameterSetName = "CSV", Position = 0)]
        public string Value
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the  list of files to set on the clipboard.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "FileList", Position = 0)]
        [ValidateNotNull()]
        public string[] Values
        {
            get;
            set;
        }

        /// <summary>
        /// Appends to the clipboard.
        /// The Default is to clear the clipboard before setting.
        /// </summary>
        [Parameter()]
        public SwitchParameter Append
        {
            get;
            set;
        }

        protected override void EndProcessing()
        {
            do
            {
                if (!Append.IsPresent)
                {
                    Clipboard.Clear();
                }

                if (FileList.IsPresent)
                {
                    try
                    {
                        Clipboard.SetFileList(Values);
                    }
                    catch (Win32Exception ex)
                    {
                        WriteError(ex, "SetClipboard.SetFileList", ErrorCategory.WriteError);
                    }
                    break;
                }

                ClipboardTextFormat textFormat = GetSelectedTextFormat();
                if (textFormat == null)
                {
                    textFormat = ClipboardTextFormat.Unicode;
                }

                try
                {
                    Clipboard.SetText(Value, textFormat);
                }
                catch (Win32Exception ex)
                {
                    WriteError(ex, "SetClipboard.SetText", ErrorCategory.WriteError);
                }

            } while (false);

            base.EndProcessing();
        }
    }
}

