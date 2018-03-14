namespace ImmotionAR.ImmotionRoom.DataSourceService.Model
{
    public class StartTrackingSystemEventArgs : CommandRequestEventArgs
    {
        public TrackingSessionConfiguration Configuration { get; set; }
    }
}
