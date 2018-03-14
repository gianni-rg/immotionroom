namespace ImmotionAR.ImmotionRoom.DataSource.ControlClient.Model
{
    public enum CommandType
    {
        Undefined = 0,
        EnableAutoDiscovery = 1,
        ServiceStatus = 2,
        StartTracking = 3,
        StopTracking = 4,
        SystemReboot = 5,
    }
}