using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerShell.Clipboard
{
    /// <summary>
    /// Provides a class for supported clipboard text formats.
    /// </summary>
    public class ClipboardTextFormat : ClipboardFormat
    {
        static Dictionary<TextDataFormat, ClipboardFormat> _textFormats = new Dictionary<TextDataFormat, ClipboardFormat>();

        static ClipboardTextFormat()
        {
            InitializeTextFormats();
        }

        static internal void InitializeTextFormats()
        {
            if (_textFormats.Count == 0)
            {
                HTML = GetFormat("HTML Format", TextDataFormat.Html, characterSize: 1);
                RTF = GetFormat("Rich Text Format", TextDataFormat.Rtf, characterSize: 1);
                CSV = GetFormat("CSV", TextDataFormat.CommaSeparatedValue, characterSize: 1);
                CSVUnicode = new ClipboardTextFormat(CSV, "CSVUnicode")
                {
                    CharacterSize = 2
                };
                XmlSpreadsheet = GetFormat("Xml Spreadsheet", TextDataFormat.XmlSpreadsheet, characterSize: 1);
                Unicode = GetFormat(NativeMethods.Format.Unicode, TextDataFormat.Unicode, characterSize: 2);
                Text = GetFormat(NativeMethods.Format.Text, TextDataFormat.Text, characterSize: 1);
                FileList = GetFormat(NativeMethods.Format.HDrop, TextDataFormat.FileList, characterSize: 2);
            }
        }

        private ClipboardTextFormat (ClipboardTextFormat source, string name)
            : base (name, source.Id)
        {
            TextFormat = source.TextFormat;
            CharacterSize = source.CharacterSize;
        }

        private ClipboardTextFormat(string name, TextDataFormat textDataFormat, int characterSize, uint id)
            : base(name, id)
        {
            TextFormat = textDataFormat;
            CharacterSize = characterSize;

        }

        #region GetFormat

        static private void UpdateFormatTables(ClipboardTextFormat format)
        {
            _textFormats.Add(format.TextFormat, format);
            _ids[format.Id] = format;
            _names[format.Name] = format;
        }
        static private ClipboardTextFormat GetFormat(NativeMethods.Format format, TextDataFormat textDataFormat, int characterSize)
        {
            ClipboardFormat clipboardFormat = GetFormat((uint)format);

            var clipboardTextFormat = new ClipboardTextFormat(clipboardFormat.Name, textDataFormat, characterSize, clipboardFormat.Id);
            UpdateFormatTables(clipboardTextFormat);
            return clipboardTextFormat;
        }

        static private ClipboardTextFormat GetFormat(string name, TextDataFormat textDataFormat, int characterSize)
        {
            ClipboardFormat clipboardFormat = GetFormat(name);

            var clipboardTextFormat = new ClipboardTextFormat(name, textDataFormat, characterSize, clipboardFormat.Id);
            UpdateFormatTables(clipboardTextFormat);
            return clipboardTextFormat;
        }

        #endregion GetFormat

        #region Static Properties

        /// <summary>
        /// Gets the Unicode registered clipboard format.
        /// </summary>
        /// <value>
        /// A <see cref="ClipboardTextFormat"/> for Unicode clipboard format.
        /// </value>
        public static ClipboardTextFormat Unicode
        {
            get; private set;
        }

        /// <summary>
        /// Gets the Text registered clipboard format.
        /// </summary>
        /// <value>
        /// A <see cref="ClipboardTextFormat"/> for Text clipboard format.
        /// </value>
        public static ClipboardTextFormat Text
        {
            get; private set;
        }

        /// <summary>
        /// Gets the RTF registered clipboard format.
        /// </summary>
        /// <value>
        /// A <see cref="ClipboardTextFormat"/> for Rich Text Format clipboard format.
        /// </value>
        public static ClipboardTextFormat RTF
        {
            get; private set;
        }

        /// <summary>
        /// Gets the HTML registered clipboard format.
        /// </summary>
        /// <value>
        /// A <see cref="ClipboardTextFormat"/> for the HyperText Markup Language clipboard format.
        /// </value>
        public static ClipboardTextFormat HTML
        {
            get; private set;
        }

        /// <summary>
        /// Gets the CSV registered clipboard format.
        /// </summary>
        /// <value>
        /// A <see cref="ClipboardTextFormat"/> for Comma Separated Values clipboard format.
        /// </value>
        public static ClipboardTextFormat CSV
        {
            get; private set;
        }

        /// <summary>
        /// Gets the CSV registered clipboard format.
        /// </summary>
        /// <value>
        /// A <see cref="ClipboardTextFormat"/> for Comma Separated Values in a Unicode text format.
        /// </value>
        public static ClipboardTextFormat CSVUnicode
        {
            get; private set;
        }

        /// <summary>
        /// Gets the Xml Spreadsheet registered clipboard format.
        /// </summary>
        /// <value>
        /// A <see cref="ClipboardTextFormat"/> for Xml Spreadsheet clipboard format.
        /// </value>
        public static ClipboardTextFormat XmlSpreadsheet
        {
            get; private set;
        }


        /// <summary>
        /// Gets the clipboard format for the FileList (HDrop) format
        /// </summary> 
        public static ClipboardFormat FileList
        {
            get;
            private set;
        }


        #endregion Static Properties

        #region Properties

        /// <summary>
        /// Gets the number of bytes in a character
        /// </summary>
        public int CharacterSize
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="TextDataFormat"/> of this instance.
        /// </summary>
        public TextDataFormat TextFormat
        {
            get;
            private set;
        }

        #endregion Properties
    }
}
