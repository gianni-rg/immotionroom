namespace ImmotionAR.ImmotionRoom.TrackingEngine.Model
{
    /// <summary>
    /// Provides extensions methods to make conversion between BodyData and Unity/C# data types
    /// </summary>
    public static class BodyDataExtensions
    {
        #region Joint utilities

        /// <summary>
        /// Gets Position Vector of the joint data
        /// </summary>
        /// <param name="joint">This joint data</param>
        /// <returns>Vector3 of Position of joint</returns>
        public static Vector3 ToVector3(this BodyJointData joint)
        {
            return new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);
        }

        ///// <summary>
        ///// Gets Position Vector of the joint data, scaled by 10 and flipped around Z
        ///// </summary>
        ///// <param name="joint">This joint data</param>
        ///// <returns>Vector3 of Position of joint</returns>
        //public static Vector3 ToUnityVector3(this BodyJointData joint)
        //{
        //    return 10 * new Vector3(joint.Position.X, joint.Position.Y, -joint.Position.Z);
        //}

        ///// <summary>
        ///// Gets Position Vector of the joint data, scaled by 10 and flipped around Z. X component gets discarded
        ///// </summary>
        ///// <param name="joint">This joint data</param>
        ///// <returns>Vector3 of Position of joint</returns>
        //public static Vector3 ToUnityVector3NoX(this BodyJointData joint)
        //{
        //    return 10 * new Vector3(0, joint.Position.Y, -joint.Position.Z);
        //}

        ///// <summary>
        ///// Gets Position of the joint data
        ///// </summary>
        ///// <param name="joint">This joint data</param>
        ///// <param name="scale">Scale that must be applied to position</param>
        ///// <returns>Vector3 of Position of joint</returns>
        //public static Vector3 ToVector3(this BodyJointData joint, float scale)
        //{
        //    return scale * new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);
        //}

        #endregion
    }

}