namespace ImmotionAR.ImmotionRoom.TrackingEngine.Walking
{
    using System;
    using Model;

    /// <summary>
    ///     Detection results about player knee walking status by a <see cref="KneeWalkingDetector" />
    /// </summary>
    internal struct KneeWalkingDetection
    {
        /// <summary>
        ///     Timestamp of this data (it is an offset since the start of the program)
        /// </summary>
        public TimeSpan Timestamp { get; set; }

        /// <summary>
        ///     True if the knee seems in the middle of a walking movement, false otherwise
        /// </summary>
        public bool IsWalking { get; set; }

        /// <summary>
        ///     Knee walking status, raw.
        /// </summary>
        public KneeWalkingStatus KneeRawStatus { get; set; }

        /// <summary>
        ///     Estimated current status of the knee. This is slightly different from <see cref="KneeRawStatus" /> property.
        ///     Raw is the present status of the physical knee.
        ///     Estimated tells the estimated knee status wrt the current walking movement.
        ///     E.g. if the knee raises for a noise spike in current frame, RawStatus would be Raising, because the knee is
        ///     raising,
        ///     but estimated status would be Still, because this raising movement would be considered as noise.
        /// </summary>
        public KneeWalkingStatus KneeEstimatedStatus { get; set; }

        /// <summary>
        ///     Angle the knee is making with regard to hip joint, in the XZ plane.
        ///     If this value is near to 0, measurements could be noisy
        /// </summary>
        public float HorizontalKneeAngle { get; set; }

        /// <summary>
        ///     Estimated walk speed for current time
        /// </summary>
        public float EstimatedWalkSpeed { get; set; }

        /// <summary>
        ///     Estimated walk speed carrier for current walking movement, if any.
        ///     I'll explain the meaning of the carrier with an example: if leg speed is computed with a sinusoidal movement, we'd
        ///     have at
        ///     different timestamp different speed values...if speed modulus is 3 and we're going at 30fps, we'd have 3*sin(0),
        ///     3*sin(0.033), 3*sin(0.066),...
        ///     as you can see, all values are different, but one value is the same: 3, that is in this case the modulus of the
        ///     speed computation.
        ///     I've called this value the carrier: it is the value that must be used to compute walk speed at each frame
        /// </summary>
        public float EstimatedWalkSpeedCarrier { get; set; }

        /// <summary>
        ///     Last detection time of a walking movement start on this knee (it is an offset since the start of the program)
        ///     The start moment is the one of the first detection of a knee Rising towards the ceiling while it was still the
        ///     frame before
        /// </summary>
        public TimeSpan LastDetectedWalkStart { get; set; }

        /// <summary>
        ///     Last detection time of a walking movement on this knee (it is an offset since the start of the program)
        ///     This is equivalent to current time if the knee is currently walking, or to last time of a knee falling to the floor
        ///     the frame
        ///     before it became still
        /// </summary>
        public TimeSpan LastDetectedWalkInstant { get; set; }

        #region Object methods

        /// <summary>
        ///     Converts object to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Knee Walking Detection\nTimestamp: {0}\nPlayer is walking state: {1}\nKnee raw status: {2}\n" +
                                 "Knee estimated status: {3}\nHorizontal knee angle (deg): {4}\nEstimated walk speed carrier: {6}\n" +
                                 "Estimated walking speed: {5}\nLast detected walk start: {7}\nLast detected walk instant: {8}",
                Timestamp, IsWalking, KneeRawStatus, KneeEstimatedStatus, (MathConstants.Rad2Deg*HorizontalKneeAngle).ToString("+000;_000;+000"), EstimatedWalkSpeed.ToString("+000.00;_000.00;+000.00"),
                EstimatedWalkSpeedCarrier.ToString("+000.00;_000.00;+000.00"), LastDetectedWalkStart, LastDetectedWalkInstant);
        }

        #endregion
    }
}
