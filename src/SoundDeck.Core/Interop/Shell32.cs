namespace SoundDeck.Core.Interop
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Provides interop with shell32.
    /// </summary>
    public static class Shell32
    {
        public const string IID_IImageList = "46EB5926-582E-4017-9FDF-E8998DAA0950";
        public const string IID_IImageList2 = "192B9D83-50FC-457B-90A0-2B82A8B5DAE1";

        public const int SHIL_LARGE = 0x0;
        public const int SHIL_SMALL = 0x1;
        public const int SHIL_EXTRALARGE = 0x2;
        public const int SHIL_SYSSMALL = 0x3;
        public const int SHIL_JUMBO = 0x4;
        public const int SHIL_LAST = 0x4;

        public const int ILD_TRANSPARENT = 0x00000001;
        public const int ILD_IMAGE = 0x00000020;

        /// <summary>
        /// Retrieves an image list.
        /// </summary>
        /// <param name="iImageList">The image type contained in the list.</param>
        /// <param name="riid">Reference to the image list interface identifier, normally IID_IImageList.</param>
        /// <param name="ppv">When this method returns, contains the interface pointer requested in riid. This is typically IImageList.</param>
        /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
        /// <remarks>https://learn.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-shgetimagelist</remarks>
        [DllImport("shell32.dll", EntryPoint = "#727")]
        public extern static int SHGetImageList(int iImageList, ref Guid riid, ref IImageList ppv);

        /// <summary>
        /// Retrieves the pointer to an item identifier list (PIDL) of an object.
        /// </summary>
        /// <param name="iUnknown">A pointer to the IUnknown of the object from which to get the PIDL.</param>
        /// <param name="ppidl">When this function returns, contains a pointer to the PIDL of the given object.</param>
        /// <returns>If this function succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.</returns>
        [DllImport("shell32.dll")]
        public static extern uint SHGetIDListFromObject([MarshalAs(UnmanagedType.IUnknown)] object iUnknown, out IntPtr ppidl);

        /// <summary>
        /// Retrieves information about an object in the file system, such as a file, folder, directory, or drive root.
        /// </summary>
        /// <param name="pszPath">A pointer to a null-terminated string of maximum length MAX_PATH that contains the path and file name. Both absolute and relative paths are valid.</param>
        /// <param name="dwFileAttributes">A combination of one or more file attribute flags (FILE_ATTRIBUTE_ values as defined in Winnt.h). If uFlags does not include the SHGFI_USEFILEATTRIBUTES flag, this parameter is ignored.</param>
        /// <param name="psfi">Pointer to a SHFILEINFO structure to receive the file information.</param>
        /// <param name="cbFileInfo">The size, in bytes, of the SHFILEINFO structure pointed to by the psfi parameter.</param>
        /// <param name="uFlags">The flags that specify the file information to retrieve. This parameter can be a combination of the following values.</param>
        /// <returns>Returns a value whose meaning depends on the uFlags parameter.</returns>
        [DllImport("Shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [Flags]
        public enum SHGFI : uint
        {
            /// <summary>get icon</summary>
            Icon = 0x000000100,
            /// <summary>get display name</summary>
            DisplayName = 0x000000200,
            /// <summary>get type name</summary>
            TypeName = 0x000000400,
            /// <summary>get attributes</summary>
            Attributes = 0x000000800,
            /// <summary>get icon location</summary>
            IconLocation = 0x000001000,
            /// <summary>return exe type</summary>
            ExeType = 0x000002000,
            /// <summary>get system icon index</summary>
            SysIconIndex = 0x000004000,
            /// <summary>put a link overlay on icon</summary>
            LinkOverlay = 0x000008000,
            /// <summary>show icon in selected state</summary>
            Selected = 0x000010000,
            /// <summary>get only specified attributes</summary>
            Attr_Specified = 0x000020000,
            /// <summary>get large icon</summary>
            LargeIcon = 0x000000000,
            /// <summary>get small icon</summary>
            SmallIcon = 0x000000001,
            /// <summary>get open icon</summary>
            OpenIcon = 0x000000002,
            /// <summary>get shell size icon</summary>
            ShellIconSize = 0x000000004,
            /// <summary>pszPath is a pidl</summary>
            PIDL = 0x000000008,
            /// <summary>use passed dwFileAttribute</summary>
            UseFileAttributes = 0x000000010,
            /// <summary>apply the appropriate overlays</summary>
            AddOverlays = 0x000000020,
            /// <summary>Get the index of the overlay in the upper 8 bits of the iIcon</summary>
            OverlayIndex = 0x000000040,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public const int NAMESIZE = 80;
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left, top, right, bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            int x;
            int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGELISTDRAWPARAMS
        {
            public int cbSize;
            public IntPtr himl;
            public int i;
            public IntPtr hdcDst;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int xBitmap;    // x offest from the upperleft of bitmap
            public int yBitmap;    // y offset from the upperleft of bitmap
            public int rgbBk;
            public int rgbFg;
            public int fStyle;
            public int dwRop;
            public int fState;
            public int Frame;
            public int crEffect;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGEINFO
        {
            public IntPtr hbmImage;
            public IntPtr hbmMask;
            public int Unused1;
            public int Unused2;
            public RECT rcImage;
        }

        [ComImport()]
        [Guid("46EB5926-582E-4017-9FDF-E8998DAA0950")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IImageList
        {
            [PreserveSig] int Add(IntPtr hbmImage, IntPtr hbmMask, ref int pi);
            [PreserveSig] int ReplaceIcon(int i, IntPtr hicon, ref int pi);
            [PreserveSig] int SetOverlayImage(int iImage, int iOverlay);
            [PreserveSig] int Replace(int i, IntPtr hbmImage, IntPtr hbmMask);
            [PreserveSig] int AddMasked(IntPtr hbmImage, int crMask, ref int pi);
            [PreserveSig] int Draw(ref IMAGELISTDRAWPARAMS pimldp);
            [PreserveSig] int Remove(int i);
            [PreserveSig] int GetIcon(int i, int flags, ref IntPtr picon);
            [PreserveSig] int GetImageInfo(int i, ref IMAGEINFO pImageInfo);
            [PreserveSig] int Copy(int iDst, IImageList punkSrc, int iSrc, int uFlags);
            [PreserveSig] int Merge(int i1, IImageList punk2, int i2, int dx, int dy, ref Guid riid, ref IntPtr ppv);
            [PreserveSig] int Clone(ref Guid riid, ref IntPtr ppv);
            [PreserveSig] int GetImageRect(int i, ref RECT prc);
            [PreserveSig] int GetIconSize(ref int cx, ref int cy);
            [PreserveSig] int SetIconSize(int cx, int cy);
            [PreserveSig] int GetImageCount(ref int pi);
            [PreserveSig] int SetImageCount(int uNewCount);
            [PreserveSig] int SetBkColor(int clrBk, ref int pclr);
            [PreserveSig] int GetBkColor(ref int pclr);
            [PreserveSig] int BeginDrag(int iTrack, int dxHotspot, int dyHotspot);
            [PreserveSig] int EndDrag();
            [PreserveSig] int DragEnter(IntPtr hwndLock, int x, int y);
            [PreserveSig] int DragLeave( IntPtr hwndLock);
            [PreserveSig] int DragMove(int x, int y);
            [PreserveSig] int SetDragCursorImage(ref IImageList punk, int iDrag, int dxHotspot, int dyHotspot);
            [PreserveSig] int DragShowNolock(int fShow);
            [PreserveSig] int GetDragImage(ref POINT ppt, ref POINT pptHotspot, ref Guid riid, ref IntPtr ppv);
            [PreserveSig] int GetItemFlags(int i, ref int dwFlags);

            [PreserveSig] int GetOverlayImage(int iOverlay, ref int piIndex);
        };
    }
}
