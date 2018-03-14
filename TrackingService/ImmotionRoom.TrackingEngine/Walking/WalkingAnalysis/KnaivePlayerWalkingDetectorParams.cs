namespace ImmotionAR.ImmotionRoom.TrackingEngine.Walking
{
    public class KnaivePlayerWalkingDetectorSettings
    {
        public const string StillResetTime_Key = "Gestures.PlayerWalkingDetector.Knaive.StillResetTime";
        public const string WalkingAngleRunningAvgAlpha_Key = "Gestures.PlayerWalkingDetector.Knaive.WalkingAngleRunningAvgAlpha";
        public const string WalkingMagnitudeRunningAvgAlpha_Key = "Gestures.PlayerWalkingDetector.Knaive.WalkingMagnitudeRunningAvgAlpha";
        public const string WalkingAngleEstimationType_Key = "Gestures.PlayerWalkingDetector.Knaive.WalkingAngleEstimationType";
        public const string PlayerMovementDetectionThresh_Key = "Gestures.PlayerWalkingDetector.Knaive.PlayerMovementDetectionThresh";
        public const string PlayerMovementDetectionTimeThreshold_Key = "Gestures.PlayerWalkingDetector.Knaive.PlayerMovementDetectionTimeThreshold";
        public const string PlayerMovementDetectionRunningAvgAlpha_Key = "Gestures.PlayerWalkingDetector.Knaive.PlayerMovementDetectionRunningAvgAlpha";
        public const string Knee_StillToRisingThreshold_Key = "Gestures.PlayerWalkingDetector.Knaive.Knee.StillToRisingThreshold";
        public const string Knee_AnyStateToFallingThreshold_Key = "Gestures.PlayerWalkingDetector.Knaive.Knee.AnyStateToFallingThreshold";
        public const string Knee_AnyStateToStillThreshold_Key = "Gestures.PlayerWalkingDetector.Knaive.Knee.AnyStateToStillThreshold";
        public const string Knee_StillAngleThreshold_Key = "Gestures.PlayerWalkingDetector.Knaive.Knee.StillAngleThreshold";
        public const string Knee_TimeToTriggerMovement_Key = "Gestures.PlayerWalkingDetector.Knaive.Knee.TimeToTriggerMovement";
        public const string Knee_TimeToTriggerStillness_Key = "Gestures.PlayerWalkingDetector.Knaive.Knee.TimeToTriggerStillness";
        public const string Knee_FallingToRisingSpeedMultiplier_Key = "Gestures.PlayerWalkingDetector.Knaive.Knee.FallingToRisingSpeedMultiplier";
        public const string Knee_AlmostStillSpeed_Key = "Gestures.PlayerWalkingDetector.Knaive.Knee.AlmostStillSpeed";
        public const string Knee_RisingAngleTolerance_Key = "Gestures.PlayerWalkingDetector.Knaive.Knee.RisingAngleTolerance";
        public const string Knee_SpikeNoiseThreshold_Key = "Gestures.PlayerWalkingDetector.Knaive.Knee.SpikeNoiseThreshold";
        public const string Knee_TriggerToSpeedMultiplier_Key = "Gestures.PlayerWalkingDetector.Knaive.TriggerToSpeedMultiplier";
        public const string Knee_UseAcceleration_Key = "Gestures.PlayerWalkingDetector.Knaive.Knee.UseAcceleration";
        public const string Knee_EstimatedFrameRate_Key = "Gestures.PlayerWalkingDetector.Knaive.Knee.EstimatedFrameRate";

    }

    /// <summary>
    ///     Enumerates differnt type of possible estimations of player walking direction
    /// </summary>
    public enum WalkingDirectionEstimator
    {
        /// <summary>
        ///     Use walking knee direction
        /// </summary>
        Knee,

        /// <summary>
        ///     Use direction perpendicular to the line connecting the two hips
        /// </summary>
        Pelvis,

        /// <summary>
        ///     Use direction perpendicular to the line connecting the two shoulder
        /// </summary>
        Shoulders
    }

    /// <summary>
    ///     Parameters for walking movement detection for <see cref="KnaivePlayerWalkingDetector" /> class
    /// </summary>
    public struct KnaivePlayerWalkingDetectorParams
    {
        /// <summary>
        ///     Parameters for knee walking movement detection
        /// </summary>
        public KneeWalkingDetectorParams KneeDetectionParams;

        /// <summary>
        ///     Time of player non-walking after which the system resets itself, waiting for any walking leg (in seconds)
        /// </summary>
        public float StillResetTime;

        /// <summary>
        ///     How the walking direction has to be estimated
        /// </summary>
        public WalkingDirectionEstimator WalkingAngleEstimationType;

        /// <summary>
        ///     Alpha constant to perform running average of walking direction during knee raising
        ///     (1 means full update, 0 means full history)
        /// </summary>
        public float WalkingAngleRunningAvgAlpha;

        /// <summary>
        ///     Alpha constant to perform running average of walking speed modulus
        ///     (1 means full update, 0 means full history)
        /// </summary>
        public float WalkingMagnitudeRunningAvgAlpha;

        /// <summary>
        ///     Threshold for player movement detection: if too high, no movement gets detected; if too low, it will be too noisy
        /// </summary>
        public float PlayerMovementDetectionThresh;

        /// <summary>
        ///     Time threshold for player movement / non movement detection, in seconds:
        ///     it represent the seconds of continuous movement (or non movement) to switch state between moving/non moving
        /// </summary>
        public float PlayerMovementDetectionTimeThreshold;

        /// <summary>
        ///     Alpha constant to perform running average of player reference position for the player movement detection algorithm
        ///     (1 means full update, 0 means full history)
        /// </summary>
        public float PlayerMovementDetectionRunningAvgAlpha;
    }
}