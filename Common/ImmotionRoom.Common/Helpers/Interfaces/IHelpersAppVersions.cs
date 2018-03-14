namespace ImmotionAR.ImmotionRoom.Helpers.Interfaces
{
    using System;

    public interface IHelpersAppVersions
    {
        string RetrieveExecutableVersion();
        DateTime RetrieveLinkerTimestamp();
    }
}