namespace ImmotionAR.ImmotionRoom.DataSourceService.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Model;

    public interface IDataSourceService
    {
        event EventHandler<DataSourceServiceStatusChangedEventArgs> StatusChanged;
        event EventHandler<TrackingServiceStatusChangedEventArgs> TrackingServiceStatusChanged;

        IReadOnlyDictionary<string, TrackingServiceInfo> KnownTrackingServices { get; }

        DataSourceState Status { get; }

        string InstanceID { get; }

        Task Start();
        Task Stop();
    }
}
