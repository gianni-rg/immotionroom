namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    public class ServiceStatusMessage
    {
        #region Properties

        public TrackingServiceState Status { get; set; }

        public string Description { get; set; }

        #endregion
    }
}
