using System;
using System.Collections.Generic;
using System.Text;

namespace PowerShell.Clipboard
{
    /// <summary>
    /// Describes the name and id of a clipboard format.
    /// </summary>
    public class ClipboardFormat
    {
        #region Static Fields

        static protected Dictionary<uint, ClipboardFormat> _ids = new Dictionary<uint, ClipboardFormat>();
        static protected Dictionary<string, ClipboardFormat> _names = new Dictionary<string, ClipboardFormat>(StringComparer.InvariantCultureIgnoreCase);
        const string UnknownClipboardFormatName = "Unknown";

        #endregion Static Fields

        #region Constructors

        static ClipboardFormat()
        {
            ClipboardFormat format = null;

            foreach (NativeMethods.Format value in Enum.GetValues(typeof(NativeMethods.Format)))
            {
                format = new ClipboardFormat(value);
                _ids.Add(format.Id, format);
                if (!_names.ContainsKey(format.Name))
                {
                    _names.Add(format.Name, format);
                }
            }
        }

        private ClipboardFormat(NativeMethods.Format format)
        {
            Id = (uint)format;
            Name = Enum.GetName(typeof(NativeMethods.Format), format);
            HasName = true;
        }

        private ClipboardFormat(uint format)
        {
            StringBuilder sb = new StringBuilder(1000);

            Id = format;
            if (NativeMethods.GetClipboardFormatName(format, sb, sb.Capacity) > 0)
            {
                Name = sb.ToString();
                HasName = true;
            }
            else
            {
                Name = UnknownClipboardFormatName;
            }
        }

        protected ClipboardFormat(string name, uint format)
        {
            Id = format;
            Name = name;
            HasName = true;
        }

        #endregion

        #region GetFormat

        static protected ClipboardFormat GetFormat(string name)
        {
            ClipboardFormat format = null;

            if (!_names.TryGetValue(name, out format))
            {
                uint id = NativeMethods.RegisterClipboardFormat(name);
                if (id > 0)
                {
                    format = GetFormat(id);
                }
            }

            return format;
        }
        
        static internal ClipboardFormat GetFormat(uint id)
        {
            ClipboardFormat format = null;

            if (!_ids.TryGetValue(id, out format))
            {
                format = new ClipboardFormat(id);
                _ids.Add(id, format);
                if (!_names.ContainsKey(format.Name))
                {
                    _names.Add(format.Name, format);
                }
            }
            return format;
        }

        #endregion GetFormat

        #region Properties

        /// <summary>
        /// Gets the id of the clipboard format.
        /// </summary>
        public uint Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the clipboard format.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets value indicating if the clipboard format name could be retrieved.
        /// </summary>
        public bool HasName
        {
            get;
            private set;
        }

        #endregion Properties
    }
}
