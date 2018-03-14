namespace ImmotionAR.ImmotionRoom.DataSource.ControlClient.Model
{
    public abstract class BaseResponse
    {
        public string ID { get; set; }
        public bool IsError { get; set; }
        public string Error { get; set; }
        public int ErrorCode { get; set; }
    }
}
