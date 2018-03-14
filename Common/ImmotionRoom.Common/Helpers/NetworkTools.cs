namespace ImmotionAR.ImmotionRoom.Helpers
{
    using Interfaces;

    public static class NetworkTools
    {
        public static IHelpersNetworkTools PlatformHelpers;

        public static string GetLocalIpAddress(int adapterIndex = 0)
        {
            return PlatformHelpers.GetLocalIpAddress(adapterIndex);
        }
    }
}