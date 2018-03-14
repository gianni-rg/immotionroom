namespace ImmotionAR.ImmotionRoom.TrackingEngine.Model
{
    ///// <summary>
    /////     Enumerations of available Body Merging methods
    ///// </summary>
    //public enum BodyMergingMethods
    //{
    //    /// <summary>
    //    ///     Simple average of body joints
    //    /// </summary>
    //    SimpleAverage,

    //    /// <summary>
    //    ///     Elaborated averaging of body joints
    //    /// </summary>
    //    ProAveraging,

    //    /// <summary>
    //    ///     Averaging of body joints, after that the system has detected which tracking boxes are more reliable for the tracking of the joints
    //    /// </summary>
    //    ReliableAveraging,
    //}

    /// <summary>
    ///     Enumeration of possible Calibration steps.
    /// </summary>
    public enum CalibrationSteps
    {
        Start,
        WaitingForBody,
        InitializingWithBody,
        Tracking,
        Done,
    };


    ///// <summary>
    /////     Defines the possible statuses in which a PlayerWalkingManager object can be.
    ///// </summary>
    //public enum DataSourcesWalkingManagerStatus
    //{
    //    PlayerIdle,
    //    PlayerWalking,
    //    PlayerJustWalked,
    //    PlayerPredictedWalking,
    //    PlayerMoving
    //}

    ///// <summary>
    /////     Enumeration of possible knee statuses.
    ///// </summary>
    //public enum PlayerKneeStatus
    //{
    //    Still,
    //    Raising,
    //    Falling
    //}

    // Summary:
    //     Specifies the state of tracking a body or body's attribute.
    public enum FrameClippedEdges
    {
        //
        // Summary:
        //     No frame edges.
        None = 0,
        //
        // Summary:
        //     Right frame edge.
        Right = 1,
        //
        // Summary:
        //     Left frame edge.
        Left = 2,
        //
        // Summary:
        //     Top frame edge.
        Top = 4,
        //
        // Summary:
        //     Bottom frame edge.
        Bottom = 8
    }
}