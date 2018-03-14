namespace ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model
{
    public class ResultResponse<T> : BaseResponse
    {
        public T Data { get; set; }
    }
}
