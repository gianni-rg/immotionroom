namespace ImmotionAR.ImmotionRoom.TrackingEngine.Walking
{
    using System;
    using System.Collections.Generic;
    using Model;

    /// <summary>
    ///     Defines a base class for objects that can detects walking movement using a player knee
    /// </summary>
    internal abstract class KneeWalkingDetector : IKneeWalkingDetector
    {
        /// <summary>
        ///     Joint type of the knee this detector should use
        /// </summary>
        protected readonly BodyJointTypes m_kneeJointType;

        /// <summary>
        ///     Joint type of the hip this detector should use
        /// </summary>
        protected readonly BodyJointTypes m_hipJointType;

        /// <summary>
        ///     Parameters for walking detection this object should use
        /// </summary>
        protected KneeWalkingDetectorParams m_walkingDetectionParams;

        /// <summary>
        ///     Last walking detection results
        /// </summary>
        protected KneeWalkingDetection m_currentDetection;

        #region Constructor

        /// <summary>
        ///     Creates a walking movement detector that uses player knee for its analysis
        /// </summary>
        /// <param name="isLeft">True if detection should regard left knee; false for right knee</param>
        /// <param name="detectionParameters">Parameters for walking detection</param>
        public KneeWalkingDetector(bool isLeft, KneeWalkingDetectorParams detectionParameters)
        {
            //assign joints to be analyzed depending on the side of the body we want to consider
            if (isLeft)
            {
                m_kneeJointType = BodyJointTypes.KneeLeft;
                m_hipJointType = BodyJointTypes.HipLeft;
            }
            else
            {
                m_kneeJointType = BodyJointTypes.KneeRight;
                m_hipJointType = BodyJointTypes.HipRight;
            }

            //copy parameters
            m_walkingDetectionParams = detectionParameters;
        }

        #endregion

        #region IKneeWalkingDetector Methods

        /// <summary>
        ///     Get last analysis result of this object about user walking gesture
        /// </summary>
        public KneeWalkingDetection CurrentDetection
        {
            get { return m_currentDetection; }
        }

        /// <summary>
        ///     Get past frame analysis result of this object about user walking gesture
        /// </summary>
        public abstract KneeWalkingDetection PreviousDetection { get; }

        /// <summary>
        ///     Perform new detection of walking movement, because new joint data is arrived.
        ///     It is advised to call this function at a very regular interval
        /// </summary>
        /// <param name="timestamp">Time since a common reference event, like the start of the program</param>
        /// <param name="body">New body joint data</param>
        public abstract void UpdateDetection(TimeSpan timestamp, BodyData body);

        /// <summary>
        ///     Serialize object info into a dictionary, for debugging purposes
        /// </summary>
        /// <returns>Object serialization into a dictionary of dictionaries (infos are subdivided into groups)</returns>
        public abstract Dictionary<string, string> DictionarizeInfo();

        #endregion
    }
}