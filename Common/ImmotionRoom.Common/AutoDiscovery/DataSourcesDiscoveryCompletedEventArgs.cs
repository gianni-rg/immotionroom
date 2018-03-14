namespace ImmotionAR.ImmotionRoom.AutoDiscovery
{
    using System;
    using Model;

    public class DataSourcesDiscoveryCompletedEventArgs : EventArgs
    {
        public DataSourceDiscoveryResult Result { get; private set; }

        public DataSourcesDiscoveryCompletedEventArgs(DataSourceDiscoveryResult result)
        {
            Result = result;
        }
    }
}
