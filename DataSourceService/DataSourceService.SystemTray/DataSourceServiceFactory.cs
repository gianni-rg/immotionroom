namespace ImmotionAR.ImmotionRoom.DataSourceService
{
#if KINECT_V1
    using DataSourceSensor.Kinect1;
#elif KINECT_V2
    using DataSourceSensor.Kinect2;
#endif
    using Interfaces;
    using Model;
    using Networking;
    using Recording;
    using Recording.Interfaces;
    using System.Configuration;

    public class DataSourceServiceFactory
    {
        public IDataSourceService Create(DataSourceConfiguration configuration, TrackingServiceInfo knownTrackingService)
        {
#if DEBUG
            // Temporary -- ONLY for DEBUG
            IDataSourceSensor dataSourceSensor;
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsFake"]) && bool.Parse(ConfigurationManager.AppSettings["IsFake"]))
            {
#if KINECT_V1
                var fakeKinect = new FakeKinect1DataSource();
#elif KINECT_V2
                var fakeKinect = new FakeKinect2DataSource();
#endif
                
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["SavedSessionId"]))
                {
                    fakeKinect.SavedSessionId = ConfigurationManager.AppSettings["SavedSessionId"];
                    fakeKinect.SavedSessionPath = ConfigurationManager.AppSettings["SavedSessionPath"];
                }

                dataSourceSensor = fakeKinect;
            }
            else
            {
#if KINECT_V1
                dataSourceSensor = new Kinect1DataSource();
#elif KINECT_V2
                dataSourceSensor = new Kinect2DataSource();
#endif
            }
#else

#if KINECT_V1
            IDataSourceSensor dataSourceSensor = new Kinect1DataSource();
#elif KINECT_V2
            IDataSourceSensor dataSourceSensor = new Kinect2DataSource();
#endif

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
