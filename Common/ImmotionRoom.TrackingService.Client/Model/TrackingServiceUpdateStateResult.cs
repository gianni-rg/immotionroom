namespace ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model
{
    public class TrackingServiceUpdateStateResult
    {
        public bool IsError { get; set; }
        public string ErrorDescription { get; set; }
        public TrackingServiceStateErrors ErrorCode { get; set; }
    }
}
