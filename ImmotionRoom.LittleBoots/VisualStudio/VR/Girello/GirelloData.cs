namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Girello
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Defines public characteristics of a Girello, usable by Unity objects.
    /// Assign this properties as scale, position and rotation of a cube to obtain an exact replica of Girello
    /// </summary>
    public class GirelloData
    {
        /// <summary>
        /// Size of the Girello box, in world coordinates
        /// </summary>
        public Vector3 Size;

        /// <summary>
        /// Center of Girello box, in world coordinates
        /// </summary>
        public Vector3 Center;

        /// <summary>
        /// Rotation of Girello box, wrt world coordinates
        /// </summary>
        public Quaternion Rotation;
    }
}
