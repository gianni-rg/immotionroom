namespace ImmotionAR.ImmotionRoom.TrackingService.Presentation.Helpers
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    internal enum TaskbarPosition
    {
        Unknown = -1,
        Left,
        Top,
        Right,
        Bottom
    }

    // See: https://winsharp93.wordpress.com/2009/06/29/find-out-size-and-position-of-the-taskbar/
    internal sealed class Taskbar
    {
        private const string ClassName = "Shell_TrayWnd";

        public Rectangle Bounds { get; private set; }
        public TaskbarPosition Position { get; private set; }

        public Point Location
        {
            get { return Bounds.Location; }
        }

        public Size Size
        {
            get { return Bounds.Size; }
        }

        //Always returns false under Windows 7
        public bool AlwaysOnTop { get; private set; }
        public bool AutoHide { get; private set; }

        public Taskbar()
        {
            var taskbarHandle = User32.FindWindow(ClassName, null);

            var data = new APPBARDATA();
            data.cbSize = (uint) Marshal.SizeOf(typeof (APPBARDATA));
            data.hWnd = taskbarHandle;
            var result = Shell32.SHAppBarMessage(ABM.GetTaskbarPos, ref data);
            if (result == IntPtr.Zero)
                throw new InvalidOperationException();

            Position = (TaskbarPosition) data.uEdge;
            Bounds = Rectangle.FromLTRB(data.rc.left, data.rc.top, data.rc.right, data.rc.bottom);

            data.cbSize = (uint) Marshal.SizeOf(typeof (APPBARDATA));
            result = Shell32.SHAppBarMessage(ABM.GetState, ref data);
            var state = result.ToInt32();
            AlwaysOnTop = (state & ABS.AlwaysOnTop) == ABS.AlwaysOnTop;
            AutoHide = (state & ABS.Autohide) == ABS.Autohide;
        }
    }

    internal enum ABM : uint
    {
        New = 0x00000000,
        Remove = 0x00000001,
        QueryPos = 0x00000002,
        SetPos = 0x00000003,
        GetState = 0x00000004,
        GetTaskbarPos = 0x00000005,
        Activate = 0x00000006,
        GetAutoHideBar = 0x00000007,
        SetAutoHideBar = 0x00000008,
        WindowPosChanged = 0x00000009,
        SetState = 0x0000000A
    }

    internal enum ABE : uint
    {
        Left = 0,
        Top = 1,
        Right = 2,
        Bottom = 3
    }

    internal static class ABS
    {
        public const int Autohide = 0x0000001;
        public const int AlwaysOnTop = 0x0000002;
    }

    internal static class Shell32
    {
        [DllImport("shell32.dll", SetLastError = true)]
        public static extern IntPtr SHAppBarMessage(ABM dwMessage, [In] ref APPBARDATA pData);
    }

    internal static class User32
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct APPBARDATA
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public ABE uEdge;
        public RECT rc;
        public int lParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}