namespace ImmotionAR.ImmotionRoom.TrackingEngine.Walking
{
    /// <summary>
    ///     Possible status of player knee wrt walking operation
    /// </summary>
    internal enum KneeWalkingStatus
    {
        /// <summary>
        ///     Unknown status
        /// </summary>
        Unknown,

        /// <summary>
        ///     Knee is still, probably on the floor
        /// </summary>
        Still,

        /// <summary>
        ///     Knee is Rising towards the ceiling
        /// </summary>
        Rising,

        /// <summary>
        ///     Knee seems to have stopped while Rising towards the ceiling
        /// </summary>
        RisingStill,

        /// <summary>
        ///     Knee is returning back to the floor, falling
        /// </summary>
        Falling
    }
}