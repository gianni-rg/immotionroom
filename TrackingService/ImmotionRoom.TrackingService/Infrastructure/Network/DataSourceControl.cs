namespace ImmotionAR.ImmotionRoom.TrackingService.Infrastructure.Network
{
    using System.Threading.Tasks;
    using Interfaces;
    using Logger;
    using Model;

    public class DataSourceControl : IDataSourceControl
    {
        #region Private fields

        private readonly ILogger m_Logger;

        #endregion

        #region Constructor

        public DataSourceControl()
        {
            m_Logger = LoggerService.GetLogger(typeof(DataSourceControl));
        }

        #endregion

        #region Methods

        public Task<bool> EnableAutoDiscoveryAsyncFor(string ip, int port)
        {
            var client = new DataSource.ControlClient.DataSourceControlClient(ip, port);

            var tcs = new TaskCompletionSource<bool>();
            client.EnableAutoDiscoveryAsync(result => tcs.SetResult(!result.IsError));

            return tcs.Task;
        }
        
        public Task<bool> StartTrackingAsyncFor(TrackingSessionDataSourceConfiguration trackingSessionConfiguration, string ip, int port)
        {
            var client = new DataSource.ControlClient.DataSourceControlClient(ip, port);

            var tcs = new TaskCompletionSource<bool>();
            client.StartTrackingAsync(trackingSessionConfiguration.ConvertToWebModel(), result => tcs.SetResult(!result.IsError));

            return tcs.Task;
        }

        public Task<bool> StopTrackingAsyncFor(string ip, int port)
        {
            var client = new DataSource.ControlClient.DataSourceControlClient(ip, port);

            var tcs = new TaskCompletionSource<bool>();
            client.StopTrackingAsync(result => tcs.SetResult(!result.IsError));

            return tcs.Task;
        }

        public Task<bool> GetStatusAsyncFor(string ip, int port)
        {
            var client = new DataSource.ControlClient.DataSourceControlClient(ip, port);

            var tcs = new TaskCompletionSource<bool>();
            client.GetStatusAsync(result => tcs.SetResult(!result.IsError));

            return tcs.Task;
        }

        #endregion
    }
}
