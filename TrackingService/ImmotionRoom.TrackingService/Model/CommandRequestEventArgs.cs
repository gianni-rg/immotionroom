namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    using System;

    public abstract class CommandRequestEventArgs : EventArgs
    {
        public string RequestId { get; set; }
    }
}
