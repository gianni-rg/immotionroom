namespace ImmotionAR.ImmotionRoom.TrackingEngine.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ImmotionAR.ImmotionRoom.TrackingEngine.Model;
    using ImmotionAR.ImmotionRoom.TrackingEngine.Tools;

    /// <summary>
    /// Provides methods to calculate a joint confidence in an advanced manner, based on Joint history and nearby joints.
    /// This is an eveolution of class <see cref="JointConfidenceCalculator"/> used by <see cref="MergingBodyReliable"/>
    /// </summary>
    internal class JointConfidenceCalculator2
    {

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
        /// The reliability of each body joint wrt the body orientation wrt the tracking boxes
        /// </summary>
        private static readonly Dictionary<BodyJointTypes, JointAngularReliabilityInfo> JointsReliability = new Dictionary<BodyJointTypes, JointAngularReliabilityInfo>(BodyJointTypesComparer.Instance)
	    {
		    { BodyJointTypes.FootLeft, new JointAngularReliabilityInfo(15* MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 95 * MathConstants.Deg2Rad)},
		    { BodyJointTypes.AnkleLeft, new JointAngularReliabilityInfo(15* MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 95 * MathConstants.Deg2Rad)},
		    { BodyJointTypes.KneeLeft, new JointAngularReliabilityInfo(15* MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 100 * MathConstants.Deg2Rad)},
		    { BodyJointTypes.HipLeft, new JointAngularReliabilityInfo(15* MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 100 * MathConstants.Deg2Rad)},
		
		    { BodyJointTypes.FootRight, new JointAngularReliabilityInfo(-15* MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 95 * MathConstants.Deg2Rad)},
		    { BodyJointTypes.AnkleRight, new JointAngularReliabilityInfo(-15* MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 95 * MathConstants.Deg2Rad)},
		    { BodyJointTypes.KneeRight, new JointAngularReliabilityInfo(-15* MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 100 * MathConstants.Deg2Rad)},
		    { BodyJointTypes.HipRight, new JointAngularReliabilityInfo(-15* MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 100 * MathConstants.Deg2Rad)},
		
		    { BodyJointTypes.HandTipLeft, new JointAngularReliabilityInfo(15* MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 95 * MathConstants.Deg2Rad)},
		    { BodyJointTypes.ThumbLeft, new JointAngularReliabilityInfo(15* MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 95 * MathConstants.Deg2Rad)},
		    { BodyJointTypes.HandLeft, new JointAngularReliabilityInfo(15* MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 95 * MathConstants.Deg2Rad)},
		    { BodyJointTypes.WristLeft, new JointAngularReliabilityInfo(15* MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 95 * MathConstants.Deg2Rad)},
		    { BodyJointTypes.ElbowLeft, new JointAngularReliabilityInfo(15* MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 95 * MathConstants.Deg2Rad)},
		    { BodyJointTypes.ShoulderLeft, new JointAngularReliabilityInfo(15* MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 95 * MathConstants.Deg2Rad)},
		
		    { BodyJointTypes.HandTipRight, new JointAngularReliabilityInfo(-15* MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 95 * MathConstants.Deg2Rad)},
		    { BodyJointTypes.ThumbRight, new JointAngularReliabilityInfo(-15* MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 95 * MathConstants.Deg2Rad)},
		    { BodyJointTypes.HandRight, new JointAngularReliabilityInfo(-15* MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 95 * MathConstants.Deg2Rad)},
		    { BodyJointTypes.WristRight, new JointAngularReliabilityInfo(-15* MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 95 * MathConstants.Deg2Rad)},
		    { BodyJointTypes.ElbowRight, new JointAngularReliabilityInfo(-15* MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 95 * MathConstants.Deg2Rad)},
		    { BodyJointTypes.ShoulderRight, new JointAngularReliabilityInfo(-15* MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 95 * MathConstants.Deg2Rad)},
		
		    { BodyJointTypes.SpineBase, new JointAngularReliabilityInfo(0 * MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 130 * MathConstants.Deg2Rad)},
		    { BodyJointTypes.SpineMid, new JointAngularReliabilityInfo(0 * MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 130 * MathConstants.Deg2Rad)},
		    { BodyJointTypes.SpineShoulder, new JointAngularReliabilityInfo(0 * MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 130 * MathConstants.Deg2Rad)},
		    { BodyJointTypes.Neck, new JointAngularReliabilityInfo(0 * MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 130 * MathConstants.Deg2Rad)},
		    { BodyJointTypes.Head, new JointAngularReliabilityInfo(0 * MathConstants.Deg2Rad, 35 * MathConstants.Deg2Rad, 90 * MathConstants.Deg2Rad)}
	    };

        /// <summary>
        /// Calculates the reliable joint confidence, using time information (joint confidence in last joints)
        /// and spatial information (the confidence of its siblings joints)
        /// </summary>
        /// <returns>The reliable joint confidence.</returns>
        /// <param name="jt">Joint type</param>
        /// <param name="history">History of man position for this source body</param>
        /// <param name="lastMan">Last calculated and filtered merged man for last frame</param>
        public static float CalculateReliableJointConfidence(BodyJointTypes jt, BodyDataHistory history, BodyData lastMan)
        {
            //get joint confidence across time
            float confidence = history.GetRunningAverageJointConfidence(jt);

            //make an average of sibling confidence
            float siblingConfidence = 0.0f;

            if (m_SiblingJoints[jt].Length >= 1)
            {
                foreach (BodyJointTypes sibJt in m_SiblingJoints[jt])
                    siblingConfidence += history.GetRunningAverageJointConfidence(sibJt);

                siblingConfidence /= m_SiblingJoints[jt].Length;

                //put minimum of siblingConfidence at 0.5
                siblingConfidence = Math.Max(siblingConfidence, 0.5f);

                //multiply confidence by sibling confidence
                confidence = confidence * siblingConfidence;
            }

            //multiply confidence by joint reliability confidence based on body orientation wrt the source tracking box
            confidence *= JointAngularReliabilityConfidence(jt, history, lastMan);

            //if joint position in current frame is very very different from joint position of merged man in last frame and the merged man joint was reliable enough,
            //reduce the confidence in this joint by a factor of two
            if (history != null && history.History != null && lastMan != null && history.History.Count >= 1 && lastMan.Joints[jt].Confidence >= 0.3f)
                if (Vector3.Distance(lastMan.Joints[jt].Position, history.History.Last().Joints[jt].Position) > 0.60f)
                    confidence /= 2;
            //CALCOLARE ORIENTAMENTO CORPO OVUNQUE COL METODO FURBO DI PRENDERE MEZZO CORPO
            //return computed confidence
            return confidence;
            ////DIRTY
            //return JointAngularReliabilityConfidence(jt, history, lastMan);
        }
        //static int Z;

        /// <summary>
        /// Calculates the joint confidence based on how the body whose joint belongs to stays in front of the DataSource
        /// </summary>
        /// <returns>The reliability joint confidence.</returns>
        /// <param name="jointType">Joint type</param>
        /// <param name="history">History of man position for this source body</param>
        /// <param name="lastMan">Last calculated and filtered merged man for last frame</param>
        public static float JointAngularReliabilityConfidence(BodyJointTypes jointType, BodyDataHistory history, BodyData lastMan)
        {
            //if this joint man is seen by a camera that is not frontal to the body, data is unreliable:
            //weight the confidence depending on the perpendicularity of body wrt the reading camera

            //calculate last man orientation in the frame of reference of the DataSources of this DataSource
            if (lastMan != null)
            {
                //		    && history.History[0].SourceDataSourceTransformationMatrix.m00 == 1) {
                //get this DataSource transformation matrix and compute its inverse
                Matrix4x4 invTransf = history.History.Last().DataSourceTransformationMatrix.Inverse;

                //compute lastman shoulder joints in this DataSource space
                Vector3 shoulderLeftPos = invTransf.MultiplyPoint3x4(lastMan.Joints[BodyJointTypes.ShoulderLeft].Position),
                shoulderRightPos = invTransf.MultiplyPoint3x4(lastMan.Joints[BodyJointTypes.ShoulderRight].Position),
                shoulderCenterPos = invTransf.MultiplyPoint3x4(lastMan.Joints[BodyJointTypes.SpineShoulder].Position);

               // Vector3 shoulderLeftPosRaw = invTransf.MultiplyPoint3x4(history.History.Last().Joints[BodyJointTypes.ShoulderLeft].Position),
               //shoulderRightPosRaw = invTransf.MultiplyPoint3x4(history.History.Last().Joints[BodyJointTypes.ShoulderRight].Position),
               //shoulderCenterPosRaw = invTransf.MultiplyPoint3x4(history.History.Last().Joints[BodyJointTypes.SpineShoulder].Position);

                //if(history.Inverted)
                //{
                //    Vector3 temp = shoulderRightPos;
                //    shoulderRightPos = shoulderLeftPos;
                //    shoulderLeftPos = temp;

                //    temp = shoulderRightPosRaw;
                //    shoulderRightPosRaw = shoulderLeftPosRaw;
                //    shoulderLeftPosRaw = temp;
                //}
                //calculate orientation in XZ plane, in range [-pi, pi] of this DataSource
                float orientation = FancyUtilities.BetweenJointsXZOrientation(shoulderLeftPos, shoulderRightPos);
                //float orientationRaw = FancyUtilities.BetweenJointsXZOrientation(shoulderLeftPosRaw, shoulderRightPosRaw);

                //if (history.History.Last().Joints[BodyJointTypes.ShoulderLeft].Confidence > history.History.Last().Joints[BodyJointTypes.ShoulderRight].Confidence)
                //    {
                //        orientationRaw = FancyUtilities.BetweenJointsXZOrientation(shoulderLeftPosRaw, shoulderCenterPosRaw);
                //        //if (jointType == BodyJointTypes.ShoulderLeft) Console.Write("USE LEFT RAW ");
                //    }
                //    else if(history.History.Last().Joints[BodyJointTypes.ShoulderLeft].Confidence < history.History.Last().Joints[BodyJointTypes.ShoulderRight].Confidence)
                //    {
                //        orientationRaw = FancyUtilities.BetweenJointsXZOrientation(shoulderCenterPosRaw, shoulderRightPosRaw);
                //        //if (jointType == BodyJointTypes.ShoulderLeft) Console.Write("USE right RAW ");
                //    }

                ////DIRTY
                if ((lastMan.Joints[BodyJointTypes.ShoulderLeft].Confidence < 0.75 || lastMan.Joints[BodyJointTypes.ShoulderRight].Confidence < 0.75)
                    && Math.Abs(lastMan.Joints[BodyJointTypes.ShoulderLeft].Confidence - lastMan.Joints[BodyJointTypes.ShoulderRight].Confidence) > 0.15)
                {
                    if (lastMan.Joints[BodyJointTypes.ShoulderLeft].Confidence > lastMan.Joints[BodyJointTypes.ShoulderRight].Confidence)
                    {
                        orientation = FancyUtilities.BetweenJointsXZOrientation(shoulderLeftPos, shoulderCenterPos);
                        //if (jointType == BodyJointTypes.ShoulderLeft) Console.Write("USE LEFT ");

                    }
                    else
                    {
                        orientation = FancyUtilities.BetweenJointsXZOrientation(shoulderCenterPos, shoulderRightPos);
                        //if (jointType == BodyJointTypes.ShoulderLeft) Console.Write("USE right ");

                    }
                }


                //if (Math.Abs(FancyUtilities.AdjustOrientation(orientationRaw, orientation) - orientation) < 55 * MathConstants.Deg2Rad)
                //    orientation = orientationRaw;

                //DIRTY
                //if (jointType == BodyJointTypes.ShoulderLeft)
                //    Console.WriteLine((history.Inverted ? "I" : "O") + "ORIENTATION: " + (AdjustKinectOrientation(orientation) * MathConstants.Rad2Deg) + "; L.SHOULDER CONFIDENCE " + JointsReliability[jointType].ComputeReliabilityConfidence(AdjustKinectOrientation(orientation)).ToString("0.000"));


                //if (jointType == BodyJointTypes.ShoulderLeft)
                //    Console.WriteLine("L.SHOULDER POSITION " + shoulderLeftPosRaw.ToString("0.000"));




                //calculate reliability confidence using angular reliability info of this joint
                return JointsReliability[jointType].ComputeReliabilityConfidence(AdjustKinectOrientation(orientation));
            }
            else
                return 1.0f;
        }

        static float AdjustKinectOrientation(float orientation)
        {
            if (orientation < 70 * MathConstants.Deg2Rad && orientation > 45 * MathConstants.Deg2Rad)
                return 45 * MathConstants.Deg2Rad + (orientation - 45 * MathConstants.Deg2Rad) * 1.66f;
            else if (orientation > -70 * MathConstants.Deg2Rad && orientation < -45 * MathConstants.Deg2Rad)
                return -45 * MathConstants.Deg2Rad + (orientation + 45 * MathConstants.Deg2Rad) * 1.66f;
            else if (orientation < 140 * MathConstants.Deg2Rad && orientation > 110 * MathConstants.Deg2Rad)
                return (float)(140 * MathConstants.Deg2Rad - (140 * MathConstants.Deg2Rad - orientation) * 1.66f);
            else if (orientation > -140 * MathConstants.Deg2Rad && orientation < -110 * MathConstants.Deg2Rad)
                return (float)(-140 * MathConstants.Deg2Rad - (-140 * MathConstants.Deg2Rad - orientation) * 1.66f);
            else
                return orientation;
        }
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