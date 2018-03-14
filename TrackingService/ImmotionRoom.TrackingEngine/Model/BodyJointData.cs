namespace ImmotionAR.ImmotionRoom.TrackingEngine.Model
{
    /// <summary>
    ///     Represent data used for each joint by this program
    /// </summary>
    public class BodyJointData
    {
        #region Private fields

        /// <summary>
        ///     Joint 3D Position, in master kinect frame of reference
        /// </summary>
        private Vector3 m_Position;

        /// <summary>
        ///     Joint 3D Orientation, in master kinect frame of reference
        /// </summary>
        private Vector4 m_Orientation;

        /// <summary>
        ///     Confidence for the detected position, in range [0, 1]
        /// </summary>
        private float m_Confidence;

        #endregion

        #region Properties

        public BodyJointTypes JointType { get; set; }

        public Vector3 Position
        {
            get { return m_Position; }
            set { m_Position = value; }
        }

        public Vector4 Orientation
        {
            get { return m_Orientation; }
            set { m_Orientation = value; }
        }

        public float Confidence
        {
            get { return m_Confidence; }
            set { m_Confidence = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the <see cref="BodyJointData" /> class.
        /// </summary>
        public BodyJointData(BodyJointTypes type)
        {
            m_Position = new Vector3();
            m_Orientation = new Vector4();
            m_Confidence = 0f;
            JointType = type;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BodyJointData" /> class.
        /// </summary>
        /// <param name="position">Position of the joint</param>
        /// <param name="confidence">Confidence of the position param</param>
        /// <param name="type"></param>
        public BodyJointData(Vector3 position, float confidence, BodyJointTypes type)
        {
            m_Position = position;
            m_Confidence = confidence;
            m_Orientation = new Vector4();
            JointType = type;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BodyJointData" /> class.
        /// </summary>
        /// <param name="position">Position of the joint</param>
        /// <param name="orientation">Orientation of the joint</param>
        /// <param name="confidence">Confidence of the position param</param>
        /// <param name="type"></param>
        public BodyJointData(Vector3 position, Vector4 orientation, float confidence, BodyJointTypes type)
        {
            m_Position = position;
            m_Confidence = confidence;
            m_Orientation = orientation;
            JointType = type;
        }
        #endregion

        #region Methods

        /// <summary>
        ///     Clone this instance.
        /// </summary>
        public object Clone()
        {
            return new BodyJointData(m_Position, m_Orientation, m_Confidence, JointType);
        }

        #endregion
    }
}