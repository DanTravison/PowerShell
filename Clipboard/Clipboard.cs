using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace PowerShell.Clipboard
{
    /// <summary>
    /// Provides clipboard utility methods.
    /// </summary>
    public static class Clipboard
    {
        static Clipboard()
        {
            // Ensure the ClipboardTextFormat static properties are initialized.
            ClipboardTextFormat.InitializeTextFormats();
        }

        /// <summary>
        /// Provides an IDisposable to manage opening and closing the clipboard.
        /// </summary>
        class ClipboardReference : IDisposable
        {
            bool _disposed;

            /// <summary>
            /// Initializes a new instance of this class and opens the clipboard.
            /// </summary>
            public ClipboardReference()
            {
                NativeMethods.OpenClipboard();
            }

            /// <summary>
            /// Disposes this instance and closes the clipboard.
            /// </summary>
            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;
                    NativeMethods.CloseClipboard();
                }
            }
        }

        static ClipboardReference OpenClipboard()
        {
            return new ClipboardReference();
        }

        #region Query Clipboard

        /// <summary>
        /// Determines if the clipboard contains data for a specific clipboard format.
        /// </summary>
        /// <param name="format">The <see cref="ClipboardFormat"/> for the format to check.</param>
        /// <returns>true if the clipboard contains data in the specified format; otherwise, false.</returns>
        public static bool Contains(ClipboardFormat format)
        {
            return NativeMethods.IsClipboardFormatAvailable(format.Id);
        }

        /// <summary>
        /// Gets the supported formats of the current clipboard contents.
        /// </summary>
        /// <returns>A <see cref="ClipboardFormat"/> array containing zero or more elements.</returns>
        /// <exception cref="Win32Exception">An error occured opening the clipboard.</exception>
        public static ClipboardFormat[] GetFormats()
        {
            List<ClipboardFormat> formats = new List<ClipboardFormat>();
            using (OpenClipboard())
            { 
                uint format = 0;
                do
                {
                    format = NativeMethods.EnumClipboardFormats(format);
                    if (format == 0)
                    {
                        break;
                    }
                    ClipboardFormat clipboardFormat = ClipboardFormat.GetFormat(format);
                    formats.Add(clipboardFormat);
                } while (true);
            }

            return formats.ToArray();
        }

        #endregion  Query Clipboard

        #region Text

        /// <summary>
        /// Gets the clipboard contents as text.
        /// </summary>
        /// <param name="format">The <see cref="ClipboardFormat"/> for the string format to retrieve.</param>
        /// <returns>The contents of the clipboard as a string.</returns>
        /// <exception cref="Win32Exception">An error occured opening the clipboard or retrieving the contents.</exception>
        public static string GetTextData(ClipboardTextFormat format)
        {
            string value = null;
            using (OpenClipboard())
            {
                IntPtr stringPointer = IntPtr.Zero;
                IntPtr handle = IntPtr.Zero;
                do
                {
                    try
                    {
                        if (!Contains(format))
                        {
                            break;
                        }

                        handle = NativeMethods.GetClipboardData(format.Id);
                        if (handle == IntPtr.Zero)
                        {
                            break;
                        }

                        stringPointer = NativeMethods.GlobalLock(handle);
                        if (stringPointer == IntPtr.Zero)
                        {
                            break;
                        }

                        if (format.CharacterSize == 1)
                        {
                            string unencodedValue = Marshal.PtrToStringAnsi(stringPointer);
                            value = Encoding.GetEncoding(0).GetString(Encoding.UTF8.GetBytes(unencodedValue));
                        }
                        else if (format.CharacterSize == 2)
                        {
                            value = Marshal.PtrToStringUni(stringPointer);
                        }
                        else
                        {
                            throw new InvalidOperationException
                            (
                                string.Format(CultureInfo.InvariantCulture, "Internal Error: Text format {0} has an unsupported character size: {1}", format.Name, format.CharacterSize)
                            );
                        }
                    }
                    finally
                    {
                        if (stringPointer != IntPtr.Zero)
                        {
                            NativeMethods.GlobalUnlock(handle);
                        }
                    }
                } while (false);
            }

            return value;
        }
 
        /// <summary>
        /// Sets the clipboard contents to a specified text value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <param name="clipboardFormat">The <see cref="ClipboardTextFormat"/> to use to set the clipboard.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is a null or empty string.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The specified <paramref name="ClipboardTextFormat"/> is not supported.</exception>
        /// <exception cref="Win32Exception">An error occured opening or setting the clipboard.</exception>
        public static void SetText(string value, ClipboardTextFormat clipboardFormat)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }

            if (clipboardFormat == null)
            {
                throw new ArgumentNullException("textFormat", "Unsupported TextDataFormat");
            }

            using (OpenClipboard())
            {
                IntPtr source = IntPtr.Zero;
                if (clipboardFormat.CharacterSize == 1)
                {
                    source = Marshal.StringToHGlobalAnsi(value);
                }
                else
                {
                    source = Marshal.StringToHGlobalUni(value);
                }

                IntPtr result = NativeMethods.SetClipboardData(clipboardFormat.Id, source);
                if (result == IntPtr.Zero)
                {
                    int win32Error = Marshal.GetLastWin32Error();
                    Marshal.FreeHGlobal(source);
                    throw new Win32Exception
                    (
                        win32Error, 
                        string.Format(CultureInfo.InvariantCulture, "Failed to set the clipboard: 0x{0:X}", win32Error)
                    );
                }
            }
        }

        #endregion Text

        #region HDROP

        /// <summary>
        /// Gets the clipboard as a file list
        /// </summary>
        /// <returns>A string array containing the file list.</returns>
        /// <exception cref="Win32Exception">An error occured opening the clipboard.</exception>
        public static string[] GetFileList()
        {
            List<string> files = new List<string>();
            
            using (OpenClipboard())
            {
                do
                {
                    if (!Contains(ClipboardTextFormat.FileList))
                    {
                        break;
                    }
                    StringBuilder sb = new StringBuilder(1000);

                    IntPtr hDrop = NativeMethods.GetClipboardData(NativeMethods.Format.HDrop);
                    if (hDrop == IntPtr.Zero)
                    {
                        break;
                    }

                    uint fileCount = NativeMethods.DragQueryFile(hDrop, uint.MaxValue, null, 0);
                    if (fileCount == 0)
                    {
                        int win32Error = Marshal.GetLastWin32Error();
                        break;
                    }

                    for (uint x = 0; x < fileCount; x++)
                    {
                        uint chars = NativeMethods.DragQueryFile(hDrop, x, sb, (uint)sb.Capacity);
                        if (chars == 0)
                        {
                            break;
                        }
                        files.Add(sb.ToString());
                        sb.Clear();
                    }
                } while (false);
            }

            return files.ToArray();
        }

        /// <summary>
        /// Sets the clipboard with a file list (CF_HDROP)
        /// </summary>
        /// <param name="files">The list of file to set.</param>
        /// <exception cref="Win32Exception">An error occured opening or setting the clipboard.</exception>
        /// <exception cref="OutOfMemoryException">Insufficient memory to set the clipboard.</exception>
        public static void SetFileList(string[] files)
        {
            IntPtr dropFilesPtr = CreateHDROP(files);

            using (OpenClipboard())
            {
                IntPtr result = NativeMethods.SetClipboardData((uint)NativeMethods.Format.HDrop, dropFilesPtr);
                if (result == IntPtr.Zero)
                {
                    int win32Error = Marshal.GetLastWin32Error();
                    Marshal.FreeHGlobal(dropFilesPtr);
                    throw new Win32Exception
                    (
                        win32Error,
                        string.Format(CultureInfo.InvariantCulture, "Failed to set the clipboard: 0x{0:X}", win32Error)
                    );
                }
            }
        }

        /// <summary>
        /// Creates a native <see cref="NativeMethods.DROPFILES"/> structure populated with the list of <paramref name="files"/>.
        /// </summary>
        /// <param name="files">The files to report in the CF_HDROP structure.</param>
        /// <returns>An <see cref="IntPtr"/> for the native structure.</returns>
        /// <exception cref="Win32Exception">Could not allocate memory for the native structure.</exception>
        static IntPtr CreateHDROP(string[] files)
        {
            IntPtr dropPtr = IntPtr.Zero;

            // calculate the total number of characters (and zero terminators) for all files
            int fileChars = 0;
            for (int x = 0; x < files.Length; x++)
            {
                fileChars += files[x].Length + 1;
            }
            // Account for the double zero terminator
            fileChars += 2;

            NativeMethods.DROPFILES dropfiles = new NativeMethods.DROPFILES()
            {
                size = Marshal.SizeOf<NativeMethods.DROPFILES>(),
                point = new NativeMethods.POINT(),
                fND = 0,
                wide = 1
            };

            // Allocate the native buffer to include the DROPFILES and list of files
            int byteCount = dropfiles.size + fileChars * 2;
            
            int win32Error = NativeMethods.GlobalAlloc(byteCount, out dropPtr);
            if (win32Error != 0)
            {
                throw new Win32Exception
                (
                    win32Error, 
                    string.Format(CultureInfo.InvariantCulture, "Failed to allocate memory for the file list: 0x{0:X}", win32Error)
                );
            }
            IntPtr dropBuffer = NativeMethods.GlobalLock(dropPtr);

            // Copy the DROPFILES structure
            Marshal.StructureToPtr(dropfiles, dropBuffer, false);

            // Copy the file list to the memory after the DROPFILES.
            IntPtr fileList = dropBuffer + dropfiles.size;
            foreach (string file in files)
            {
                byte[] bytes = Encoding.Unicode.GetBytes(file.ToCharArray());
                Marshal.Copy(bytes, 0, fileList, bytes.Length);
                fileList += bytes.Length + 2;
            }

            NativeMethods.GlobalUnlock(dropPtr);

            return dropPtr;
        }

        #endregion HDROP

        /// <summary>
        /// Clears the clipboard.
        /// </summary>
        /// <exception cref="Win32Exception">An error occured opening or clearing the clipboard.</exception>
        public static void Clear()
        {
            using (OpenClipboard())
            {
                if (!NativeMethods.EmptyClipboard())
                {
                    int win32Error = Marshal.GetLastWin32Error();
                    throw new Win32Exception
                    (
                        win32Error, 
                        string.Format(CultureInfo.InvariantCulture, "Failed to clear the clipboard: 0x{0:X}", win32Error)
                    );
                }
            }
        }
    }
}

