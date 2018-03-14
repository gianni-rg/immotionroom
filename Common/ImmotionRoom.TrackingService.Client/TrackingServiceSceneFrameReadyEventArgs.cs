namespace ImmotionAR.ImmotionRoom.TrackingService.DataClient
{
    using System;
    using Model;

    public class TrackingServiceSceneFrameReadyEventArgs : EventArgs
    {
        public TrackingServiceSceneFrame Frame { get; private set; }

        public TrackingServiceSceneFrameReadyEventArgs(TrackingServiceSceneFrame frame)
        {
            Frame = frame;
        }
    }
}
