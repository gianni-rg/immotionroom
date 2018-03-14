namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    using System;
    using TrackingEngine.Model;

    public class DataFrameReadyEventArgs : EventArgs
    {
        public SceneFrame Frame { get; private set; }

        public DataFrameReadyEventArgs(SceneFrame frame)
        {
            Frame = frame;
        }
    }
}
