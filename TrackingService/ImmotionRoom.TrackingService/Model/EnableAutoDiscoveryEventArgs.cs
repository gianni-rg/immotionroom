namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    public class EnableAutoDiscoveryEventArgs : CommandRequestEventArgs
    {
        public AutoDiscoveryParameters Parameters { get; set; }
    }
}
