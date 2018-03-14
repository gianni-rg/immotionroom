namespace ImmotionAR.ImmotionRoom.Helpers
{
    using Interfaces;

    public static class SystemManagement
    {
        public static IHelpersSystemManagement PlatformHelpers;

        /// <summary>
        ///     Logoffs the system
        /// </summary>
        public static void LogOff()
        {
            PlatformHelpers.LogOff();
        }

        /// <summary>
        ///     Reboots the system
        /// </summary>
        public static void Reboot()
        {
            PlatformHelpers.Reboot();
        }

        /// <summary>
        ///     Shutdowns the system
        /// </summary>
        public static void Shutdown()
        {
            PlatformHelpers.Shutdown();
        }
    }
}