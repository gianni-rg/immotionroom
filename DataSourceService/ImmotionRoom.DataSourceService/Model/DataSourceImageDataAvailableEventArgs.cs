namespace ImmotionAR.ImmotionRoom.DataSourceService.Model
{
    using Protocol;

    public class DataSourceImageDataAvailableEventArgs : CommandRequestEventArgs
    {
        public SensorVideoStreamFrame Data { get; private set; }

        public DataSourceImageDataAvailableEventArgs(SensorVideoStreamFrame data)
        {
            Data = data;
        }
    }
}
