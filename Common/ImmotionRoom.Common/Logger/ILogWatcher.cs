namespace ImmotionAR.ImmotionRoom.Logger
{
    using System;
    using System.Collections.Generic;

    public interface ILogWatcher
    {
        event EventHandler Updated;
        List<string> LogContent { get; set; }
        void NewEventsAvaialble();
    }
}