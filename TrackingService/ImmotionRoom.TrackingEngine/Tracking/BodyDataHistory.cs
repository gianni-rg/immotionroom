namespace ImmotionAR.ImmotionRoom.TrackingEngine.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ImmotionAR.ImmotionRoom.TrackingEngine.Model;
    using ImmotionAR.ImmotionRoom.TrackingEngine.Tools;

    /// <summary>
    /// Provides history for a BodyData for the last frames
    /// </summary>
    internal class BodyDataHistory
    {
        /// <summary>
        /// Actual list of joint men
        /// </summary>
        private readonly List<BodyData> m_MenHistory;

        /// <summary>
        /// The desired length of the history
        /// </summary>
        private readonly int m_DesiredLength;

        /// <summary>
        /// The confidence of this BodyData (with its history), given its orientation wrt its source DataSource
        /// </summary>
        private float m_OrientationConfidence;

        /// <summary>
        /// True if last body is inverted wrt the model body
        /// </summary>
        private bool m_IsInverted;

        /// <summary>
        /// Man whose joints are running average of joints of the man through its history
        /// </summary>
        private readonly Dictionary<BodyJointTypes, float> m_RunningAverageMan;

        #region Public properties

        public List<BodyData> History
        {
            get
            {
                return m_MenHistory;
            }
        }

        /// <summary>
        /// Gets the confidence of this BodyData (with its history), given its orientation wrt its source DataSource
        /// </summary>
        /// <value>The orientation confidence.</value>
        public float OrientationConfidence
        {
            get
            {
                return m_OrientationConfidence;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="BodyDataHistory"/> is inverted wrt the tracking body model
        /// </summary>
        /// <value><c>true</c> if inverted; otherwise, <c>false</c>.</value>
        public bool Inverted
        {
            get
            {
                return m_IsInverted;
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MatricesQueue"/> class.
        /// </summary>
        /// <param name="desiredLength">Desired length for the joint men history</param>
        public BodyDataHistory(int desiredLength)
        {
            m_DesiredLength = desiredLength;

            //creation and initialization of matrix list
            m_MenHistory = new List<BodyData>();

            m_OrientationConfidence = 1.0f;
            m_IsInverted = false;

            // Dictionary Enum Optimization
            // See: http://www.codeproject.com/Articles/33528/Accelerating-Enum-Based-Dictionaries-with-Generic
            // See: http://www.somasim.com/blog/2015/08/c-performance-tips-for-unity-part-2-structs-and-enums/
            // See: http://stackoverflow.com/questions/7143948/efficiency-of-using-iequalitycomparer-in-dictionary-vs-hashcode-and-equals
            m_RunningAverageMan = new Dictionary<BodyJointTypes, float>(BodyJointTypesComparer.Instance);
        }

        /// <summary>
        /// Add a new value to the history. The new value is added to the tail of the list, while the value at the
        /// top gets removed
        /// </summary>
        /// <param name="newMatrix">New BodyData</param>
        public void PushNewValue(BodyData newBodyData)
        {
            if (m_MenHistory.Count >= m_DesiredLength)
                m_MenHistory.RemoveAt(0);

            m_MenHistory.Add(newBodyData);
        }

        /// <summary>
        /// Add a new value to the history. The new value is added to the tail of the list, while the value at the
        /// top gets removed.
        /// The orientation confidence, the joints confidence running average and the inversion flag of this body get updated
        /// </summary>
        /// <param name="newMatrix">New BodyData</param>
        /// <param name="lastMan">Last calculated and filtered merged man for last frame</param>
        /// <param name="runningAlpha">Update factor for the running average precomputation, in range [0, 1] (0 means no confidence in new data, 1 means no confidence in old data)</param>
        public void PushNewValue(BodyData newBodyData, BodyData lastMan, float runningAlpha)
        {
            if (m_MenHistory.Count >= m_DesiredLength)
                m_MenHistory.RemoveAt(0);

            m_MenHistory.Add(newBodyData);

            if (lastMan != null)
            {
                m_OrientationConfidence = JointConfidenceCalculator.CalculateOrientationBodyDataConfidence(this, lastMan);
                m_IsInverted = CheckBodyInversion(lastMan);

                foreach (BodyJointTypes jt in m_MenHistory.Last().Joints.Keys)
                    m_RunningAverageMan[jt] = GetRunningAverageJointConfidence(jt, runningAlpha);
            }
        }

        /// <summary>
        /// Gets the average joint confidence through all the history
        /// </summary>
        /// <returns>The average joint confidence</returns>
        /// <param name="jt">Type of joint of interest</param>
        public float GetAverageJointConfidence(BodyJointTypes jt)
        {
            float confidence = 0;

            foreach (BodyData bodyData in m_MenHistory)
                confidence += bodyData.Joints[jt].Confidence;

            if (m_MenHistory.Count == 0)
                return 0f;
            else
                return confidence / m_MenHistory.Count;

        }

        /// <summary>
        /// Gets the running average joint confidence through all the history, as has been precomputed with last body addition
        /// </summary>
        /// <returns>The average joint confidence</returns>
        /// <param name="jt">Type of joint of interest</param>
        public float GetRunningAverageJointConfidence(BodyJointTypes jt)
        {
            if (m_RunningAverageMan.ContainsKey(jt))
                return m_RunningAverageMan[jt];
            else
            {
                return GetRunningAverageJointConfidence(jt, 0.33f);
            }
        }

        /// <summary>
        /// Gets the running average joint confidence through all the history
        /// </summary>
        /// <returns>The average joint confidence</returns>
        /// <param name="jt">Type of joint of interest</param>
        /// <param name="alpha">Update factor in range [0, 1] (0 means no confidence in new data, 1 means no confidence in old data)</param>
        public float GetRunningAverageJointConfidence(BodyJointTypes jt, float alpha)
        {
            float confidence = m_MenHistory[0].Joints[jt].Confidence;

            for (int i = 1; i < m_MenHistory.Count; i++)
                confidence = (alpha) * m_MenHistory[i].Joints[jt].Confidence + (1 - alpha) * confidence;

            return confidence;
        }

        /// <summary>
        /// Checks if last body is inverted wrt previous frames bodies.
        /// This is necessary because DataSource swaps left and right body parts when body is orientated backwards
        /// </summary>
        /// <returns><c>true</c>, if body inversion was found, <c>false</c> otherwise.</returns>
        public bool CheckBodyInversion()
        {
            if (m_MenHistory.Count < 2)
                return false;

            //if left shoulder is in place of right shoulder and vice-versa, an inversion has been found
            float leftToLeftDistance = Vector3.Distance(m_MenHistory.First().Joints[BodyJointTypes.ShoulderLeft].Position,
                                                         m_MenHistory.Last().Joints[BodyJointTypes.ShoulderLeft].Position);
            float rightTorightDistance = Vector3.Distance(m_MenHistory.First().Joints[BodyJointTypes.ShoulderRight].Position,
                                                           m_MenHistory.Last().Joints[BodyJointTypes.ShoulderRight].Position);
            float leftToRightDistance = Vector3.Distance(m_MenHistory.First().Joints[BodyJointTypes.ShoulderLeft].Position,
                                                          m_MenHistory.Last().Joints[BodyJointTypes.ShoulderRight].Position);
            float rightToLeftDistance = Vector3.Distance(m_MenHistory.First().Joints[BodyJointTypes.ShoulderRight].Position,
                                                         m_MenHistory.Last().Joints[BodyJointTypes.ShoulderLeft].Position);

            if (leftToLeftDistance > leftToRightDistance && rightTorightDistance > rightToLeftDistance)
                return true;
            else
                return false;
        }

        //bool currentInv = false;

        /// <summary>
        /// Checks if last body is inverted wrt previous frames bodies.
        /// This is necessary because DataSource swaps left and right body parts when body is orientated backwards
        /// </summary>
        /// <returns><c>true</c>, if body inversion was found, <c>false</c> otherwise.</returns>
        public bool CheckBodyInversion(BodyData lastMan)
        {
            
            if (m_MenHistory.Count < 1 || m_MenHistory.First() == null || m_MenHistory.First().Joints == null || lastMan == null)
                return false;

            //if the order of shoulders in this DataSource is the opposite of the order of shoulders in merged body, this
            //DataSource has the body inverted
            BodyJointTypes leftJointType = BodyJointTypes.ShoulderLeft,
                             rightJointType = BodyJointTypes.ShoulderRight;

            Vector3 shoulderLeftPos = m_MenHistory.Last().Joints[leftJointType].Position,
            shoulderRightPos = m_MenHistory.Last().Joints[rightJointType].Position;

            Vector3 shoulderLeftPosLastMan = lastMan.Joints[leftJointType].Position,
            shoulderRightPosLastMan = lastMan.Joints[rightJointType].Position;

            float orientation = FancyUtilities.BetweenJointsXZOrientation(shoulderRightPos, shoulderLeftPos);
            float orientationLastMan = FancyUtilities.AdjustOrientation(FancyUtilities.BetweenJointsXZOrientation(shoulderRightPosLastMan, shoulderLeftPosLastMan),
                                                                        orientation);

            if (Math.Abs(orientationLastMan - orientation) > Math.PI * 2 / 3)
                return true;
            else
                return false;

            //other method, uncomment to use it
            //		Matrix4x4 invTransf = m_menHistory.Last().SourceDataSourceTransformationMatrix.inverse;
            //
            //		Vector3 shoulderLeftPosLastMan = invTransf.MultiplyPoint3x4 (lastMan.Joints [BodyJointTypes.ShoulderLeft].Position), 
            //		shoulderRightPosLastMan = invTransf.MultiplyPoint3x4 (lastMan.Joints [BodyJointTypes.ShoulderRight].Position);
            //
            //		float orientationLastMan = FancyUtilities.AdjustOrientation(FancyUtilities.BetweenJointsXZOrientation (shoulderLeftPosLastMan, shoulderRightPosLastMan),
            //		                                                            0);
            //
            //		//Debug.Log ("Or" + orientationLastMan);
            //		if (Mathf.Abs (orientationLastMan - 0) > Mathf.PI * 2 / 3)
            //			return true;
            //		else
            //			return false;
        }

    }


}