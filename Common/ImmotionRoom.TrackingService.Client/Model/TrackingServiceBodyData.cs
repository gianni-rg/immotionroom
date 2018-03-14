namespace ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model
{
    using System.Collections.Generic;

    public class TrackingServiceBodyData
    {
        #region Properties

        public ulong Id { get; set; }

        public int NumberOfMergedBodies
        {
            get { return DataSources != null ? DataSources.Count : 0; }
        }

        /// <summary>
        ///     Gets the joints of this Body
        /// </summary>
        /// <value>The joints</value>
        public IDictionary<TrackingServiceBodyJointTypes, TrackingServiceBodyJointData> Joints { get; private set; }

        /// <summary>
        ///     Position of the Body
        /// </summary>
        public TrackingServiceVector3 Position { get; set; }

        /// <summary>
        ///     Data sources that provide data for the joints of this Body
        /// </summary>
        public IList<byte> DataSources { get; set; }

        /// <summary>
        ///     Gets the recognized gestures for this Body
        /// </summary>
        /// <value>The joints</value>
        public IDictionary<TrackingServiceBodyGestureTypes, TrackingServiceBodyGesture> Gestures { get; private set; }

        /// <summary>
        ///     Gets information about the left hand (if enabled). Otherwise this property is always null.
        /// </summary>
        public TrackingServiceHandData LeftHand { get; set; }

        /// <summary>
        ///     Gets information about the right hand (if enabled). Otherwise this property is always null.
        /// </summary>
        public TrackingServiceHandData RightHand { get; set; }

        /// <summary>
        ///     Gives information about the position of the body in respect of the scene, if the body is
        ///     outside any of the side of the field of view of a DataSource. This value is meaningful
        ///     when used in Calibration/Diagnostic for each raw data stream only. For merged stream,
        ///     use Scene boundaries and Virtual Game Area limits.
        /// </summary>
        public TrackingServiceSceneClippedEdges ClippedEdges { get; set; }

        #endregion

        #region Constructor

        public TrackingServiceBodyData()
        {
            Joints = new Dictionary<TrackingServiceBodyJointTypes, TrackingServiceBodyJointData>(TrackingServiceBodyJointTypesComparer.Instance);
            Gestures = new Dictionary<TrackingServiceBodyGestureTypes, TrackingServiceBodyGesture>(TrackingServiceBodyGestureTypesComparer.Instance);
            DataSources = new List<byte>();
            Position = new TrackingServiceVector3();
        }

        #endregion
    }
}
