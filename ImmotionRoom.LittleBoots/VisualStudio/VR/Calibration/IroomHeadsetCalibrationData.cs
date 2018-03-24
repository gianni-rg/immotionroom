namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Calibration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Calibration data of calibration performed between ImmotionRoom system and a generic headset
    /// </summary>
    public class IroomHeadsetCalibrationData
    {
        /// <summary>
        /// The ID of the body used to calibrate the system. This should represent the player of the game.
        /// </summary>
        public ulong UserBodyId { get; set; }

        /// <summary>
        /// Found rotation matrix for the ImmotionRoom system wrt the headset frame of reference.
        /// (Multiply this matrix by ImmotionRoom system data (with z axis flipped) to obtain the same data in HMD frame of reference)
        /// </summary>
        public Matrix4x4 CalibrationRotationMatrix { get; set; }

        /// <summary>
        /// Found translation matrix for the ImmotionRoom system wrt the headset frame of reference
        /// (Multiply this matrix by ImmotionRoom system data (with z axis flipped) to obtain the same data in HMD frame of reference)
        /// </summary>
        public Matrix4x4 CalibrationTranslationMatrix { get; set; }

        /// <summary>
        /// Found calibration matrix for the ImmotionRoom system wrt the headset frame of reference
        /// (Multiply this matrix by ImmotionRoom system data (with z axis flipped) to obtain the same data in HMD frame of reference)
        /// </summary>
        public Matrix4x4 CalibrationMatrix { get; set; }

        /// <summary>
        /// The height of user found during the calibration step
        /// </summary>
        public float UserHeight { get; set; }
    }
}
