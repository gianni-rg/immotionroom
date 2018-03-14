namespace ImmotionAR.ImmotionRoom.TrackingService.Presentation.Helpers
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    // See: http://stackoverflow.com/questions/5977445/how-to-get-windows-display-settings
    internal static class ScaleFactor
    {
        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        public enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117

            // http://pinvoke.net/default.aspx/gdi32/GetDeviceCaps.html
        }


        public static float GetScalingFactor()
        {
            var g = Graphics.FromHwnd(IntPtr.Zero);
            var desktop = g.GetHdc();
            var LogicalScreenHeight = GetDeviceCaps(desktop, (int) DeviceCap.VERTRES);
            var PhysicalScreenHeight = GetDeviceCaps(desktop, (int) DeviceCap.DESKTOPVERTRES);

            var ScreenScalingFactor = PhysicalScreenHeight/(float) LogicalScreenHeight;

            return ScreenScalingFactor; // 1.25 = 125%
        }
    }
}
