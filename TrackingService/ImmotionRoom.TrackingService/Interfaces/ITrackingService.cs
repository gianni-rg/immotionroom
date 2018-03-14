namespace ImmotionAR.ImmotionRoom.TrackingService.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Model;

    public interface ITrackingService
    {
        event EventHandler<TrackingServiceStatusChangedEventArgs> StatusChanged;
        event EventHandler<DataSourceStatusChangedEventArgs> DataSourceStatusChanged;

        IReadOnlyDictionary<string, DataSourceInfo> KnownDataSources { get; }

        TrackingServiceState Status { get; }

        string InstanceID { get; }

        Task Start();
        Task Stop();
    }
}
