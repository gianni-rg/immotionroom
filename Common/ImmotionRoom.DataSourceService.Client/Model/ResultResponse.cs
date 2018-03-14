namespace ImmotionAR.ImmotionRoom.DataSource.ControlClient.Model
{
    public class ResultResponse<T> : BaseResponse
    {
        public T Data { get; set; }
    }
}
