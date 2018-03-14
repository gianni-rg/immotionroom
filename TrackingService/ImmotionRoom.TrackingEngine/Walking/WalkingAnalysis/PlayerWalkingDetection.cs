namespace ImmotionAR.ImmotionRoom.TrackingEngine.Walking
{
    using System;
    using Model;

    /// <summary>
    ///     Detection results about player knee walking status by a <see cref="IPlayerWalkingDetector" /> object
    /// </summary>
    public class PlayerWalkingDetection : BodyGesture
    {
        #region Properties
        /// <summary>
        ///     Identifier of the Body associated to this detection
        /// </summary>
        public ulong BodyId { get; set; }

        /// <summary>
        ///     Timestamp of this data (it is an offset since the start of the program)
        /// </summary>
        public TimeSpan Timestamp { get; set; }

        /// <summary>
        ///     True if the player seems in the middle of a walking movement, false otherwise
        /// </summary>
        public bool IsWalking { get; set; }

        /// <summary>
        ///     True if the player seems to move around the tracking area, false otherwise
        /// </summary>
        public bool IsMoving { get; set; }

        /// <summary>
        ///     Estimated walk speed for current time
        /// </summary>
        public Vector3 EstimatedWalkSpeed { get; set; } 
        #endregion

        #region Constructor

        public PlayerWalkingDetection()
        {
            GestureType = BodyGestureTypes.Walking;
        }
        #endregion

        #region Public methods

        /// <summary>
        ///     Converts object to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Player Walking Detection\nTimestamp: {0}\nPlayer is walking state: {1}\nPlayer is moving state: {2}\nEstimated walking speed: {3}",
                Timestamp, IsWalking, IsMoving, EstimatedWalkSpeed.ToString("+000.00;_000.00;+000.00"));
        }

        #endregion
    }
}