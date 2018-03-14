namespace ImmotionAR.ImmotionRoom.DataSourceService.Model
{
    using System;

    public class DataSourceServiceStatusChangedEventArgs : EventArgs
    {
        public DataSourceState Status { get; private set; }
        public DataSourceStateErrors Error { get; private set; }

        public DataSourceServiceStatusChangedEventArgs(DataSourceState status, DataSourceStateErrors error)
        {
            Status = status;
            Error = error;
        }
    }
}
