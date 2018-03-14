namespace ImmotionAR.ImmotionRoom.TrackingEngine.Walking
{
    /// <summary>
    ///     Defines possible status of player movement (i.e. translation of himself)
    /// </summary>
    public enum PlayerMovingStatus
    {
        /// <summary>
        ///     Player is non moving: he stays still (maybe walking in place, but still)
        /// </summary>
        NonMoving,

        /// <summary>
        ///     Player seems non moving, but currently some movements have been detected
        /// </summary>
        NonMovingWithMovements,

        /// <summary>
        ///     Player is moving
        /// </summary>
        Moving,

        /// <summary>
        ///     Player is moving, but currently some frames of stillness have been detected
        /// </summary>
        MovingWithNonMovements
    }
}