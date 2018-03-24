using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Uma
{
    /// <summary>
    /// Enumeration of main joints on UMA Avatar body
    /// </summary>
    public enum UmaJointTypes
    {
        //Globals
        Root,
        Position,

        //main spine
        Hips,
        LowerBack,
        Spine,
        SpineUp,

        //head & co
        Neck,
        Head,
        LeftEye,
        RightEye,

        //Upper right part
        RightShoulder,
        RightArm,
        RightForeArm,
        RightForeArmTwist,
        RightHand,
        RightHandLittle,
        RightHandLittle_1,
        RightHandLittle_2,
        RightHandRing,
        RightHandRing_1,
        RightHandRing_2,
        RightHandMiddle,
        RightHandMiddle_1,
        RightHandMiddle_2,
        RightHandIndex,
        RightHandIndex_1,
        RightHandIndex_2,
        RightHandThumb,
        RightHandThumb_1,
        RightHandThumb_2,

        //Upper left part
        LeftShoulder,
        LeftArm,
        LeftForeArm,
        LeftForeArmTwist,
        LeftHand,
        LeftHandLittle,
        LeftHandLittle_1,
        LeftHandLittle_2,
        LeftHandRing,
        LeftHandRing_1,
        LeftHandRing_2,
        LeftHandMiddle,
        LeftHandMiddle_1,
        LeftHandMiddle_2,
        LeftHandIndex,
        LeftHandIndex_1,
        LeftHandIndex_2,
        LeftHandThumb,
        LeftHandThumb_1,
        LeftHandThumb_2,

        //Lower left part
        LeftUpLeg,
        LeftLeg,
        LeftFoot,
        LeftToeBase,

        //Lower right part
        RightUpLeg,
        RightLeg,
        RightFoot,
        RightToeBase
    }
}
