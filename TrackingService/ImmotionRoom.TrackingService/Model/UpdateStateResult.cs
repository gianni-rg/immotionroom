namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    public class UpdateStateResult
    {
        public bool IsError { get; set; }
        public string ErrorDescription { get; set; }
        public TrackingServiceStateErrors ErrorCode { get; set; }
    }
}
