namespace ImmotionAR.ImmotionRoom.Helpers
{
    using System;
    using Interfaces;

    public static class AppVersions
    {
        public static IHelpersAppVersions PlatformHelpers;

        public static string RetrieveExecutableVersion()
        {
            return PlatformHelpers.RetrieveExecutableVersion();
        }

        public static DateTime RetrieveLinkerTimestamp()
        {
            return PlatformHelpers.RetrieveLinkerTimestamp();
        }
    }
}