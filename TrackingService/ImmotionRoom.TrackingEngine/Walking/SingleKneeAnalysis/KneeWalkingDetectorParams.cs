namespace ImmotionAR.ImmotionRoom.TrackingEngine.Walking
{
    /// <summary>
    ///     Parameters for player walking detection using informations on knee joints
    /// </summary>
    public struct KneeWalkingDetectorParams
    {
        /// <summary>
        ///     If this flag is true, heuristics on walking detection will be made using joint acceleration;
        ///     If it is false, linear speed will be used
        /// </summary>
        public bool UseAcceleration { get; set; }

        /// <summary>
        ///     Angle of leg wrt the vertical silhouette of the player, below which the knee is considered as still.
        ///     This value, expressed in radians, is useful to discard knee joint position measurement noise
        /// </summary>
        public float StillAngleThreshold { get; set; }

        /// <summary>
        ///     Lower threshold for joint speed or acceleration (depending on <see cref="UseAcceleration" /> attribute) of the
        ///     joint,
        ///     to begin considering this knee joint as Rising towards the ceiling while it was still.
        ///     The value refer to the module in direction of the forward vector of the user (the direction his legs are facing)
        /// </summary>
        public float StillToRisingThreshold { get; set; }

        /// <summary>
        ///     Lower threshold for joint speed or acceleration (depending on <see cref="UseAcceleration" /> attribute) of the
        ///     joint,
        ///     to begin considering this joint as falling towards the floor.
        ///     The value refer to the module in direction of the forward vector of the user (the direction his legs are facing)
        /// </summary>
        public float AnyStateToFallingThreshold { get; set; }

        /// <summary>
        ///     Higher threshold for joint speed or acceleration (depending on <see cref="UseAcceleration" /> attribute) of the
        ///     joint,
        ///     to begin considering this joint as staying still.
        ///     The value refer to the module in direction of the forward vector of the user (the direction his legs are facing)
        /// </summary>
        public float AnyStateToStillThreshold { get; set; }

        /// <summary>
        ///     Tolerance of horizontal knee angles, in radians, between consecutive frames, for the leg movement to be considered
        ///     as coherent
        /// </summary>
        public float RisingAngleTolerance { get; set; }

        /// <summary>
        ///     Lower threshold for joint speed or acceleration (depending on <see cref="UseAcceleration" /> attribute) of the
        ///     joint,
        ///     to consider this movement as a noise spike due to bad joint detection
        /// </summary>
        public float SpikeNoiseThreshold { get; set; }

        /// <summary>
        ///     Amount of consecutive detected movement time, in seconds, to make the detector trigger the movement action.
        ///     This can be used to avoid noise false positives
        /// </summary>
        public float TimeToTriggerMovement { get; set; }

        /// <summary>
        ///     Amount of consecutive detected stillness time, to make the detector trigger the stillness action.
        ///     This can be used to avoid noise false negatives
        /// </summary>
        public float TimeToTriggerStillness { get; set; }

        /// <summary>
        ///     Speed to assign to the walk movement if the leg is falling and appears to be stopped but we are not sure about it
        ///     yet
        /// </summary>
        public float AlmostStillSpeed { get; set; }

        /// <summary>
        ///     Number that maps the trigger movement speed of the leg to the main speed of current walk movement (the carrier).
        ///     I.e. this number is the one that decides the speed of the step, basing on the speed of the Rising leg movement.
        ///     Notice that this value regards always the speed, acceleration is never taken in count.
        /// </summary>
        public float TriggerToSpeedMultiplier { get; set; }

        /// <summary>
        ///     Value that maps player speed in the knee falling movement wrt to speed during rising movement.
        ///     (e.g. if this value is 2, the player will go 2 times faster during falling knee movement than during rising
        ///     movement)
        /// </summary>
        public float FallingToRisingSpeedMultiplier { get; set; }

        /// <summary>
        ///     Estimated frame rate of the body tracking service (serves only for array allocations... overvalue, if in doubt)
        /// </summary>
        public int EstimatedFrameRate { get; set; }
    }
}
