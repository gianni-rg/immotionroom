namespace ImmotionAR.ImmotionRoom.Helpers
{
    using System;
    using System.IO;
    using System.Reflection;
    using Interfaces;

    public class HelpersAppVersions : IHelpersAppVersions
    {
        public string RetrieveExecutableVersion()
        {
            return Assembly.GetCallingAssembly().GetName().Version.ToString();
        }

        public DateTime RetrieveLinkerTimestamp()
        {
            var filePath = Assembly.GetCallingAssembly().Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            var b = new byte[2048];
            Stream s = null;

            try
            {
                s = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }

            var i = BitConverter.ToInt32(b, c_PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
            var dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);
            return dt;
        }
    }
}