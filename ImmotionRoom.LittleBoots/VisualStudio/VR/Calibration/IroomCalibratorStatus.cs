namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Calibration
{

    /// <summary>
    /// Enumeration of possible ImmotionRoom-headset calibrator statuses. Used in <see cref="IroomHeadsetCalibrator"/> class.
    /// The members name are self-explanatory
    /// </summary>
    public enum IroomCalibratorStatus
    {
        None,
        WaitingForBody,
        EndOfWaiting,
        RotatingBodyToOrigin,
        EndOfRotating,
        BodyStandingStill,
        InitializingWithBody,
        EndOfInitialization,
        Calibrating,
        Done,
        Exit
    };

}