namespace ImmotionAR.ImmotionRoom.Common.Helpers.Math
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ImmotionAR.ImmotionRoom.Protocol;

    /// <summary>
    /// Class with utilities for operations on joints
    /// </summary>
    public static class SensoryJointUtilities
    {
        #region Joints utilities

        /// <summary>
        /// Computes the distance between 2 body joints
        /// </summary>
        /// <param name="jointA">First body joint</param>
        /// <param name="jointB">Second body joint</param>
        /// <returns>Distance between the two joints</returns>
        public static float JointsDistance(SensorBodyJointData jointA, SensorBodyJointData jointB)
        {
            float diffX = jointA.PositionX - jointB.PositionX;
            float diffY = jointA.PositionY - jointB.PositionY;
            float diffZ = jointA.PositionZ - jointB.PositionZ;

            return (float)Math.Sqrt(diffX * diffX + diffY * diffY + diffZ * diffZ);
        }

        /// <summary>
        /// Performs difference of two joints positions
        /// </summary>
        /// <param name="jointA">First joint</param>
        /// <param name="jointB">Second joint</param>
        /// <returns>jointA pos - jointB pos</returns>
        public static SensoryJointVector3 JointsPositionDiff(SensorBodyJointData jointA, SensorBodyJointData jointB)
        {
            return new SensoryJointVector3()
            {
                x = jointA.PositionX - jointB.PositionX,
                y = jointA.PositionY - jointB.PositionY,
                z = jointA.PositionZ - jointB.PositionZ,
            };
        }

        /// <summary>
        /// Adds a vector to a joint position, returning the computed position
        /// </summary>
        /// <param name="joint">Joint</param>
        /// <param name="vector">Vector to add</param>
        /// <returns>jointA pos + vector</returns>
        public static SensoryJointVector3 JointsPositionAddVector(SensorBodyJointData joint, SensoryJointVector3 vector)
        {
            return new SensoryJointVector3()
            {
                x = joint.PositionX + vector.x,
                y = joint.PositionY + vector.y,
                z = joint.PositionZ + vector.z,
            };
        }

        /// <summary>
        /// Adds a vector to a joint position and store the result as the new joint position
        /// </summary>
        /// <param name="joint">Joint</param>
        /// <param name="vector">Vector to add</param>
        public static void JointsPositionIncrementWithVector(SensorBodyJointData joint, SensoryJointVector3 vector)
        {
            joint.PositionX += vector.x;
            joint.PositionY += vector.y;
            joint.PositionZ += vector.z;
        }

        /// <summary>
        /// Makes a linear interpolation of two joints position and stores result in another joint.
        /// Result will be alpha * jointA + (1 - alpha) * jointB
        /// </summary>
        /// <param name="jointA">First joint to consider</param>
        /// <param name="jointB">Second joint</param>
        /// <param name="alpha">Interpolation factor, in range [0, 1]</param>
        /// <param name="jointResult">Joint where to store the resulting lerp-ed position</param>
        public static void JointsPositionLerp(SensorBodyJointData jointA, SensorBodyJointData jointB, float alpha, SensorBodyJointData jointResult)
        {
            jointResult.PositionX = jointA.PositionX * alpha + jointB.PositionX * (1 - alpha);
            jointResult.PositionY = jointA.PositionY * alpha + jointB.PositionY * (1 - alpha);
            jointResult.PositionZ = jointA.PositionZ * alpha + jointB.PositionZ * (1 - alpha);
        }

        /// <summary>
        /// Extends the segment going from jointA and jointB, creating another joint staying on the same connecting line and beyond jointB
        /// </summary>
        /// <param name="jointA">Starting joint</param>
        /// <param name="jointB">Ending joint</param>
        /// <param name="extensionFactor">How long should be the segment from jointB to the newly created jointResult, in proportion to the length of the segment from jointA to jointB</param>
        /// <param name="jointResult">Joint where to store the resulting extended position</param>
        public static void ExtendJointsSegment(SensorBodyJointData jointA, SensorBodyJointData jointB, float extensionFactor, SensorBodyJointData jointResult)
        {
            jointResult.PositionX = jointB.PositionX + extensionFactor * (jointA.PositionX - jointB.PositionX);
            jointResult.PositionY = jointB.PositionY + extensionFactor * (jointA.PositionY - jointB.PositionY);
            jointResult.PositionZ = jointB.PositionZ + extensionFactor * (jointA.PositionZ - jointB.PositionZ);
        }

        #endregion
    }
}
