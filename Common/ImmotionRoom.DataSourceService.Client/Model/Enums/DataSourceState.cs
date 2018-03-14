namespace ImmotionAR.ImmotionRoom.DataSource.ControlClient.Model
{
    public enum DataSourceState
    {
        Unknown = 0,
        AutoDiscovery = 1,
        Idle = 2,
        Calibration = 3,
        Running = 4,
        Starting = 5,
        Stopped = 6,
        Error = 7,
    }
}
