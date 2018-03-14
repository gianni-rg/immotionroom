namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    using System;

    public class DataSourceStatusChangedEventArgs : EventArgs
    {
        public string DataSourceId { get; private set; }
        public bool IsActive { get; private set; }

        public DataSourceStatusChangedEventArgs(string dataSourceIdId, bool isActive)
        {
            DataSourceId = dataSourceIdId;
            IsActive = isActive;
        }
    }
}
