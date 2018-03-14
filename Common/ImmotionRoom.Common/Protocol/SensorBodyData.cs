namespace ImmotionAR.ImmotionRoom.Protocol
{
    using System;
    using System.Collections.Generic;

    public class SensorBodyData
    {
        [CLSCompliant(false)]
        public ulong TrackingId { get; set; }

        public SceneClippedEdges ClippedEdges { get; set; }

        public SensorHandData LeftHand { get; set; }
        public SensorHandData RightHand { get; set; }

        public IDictionary<SensorBodyJointTypes, SensorBodyJointData> Joints { get; private set; }

        public SensorBodyData()
        {
            Joints = new Dictionary<SensorBodyJointTypes, SensorBodyJointData>(SensorBodyJointTypesComparer.Instance);
        }
    }
}
