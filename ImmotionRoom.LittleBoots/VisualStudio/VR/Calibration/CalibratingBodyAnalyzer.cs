namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Calibration
{
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataSourcesManagement;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Tools;
    using ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Offers method to analyze the body that is currently calibrating the system, to see if it is moving or similar stuff
    /// </summary>
    internal class CalibratingBodyAnalyzer
    {
        #region Constants definition

        /// <summary>
        /// Tolerance, in meters, of user movements to be still considered "still" (no pun intended)
        /// during initialization stage
        /// </summary>
        private const float StandingMovementTolerance = 0.095f;

        /// <summary>
        /// Squared value of StandingMovementTolerance
        /// </summary>
        private const float SquaredStandingMovementTolerance = StandingMovementTolerance * StandingMovementTolerance;

        /// <summary>
        /// Tolerance, in meters, of spine points distance in the XZ plane for the user to be considered standing erect
        /// </summary>
        private const float StandingErectSpineTolerance = 0.15f;

        /// <summary>
        /// Squared value of StandingErectSpineTolerance
        /// </summary>
        private const float SquaredStandingErectSpineTolerance = StandingErectSpineTolerance * StandingErectSpineTolerance;

        /// <summary>
        /// Key joints used to detect user orientation
        /// </summary>
        private static readonly TrackingServiceBodyJointTypes[] KeyJoints = new TrackingServiceBodyJointTypes[] 
	    {
		    TrackingServiceBodyJointTypes.ShoulderLeft,
		    TrackingServiceBodyJointTypes.ShoulderRight,
		    TrackingServiceBodyJointTypes.SpineMid,
		    TrackingServiceBodyJointTypes.SpineShoulder
	    };

        /// <summary>
        /// Key joints used to detect if user is erect
        /// </summary>
        private static readonly TrackingServiceBodyJointTypes[] SpineJoints = new TrackingServiceBodyJointTypes[] 
	    {
		    TrackingServiceBodyJointTypes.SpineBase,
		    TrackingServiceBodyJointTypes.SpineMid,
		    TrackingServiceBodyJointTypes.SpineShoulder,
		    TrackingServiceBodyJointTypes.Neck
	    };

        #endregion

        #region Physical body constants definition

        /// <summary>
        /// Distance, in meters, from the tracking service head joint to actual top of the head
        /// </summary>
        internal const float HeadJointToHeadTopDistance = 0.1f;

        /// <summary>
        /// Distance, in meters, from the tracking service foot joint to tracking service ankle joint
        /// </summary>
        internal const float FootToAnkleJointDistance = 0.11f;

        /// <summary>
        /// Distance, along the y axis, of the top of the head from the eyes, in meters
        /// </summary>
        internal const float HeadToEyeHeight = -0.03f;

        /// <summary>
        /// Distance, along the z axis, of the top of the head from the eyes, in meters
        /// </summary>
        internal const float HeadToEyeFront = 0.02f;

        #endregion

        #region Private fields

        /// <summary>
        /// Provider of calibrating user data
        /// </summary>
        private BodyDataProvider m_calibratingBodyProvider;

        /// <summary>
        /// Body detected at the last call to <see cref="PlayerStandingStillCheck"/>
        /// </summary>
        private TrackingServiceBodyData m_lastBodyData;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="calibratingBodyDataProvider">Provider of data for calibrating user's body</param>
        internal CalibratingBodyAnalyzer(BodyDataProvider calibratingBodyDataProvider)
        {
            m_calibratingBodyProvider = calibratingBodyDataProvider;
        }

        #endregion

        #region Internal Analysis methods

        /// <summary>
        /// Get calibrating user orientation around the Y axis.
        /// </summary>
        /// <returns>Unity matrix representing the user's rotation</returns>
        internal Matrix4x4 GetCalibratingUserRotationMatrix()
        {
            return GetBodyRotation(m_calibratingBodyProvider.LastBody);
        }

        /// <summary>
        /// Get orientation angle of a certain body, using shoulders angle.
        /// </summary>
        /// <returns>Rotation around the y-axis of the user</returns>
        internal float GetCalibratingUserRotationAngle()
        {
            return GetBodyOrientation(m_calibratingBodyProvider.LastBody);
        }

        /// <summary>
        /// Detects if player is standing erect and still during final calibration stage
        /// </summary>
        /// <returns>True if player is behaving correctly, false otherwise</returns>
        internal bool CalibratingUserStandingStill()
        {
            return PlayerStandingStillCheck();
        }

        /// <summary>
        /// Compute current user height, as seen in current frame
        /// </summary>
        /// <returns>Height of user, in meters</returns>
        internal float GetCalibratingUserHeight()
        {
            return CalculateUserHeight();
        }

        #endregion

        #region Private bodies utilities

        /// <summary>
        /// Get calibrating user orientation around the Y axis.
        /// Returns identity matrix if a null user gets passed
        /// </summary>
        /// <param name="userBody">User body data to analyze</param>
        /// <returns>Unity matrix representing this rotation</returns>
        private static Matrix4x4 GetBodyRotation(TrackingServiceBodyData userBody)
        {
            if (userBody == null)
                return Matrix4x4.identity;

            //use shoulders angle
            float angle = GetBodyOrientation(userBody);

            //this is the only angle we need to compute the rotation matrix
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.up), Vector3.one);

            return rotationMatrix;
        }

        /// <summary>
        /// Get orientation angle of a certain body, using shoulders angle.
        /// Returns 0 if a null user gets passed
        /// </summary>
        /// <param name="userBody">Body to see the rotation of</param>
        /// <returns>Rotation around the y-axis of the user</returns>
        private static float GetBodyOrientation(TrackingServiceBodyData userBody)
        {
            if (userBody == null)
                return 0;

            return UnityUtilities.BetweenJointsXZOrientation(userBody.Joints[TrackingServiceBodyJointTypes.ShoulderLeft].ToVector3(),
                                                             userBody.Joints[TrackingServiceBodyJointTypes.ShoulderRight].ToVector3());
        }

        /// <summary>
        /// Detects if player is standing erect and still during final calibration stage
        /// </summary>
        /// <returns>True if player is behaving correctly, false otherwise</returns>
        private bool PlayerStandingStillCheck()
        {
            if (m_calibratingBodyProvider.LastBody == null)
                return false;

            //if we have no last data, return no movement
            if (m_lastBodyData == null)
            {
                m_lastBodyData = m_calibratingBodyProvider.LastBody;
                return false;
            }

            //cycle through the key joints and check if the user stands still wrt the last call to this methods
            bool wrongBehaviour = false;

            foreach (TrackingServiceBodyJointTypes jt in KeyJoints)
            {
                if (!(Vector3.Distance(m_calibratingBodyProvider.LastBody.Joints[jt].ToVector3(), m_lastBodyData.Joints[jt].ToVector3()) < SquaredStandingMovementTolerance))
                    wrongBehaviour = true;
            }

            //check if the player stands erect using spine points
            foreach (TrackingServiceBodyJointTypes jt in SpineJoints)
            {
                if (!(UnityUtilities.BetweenJointsXZSqrDistance(m_calibratingBodyProvider.LastBody.Joints[jt].ToVector3(), m_calibratingBodyProvider.LastBody.Joints[0].ToVector3()) < SquaredStandingErectSpineTolerance))
                    wrongBehaviour = true;
            }

            //save last body data
            m_lastBodyData = m_calibratingBodyProvider.LastBody;

            return wrongBehaviour;
        }

        /// <summary>
        /// Compute current user height, as seen in current frame
        /// </summary>
        /// <returns>Height of user, in meters</returns>
        private float CalculateUserHeight()
        {
            if (m_calibratingBodyProvider.LastBody == null)
                return 0;

            //compute average length of legs
            float avgLegsLength = (UnityUtilities.BetweenJointsDistance(m_calibratingBodyProvider.LastBody, TrackingServiceBodyJointTypes.HipLeft, TrackingServiceBodyJointTypes.KneeLeft) +
                                   UnityUtilities.BetweenJointsDistance(m_calibratingBodyProvider.LastBody, TrackingServiceBodyJointTypes.KneeLeft, TrackingServiceBodyJointTypes.AnkleLeft) +
                                   UnityUtilities.BetweenJointsDistance(m_calibratingBodyProvider.LastBody, TrackingServiceBodyJointTypes.HipRight, TrackingServiceBodyJointTypes.KneeRight) +
                                   UnityUtilities.BetweenJointsDistance(m_calibratingBodyProvider.LastBody, TrackingServiceBodyJointTypes.KneeRight, TrackingServiceBodyJointTypes.AnkleRight)) / 2;

            //compute height, adding the length of the whole spine and head to the one of the legs
            float userHeight = UnityUtilities.BetweenJointsDistance(m_calibratingBodyProvider.LastBody, TrackingServiceBodyJointTypes.Neck, TrackingServiceBodyJointTypes.Head) +
                               UnityUtilities.BetweenJointsDistance(m_calibratingBodyProvider.LastBody, TrackingServiceBodyJointTypes.Neck, TrackingServiceBodyJointTypes.SpineShoulder) +
                               UnityUtilities.BetweenJointsDistance(m_calibratingBodyProvider.LastBody, TrackingServiceBodyJointTypes.SpineShoulder, TrackingServiceBodyJointTypes.SpineMid) +
                               UnityUtilities.BetweenJointsDistance(m_calibratingBodyProvider.LastBody, TrackingServiceBodyJointTypes.SpineMid, TrackingServiceBodyJointTypes.SpineBase) +
                               avgLegsLength;

            //add head tip and ankle to floor distance and then return the obtained value
            return userHeight + HeadJointToHeadTopDistance + FootToAnkleJointDistance;
        }

        #endregion
    }
}
