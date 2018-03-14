namespace ImmotionAR.ImmotionRoom.TrackingEngine.Model
{
    /// <summary>
    ///     HandData: hand information as seen from a Tracking Sensor.
    /// </summary>
    public class BodyHandData
    {
        public float Confidence { get; set; }
        public BodyHandState State { get; set; }
    }
}