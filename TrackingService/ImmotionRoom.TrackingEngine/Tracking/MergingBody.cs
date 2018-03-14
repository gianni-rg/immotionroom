namespace ImmotionAR.ImmotionRoom.TrackingEngine.Tracking
{
    using System.Collections.Generic;
    using System.Linq;
    using Model;

    /// <summary>
    ///     Represents all data structures (body, skeletons, etc...) that have to be merged into a single body
    ///     and offers methods to perform such merging operation.
    ///     This class operates using a simple averaging merging method. Its subclasses offer more performing operations.
    /// </summary>
    internal sealed class MergingBody : BaseMergingBody
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MergingBody" /> class.
        /// </summary>
        /// <param name="id">Unique dentifier for this merging body</param>
        public MergingBody(ulong id) : base(id)
        {
            Merge();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MergingBody" /> class.
        /// </summary>
        /// <param name="id">Unique dentifier for this merging body</param>
        /// <param name="noMerge">True if Merge function must NOT be called, otherwise false</param>
        public MergingBody(ulong id, bool noMerge) : base(id)
        {
            if (!noMerge)
            {
                Merge();
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MergingBody" /> class.
        /// </summary>
        /// <param name="id">Unique dentifier for this merging body</param>
        /// <param name="firstMan">First body to be added to this merging body</param>
        /// <param name="firstManSourceId"></param>
        public MergingBody(ulong id, BodyData firstMan, byte firstManSourceId) : base(id)
        {
            AddUpdateBody(firstMan, firstManSourceId);
            Merge(); // Create first merging body, that will be equal to firstMan
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MergingBody" /> class.
        /// </summary>
        /// <param name="id">Unique dentifier for this merging body</param>
        /// <param name="firstMan">First body to be added to this merging body</param>
        /// <param name="firstManSourceId"></param>
        /// <param name="noOperations">True if Merge and AddUpdateBody function must NOT be called, otherwise false</param>
        public MergingBody(ulong id, BodyData firstMan, byte firstManSourceId, bool noOperations) : base(id)
        {
            if (noOperations)
            {
                return;
            }

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
            }

            if (m_BodiesTrackingSources.Contains(bodySourceId))
            {
                m_BodiesTrackingSources.Remove(bodySourceId);
            }
        }

        /// <summary>
        ///     Merge the bodies associated with this instance.
        ///     The merged bodies is returned and also saved inside this object (retrievable through the LastMergedMan property)
        /// </summary>
        internal override BodyData Merge()
        {
            // Call chosen merging method
            return SimpleAveragingMerging();
        }

        /// <summary>
        ///     Merges the bodies simply averaging all the joints positions
        /// </summary>
        /// <returns>The averaged body</returns>
        private BodyData SimpleAveragingMerging()
        {
            if (m_SourceBodies.Count < 1)
            {
                m_FilteredBody = m_MergedBody = new BodyData(m_Id, new Dictionary<BodyJointTypes, BodyJointData>(BodyJointTypesComparer.Instance), m_BodiesTrackingSources.ToList());
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

                // For each joint type, loop through all bodies and sum the joint positions, weighted by the joint position confidence
                foreach (var man in m_SourceBodies.Values)
                {
                    sum += man.Joints[jt].Position*man.Joints[jt].Confidence;
                    confidenceSum += man.Joints[jt].Confidence;
                }

                // Calculate average joint position and confidence
                // If all joints are not tracked (sum of confidences is null), take position of joint in the first man
                if (confidenceSum > 0)
                {
                    sum /= confidenceSum; //remember that the average is weighted by confidence
                }
                else
                    sum = firstBodyData.Joints[jt].Position;

                confidenceSum /= m_SourceBodies.Count;

                // Add new joint to averageJoints collection
                averageJoints[jt] = new BodyJointData(sum, confidenceSum, jt);
            }

            // Create a new average man using found average joints data
            m_FilteredBody = m_MergedBody = new BodyData(m_Id, averageJoints, m_BodiesTrackingSources.ToList());

            // Return merged man
            return m_MergedBody;
        }
    }
}