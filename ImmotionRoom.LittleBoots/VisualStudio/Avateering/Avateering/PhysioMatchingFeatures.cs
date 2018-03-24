namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Constants defining standard features IDs for the <see cref="IAvatarPhysioMatcher"/> class
    /// </summary>
    public static class PhysioMatchingFeatures
    {
        /// <summary>
        /// Overall height of the user, from neck to ankles
        /// </summary>
        public const int Height = 0;

        /// <summary>
        /// Distance between the two shoulders of the user
        /// </summary>
        public const int ShouldersWidth = 4;

        /// <summary>
        /// Mean length of user's arms, from shoulder to wrist
        /// </summary>
        public const int ArmsLength = 1;

        /// <summary>
        /// Mean length of user's forearms, from elbow to wrist
        /// </summary>
        public const int ForeArmsLength = 2;

        /// <summary>
        /// Mean length of user's legs, from hip to ankle
        /// </summary>
        public const int LegsLength = 3;
    }
}
