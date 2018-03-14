namespace ImmotionAR.ImmotionRoom.TrackingEngine.Calibration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ImmotionAR.ImmotionRoom.TrackingEngine.Model;
    using ImmotionAR.ImmotionRoom.TrackingEngine.Tools;

    /// <summary>
    /// Calculates the best matrix that aligns a BodyData read from a DataSource remote server (slave)
    /// and the body read from a local DataSource connected to the computer (master).

    /// </summary>
    internal class BodySkeletonAligner
    {
        /// <summary>
        /// The maximum number of joints to stay in the slave and master points lists.
        /// If the joints count reaches this thresh, the older CutAwayJointsNumber joints
        /// are removed from the list
        /// </summary>
        const int MaximumJointsThresh = 500;

        /// <summary>
        ///     List of body upper joints that usually are stabler during tracking and can be used for centroid computation
        /// </summary>
        private static readonly BodyJointTypes[] StableJointsForCentroidsUpper =
        {
            BodyJointTypes.ShoulderLeft,
            BodyJointTypes.ShoulderRight,
            BodyJointTypes.SpineShoulder,
            BodyJointTypes.Neck,
        };

        /// <summary>
        ///     List of body lower joints that usually are stabler during tracking and can be used for centroid computation
        /// </summary>
        private static readonly BodyJointTypes[] StableJointsForCentroidsLower =
        {
            BodyJointTypes.SpineBase,
            BodyJointTypes.SpineMid,
            BodyJointTypes.HipLeft,
            BodyJointTypes.HipRight
        };

        /// <summary>
        /// Number of joints to be removed from the m_slavePoints and m_masterPoints when the maximum
        /// joints threshold gets reached
        /// </summary>
        const int CutAwayJointsNumber = 100;

        /// <summary>
        /// Joints on which the alignment between bodies has to be performed
        /// </summary>
        readonly BodyJointTypes[] m_KeyJoints;

        /// <summary>
        /// Key joints read from the remote DataSource
        /// </summary>
        List<Vector3> m_SlavePoints;

        /// <summary>
        /// Key joints read from the local DataSource
        /// </summary>
        List<Vector3> m_MasterPoints;

        /// <summary>
        /// Last computed valid alignment matrix
        /// </summary>
        Matrix4x4 m_LastAlignmentMatrix;

        /// <summary>
        /// Gets the last alignment matrix.
        /// </summary>
        /// <value>The last alignment matrix.</value>
        public Matrix4x4 LastAlignmentMatrix
        {
            get
            {
                return m_LastAlignmentMatrix;
            }
        }

        /// <summary>
        /// Gets the key joints of calibration process
        /// </summary>
        /// <value>The key joints</value>
        public BodyJointTypes[] KeyJoints
        {
            get
            {
                return m_KeyJoints;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BodySkeletonAligner"/> class.
        /// </summary>
        /// <param name="keyJoints">The key joints around which the alignment operation has to be performed</param>
        public BodySkeletonAligner(BodyJointTypes[] keyJoints)
        {
            m_KeyJoints = keyJoints;
            this.Reset();
        }

        /// <summary>
        /// Adds a new key joints to the alignment computation between the two frame of reference
        /// </summary>
        /// <param name="masterBody">Master body, as detected by the DataSource local master</param>
        /// <param name="slaveSkeleton">Slave BodyData, as detected by the DataSource remote slave. It must represent THE SAME
        /// person as the one depicted by the masterBody parameter</param>
        /// <param name="useCentroid">True if only tracked body centroids must be used for alignment; false to use all the single matching joints</param>
        public void AddKeyJoints(BodyData masterBody, BodyData slaveSkeleton, bool useCentroid)
        {
            //control joints number: if they are too many, remove older joints
            //to avoid computation time explosion
            if (m_MasterPoints.Count > MaximumJointsThresh)
            {
                m_MasterPoints.RemoveRange(0, CutAwayJointsNumber);
                m_SlavePoints.RemoveRange(0, CutAwayJointsNumber);
            }

            //add keypoints from current frame
            GetKeyJointPositionsFromBodySkeleton(masterBody, slaveSkeleton, useCentroid);
        }

        /// <summary>
        /// Reset this instance.
        /// Notice that the key joints type remain unchanged by this function
        /// </summary>
        public void Reset()
        {
            m_SlavePoints = new List<Vector3>();
            m_MasterPoints = new List<Vector3>();
            m_LastAlignmentMatrix = Matrix4x4.Identity;
        }

        /// <summary>
        /// Computes the alignment matrix that converts slave points to master points.
        /// It is the rototranslation matrix that best fits between the two sets of points.
        /// The result is returned as stored in LastAlignmentMatrix property
        /// </summary>
        /// <returns>The alignment matrix</returns>
        public Matrix4x4 ComputeAlignmentMatrix()
        {
            //compute the RT marix
            Matrix4x4 returnMatrix = PointsTransformCalculator.FindRTmatrix(m_SlavePoints, m_MasterPoints);

            //save it and return it
            m_LastAlignmentMatrix = returnMatrix;

            return returnMatrix;
        }

        /// <summary>
        /// Gets the key joint positions from the provided BodyData and body
        /// and adds them to the master/slave point collections
        /// </summary>
        /// <param name="masterBody">Master body whose joints positions have to be extracted</param>
        /// <param name="slaveBody">Slave BodyData whose joints positions have to be extracted</param>
        /// <param name="useCentroid">True if only tracked body centroids must be used for alignment; false to use all the single matching joints</param>
        private void GetKeyJointPositionsFromBodySkeleton(BodyData masterBody, BodyData slaveBody, bool useCentroid)
        {
            //if we have to use centroids
            if (useCentroid)
            {
                Vector3 masterCentroid, slaveCentroid;

                //we compute an upper and lower centroid for each body: the reason is that with only one centroid, if the tracking box have an almost equal height, the system is
                //unable to detect the sense of the up vector, unless the user jumps and crouchs continuously (which is often not the case)

                if (ComputeMasterSlaveStableCentroids(masterBody, slaveBody, StableJointsForCentroidsUpper, out masterCentroid, out slaveCentroid))
                {
                    m_MasterPoints.Add(masterCentroid);
                    m_SlavePoints.Add(slaveCentroid);
                }

                if (ComputeMasterSlaveStableCentroids(masterBody, slaveBody, StableJointsForCentroidsLower, out masterCentroid, out slaveCentroid))
                {
                    m_MasterPoints.Add(masterCentroid);
                    m_SlavePoints.Add(slaveCentroid);
                }
                    

            }
            //else add single joints
            else
            {
                //for each required key joint type
                foreach (BodyJointTypes jt in m_KeyJoints)
                    //check if it is a stable tracked joints. If it isn't so, discard it
                    if (masterBody.Joints[jt].Confidence == 1.0 && slaveBody.Joints[jt].Confidence == 1.0)
                    {
                        //get joint data in both skeletons
                        m_MasterPoints.Add(FancyUtilities.GetVector3FromJoint(masterBody.Joints[jt], 1.0f));
                        m_SlavePoints.Add(FancyUtilities.GetVector3FromJoint(slaveBody.Joints[jt], 1.0f));
                    }
            }

        }

        /// <summary>
        /// Compute centroids of tracked body joints in both a master and a slave tracking box, using only some kinds of body joints
        /// </summary>
        /// <param name="masterBody">Master tracking box body</param>
        /// <param name="slaveBody">Slave tracking box body</param>
        /// <param name="jointTypes">Joint types to consider</param>
        /// <param name="masterCentroid">Out parameter that will hold the computed centroid for the master body</param>
        /// <param name="slaveCentroid">Out parameter that will hold the computed centroid for the slave body</param>
        /// <returns>True if computation was successful, false otherwise (no common tracked joints)</returns>
        private bool ComputeMasterSlaveStableCentroids(BodyData masterBody, BodyData slaveBody, BodyJointTypes[] jointTypes, out Vector3 masterCentroid, out Vector3 slaveCentroid)
        {
            //compute centroid of all stable joints that are tracked in both tracking boxes
            masterCentroid = Vector3.Zero;
            slaveCentroid = Vector3.Zero;
            int jointsCount = 0;

            foreach (BodyJointTypes jt in jointTypes)
            {
                if (masterBody.Joints[jt].Confidence > 0 && slaveBody.Joints[jt].Confidence > 0)
                {
                    masterCentroid += masterBody.Joints[jt].Position;
                    slaveCentroid += slaveBody.Joints[jt].Position;
                    jointsCount++;
                }
            }

            if (jointsCount > 0)
            {
                masterCentroid /= jointsCount;
                slaveCentroid /= jointsCount;
                return true;
            }
            else
                return false;
        }
    }
}