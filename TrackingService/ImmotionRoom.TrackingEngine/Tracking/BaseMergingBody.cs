namespace ImmotionAR.ImmotionRoom.TrackingEngine.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Model;

    /// <summary>
    ///     Represents all data structures (body, skeletons, etc...) that have to be merged into a single body
    ///     and offers methods to perform such merging operation.
    ///     This class operates using a simple averaging merging method. Its subclasses offer more performing operations.
    /// </summary>
    internal abstract class BaseMergingBody
    {
        /// <summary>
        ///     ID of this merging body
        /// </summary>
        protected readonly ulong m_Id;

        /// <summary>
        ///     The bodies that have to be merged to create a compound average body
        /// </summary>
        protected readonly Dictionary<ulong, BodyData> m_SourceBodies;

        /// <summary>
        ///     Collection of DataSources from which the bodies of m_SourceBodies are retrieved
        /// </summary>
        protected readonly HashSet<byte> m_BodiesTrackingSources;

        /// <summary>
        ///     The man resulting from last merging operation on m_JointMen array
        /// </summary>
        protected BodyData m_MergedBody;

        /// <summary>
        ///     The man resulting from last merging operation on m_JointMen array, filtered using old frames data
        /// </summary>
        protected BodyData m_FilteredBody;

        #region Public properties

        /// <summary>
        ///     Get ID of this merging body
        /// </summary>
        public ulong Id
        {
            get { return m_Id; }
        }

        /// <summary>
        ///     Returns the body resulting from the last merging operation
        /// </summary>
        /// <value>The last merged man</value>
        public BodyData LastMergedMan
        {
            get { return m_MergedBody; }
        }

        /// <summary>
        ///     Returns the body resulting from the last merging operation, filtered using old frames data
        /// </summary>
        /// <value>The last filtered man</value>
        public BodyData LastFilteredMan
        {
            get { return m_FilteredBody; }
        }

        /// <summary>
        ///     Gets the merging bodies number
        /// </summary>
        /// <value>The merging bodies number</value>
        public int MergingBodiesNumber
        {
            get { return m_SourceBodies.Count; }
        }

        ///// <summary>
        /////     Gets the centroid of the last merged man
        ///// </summary>
        ///// <value>The last merged man</value>
        //public Vector3 MergedCentroid
        //{
        //    get { return m_MergedBody.Centroid; }
        //}

        /// <summary>
        ///     Gets the centroid of all merging bodies
        /// </summary>
        /// <value>The centroid of all merging bodies</value>
        public Vector3 RawCentroid
        {
            get
            {
                // Compute centroids, remembering that each centroid is an average itself
                var sum = Vector3.Zero;
                var pointCounts = 0;

                if (m_SourceBodies.Any())
                {
                    foreach (var man in m_SourceBodies.Values)
                    {
                        sum += man.Centroid*man.Joints.Count;
                        pointCounts += man.Joints.Count;
                    }

                    sum /= pointCounts;
                }

                return sum;
            }
        }

        /// <summary>
        ///     Gets the DataSources that provide the bodies merged into this object
        /// </summary>
        public HashSet<byte> SourcesOfBodies
        {
            get { return m_BodiesTrackingSources; }
        }

        #endregion

        /// <summary>
        ///     Initializes a new instance of the <see cref="MergingBody" /> class.
        /// </summary>
        /// <param name="id">Unique dentifier for this merging body</param>
        protected BaseMergingBody(ulong id)
        {
            m_SourceBodies = new Dictionary<ulong, BodyData>();
            m_BodiesTrackingSources = new HashSet<byte>();
            m_Id = id;
        }
        
        /// <summary>
        ///     Determines whether this instance contains a body with the specified id
        /// </summary>
        /// <returns><c>true</c> if this instance contains a body with the specified id; otherwise, <c>false</c>.</returns>
        /// <param name="bodyId">Body identifier</param>
        public bool HasBodyId(ulong bodyId)
        {
            return m_SourceBodies.ContainsKey(bodyId);
        }

        /// <summary>
        ///     Add a new body to current body merging element, or update existing body.
        ///     Merged body is NOT automatically updated
        /// </summary>
        /// <param name="bodyData">Body to be added or updated into this group of bodies to be merged together</param>
        /// <param name="bodySourceId"></param>
        internal abstract void AddUpdateBody(BodyData bodyData, byte bodySourceId);

        /// <summary>
        ///     Removes a body from current body merging element
        ///     Merged body is NOT automatically updated
        /// </summary>
        /// <param name="bodyId">Body identifier of the body that has to be removed</param>
        /// <param name="bodySourceId"></param>
        internal abstract void RemoveBody(ulong bodyId, byte bodySourceId);

        /// <summary>
        ///     Computes the distance of a body from this compound body.
        ///     Distance is computed using centroids (BodyData centroid and last computed merged centroid)
        /// </summary>
        /// <returns>Distance of provided body from this merged body</returns>
        /// <param name="bodyData">Body whose distance from this object has to be calculated</param>
        public float DistanceFromBody(BodyData bodyData)
        {
            return Vector3.Distance(bodyData.StableCentroid, m_MergedBody.StableCentroid);
        }

        /// <summary>
        ///     Merge the bodies associated with this instance.
        ///     The merged bodies is returned and also saved inside this object (retrievable through the LastMergedMan property)
        /// </summary>
        internal abstract BodyData Merge();

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
                if (confidenceSum != 0.0f)
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