namespace ImmotionAR.ImmotionRoom.TrackingEngine.Tracking
{
    using System.Collections.Generic;
    using System.Linq;
    using Model;

    /// <summary>
    ///     Represents all data structures (body, skeletons, etc...) that have to be merged into a single body
    ///     and offers methods to perform such merging operation.
    ///     This class operates using a simple averaging merging method, but it uses euristics to make the fusion process
    ///     more stable. Furthermore, the resulting Skeleton is filtered using
    /// </summary>
    internal sealed class MergingBodyPro : BaseMergingBody
    {
        /// <summary>
        ///     The size of the history of bodies.
        /// </summary>
        private const int HistoryFramesSize = 8;

        /// <summary>
        ///     The running average alpha factor, in range [0, 1].
        ///     The more it is closer to 1, the more last values in History are considered
        /// </summary>
        private const float RunningAverageAlpha = 0.33f;

        /// <summary>
        ///     Data structures useful for filtering the merged man in each frame
        /// </summary>
        private readonly JointsPositionDoubleExponentialFilter m_MergedManFilter;

        /// <summary>
        ///     The history of past frames of bodies that have to be merged to create a compound average body
        /// </summary>
        private readonly Dictionary<ulong, BodyDataHistory> m_JointMenHistory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MergingBody" /> class.
        /// </summary>
        /// <param name="id">Unique dentifier for this merging body</param>
        /// <param name="smoothingParams">Parameters using for the smoothing of the body tracking</param>
        public MergingBodyPro(ulong id, TransformSmoothParameters smoothingParams) : base(id)
        {
            m_MergedManFilter = new JointsPositionDoubleExponentialFilter();
            m_MergedManFilter.Init(smoothingParams);

            m_JointMenHistory = new Dictionary<ulong, BodyDataHistory>();

            Merge();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MergingBody" /> class.
        /// </summary>
        /// <param name="id">Unique dentifier for this merging body</param>
        /// <param name="firstMan">First body to be added to this merging body</param>
        /// <param name="firstManSourceId"></param>
        /// <param name="smoothingParams"></param>
        public MergingBodyPro(ulong id, BodyData firstMan, byte firstManSourceId, TransformSmoothParameters smoothingParams) : base(id)
        {
            m_MergedManFilter = new JointsPositionDoubleExponentialFilter();

            m_JointMenHistory = new Dictionary<ulong, BodyDataHistory>();

            m_MergedManFilter.Init(smoothingParams);

            AddUpdateBody(firstMan, firstManSourceId);

            Merge(); // Create first merging body, that will be equal to firstMan
        }

        /// <summary>
        ///     Add a new body to current body merging element, or update existing body.
        ///     Merged body is NOT automatically updated
        /// </summary>
        /// <param name="bodyData">Body to be added or updated into this group of bodies to be merged together</param>
        /// <param name="bodySourceId"></param>
        internal override void AddUpdateBody(BodyData bodyData, byte bodySourceId)
        {
            m_SourceBodies[bodyData.Id] = bodyData;
            m_BodiesTrackingSources.Add(bodySourceId);

            if (m_JointMenHistory == null)
            {
                return;
            }

            // Add new body to body history, or create a new history if no history exists
            if (m_JointMenHistory.ContainsKey(bodyData.Id))
            {
                m_JointMenHistory[bodyData.Id].PushNewValue(bodyData, m_MergedBody, RunningAverageAlpha);
            }
            else
            {
                m_JointMenHistory[bodyData.Id] = new BodyDataHistory(HistoryFramesSize);
                m_JointMenHistory[bodyData.Id].PushNewValue(bodyData);
            }
        }

        /// <summary>
        ///     Removes a body from current body merging element
        ///     Merged body is NOT automatically updated
        /// </summary>
        /// <param name="bodyId">Body identifier of the body that has to be removed</param>
        /// <param name="bodySourceId"></param>
        internal override void RemoveBody(ulong bodyId, byte bodySourceId)
        {
            if (m_SourceBodies.ContainsKey(bodyId))
            {
                m_SourceBodies.Remove(bodyId);
                m_JointMenHistory.Remove(bodyId);

                if (m_BodiesTrackingSources.Contains(bodySourceId))
                {
                    m_BodiesTrackingSources.Remove(bodySourceId);
                }
            }
        }

        /// <summary>
        ///     Merge the bodies associated with this instance.
        ///     The merged bodies is returned and also saved inside this object (retrievable through the LastMergedMan property)
        /// </summary>
        internal override BodyData Merge()
        {
            return ProAveragingMethod();
        }

        /// <summary>
        ///     Merges the bodies averaging joints positions in a smart way and applying a filter over the frames
        /// </summary>
        /// <returns>The averaged body</returns>
        private BodyData ProAveragingMethod()
        {
            if (m_SourceBodies.Count < 1)
            {
                if (m_MergedBody == null)
                {
                    m_FilteredBody = m_MergedBody = new BodyData(m_Id, new Dictionary<BodyJointTypes, BodyJointData>(BodyJointTypesComparer.Instance), m_BodiesTrackingSources.ToList());
                }

                return m_MergedBody;
            }

            // Average all joints positions
            var firstBodyData = m_SourceBodies.First().Value;

            var averageJoints = new Dictionary<BodyJointTypes, BodyJointData>(BodyJointTypesComparer.Instance);

            // Loop through all joint types
            foreach (var jt in firstBodyData.Joints.Keys)
            {
                var sum = Vector3.Zero;
                var confidenceSum = 0f;

                // For each joint type, loop through all bodies and sum the joint positions, weighted by the joint position confidence.
                // notice that we take confidence using body history
                foreach (var man in m_SourceBodies.Values)
                {
                    // Check if this body is inverted wrt the merged body (i.e. left and right have been flipped)
                    if (m_JointMenHistory[man.Id].Inverted)
                    {
                        var realJt = BodyConstants.InversionJointsMap[jt];

                        var confidence = JointConfidenceCalculator.CalculateReliableJointConfidence(realJt, m_JointMenHistory[man.Id], m_FilteredBody);

                        sum += man.Joints[realJt].Position*confidence;
                        confidenceSum += confidence;
                    }
                    else
                    {
                        var confidence = JointConfidenceCalculator.CalculateReliableJointConfidence(jt, m_JointMenHistory[man.Id], m_FilteredBody);

                        sum += man.Joints[jt].Position*confidence;
                        confidenceSum += confidence;
                    }
                }

                // Calculate average joint position and confidence
                // If all joints are not tracked (sum of confidences is near null), take average of all joints
                // weighted by orientation of bodies (if bodies are frontal to DataSource or not)
                if (confidenceSum > 0.001)
                {
                    sum /= confidenceSum; // Remember that the average is weighted by confidence
                }
                else
                {
                    sum = Vector3.Zero;
                    confidenceSum = 0f;

                    foreach (var man in m_SourceBodies.Values)
                    {
                        // Check if this body is inverted wrt the merged body (i.e. left and right have been flipped)
                        if (m_JointMenHistory[man.Id].Inverted)
                        {
                            var realJt = BodyConstants.InversionJointsMap[jt];
                            var confidence = 0.5f*m_JointMenHistory[man.Id].OrientationConfidence;
                            sum += man.Joints[realJt].Position*confidence;

                            confidenceSum += confidence;
                        }
                        else
                        {
                            var confidence = 0.5f*m_JointMenHistory[man.Id].OrientationConfidence;
                            sum += man.Joints[jt].Position*confidence;

                            confidenceSum += confidence;
                        }
                    }

                    // If this sum of confidence is still zero, take value from first DataSource
                    if (confidenceSum > 0)
                    {
                        sum /= confidenceSum;
                    }
                    else
                    {
                        sum = m_SourceBodies.First().Value.Joints[jt].Position;
                    }
                }

                confidenceSum /= m_SourceBodies.Count;

                // Add new joint to averageJoints collection
                averageJoints[jt] = new BodyJointData(sum, confidenceSum, jt);
            }

            // Create a new average man using found average joints data
            m_MergedBody = new BodyData(m_Id, averageJoints, m_BodiesTrackingSources.ToList());

            // Filter results
            m_FilteredBody = m_MergedManFilter.UpdateFilter(m_MergedBody);
            m_FilteredBody = new BodyData(m_FilteredBody, m_FilteredBody.DataSourceTransformationMatrix, m_BodiesTrackingSources.ToList());

            // Return merged man
            return m_MergedBody;
        }
    }
}