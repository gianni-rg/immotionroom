namespace ImmotionAR.ImmotionRoom.TrackingEngine.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ImmotionAR.ImmotionRoom.TrackingEngine.Model;
    using ImmotionAR.ImmotionRoom.TrackingEngine.Tools;

    /// <summary>
    /// Provides methods to calculate a joint confidence in an advanced manner, based on Joint history and nearby joints
    /// </summary>
    internal class JointConfidenceCalculator
    {
        /// <summary>
        /// The size of the angular confidence slice.
        /// This is useful to verify how a body is frontal to a particular DataSource and modify the body joints confidence accordingly,
        /// because the less he is frontal, the less the joints are reliable.
        /// The [-pi, +pi] space is partitioned in confidence slices, or better, the space [0, pi], the range of abs angular offset
        /// is partitioned in slices of a certain angle.
        /// If the offset stays in the first slice, the confidence is 1.
        /// If it stays in the second, the confidence is
        /// </summary>
        //private static readonly float m_AngularConfidenceSliceSize;

        // Dictionary Enum Optimization
        // See: http://www.codeproject.com/Articles/33528/Accelerating-Enum-Based-Dictionaries-with-Generic
        // See: http://www.somasim.com/blog/2015/08/c-performance-tips-for-unity-part-2-structs-and-enums/
        // See: http://stackoverflow.com/questions/7143948/efficiency-of-using-iequalitycomparer-in-dictionary-vs-hashcode-and-equals

        /// <summary>
        /// The sibling joints for each joint. The sibling joints can influence a joint confidence
        /// (e.g. if shoulder joint is not tracked, elbow joint can't be reliable)
        /// </summary>
        private static readonly Dictionary<BodyJointTypes, BodyJointTypes[]> m_SiblingJoints = new Dictionary<BodyJointTypes, BodyJointTypes[]>(BodyJointTypesComparer.Instance)
	{
		{ BodyJointTypes.FootLeft, new BodyJointTypes[] {BodyJointTypes.AnkleLeft, BodyJointTypes.KneeLeft, BodyJointTypes.HipLeft }},
		{ BodyJointTypes.AnkleLeft, new BodyJointTypes[] {BodyJointTypes.KneeLeft, BodyJointTypes.HipLeft }},
		{ BodyJointTypes.KneeLeft, new BodyJointTypes[] {BodyJointTypes.HipLeft }},
		{ BodyJointTypes.HipLeft, new BodyJointTypes[] { }},
		
		{ BodyJointTypes.FootRight, new BodyJointTypes[] {BodyJointTypes.AnkleRight, BodyJointTypes.KneeRight, BodyJointTypes.HipRight }},
		{ BodyJointTypes.AnkleRight, new BodyJointTypes[] {BodyJointTypes.KneeRight, BodyJointTypes.HipRight }},
		{ BodyJointTypes.KneeRight, new BodyJointTypes[] {BodyJointTypes.HipRight }},
		{ BodyJointTypes.HipRight, new BodyJointTypes[] { }},
		
		{ BodyJointTypes.HandTipLeft, new BodyJointTypes[] {BodyJointTypes.HandLeft, BodyJointTypes.WristLeft, BodyJointTypes.ElbowLeft, BodyJointTypes.ShoulderLeft }},
		{ BodyJointTypes.ThumbLeft, new BodyJointTypes[] {BodyJointTypes.HandLeft, BodyJointTypes.WristLeft, BodyJointTypes.ElbowLeft, BodyJointTypes.ShoulderLeft }},
		{ BodyJointTypes.HandLeft, new BodyJointTypes[] {BodyJointTypes.WristLeft, BodyJointTypes.ElbowLeft, BodyJointTypes.ShoulderLeft }},
		{ BodyJointTypes.WristLeft, new BodyJointTypes[] {BodyJointTypes.ElbowLeft, BodyJointTypes.ShoulderLeft }},
		{ BodyJointTypes.ElbowLeft, new BodyJointTypes[] {BodyJointTypes.ShoulderLeft }},
		{ BodyJointTypes.ShoulderLeft, new BodyJointTypes[] { }},
		
		{ BodyJointTypes.HandTipRight, new BodyJointTypes[] {BodyJointTypes.HandRight, BodyJointTypes.WristRight, BodyJointTypes.ElbowRight, BodyJointTypes.ShoulderRight }},
		{ BodyJointTypes.ThumbRight, new BodyJointTypes[] {BodyJointTypes.HandRight, BodyJointTypes.WristRight, BodyJointTypes.ElbowRight, BodyJointTypes.ShoulderRight }},
		{ BodyJointTypes.HandRight, new BodyJointTypes[] {BodyJointTypes.WristRight, BodyJointTypes.ElbowRight, BodyJointTypes.ShoulderRight }},
		{ BodyJointTypes.WristRight, new BodyJointTypes[] {BodyJointTypes.ElbowRight, BodyJointTypes.ShoulderRight }},
		{ BodyJointTypes.ElbowRight, new BodyJointTypes[] {BodyJointTypes.ShoulderRight }},
		{ BodyJointTypes.ShoulderRight, new BodyJointTypes[] { }},
		
		{ BodyJointTypes.SpineBase, new BodyJointTypes[] { }},
		{ BodyJointTypes.SpineMid, new BodyJointTypes[] { }},
		{ BodyJointTypes.SpineShoulder, new BodyJointTypes[] { }},
		{ BodyJointTypes.Neck, new BodyJointTypes[] { }},
		{ BodyJointTypes.Head, new BodyJointTypes[] { }}
	};

        /// <summary>
        /// Calculates the reliable joint confidence, using time information (joint confidence in last joints)
        /// and spatial information (the confidence of its siblings joints)
        /// </summary>
        /// <returns>The reliable joint confidence.</returns>
        /// <param name="jt">Joint type</param>
        /// <param name="history">History of man position</param>
        /// <param name="lastMan">Last calculated and filtered merged man for last frame</param>
        public static float CalculateReliableJointConfidence(BodyJointTypes jt, BodyDataHistory history, BodyData lastMan)
        {
            //get joint confidence
            float confidence = history.GetRunningAverageJointConfidence(jt);

            //make an average of sibling confidence
            float siblingConfidence = 0.0f;

            if (m_SiblingJoints[jt].Length >= 1)
            {

                foreach (BodyJointTypes sibJt in m_SiblingJoints[jt])
                    siblingConfidence += history.GetRunningAverageJointConfidence(sibJt);

                siblingConfidence /= m_SiblingJoints[jt].Length;

                //put minimum of siblingConfidence at 0.4
                siblingConfidence = Math.Max(siblingConfidence, 0.4f);

                //multiply confidence by sibling confidence
                confidence = confidence * siblingConfidence;
            }

            //if this joint man is seen by a camera that is not frontal to the body, data is unreliable:
            //weight the confidence depending on the perpendicularity of body wrt the reading camera
            confidence *= history.OrientationConfidence;

            //if joint position in current frame is very very different from joint position of merged man in last frame,
            //reduce the confidence in this joint by a factor of ten
            if (history != null && history.History != null && lastMan != null && history.History.Count >= 1)
                if (Vector3.Distance(lastMan.Joints[jt].Position, history.History.Last().Joints[jt].Position) > 0.33f)
                    confidence *= 0.075f;

            //return computed confidence
            return confidence;
        }
        //static int Z;

        /// <summary>
        /// Calculates the joint confidence based on how the body whose joint belongs to stays in front of the DataSource
        /// </summary>
        /// <returns>The reliable joint confidence.</returns>
        /// <param name="history">History of man position</param>
        /// <param name="lastMan">Last calculated and filtered merged man for last frame</param>
        public static float CalculateOrientationBodyDataConfidence(BodyDataHistory history, BodyData lastMan)
        {
            //if this joint man is seen by a camera that is not frontal to the body, data is unreliable:
            //weight the confidence depending on the perpendicularity of body wrt the reading camera

            //calculate last man orientation in the frame of reference of the DataSources of this DataSource
            if (lastMan != null && history != null && history.History != null && history.History.Count > 0)
            {
                //		    && history.History[0].SourceDataSourceTransformationMatrix.m00 == 1) {
                //get this DataSource transformation matrix and compute its inverse
                Matrix4x4 invTransf = history.History.Last().DataSourceTransformationMatrix.Inverse;

                //compute lastman shoulder joints in this DataSource space
                Vector3 shoulderLeftPos = invTransf.MultiplyPoint3x4(lastMan.Joints[BodyJointTypes.ShoulderLeft].Position),
                shoulderRightPos = invTransf.MultiplyPoint3x4(lastMan.Joints[BodyJointTypes.ShoulderRight].Position);

                //calculate orientation in XZ plane, in range [-pi, pi] of this DataSource
                float orientation = FancyUtilities.BetweenJointsXZOrientation(shoulderLeftPos, shoulderRightPos);

                orientation = Math.Abs(orientation);

                //calculate angular confidence based on orientation.
                //Notice that if angle is > 90°, confidence is almost null. But if it is > 150°, it has good confidence
                //Notice also that we use pi / 1.15f instead of pi / 2.0f to give little higher weights
                float orientationConfidence = (float) (orientation > Math.PI / 2 ? (orientation > Math.PI * 5 / 6 ? 0.1f : 0.05f) : (Math.PI / 1.15f - orientation) / (Math.PI / 1.15f));

                //			if(Z++ % 100 == 0)
                //				Debug.Log("Orientation Confidence " + orientationConfidence);

                //return computed confidence
                return orientationConfidence;
            }
            else
                return 1.0f;
        }

    }
}