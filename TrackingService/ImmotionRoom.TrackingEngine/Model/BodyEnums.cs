namespace ImmotionAR.ImmotionRoom.TrackingEngine.Model
{
    // Summary:
    //     The types of joints of a Body.
    public enum BodyJointTypes
    {
        // Summary:
        //     Base of the spine.
        SpineBase = 0,
        //
        // Summary:
        //     Middle of the spine.
        SpineMid = 1,
        //
        // Summary:
        //     Neck.
        Neck = 2,
        //
        // Summary:
        //     Head.
        Head = 3,
        //
        // Summary:
        //     Left shoulder.
        ShoulderLeft = 4,
        //
        // Summary:
        //     Left elbow.
        ElbowLeft = 5,
        //
        // Summary:
        //     Left wrist.
        WristLeft = 6,
        //
        // Summary:
        //     Left hand.
        HandLeft = 7,
        //
        // Summary:
        //     Right shoulder.
        ShoulderRight = 8,
        //
        // Summary:
        //     Right elbow.
        ElbowRight = 9,
        //
        // Summary:
        //     Right wrist.
        WristRight = 10,
        //
        // Summary:
        //     Right hand.
        HandRight = 11,
        //
        // Summary:
        //     Left hip.
        HipLeft = 12,
        //
        // Summary:
        //     Left knee.
        KneeLeft = 13,
        //
        // Summary:
        //     Left ankle.
        AnkleLeft = 14,
        //
        // Summary:
        //     Left foot.
        FootLeft = 15,
        //
        // Summary:
        //     Right hip.
        HipRight = 16,
        //
        // Summary:
        //     Right knee.
        KneeRight = 17,
        //
        // Summary:
        //     Right ankle.
        AnkleRight = 18,
        //
        // Summary:
        //     Right foot.
        FootRight = 19,
        //
        // Summary:
        //     Between the shoulders on the spine.
        SpineShoulder = 20,
        //
        // Summary:
        //     Tip of the left hand.
        HandTipLeft = 21,
        //
        // Summary:
        //     Left thumb.
        ThumbLeft = 22,
        //
        // Summary:
        //     Tip of the right hand.
        HandTipRight = 23,
        //
        // Summary:
        //     Right thumb.
        ThumbRight = 24,
    }

    //// Summary:
    ////     Specifies the state of tracking a body or body's attribute.
    //public enum BodyTrackingState
    //{
    //    // Summary:
    //    //     The joint data is not tracked and no data is known about this joint.
    //    NotTracked = 0,
    //    //
    //    // Summary:
    //    //     The joint data is inferred and confidence in the position data is lower than
    //    //     if it were Tracked.
    //    Inferred = 1,
    //    //
    //    // Summary:
    //    //     The joint data is being tracked and the data can be trusted.
    //    Tracked = 2,
    //}


    // Summary:
    //     The types of supported Body Gestures
    public enum BodyGestureTypes
    {
        Walking = 0
    }

    //
    // Summary:
    //     The state of a hand of a body.
    public enum BodyHandState
    {
        //
        // Summary:
        //     Undetermined hand state.
        Unknown = 0,
        //
        // Summary:
        //     Hand not tracked.
        NotTracked = 1,
        //
        // Summary:
        //     Open hand.
        Open = 2,
        //
        // Summary:
        //     Closed hand.
        Closed = 3,
        //
        // Summary:
        //     Lasso (pointer) hand.
        Lasso = 4
    }
}
