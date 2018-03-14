namespace ImmotionAR.ImmotionRoom.TrackingEngine.Model
{
    /// <summary>
    ///     Data about a detected step movement
    /// </summary>
    internal class StepDetectedData
    {
        #region Public properties

        /// <summary>
        ///     Gets or sets a value indicating whether data inside this class is valid
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is relative to the left knee.
        /// </summary>
        /// <value><c>true</c> if this instance is relative to the left knee; otherwise, <c>false</c>.</value>
        public bool IsLeft { get; set; }

        /// <summary>
        ///     Gets or sets the step amplitude.
        /// </summary>
        /// <value>The step amplitude.</value>
        public float StepAmplitude { get; set; }

        /// <summary>
        ///     Gets or sets the duration of the step.
        /// </summary>
        /// <value>The duration of the step.</value>
        public float StepDuration { get; set; }

        /// <summary>
        ///     Gets or sets the knee angle.
        /// </summary>
        /// <value>The knee angle.</value>
        public float KneeAngle { get; set; }

        #endregion

        /// <summary>
        ///     Initializes a new instance of the <see cref="StepDetectedData" /> class.
        /// </summary>
        public StepDetectedData()
        {
            IsValid = false;
            StepAmplitude = 0;
            StepDuration = 0;
            KneeAngle = 0f;
            IsLeft = true;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="StepDetectedData" /> class.
        /// </summary>
        /// <param name="isLeft">True if step is from left knee, false otherwise</param>
        /// <param name="amplitude">Amplitude of step</param>
        /// <param name="duration">Duration of step</param>
        /// <param name="kneeAngle">Knee angle</param>
        public StepDetectedData(bool isLeft, float amplitude, float duration, float kneeAngle)
        {
            IsValid = true;
            StepAmplitude = amplitude;
            StepDuration = duration;
            KneeAngle = kneeAngle;
            IsLeft = isLeft;
        }
    }
}
