namespace ImmotionAR.ImmotionRoom.Helpers
{
    using Interfaces;

    public static class RegistryTools
    {
        public static IHelpersRegistry PlatformHelpers;

        /// <summary>
        ///     Reads a string setting from the specified Registry Key (under LocalMachine)
        /// </summary>
        public static string ReadSetting(string subKey, string keyName)
        {
            return PlatformHelpers.ReadSetting(subKey, keyName);
        }

        /// <summary>
        ///     Writes a string setting to the specified Registry Key (under LocalMachine)
        /// </summary>
        public static bool WriteSetting(string subKey, string keyName, string value)
        {
            return PlatformHelpers.WriteSetting(subKey, keyName, value);
        }
    }
}