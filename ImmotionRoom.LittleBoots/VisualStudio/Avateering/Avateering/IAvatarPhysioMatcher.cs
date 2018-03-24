namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Base interface for all objects that help matching an Avatar shape to the actual body in the physical world
    /// (e.g. matching the length of the arms, the lengths, the height, etc...)
    /// </summary>
    public interface IAvatarPhysioMatcher
    {
        /// <summary>
        /// Match a particular feature of the avatar with the user body in the physical world.
        /// So, for example, makes the avatar to match the user's body height.
        /// This is a coroutine, because the method could require various iterations steps
        /// </summary>
        /// <param name="featureID">ID of the feature to consider (e.g. height, arm length, etc...). The value depends on the particular implementation. It is advised the use of <see cref="PhysioMatchingFeatures"/> class constants</param>
        /// <param name="value">The value that feature must assume (i.e. the value of the feature for the user's body)</param>
        /// <returns></returns>
        IEnumerator MatchFeature(int featureID, float value);
    }
}
