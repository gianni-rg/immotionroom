namespace ImmotionAR.ImmotionRoom.TrackingService.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Model;
    using TrackingEngine.Model;

    public interface IDataSourceService
    {
        event EventHandler<DataSourceStatusChangedEventArgs> DataSourceStatusChanged;

        Task StartAsync(TrackingSessionDataSourceConfiguration trackingSessionConfiguration);

        void Stop();

        void Update(double deltaTime);

        IDictionary<string, SceneFrame> DataSources { get; }

        // GIANNI TODO: A refactory should be needed here... not the right place ?
        // This class is used by BodyTrackingService... but the monitor functionality
        // is used in the TrackingService...
        void StartMonitor();

        void StopMonitor();
    }
}
