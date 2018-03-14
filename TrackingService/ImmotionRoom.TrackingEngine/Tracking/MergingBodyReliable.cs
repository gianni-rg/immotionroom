namespace ImmotionAR.ImmotionRoom.TrackingEngine.Tracking
{
    using System.Collections.Generic;
    using System.Linq;
    using Model;

    /// <summary>
    ///     Represents all data structures (body, skeletons, etc...) that have to be merged into a single body
    ///     and offers methods to perform such merging operation.
    ///     This class operates using an averaging merging method, after have used euristics to understand which tracking boxes
    ///     are more reliable to see a certain join, to make the fusion process
    ///     more stable. Furthermore, the resulting Skeleton is filtered using the double exponential filter
    /// </summary>
    internal sealed class MergingBodyReliable : BaseMergingBody
    {
        /// <summary>
        ///     The size of the history of bodies.
        /// </summary>
        private const int HistoryFramesSize = 30; //approx 1 second

        /// <summary>
        ///     The running average alpha factor, in range [0, 1].
        ///     The more it is closer to 0, the more last values in History are considered
        /// </summary>
        private const float RunningAverageAlpha = 0.36f;

        /// <summary>
        ///     Data structures useful for filtering the merged man in each frame
        /// </summary>
        private readonly JointsPositionDoubleExponentialFilter2 m_MergedManFilter;

        /// <summary>
        ///     The history of past frames of bodies that have to be merged to create a compound average body
        /// </summary>
        private readonly Dictionary<ulong, BodyDataHistory> m_JointMenHistory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MergingBody" /> class.
        /// </summary>
        /// <param name="id">Unique dentifier for this merging body</param>
        /// <param name="smoothingParams">Parameters using for the smoothing of the body tracking</param>
        /// <param name="filterRunningAvgConfidence">
        ///     Alpha constant for joint confidence running average filtering (0 means full
        ///     history, 1 full update)
        /// </param>
        public MergingBodyReliable(ulong id, TransformSmoothParameters smoothingParams, float filterRunningAvgConfidence) : base(id)
        {
            m_MergedManFilter = new JointsPositionDoubleExponentialFilter2();
            m_MergedManFilter.Init(smoothingParams, filterRunningAvgConfidence);
            m_JointMenHistory = new Dictionary<ulong, BodyDataHistory>();

            Merge();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MergingBody" /> class.
        /// </summary>
        /// <param name="id">Unique dentifier for this merging body</param>
        /// <param name="firstMan">First body to be added to this merging body</param>
        /// <param name="smoothingParams"></param>
        /// <param name="filterRunningAvgConfidence">
        ///     Alpha constant for joint confidence running average filtering (0 means full
        ///     history, 1 full update)
        /// </param>
        /// <param name="firstManSourceId"></param>
        public MergingBodyReliable(ulong id, BodyData firstMan, byte firstManSourceId, TransformSmoothParameters smoothingParams, float filterRunningAvgConfidence) : base(id)
        {
            m_MergedManFilter = new JointsPositionDoubleExponentialFilter2();

            m_JointMenHistory = new Dictionary<ulong, BodyDataHistory>();

            m_MergedManFilter.Init(smoothingParams, filterRunningAvgConfidence);
            
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
            return ReliableAveragingMethod();
        }

        /// <summary>
        ///     Merges the bodies averaging joints positions in a smart way (that consider joints tracking reliability for each
        ///     tracking box) and applying a filter over the frames
        /// </summary>
        /// <returns>The averaged body</returns>
        private BodyData ReliableAveragingMethod()
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
                var foundConfidence = 0f;

                // For each joint type, loop through all bodies and sum the joint positions, weighted by the joint position confidence.
                // notice that we take confidence using body history
                foreach (var man in m_SourceBodies.Values)
                {
                    var confidence = JointConfidenceCalculator2.CalculateReliableJointConfidence(jt, m_JointMenHistory[man.Id], m_FilteredBody);
                    sum += man.Joints[jt].Position*confidence;
                    confidenceSum += confidence;

                    // Take the highest joint confidence
                    if (foundConfidence < confidence)
                    {
                        foundConfidence = confidence;
                    }
                }

                // Calculate average joint position and confidence
                // If we have some reliable data, make the weighted average and return computed joint position
                if (confidenceSum > 0.001)
                {
                    sum /= confidenceSum; // Remember that the average position is weighted by confidence
                }
                // If the joint detected by all the data sources is not tracked (sum of confidences is near null, so confidence of EVERY data source is 0 for this joint) 
                else
                {
                    // We have two possible cases:
                    // 1) Some joint had a tracking confidence > 0, but its angular reliability is 0: in this case we have unreliable values, but it is the best we can find, so we take them and average them. Assign
                    //    confidence of 0.1;
                    // 2) All joint had a 0 tracking confidence, so we have all garbage. Take the one from the first kinect and assign it confidence of 0.01;

                    // Try to apply case 1

                    sum = Vector3.Zero;
                    confidenceSum = 0f;

                    // Average all tracked joints, if any
                    foreach (var man in m_SourceBodies.Values)
                    {
                        if (man.Joints[jt].Confidence > 0)
                        {
                            // Check if this body is inverted wrt the merged body (i.e. left and right have been flipped)
                            sum += m_JointMenHistory[man.Id].Inverted ? man.Joints[BodyConstants.InversionJointsMap[jt]].Position : man.Joints[jt].Position;
                            confidenceSum += 1;
                        }
                    }

                    // If this sum of confidence is > 0, we are in case 1... make the average and return value
                    if (confidenceSum > 0)
                    {
                        sum /= confidenceSum;
                        foundConfidence = 0.1f;
                    }
                    // Else, still zero, we are in case 2
                    else
                    {
                        sum = m_SourceBodies.First().Value.Joints[jt].Position;
                        foundConfidence = 0.01f;
                    }
                }

                // Add new joint to averageJoints collection
                averageJoints[jt] = new BodyJointData(sum, foundConfidence, jt);
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