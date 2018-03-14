namespace ImmotionAR.ImmotionRoom.TrackingEngine.Model
{
    using System.Collections.Generic;

    public enum PlayerWalkingDetectorTypes
    {
        Unknown,
        KnaivePlayerWalkingDetector
    }

    public class WalkingDetectionConfiguration
    {
        public bool Enabled { get; set; }
        public PlayerWalkingDetectorTypes WalkingDetector { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }
}
