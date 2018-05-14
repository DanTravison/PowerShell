using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace PowerShell.Clipboard
{
    internal class NativeMethods
    {
        /// <summary>
        /// Provides flags to control allocate of memory by <see cref="GlobalAlloc"/>.
        /// </summary>
        [Flags]
        internal enum AllocFlags : uint
        {
            /// <summary>
            /// Allocates fixed memory. The return value is a pointer.
            /// </summary>
            Fixed = 0x0,

            /// <summary>
            /// Allocates movable memory. Memory blocks are never moved in physical memory, but they can be moved within the default heap.
            /// The return value is a handle to the memory object. To translate the handle into a pointer, use the <see cref="GlobalLock"/> function.
            /// </summary>
            Moveable = 0x02,

            /// <summary>
            /// Initializes memory contents to zero.
            /// </summary>
            ZeroInit = 0x40,

            /// <summary>
            /// Allocates <see cref="Fixed"/> memory and initializes it to zeros.
            /// The return value is a pointer.
            /// </summary>
            Pointer = Fixed | ZeroInit,

            /// <summary>
            /// Allocates <see cref="Moveable"/> memory and initializes it to zeros.
            /// The return value is a handle to the memory object. To translate the handle into a pointer, use the <see cref="GlobalLock"/> function.
            /// </summary>
            Handle = Moveable | ZeroInit
        };


        internal enum Format : uint
        {
            None = 0,
            Text = 1,
            Bitmap = 2,
            MetaFilePict = 3,
            Sylk = 4,
            Dif = 5,
            Tiff = 6,
            OemText = 7,
            Dib = 8,
            Palette = 9,
            PenData = 10,
            Riff = 11,
            Wave = 12,
            Unicode = 13,
            ENHMetaFile = 14,
            HDrop = 15,
            Locale = 16,
            DIBV5 = 17,

            OwnerDisplay = 0x0080,
            DspText = 0x0081,
            DspBitmap = 0x0082,
            DspMetaFilePict = 0x0083,
            DspEnhmetaFile = 0x008e,
            PrivateFirst = 0x0200,
            PrivateLast = 0x02ff,
            GdiObjfirst = 0x0300,
            GdiObjLast = 0x03ff
        }

        [Flags]
        public enum Tymed : uint
        {
            /// <sumry>
            /// No data is being passed. 
            /// </summary>
            NULL = 0,
            /// <summary>
            /// The storage medium is a global memory handle (HGLOBAL).
            /// </summary>
            HGlobal = 1,
            /// <summary>
            /// The storage medium is a disk file identified by a path.
            /// </summary>
            File = 2,
            /// <summary>
            /// The storage medium is a stream object identified by an IStream pointer.
            /// </summary>
            IStream = 4,
            /// <summary>
            /// The storage medium is a storage component identified by an IStorage pointer.
            /// </summary>
            IStorage = 8,
            /// <summary>
            /// The storage medium is a GDI component (HBITMAP).
            /// </summary>
            GDI = 16,
            /// <summary>
            /// The storage medium is a metafile (HMETAFILE). Use the GDI functions to access the metafile's data.
            /// </summary>
            MFPict = 32,
            /// <summary>
            /// The storage medium is an enhanced metafile.
            /// </summary>
            ENHMF = 64
        };

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public Int32 X;
            public Int32 Y;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DROPFILES
        {
            public Int32 size;
            public POINT point;
            public Int32 fND;
            public Int32 wide;
        }

        static int _openRetryCount = 3;

        public static void OpenClipboard()
        {
            int win32Error = 0;
            int retryCount = _openRetryCount;
            while (retryCount > 0)
            {
                if (retryCount < _openRetryCount)
                {
                    Thread.Sleep(100);
                }

                if (OpenClipboard(IntPtr.Zero))
                {
                    win32Error = 0;
                    break;
                }

                win32Error = Marshal.GetLastWin32Error();
                if (win32Error != 5)
                {
                    break;
                }
             
                // Clipboard in use
                retryCount--;
            }

            if (win32Error != 0)
            {
                throw new Win32Exception
                (
                    win32Error, 
                    string.Format(CultureInfo.InvariantCulture, "Failed to open the clipboard: 0x{0:X}", win32Error)
                );
            }
        }

        [DllImport("user32.dll", EntryPoint = "OpenClipboard", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool OpenClipboard([In] IntPtr hWndNewOwner);

        [DllImport("user32.dll", EntryPoint = "CloseClipboard", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseClipboard();

        [DllImport("user32.dll", EntryPoint = "GetClipboardData", SetLastError = true)]
        public static extern IntPtr GetClipboardData(Format format);

        [DllImport("user32.dll", EntryPoint = "GetClipboardData", SetLastError = true)]
        public static extern IntPtr GetClipboardData(uint format);

        [DllImport("user32.dll", EntryPoint = "SetClipboardData", SetLastError = true)]
        public static extern IntPtr SetClipboardData(uint format, IntPtr mem);

        [DllImport("user32.dll", EntryPoint = "EmptyClipboard", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EmptyClipboard();

        [DllImport("user32.dll", EntryPoint = "IsClipboardFormatAvailable", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsClipboardFormatAvailable(uint format);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint RegisterClipboardFormat(string format);

        [DllImport("user32.dll", EntryPoint = "EnumClipboardFormats", SetLastError = true)]
        public static extern uint EnumClipboardFormats(uint format);

        [DllImport("user32.dll", EntryPoint = "GetClipboardFormatName", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int GetClipboardFormatName(uint format, [Out] StringBuilder name, int maxCount);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GlobalLock(IntPtr handle);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool GlobalUnlock(IntPtr mem);

        const uint MemoryMovable = 0x0002;
        const uint MemoryZeroInit = 0x0040;

        public static int GlobalAlloc(int bytes, out  IntPtr buffer)
        {
            return GlobalAlloc(bytes, AllocFlags.Handle, out buffer);
        }

        public static int GlobalAlloc(int bytes, AllocFlags flags, out IntPtr buffer)
        {
            int result = 0;
            buffer = GlobalAlloc((uint) flags, (UIntPtr)bytes);
            if (buffer == IntPtr.Zero)
            {
                result = Marshal.GetLastWin32Error();
            }
            return result;
        }
        
        [DllImport("kernel32.dll", EntryPoint = "GlobalAlloc", SetLastError = true)]
        private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalFree(IntPtr buffer);

        [DllImport("shell32.dll", EntryPoint = "DragQueryFileW", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern uint DragQueryFile(IntPtr hDrop, uint index, [Out] StringBuilder file, uint maxCount);
    }
}
