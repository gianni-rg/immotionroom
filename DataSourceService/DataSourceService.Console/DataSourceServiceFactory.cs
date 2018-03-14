namespace ImmotionAR.ImmotionRoom.DataSourceService
{
    using DataSourceSensor.Kinect2;
    using ImmotionAR.ImmotionRoom.Recording;
    using Interfaces;
    using Model;
    using Networking;
    using Recording.Interfaces;
#if DEBUG
    using System.Configuration;
#endif

    public class DataSourceServiceFactory
    {
        public IDataSourceService Create(DataSourceConfiguration configuration, TrackingServiceInfo knownTrackingService)
        {
#if DEBUG
            // Temporary -- ONLY for DEBUG
            IDataSourceSensor dataSourceSensor;
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsFake"]) && bool.Parse(ConfigurationManager.AppSettings["IsFake"]))
            {
                var fakeKinect = new FakeKinect2DataSource();
                
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["SavedSessionId"]))
                {
                    fakeKinect.SavedSessionId = ConfigurationManager.AppSettings["SavedSessionId"];
                    fakeKinect.SavedSessionPath = ConfigurationManager.AppSettings["SavedSessionPath"];
                }

                dataSourceSensor = fakeKinect;
            }
            else
            {
                dataSourceSensor = new Kinect2DataSource();
            }
#else
            IDataSourceSensor dataSourceSensor = new Kinect2DataSource();
#endif

            IVideoRecorder colorVideoRecorder = new VideoRecorder();
            IVideoRecorder depthVideoRecorder = new VideoRecorder();

            return new DataSourceService(configuration, knownTrackingService,
                dataSourceSensor, 
                new TcpServerFactory(),
                //new TcpClientFactory(),
                new UdpClientFactory(), 
                new NetworkClientFactory(),
                colorVideoRecorder,
                depthVideoRecorder);
        }
    }
}
