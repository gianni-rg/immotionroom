namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    using System;

    public class DataSourceFoundEventArgs : EventArgs
    {
        public DataSourceInfo DataSource { get; private set; }
        public string LicenseId { get; private set; }

        public DataSourceFoundEventArgs(DataSourceInfo info, string licenseId)
        {
            DataSource = info;
            LicenseId = licenseId;
        }
    }
}
