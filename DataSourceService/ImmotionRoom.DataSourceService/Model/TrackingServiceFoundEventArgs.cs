namespace ImmotionAR.ImmotionRoom.DataSourceService.Model
{
    using System;

    public class TrackingServiceFoundEventArgs : EventArgs
    {
        public TrackingServiceInfo TrackingService { get; private set; }
        public string LicenseId { get; private set; }

        public TrackingServiceFoundEventArgs(TrackingServiceInfo info, string licenseId)
        {
            TrackingService = info;
            LicenseId = licenseId;
        }
    }
}
