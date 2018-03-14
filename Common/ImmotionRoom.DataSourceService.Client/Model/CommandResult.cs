namespace ImmotionAR.ImmotionRoom.DataSource.ControlClient.Model
{
    using System;

    public class CommandResult<T>
    {
        public T Data { get; set; }
        public string RequestId { get; set; }
        public bool Read { get; set; }
        public DateTime Timestamp { get; set; }

        public CommandResult()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
}
