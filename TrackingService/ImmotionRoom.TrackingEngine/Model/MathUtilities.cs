namespace ImmotionAR.ImmotionRoom.TrackingEngine.Model
{
    using System;

    /// <summary>
    ///     Offers mathematical operations utilities
    /// </summary>
    internal static class MathUtilities
    {
        #region Angles utilities

        /// <summary>
        ///     Calculate atan2 and clamp result in range [-pi, +pi]
        /// </summary>
        /// <returns>The desired atan2</returns>
        /// <param name="y">The y coordinate</param>
        /// <param name="x">The x coordinate</param>
        public static float ClampedAtan2(float y, float x)
        {
            var orientation = Math.Atan2(y, x);

            //clamp orientation in [-pi, +pi] range 
            while (orientation < -Math.PI)
                orientation += 2*Math.PI;

            while (orientation > +Math.PI)
                orientation -= 2*Math.PI;

            return (float) orientation;
        }

        /// <summary>
        ///     Adjusts an orientation so that it stays in range [-pi, +pi) of distance from a given orientation reference
        /// </summary>
        /// <returns>The orientation adjusted to stay near the reference</returns>
        /// <param name="orientation">Orientation</param>
        /// <param name="orientationReference">Orientation reference</param>
        public static float AdjustOrientation(double orientation, double orientationReference)
        {
            //clamp orientation in [-pi, +pi] range from reference
            while (orientation <= orientationReference - Math.PI)
                orientation += 2*Math.PI;

            while (orientation > orientationReference + Math.PI)
                orientation -= 2*Math.PI;

            return (float) orientation;
        }

        /// <summary>
        ///     Computes smart difference between two angles
        ///     (e.g. it considers that angles 0 and 2pi are equivalent, so their difference is zero)
        /// </summary>
        /// <param name="angle1">First angle</param>
        /// <param name="angle2">Second angle</param>
        /// <returns>Distance between two angles, in radians</returns>
        public static double AdjustedAnglesAbsDifference(double angle1, double angle2)
        {
            return Math.Abs(angle1 - AdjustOrientation(angle2, angle1));
        }

        /// <summary>
        ///     Helper functions that determines the angle from a 2D-vector to another
        /// </summary>
        /// <param name="from">Start vector</param>
        /// <param name="to">Destination vector</param>
        /// <returns>Signed angle between the two vectors in the range [-180, +180]</returns>
        public static float SignedVector2Angle(Vector2 from, Vector2 to)
        {
            //from http://answers.unity3d.com/questions/162177/vector2angles-direction.html

            //take the angle magnitude using Unity function and determine the direction using the cross product orientation
            var ang = Vector2.Angle(from, to);
            var cross = Vector3.Cross(from, to);

            if (cross.Z > 0)
                ang = -ang;

            return ang;
        }

        #endregion

        #region Joint utilities

        /// <summary>
        ///     Calculate orientation of line segment that goes from startJoint to endJoint, in the XZ plane (Y component is
        ///     discarded)
        /// </summary>
        /// <returns>The joints segment XZ orientation</returns>
        /// <param name="startJoint">Start joint</param>
        /// <param name="endJoint">End joint</param>
        public static float BetweenJointsXZOrientation(Vector3 startJoint, Vector3 endJoint)
        {
            var diffJoint = endJoint - startJoint;

            return ClampedAtan2(diffJoint.Z, diffJoint.X);
        }

        /// <summary>
        ///     Calculate module of line segment that goes from startJoint to endJoint, in the XZ plane (Y component is discarded)
        /// </summary>
        /// <returns>The joints segment XZ orientation</returns>
        /// <param name="startJoint">Start joint</param>
        /// <param name="endJoint">End joint</param>
        public static double BetweenJointsXZDistance(Vector3 startJoint, Vector3 endJoint)
        {
            return Math.Sqrt(BetweenJointsXZSqrDistance(startJoint, endJoint));
        }

        /// <summary>
        ///     Calculate squared module of line segment that goes from startJoint to endJoint, in the XZ plane (Y component is
        ///     discarded)
        /// </summary>
        /// <returns>The joints segment XZ orientation</returns>
        /// <param name="startJoint">Start joint</param>
        /// <param name="endJoint">End joint</param>
        public static double BetweenJointsXZSqrDistance(Vector3 startJoint, Vector3 endJoint)
        {
            var diffJoint = endJoint - startJoint;

            return new Vector2(diffJoint.X, diffJoint.Z).SqrMagnitude;
        }

        #endregion

        //public static int Clamp(int value, int min, int max)
        //{
        //    if (value < min)
        //    {
        //        value = min;
        //    }
        //    else if (value > max)
        //    {
        //        value = max;
        //    }
        //    return value;
        //}

        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }
            return value;
        }
    }
}