using System.Management.Automation;

namespace PowerShell.Clipboard
{
    /// <summary>
    /// Provides a base class for the set and get clipboard commands.
    /// </summary>
    public abstract class ClipboardFormatCommand : ClipboardCommand
    {
        /// <summary>
        /// Gets or sets the expected clipboard format to <see cref="ClipboardTextFormat.Text"/>.
        /// </summary>
        [Parameter(ParameterSetName = "Text")]
        public SwitchParameter Text
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the expected clipboard format to <see cref="ClipboardTextFormat.Unicode"/>.
        /// </summary>
        [Parameter(ParameterSetName = "Unicode")]
        public SwitchParameter Unicode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the expected clipboard format to <see cref="ClipboardTextFormat.HTML"/>.
        /// </summary>
        [Parameter(ParameterSetName = "HTML")]
        public SwitchParameter HTML
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the expected clipboard format to <see cref="ClipboardTextFormat.XmlSpreadsheet"/>.
        /// </summary>
        [Parameter(ParameterSetName = "XmlSpreadsheet")]
        public SwitchParameter XmlSpreadsheet
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the expected clipboard format to <see cref="ClipboardTextFormat.RTF"/>.
        /// </summary>
        [Parameter(ParameterSetName = "RTF")]
        public SwitchParameter RTF
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the expected clipboard format to <see cref="ClipboardTextFormat.CSV"/>.
        /// </summary>
        [Parameter(ParameterSetName = "CSV")]
        public SwitchParameter CSV
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the expected clipboard format to <see cref="ClipboardFormat.FileList"/>.
        /// </summary>
        [Parameter(ParameterSetName = "FileList")]
        public SwitchParameter FileList
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the <see cref="ClipboardTextFormat"/> for the associated parameter
        /// </summary>
        /// <returns>A <see cref="ClipboardTextFormat"/> for the associated parameter; otherwise, a null reference if none of the text format parameters was selected.</returns>
        protected ClipboardTextFormat GetSelectedTextFormat()
        {
            if (Text.IsPresent)
            {
                return ClipboardTextFormat.Text;
            }
            if (HTML.IsPresent)
            {
                return ClipboardTextFormat.HTML;
            }
            if (XmlSpreadsheet.IsPresent)
            {
                return ClipboardTextFormat.XmlSpreadsheet;
            }
            if (RTF.IsPresent)
            {
                return ClipboardTextFormat.RTF;
            }
            if (CSV.IsPresent)
            {
                return ClipboardTextFormat.CSV;
            }

            if (Unicode.IsPresent)
            {
                return ClipboardTextFormat.Unicode;
            }
            return null;
        }
    }
}
