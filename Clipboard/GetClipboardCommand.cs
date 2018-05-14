using System.ComponentModel;
using System.Globalization;
using System.Management.Automation;

namespace PowerShell.Clipboard
{
    /// <summary>
    /// Provides a PowerShell cmdlet for retrieving the clipboard content.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "Clipboard", DefaultParameterSetName = "Text")]
    public sealed class GetClipboardCommand : ClipboardFormatCommand
    {
        /// <summary>
        /// Clipboard formats search order when no explicit format is specified.
        /// The order attempts to use more specific formatting to less specific formatting to 
        /// avoid loss of formatting.
        /// </summary>
        static ClipboardFormat[] _defaultFormats =
        {
            ClipboardTextFormat.XmlSpreadsheet,
            ClipboardTextFormat.HTML,
            ClipboardTextFormat.CSV,
            ClipboardTextFormat.Unicode,
            ClipboardTextFormat.RTF
        };

        /// <summary>
        /// Overrides the expected character size for CSV to use Unicode.
        /// </summary>
        [Parameter(ParameterSetName = "CSV")]
        public SwitchParameter AsUnicode
        {
            get;
            set;
        }

        protected override void EndProcessing()
        {
            do
            {
                if (FileList.IsPresent)
                {
                    GetFileList(writeWarning:true);
                    break;
                }
                ClipboardTextFormat textFormat = GetSelectedTextFormat();

                if (textFormat != null)
                {
                    if (textFormat.TextFormat == TextDataFormat.CommaSeparatedValue && AsUnicode.IsPresent)
                    {
                        textFormat = ClipboardTextFormat.CSVUnicode;
                    }
                    GetText(textFormat, writeWarning: true);
                    break;
                }

                //
                // If no explicit format is specified; search for supported formats
                //

                if (GetFileList(writeWarning:false))
                {
                    break;
                }

                bool found = false;
                foreach (ClipboardTextFormat format in _defaultFormats)
                {
                    if (GetText(format, writeWarning:false))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    WriteWarning("The clipboard does not contain a supported clipboard format");
                }
            } while (false);

            base.EndProcessing();
        }

        bool GetText(ClipboardTextFormat format, bool writeWarning)
        {
            string value = null;

            try
            {
                if (Clipboard.Contains(format))
                {
                    value = Clipboard.GetTextData(format);
                    if (value != null)
                    {
                        WriteVerbose(string.Format(CultureInfo.InvariantCulture, "Found {0} text", format.Name));
                        WriteObject(value);
                        return true;
                    }
                }
                else if (writeWarning)
                {
                    WriteWarning(string.Format(CultureInfo.InvariantCulture, "The clipboard does not contain {0} data", format.Name));
                }
            }
            catch (Win32Exception ex)
            {
                base.WriteError(ex, "GetClipboard.GetText", ErrorCategory.ResourceUnavailable);
            }
            return false;
        }

        bool GetFileList(bool writeWarning)
        {
            try
            {
                if (Clipboard.Contains(ClipboardTextFormat.FileList))
                {
                    string[] values = Clipboard.GetFileList();
                    foreach (string value in values)
                    {
                        WriteObject(value);
                    }
                    return true;
                }

                if (writeWarning)
                {
                    WriteWarning("The clipboard does not contain a file list");
                }
            }
            catch (Win32Exception ex)
            {
                base.WriteError(ex, "GetClipboard.GetFileList", ErrorCategory.ResourceUnavailable);
            }
            return false;
        }
    }   
}
