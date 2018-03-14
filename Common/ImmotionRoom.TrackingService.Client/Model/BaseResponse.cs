namespace ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model
{
    public abstract class BaseResponse
    {
        public string ID { get; set; }
        public bool IsError { get; set; }
        public string ErrorDescription { get; set; }
        public int ErrorCode { get; set; }
    }
}
