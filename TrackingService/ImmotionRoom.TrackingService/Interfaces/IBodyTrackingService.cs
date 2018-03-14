namespace ImmotionAR.ImmotionRoom.TrackingService.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Model;

    public interface IBodyTrackingService
    {
        BodyTrackingServiceState CurrentState { get; }

        int ActiveClients { get; }
        IReadOnlyDictionary<string, SceneDataStreamerInfo> SceneDataStreamers { get; }

        Task StartTrackingAsync(TrackingSessionConfiguration sessionConfiguration = null);

        void StopTracking();

        Task StartCalibrationProcedureAsync(TrackingSessionConfiguration sessionConfiguration = null);
        void ExecuteCalibrationStep(CalibrationParameters parameters);
        void CompleteCalibration();

        Task StartDiagnosticModeAsync(TrackingSessionConfiguration sessionConfiguration = null);

        void StopDiagnosticMode();

        // GIANNI TODO: A refactory should be needed here... not the right place ?
        // This class is used by BodyTrackingService... but the monitor functionality
        // is used in the TrackingService...
        event EventHandler<DataSourceStatusChangedEventArgs> DataSourceStatusChanged;
        void StartDataSourceMonitor();
        void StopDataSourceMonitor();

    }
}
