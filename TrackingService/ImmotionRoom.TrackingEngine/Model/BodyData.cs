namespace ImmotionAR.ImmotionRoom.TrackingEngine.Model
{
    using System.Collections.Generic;

    /// <summary>
    ///     BodyData: a man made of joints, as seen by a skeletal tracker.
    ///     It is also useful for creating a skeleton starting only by joints position.
    /// </summary>
    public class BodyData
    {
        #region Private fields

        /// <summary>
        ///     Number of bodies that are actually merged to produce this BodyData
        /// </summary>
        /// <summary>
        ///     ID of the body
        /// </summary>
        private readonly ulong m_Id;

        /// <summary>
        ///     Trasformation that must be applied to joints to put them in the world frame of reference
        /// </summary>
        private Matrix4x4 m_Transform;

        /// <summary>
        ///     Actual joints data, extracted from the source object and with transformation (if any) applied
        /// </summary>
        private IDictionary<BodyJointTypes, BodyJointData> m_JointsData;

        /// <summary>
        ///     Data sources that provide data for the joints of this object
        /// </summary>
        private readonly IList<byte> m_DataSources;

        /// <summary>
        ///     Recognized gestures for this Body
        /// </summary>
        private IDictionary<BodyGestureTypes, BodyGesture> m_Gestures;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the id of this body
        /// </summary>
        /// <value>The identifier of this body</value>
        public ulong Id
        {
            get { return m_Id; }
        }

        /// <summary>
        ///     Gets the DataSource transformation matrix, the matrix that maps this slave DataSource BodyData to the master
        ///     DataSource
        /// </summary>
        public Matrix4x4 DataSourceTransformationMatrix
        {
            get { return m_Transform; }
        }

        /// <summary>
        ///     Gets the joints of this man
        /// </summary>
        /// <value>The joints</value>
        public IDictionary<BodyJointTypes, BodyJointData> Joints
        {
            get { return m_JointsData; }
        }

        /// <summary>
        ///     Gets the body centroid
        /// </summary>
        /// <value>The body centroid</value>
        public Vector3 Centroid
        {
            get
            {
                var sum = Vector3.Zero;

                if (m_JointsData.Count > 0)
                {
                    foreach (var pos in m_JointsData.Values)
                    {
                        sum += pos.Position;
                    }

                    sum /= m_JointsData.Count;
                }

                return sum;
            }
        }

        /// <summary>
        ///     Gets the centroid of the joints that usually are more reliable inside the skeletal tracker
        ///     (e.g. the feet are always noisy)
        /// </summary>
        /// <value>The centroid of the stable joints of the tracked body</value>
        public Vector3 StableCentroid
        {
            get
            {
                var sum = Vector3.Zero;

                if (m_JointsData.Count > 0)
                {
                    foreach (var jt in BodyConstants.StableJoints)
                    {
                        sum += m_JointsData[jt].Position;
                    }

                    sum /= m_JointsData.Count;
                }

                return sum;
            }
        }

        /// <summary>
        ///     Get the IDs of the data sources that provide data for the joints of this object
        /// </summary>
        public IList<byte> DataSources
        {
            get { return m_DataSources; }
        }

        /// <summary>
        ///     Gets the recognized gesture for this Body
        /// </summary>
        /// <value>The joints</value>
        public IDictionary<BodyGestureTypes, BodyGesture> Gestures
        {
            get { return m_Gestures; }
        }

        /// <summary>
        ///     Gets information about the left hand (if enabled). Otherwise this property is always null.
        /// </summary>
        public BodyHandData LeftHand { get; set; }

        /// <summary>
        ///     Gets information about the right hand (if enabled). Otherwise this property is always null.
        /// </summary>
        public BodyHandData RightHand { get; set; }

        /// <summary>
        ///     Gives information about the position of the body in respect of the frame, if the body is
        ///     outside any of the side of the field of view of a DataSource. This value is meaningful
        ///     when used in Calibration/Diagnostic for each raw data stream only. For merged stream,
        ///     use Scene boundaries and Virtual Game Area limits.
        /// </summary>
        public FrameClippedEdges ClippedEdges { get; set; }
        #endregion

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the <see cref="BodyData" /> class.
        /// </summary>
        /// <param name="id">Id of this joints man</param>
        /// <param name="dataSources">Data sources that provide data for the joints of this body</param>
        public BodyData(ulong id, IList<byte> dataSources)
        {
            m_Id = id;
            m_DataSources = dataSources;
            m_Transform = Matrix4x4.Identity;
            m_JointsData = new Dictionary<BodyJointTypes, BodyJointData>(BodyJointTypesComparer.Instance);
            m_Gestures = new Dictionary<BodyGestureTypes, BodyGesture>(BodyGestureTypesComparer.Instance);
            UpdateData();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BodyData" /> class.
        /// </summary>
        /// <param name="id">Id of this joints man</param>
        /// <param name="jointsData">Joints data, computed by hand</param>
        /// <param name="dataSources">Data sources that provide data for the joints of this body</param>
        public BodyData(ulong id, IDictionary<BodyJointTypes, BodyJointData> jointsData, IList<byte> dataSources)
        {
            m_Id = id;
            m_DataSources = dataSources;
            m_Transform = Matrix4x4.Identity;
            m_JointsData = jointsData;
            m_Gestures = new Dictionary<BodyGestureTypes, BodyGesture>(BodyGestureTypesComparer.Instance);

            UpdateData();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BodyData" /> class.
        /// </summary>
        /// <param name="body">Joint Man that has to be copied inside this instance</param>
        /// <param name="newTransform">New transformation for the joints</param>
        /// <param name="dataSources">Data sources that provide data for the joints of this body</param>
        public BodyData(BodyData body, Matrix4x4 newTransform, IList<byte> dataSources)
        {
            m_Id = body.Id;
            m_DataSources = dataSources;
            m_Transform = newTransform;
            m_JointsData = body.Joints;
            m_Gestures = new Dictionary<BodyGestureTypes, BodyGesture>(BodyGestureTypesComparer.Instance);

            UpdateData();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BodyData" /> class.
        /// </summary>
        /// <param name="body">Joint Man that has to be copied inside this instance</param>
        /// <param name="newTransform">New transformation for the joints</param>
        /// <param name="dataSources">Data sources that provide data for the joints of this body</param>
        /// <param name="gestures">Detected gestures for this body</param>
        public BodyData(BodyData body, Matrix4x4 newTransform, IList<byte> dataSources, IDictionary<BodyGestureTypes, BodyGesture> gestures)
        {
            m_Id = body.Id;
            m_DataSources = dataSources;
            m_Transform = newTransform;
            m_JointsData = body.Joints;
            m_Gestures = gestures;

            UpdateData();
        }

        #endregion

        #region Private methods

        /// <summary>
        ///     Updates joints data, reading them from the actual source
        /// </summary>
        private void UpdateData()
        {
            // Detect joints source type and call appropriate function
            if (m_JointsData != null && m_JointsData.Count > 0)
            {
                UpdateDataFromJoints();
            }
        }

        /// <summary>
        ///     Updates the data from raw joints positions
        /// </summary>
        private void UpdateDataFromJoints()
        {
            var trasformedJoints = new Dictionary<BodyJointTypes, BodyJointData>(BodyJointTypesComparer.Instance);

            // Transform joints using the provided transformation
            foreach (var jointElem in m_JointsData)
            {
                trasformedJoints.Add(jointElem.Key, new BodyJointData(m_Transform.MultiplyPoint3x4(jointElem.Value.Position), jointElem.Value.Confidence, jointElem.Value.JointType));
            }

            m_JointsData = trasformedJoints;
        }

        #endregion
    }
}