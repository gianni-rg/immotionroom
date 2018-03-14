namespace ImmotionAR.ImmotionRoom.TrackingEngine.Model
{
    /// <summary>
    ///     Parameters for the detection of a player step performed with a single knee
    /// </summary>
    internal class StepDetectorParams
    {
        #region Public properties

        /// <summary>
        ///     Gets or sets the minimum movement amplitude.
        /// </summary>
        /// <value>The minimum movement amplitude.</value>
        public float MinMovementAmplitude { get; set; }

        /// <summary>
        ///     Gets or sets the max movement amplitude.
        /// </summary>
        /// <value>The max movement amplitude.</value>
        public float MaxMovementAmplitude { get; set; }

        /// <summary>
        ///     Gets or sets the minimum stepping time.
        /// </summary>
        /// <value>The minimum stepping time.</value>
        public float MinimumSteppingTime { get; set; }

        /// <summary>
        ///     Gets or sets the maximum stepping time.
        /// </summary>
        /// <value>The maximum stepping time.</value>
        public float MaximumSteppingTime { get; set; }

        /// <summary>
        ///     Gets or sets the minimum raisings number.
        /// </summary>
        /// <value>The minimum raisings number.</value>
        public int MinimumRaisings { get; set; }

        /// <summary>
        ///     Gets or sets the angle tolerance.
        /// </summary>
        /// <value>The angle tolerance.</value>
        public float AngleTolerance { get; set; }

        /// <summary>
        ///     Gets or sets the minimum raisings for prediction.
        /// </summary>
        /// <value>The minimum raisings for prediction.</value>
        public int MinimumRaisingsForPrediction { get; set; }

        /// <summary>
        ///     Gets or sets the angle tolerance for prediction.
        /// </summary>
        /// <value>The angle tolerance for prediction.</value>
        public float AngleToleranceForPrediction { get; set; }

        #endregion

        /// <summary>
        ///     Initializes a new instance of the <see cref="StepDetectorParams" /> class.
        /// </summary>
        public StepDetectorParams()
        {
            MinMovementAmplitude = 0;
            MaxMovementAmplitude = 0;
            MinimumSteppingTime = 0;
            MaximumSteppingTime = 0;
            MinimumRaisings = 0;
            AngleTolerance = 0;
            MinimumRaisingsForPrediction = 0;
            AngleToleranceForPrediction = 0;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="StepDetectorParams" /> class.
        /// </summary>
        /// <param name="minMovementAmplitude">Minimum knee movement to be considered as not noise, in meters</param>
        /// <param name="maxMovementAmplitude">Maximum knee movement to be considered as not glitch, in meters</param>
        /// <param name="minimumSteppingTime">
        ///     Minimum consecutive movement time, so that knee motion can be considered as not
        ///     noise, in seconds
        /// </param>
        /// <param name="maximumSteppingTime">
        ///     Maximum consecutive movement time, so that knee motion can be considered as not slow
        ///     movement, in seconds
        /// </param>
        /// <param name="minimumRaisings">Minimum consecutive raising movements to consider a knee motion as a step</param>
        /// <param name="angleTolerance">Angle tolerance of consecutive raising movements, to be considered coherent movement</param>
        /// <param name="minimumRaisingsForPrediction">
        ///     Minimum consecutive raising movements to consider a knee motion as a
        ///     possible predicted step
        /// </param>
        /// <param name="angleToleranceForPrediction">
        ///     Angle tolerance of reference angle to consider a predicted step of a knee to
        ///     be coherent with actual step of the other knee
        /// </param>
        public StepDetectorParams(float minMovementAmplitude, float maxMovementAmplitude, float minimumSteppingTime,
            float maximumSteppingTime, int minimumRaisings, float angleTolerance,
            int minimumRaisingsForPrediction, float angleToleranceForPrediction)
        {
            MinMovementAmplitude = minMovementAmplitude;
            MaxMovementAmplitude = maxMovementAmplitude;
            MinimumSteppingTime = minimumSteppingTime;
            MaximumSteppingTime = maximumSteppingTime;
            MinimumRaisings = minimumRaisings;
            AngleTolerance = angleTolerance;
            MinimumRaisingsForPrediction = minimumRaisingsForPrediction;
            AngleToleranceForPrediction = angleToleranceForPrediction;
        }
    }
}