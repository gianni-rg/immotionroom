namespace ImmotionAR.ImmotionRoom.DataSourceService.Model
{
    using Protocol;

    public class DataSourceDataAvailableEventArgs : CommandRequestEventArgs
    {
        public SensorDataFrame Data { get; private set; }

        public DataSourceDataAvailableEventArgs(SensorDataFrame data)
        {
            Data = data;
        }
    }
}
