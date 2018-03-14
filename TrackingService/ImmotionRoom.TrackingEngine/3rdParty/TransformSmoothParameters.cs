namespace ImmotionAR.ImmotionRoom.TrackingEngine
{
    /// <summary>
    /// Transform smooth parameters, used by JointsPositionDoubleExponentialFilter class
    /// </summary>
    internal class TransformSmoothParameters
    {
        /// <summary>
        /// How much soothing will occur.  Will lag when too high.
        /// Smoothing = [0..1], lower values is closer to the raw data and more noisy
        /// </summary>
        private float m_Smoothing;

        /// <summary>
        /// How much to correct back from prediction.  Can make things springy.
        /// Correction = [0..1], higher values correct faster and feel more responsive
        /// </summary>
        private float m_Correction;

        /// <summary>
        /// Amount of prediction into the future to use. Can over shoot when too high.
        /// Prediction = [0..n], how many frames into the future we want to predict
        /// </summary>
        private float m_Prediction;

        /// <summary>
        /// Size of the radius where jitter is remove.
        /// JitterRadius = The deviation distance in m that defines jitter.
        /// </summary>
        private float m_JitterRadius;

        /// <summary>
        /// MaxDeviation = The maximum distance in m that filtered positions are allowed to deviate from raw data.
        /// Size of the max prediction radius Can snap back to noisy data when too high
        /// </summary>
        private float m_MaxDeviationRadius;

        #region Public properties

        /// <summary>
        /// Gets or sets the smoothing.
        /// </summary>
        /// <value>The smoothing.</value>
        public float Smoothing
        {
            get
            {
                return m_Smoothing;
            }
            set
            {
                m_Smoothing = value;

            }
        }

        /// <summary>
        /// Gets or sets the correction.
        /// </summary>
        /// <value>The correction.</value>
        public float Correction
        {
            get
            {
                return m_Correction;
            }
            set
            {
                m_Correction = value;

            }
        }

        /// <summary>
        /// Gets or sets the prediction.
        /// </summary>
        /// <value>The prediction.</value>
        public float Prediction
        {
            get
            {
                return m_Prediction;
            }
            set
            {
                m_Prediction = value;

            }
        }

        /// <summary>
        /// Gets or sets the jitter radius.
        /// </summary>
        /// <value>The jitter radius.</value>
        public float JitterRadius
        {
            get
            {
                return m_JitterRadius;
            }
            set
            {
                m_JitterRadius = value;

            }
        }

        /// <summary>
        /// Gets or sets the max deviation radius.
        /// </summary>
        /// <value>The max deviation radius.</value>
        public float MaxDeviationRadius
        {
            get
            {
                return m_MaxDeviationRadius;
            }
            set
            {
                m_MaxDeviationRadius = value;

            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformSmoothParameters"/> struct.
        /// </summary>
        public TransformSmoothParameters()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformSmoothParameters"/> class.
        /// </summary>
        /// <param name="smoothing">Smoothing = [0..1], lower values is closer to the raw data and more noisy</param>
        /// <param name="correction">Correction = [0..1], higher values correct faster and feel more responsive</param>
        /// <param name="prediction">Prediction = [0..n], how many frames into the future we want to predict</param>
        /// <param name="jitterRadius">JitterRadius = The deviation distance in m that defines jitter</param>
        /// <param name="maxDeviationRadius">MaxDeviation = The maximum distance in m that filtered positions are allowed to deviate from raw data</param>
        public TransformSmoothParameters(float smoothing, float correction, float prediction, float jitterRadius, float maxDeviationRadius)
        {
            m_Smoothing = smoothing;
            m_Correction = correction;
            m_Prediction = prediction;
            m_JitterRadius = jitterRadius;
            m_MaxDeviationRadius = maxDeviationRadius;
        }

    }
}