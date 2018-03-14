namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    public class StartTrackingSystemEventArgs : CommandRequestEventArgs
    {
        public TrackingSessionConfiguration Configuration { get; set; }
    }
}
