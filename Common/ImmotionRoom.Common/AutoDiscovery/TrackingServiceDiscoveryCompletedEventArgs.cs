namespace ImmotionAR.ImmotionRoom.AutoDiscovery
{
    using System;
    using Model;

    public class TrackingServiceDiscoveryCompletedEventArgs : EventArgs
    {
        public TrackingServiceDiscoveryResult Result { get; private set; }

        public TrackingServiceDiscoveryCompletedEventArgs(TrackingServiceDiscoveryResult result)
        {
            Result = result;
        }
    }
}
