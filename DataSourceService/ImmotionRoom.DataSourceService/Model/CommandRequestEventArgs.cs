namespace ImmotionAR.ImmotionRoom.DataSourceService.Model
{
    using System;

    public abstract class CommandRequestEventArgs : EventArgs
    {
        public string RequestId { get; set; }
    }
}
