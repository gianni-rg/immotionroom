namespace ImmotionAR.ImmotionRoom.TrackingEngine.Model
{
    /// <summary>
    ///     Extensions methods for mathematical operations
    /// </summary>
    internal static class MathExtensions
    {
        #region To 4x4 Matrix Conversions

        /// <summary>
        ///     Convert a 3-dimensional vector to an equivalent translation matrix
        /// </summary>
        /// <param name="translationVector">Translation vector</param>
        /// <returns>Translation 4x4 matrix</returns>
        public static Matrix4x4 ToTranslationMatrix(this Vector3 translationVector)
        {
            return new Matrix4x4(
                1, 0, 0, translationVector.X,
                0, 1, 0, translationVector.Y,
                0, 0, 1, translationVector.Z,
                0, 0, 0, 1);
        }

        /// <summary>
        ///     Convert a rotation expressed by a quaternion to an equivalent rotational matrix
        /// </summary>
        /// <param name="rotationQuaternion">Rotation quaternion</param>
        /// <returns>Rotation 4x4 matrix</returns>
        public static Matrix4x4 ToRotationalMatrix(this Quaternion rotationQuaternion)
        {
            //conversions by http://fabiensanglard.net/doom3_documentation/37726-293748.pdf
            //memory-optimized version
            //return new Matrix4x4(1 - 2 * rotationQuaternion.Y * rotationQuaternion.Y - 2 * rotationQuaternion.Z * rotationQuaternion.Z, 2 * rotationQuaternion.X * rotationQuaternion.Y + 2 * rotationQuaternion.W * rotationQuaternion.Z, 2 * rotationQuaternion.X * rotationQuaternion.Z - 2 * rotationQuaternion.W * rotationQuaternion.Y, 0,
            //                     2 * rotationQuaternion.X * rotationQuaternion.Y - 2 * rotationQuaternion.W * rotationQuaternion.Z, 1 - 2 * rotationQuaternion.X * rotationQuaternion.X - 2 * rotationQuaternion.Z * rotationQuaternion.Z, 2 * rotationQuaternion.Y * rotationQuaternion.Z + 2 * rotationQuaternion.W * rotationQuaternion.X, 0,
            //                     2 * rotationQuaternion.X * rotationQuaternion.Z + 2 * rotationQuaternion.W * rotationQuaternion.Y, 2 * rotationQuaternion.Y * rotationQuaternion.Z - 2 * rotationQuaternion.W * rotationQuaternion.X, 1 - 2 * rotationQuaternion.X * rotationQuaternion.X - 2 * rotationQuaternion.Y * rotationQuaternion.Y, 0,
            //                     0, 0, 0, 1); 

            //cpu-optimized version
            float x = rotationQuaternion.X, y = rotationQuaternion.Y, z = rotationQuaternion.Z, w = rotationQuaternion.W;
            float xx = 2*x, yy = 2*y, zz = 2*z;
            float xx2 = xx*x, yy2 = yy*y, zz2 = zz*z;

            // LEFT-HANDED!
            // https://en.wikipedia.org/wiki/Rotation_matrix
            return new Matrix4x4(1 - yy2 - zz2, xx*y - zz*w, xx*z + yy*w, 0,
                xx*y + zz*w, 1 - xx2 - zz2, yy*z - xx*w, 0,
                xx*z - yy*w, yy*z + xx*w, 1 - xx2 - yy2, 0,
                0, 0, 0, 1);
        }

        /// <summary>
        ///     Convert a 3-dimensional vector to an equivalent scale matrix
        /// </summary>
        /// <param name="scaleVector">Scale vector</param>
        /// <returns>Scale 4x4 matrix</returns>
        public static Matrix4x4 ToScaleMatrix(this Vector3 scaleVector)
        {
            return new Matrix4x4(scaleVector.X, 0, 0, 0,
                0, scaleVector.Y, 0, 0,
                0, 0, scaleVector.Z, 0,
                0, 0, 0, 1);
        }

        #endregion
    }
}
