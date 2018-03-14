namespace ImmotionAR.ImmotionRoom.DataSourceService.Model
{
    using System;

    public class TrackingServiceStatusChangedEventArgs : EventArgs
    {
        public string TrackingServiceId { get; private set; }
        public bool IsActive { get; private set; }

        public TrackingServiceStatusChangedEventArgs(string trackingServiceId, bool isActive)
        {
            TrackingServiceId = trackingServiceId;
            IsActive = isActive;
        }
    }
}
