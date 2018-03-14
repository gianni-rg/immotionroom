namespace ImmotionAR.ImmotionRoom.DataSourceService.Interfaces
{
    using System;
    using Model;

    public interface IDataSourceSensor
    {
        event EventHandler<SensorStatusChangedEventArgs> SensorStatusChanged;
        event EventHandler<DataSourceDataAvailableEventArgs> SkeletonDataAvailable;
        event EventHandler<DataSourceImageDataAvailableEventArgs> ColorDataAvailable;
        event EventHandler<DataSourceImageDataAvailableEventArgs> DepthDataAvailable;

        bool Start(TrackingSessionConfiguration trackingSessionConfiguration);
        bool Stop();

        bool SourceEnabled { get; set; }
        bool SkeletonStreamEnabled { get; set; }
        bool ColorStreamEnabled { get; set; }
        bool DepthStreamEnabled { get; set; }
    }
}
