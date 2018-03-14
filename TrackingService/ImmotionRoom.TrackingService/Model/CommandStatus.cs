namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    public class CommandStatus
    {
        public string Id { get; set; }
        public CommandRequestStatus Status { get; set; }

        public bool IsError
        {
            get { return Status == CommandRequestStatus.Error || Status == CommandRequestStatus.Undefined; }
        }

        public string ErrorDescription { get; set; }
        public int ErrorCode { get; set; }
    }
}
