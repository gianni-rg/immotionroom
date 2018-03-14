namespace ImmotionAR.ImmotionRoom.Logger
{
    using System;
    using System.Collections.Generic;

    public class LogWatcher : ILogWatcher
    {
        public event EventHandler Updated;
        public List<string> LogContent { get; set; }

        public void NewEventsAvaialble()
        {
            var localHandler = Updated;
            if (localHandler != null)
            {
                localHandler(this, EventArgs.Empty);
            }
        }
    }
}