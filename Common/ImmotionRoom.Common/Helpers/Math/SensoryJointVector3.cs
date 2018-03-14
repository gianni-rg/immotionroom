namespace ImmotionAR.ImmotionRoom.Common.Helpers.Math
{
    /// <summary>
    /// Represents a 3-dimensional vector
    /// </summary>
    public class SensoryJointVector3
    {
        #region Public fields

        public float x;
        public float y;
        public float z;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets vector magnitude
        /// </summary>
        public float Magnitude
        {
            get
            {
                return (float)System.Math.Sqrt(x * x + y * y + z * z);
            }
        }

        /// <summary>
        /// Gets normalized version of this vector
        /// </summary>
        public SensoryJointVector3 Normalized
        {
            get
            {
                return this * (1 / this.Magnitude);
            }
        }

        #endregion

        #region Basic vectors operation

        /// <summary>
        /// Multiply a vector by a scalar
        /// </summary>
        /// <param name="vector">Vector</param>
        /// <param name="scalar">Scalar</param>
        /// <returns>Scalara multiplication</returns>
        public static SensoryJointVector3 operator*(SensoryJointVector3 vector, float scalar)
        {
            return new SensoryJointVector3()
            {
                x = vector.x * scalar,
                y = vector.y * scalar,
                z = vector.z * scalar,
            };
        }

        #endregion

        #region Vector products

        /// <summary>
        /// Computes the cross product of two vectors
        /// </summary>
        /// <param name="vector1">Left vector of cross product</param>
        /// <param name="vector2">Right vector of cross product</param>
        /// <returns>Cross product</returns>
        public static SensoryJointVector3 CrossProduct(SensoryJointVector3 vector1, SensoryJointVector3 vector2)
        {
            //from https://it.wikipedia.org/wiki/Prodotto_vettoriale
            return new SensoryJointVector3()
            {
                x = vector1.y * vector2.z - vector1.z * vector2.y,
                y = vector1.z * vector2.x - vector1.x * vector2.z,
                z = vector1.x * vector2.y - vector1.y * vector2.x,
            };
        }

        /// <summary>
        /// Computes the dot product of two vectors
        /// </summary>
        /// <param name="vector1">Left vector of dot product</param>
        /// <param name="vector2">Right vector of dot product</param>
        /// <returns>Dot Product between two vectors</returns>
        public static float DotProduct(SensoryJointVector3 vector1, SensoryJointVector3 vector2)
        {
            return vector1.x * vector2.x + vector1.y * vector2.y + vector1.z * vector2.z;
        }

        #endregion
    }

    /// <summary>
    /// Provides extension methods for <see cref="SensoryJointVector3"/> class
    /// </summary>
    public static class SensoryJointVector3Extensions
    {
        
    }
}
