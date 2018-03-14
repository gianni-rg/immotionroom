namespace ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model
{
    using System;

    public class TrackingServiceWalkingGesture : TrackingServiceBodyGesture
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
        public TrackingServiceVector3 EstimatedWalkSpeed { get; set; }

        public TrackingServiceWalkingGesture()
        {
            GestureType = TrackingServiceBodyGestureTypes.Walking;
            EstimatedWalkSpeed = new TrackingServiceVector3();
        }
        /// <summary>
        ///     Converts object to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Player Walking Detection\nTimestamp: {0}\nPlayer is walking state: {1}\nPlayer is moving state: {2}\nEstimated walking speed: {3}", Timestamp, IsWalking, IsMoving, EstimatedWalkSpeed);
        }

        #endregion
    }
}
