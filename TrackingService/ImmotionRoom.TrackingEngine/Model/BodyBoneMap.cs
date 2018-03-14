namespace ImmotionAR.ImmotionRoom.TrackingEngine.Model
{
    using System.Collections.Generic;

    internal static class BodyConstants
    {
        /// <summary>
        ///     List of body joints that usually are stabler during tracking
        /// </summary>
        internal static readonly BodyJointTypes[] StableJoints =
        {
            BodyJointTypes.ShoulderLeft,
            BodyJointTypes.ShoulderRight,
            BodyJointTypes.SpineBase,
            BodyJointTypes.SpineMid,
            BodyJointTypes.SpineShoulder,
            BodyJointTypes.Neck
        };

        /// <summary>
        ///     The inversion joints map: it maps each joint to its counterpart when the body is inverted (i.e. left-to-right)
        /// </summary>
        internal static readonly IDictionary<BodyJointTypes, BodyJointTypes> InversionJointsMap = new Dictionary<BodyJointTypes, BodyJointTypes>(BodyJointTypesComparer.Instance)
        {
            {BodyJointTypes.FootLeft, BodyJointTypes.FootRight},
            {BodyJointTypes.AnkleLeft, BodyJointTypes.AnkleRight},
            {BodyJointTypes.KneeLeft, BodyJointTypes.KneeRight},
            {BodyJointTypes.HipLeft, BodyJointTypes.HipRight},
            {BodyJointTypes.FootRight, BodyJointTypes.FootLeft},
            {BodyJointTypes.AnkleRight, BodyJointTypes.AnkleLeft},
            {BodyJointTypes.KneeRight, BodyJointTypes.KneeLeft},
            {BodyJointTypes.HipRight, BodyJointTypes.HipLeft},
            {BodyJointTypes.HandTipLeft, BodyJointTypes.HandTipRight},
            {BodyJointTypes.ThumbLeft, BodyJointTypes.ThumbRight},
            {BodyJointTypes.HandLeft, BodyJointTypes.HandRight},
            {BodyJointTypes.WristLeft, BodyJointTypes.WristRight},
            {BodyJointTypes.ElbowLeft, BodyJointTypes.ElbowRight},
            {BodyJointTypes.ShoulderLeft, BodyJointTypes.ShoulderRight},
            {BodyJointTypes.HandTipRight, BodyJointTypes.HandTipLeft},
            {BodyJointTypes.ThumbRight, BodyJointTypes.ThumbLeft},
            {BodyJointTypes.HandRight, BodyJointTypes.HandLeft},
            {BodyJointTypes.WristRight, BodyJointTypes.WristLeft},
            {BodyJointTypes.ElbowRight, BodyJointTypes.ElbowLeft},
            {BodyJointTypes.ShoulderRight, BodyJointTypes.ShoulderLeft},
            {BodyJointTypes.SpineBase, BodyJointTypes.SpineBase},
            {BodyJointTypes.SpineMid, BodyJointTypes.SpineMid},
            {BodyJointTypes.SpineShoulder, BodyJointTypes.SpineShoulder},
            {BodyJointTypes.Neck, BodyJointTypes.Neck},
            {BodyJointTypes.Head, BodyJointTypes.Head}
        };

        ///// <summary>
        /////     Map of body: maps each joint to its father
        ///// </summary>
        ///// 
        //internal static readonly IDictionary<BodyJointTypes, BodyJointTypes> BoneMap = new Dictionary<BodyJointTypes, BodyJointTypes>(BodyJointTypesComparer.Instance)
        //{
        //    {BodyJointTypes.FootLeft, BodyJointTypes.AnkleLeft},
        //    {BodyJointTypes.AnkleLeft, BodyJointTypes.KneeLeft},
        //    {BodyJointTypes.KneeLeft, BodyJointTypes.HipLeft},
        //    {BodyJointTypes.HipLeft, BodyJointTypes.SpineBase},
        //    {BodyJointTypes.FootRight, BodyJointTypes.AnkleRight},
        //    {BodyJointTypes.AnkleRight, BodyJointTypes.KneeRight},
        //    {BodyJointTypes.KneeRight, BodyJointTypes.HipRight},
        //    {BodyJointTypes.HipRight, BodyJointTypes.SpineBase},
        //    {BodyJointTypes.HandTipLeft, BodyJointTypes.HandLeft},
        //    {BodyJointTypes.ThumbLeft, BodyJointTypes.HandLeft},
        //    {BodyJointTypes.HandLeft, BodyJointTypes.WristLeft},
        //    {BodyJointTypes.WristLeft, BodyJointTypes.ElbowLeft},
        //    {BodyJointTypes.ElbowLeft, BodyJointTypes.ShoulderLeft},
        //    {BodyJointTypes.ShoulderLeft, BodyJointTypes.SpineShoulder},
        //    {BodyJointTypes.HandTipRight, BodyJointTypes.HandRight},
        //    {BodyJointTypes.ThumbRight, BodyJointTypes.HandRight},
        //    {BodyJointTypes.HandRight, BodyJointTypes.WristRight},
        //    {BodyJointTypes.WristRight, BodyJointTypes.ElbowRight},
        //    {BodyJointTypes.ElbowRight, BodyJointTypes.ShoulderRight},
        //    {BodyJointTypes.ShoulderRight, BodyJointTypes.SpineShoulder},
        //    {BodyJointTypes.SpineBase, BodyJointTypes.SpineMid},
        //    {BodyJointTypes.SpineMid, BodyJointTypes.SpineShoulder},
        //    {BodyJointTypes.SpineShoulder, BodyJointTypes.Neck},
        //    {BodyJointTypes.Neck, BodyJointTypes.Head},
        //};
    }
}