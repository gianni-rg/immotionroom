namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    using System;

    public class TrackingServiceStatusChangedEventArgs : EventArgs
    {
        public TrackingServiceState Status { get; private set; }
        public TrackingServiceStateErrors Error { get; private set; }

        public TrackingServiceStatusChangedEventArgs(TrackingServiceState status, TrackingServiceStateErrors error)
        {
            Status = status;
            Error = error;
        }
    }
}
