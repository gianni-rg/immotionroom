namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Uma
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using ImmotionAR.ImmotionRoom.LittleBoots.Avateering;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Uma.Generators;

    /// <summary>
    /// Manages UMA Avatars that follow users movements
    /// </summary>
    public class BodiesUmaManager : BodiesAvateeringManager
    {
        #region Unity public properties

        /// <summary>
        /// Reference to the generator responsible to create the UMA-compatible avatar
        /// </summary>
        [Tooltip("Reference to the element responsible for Creation of the avatars")]
        public UmaBodyGenerator AvatarGenerator;

        /// <summary>
        /// True if colliders have to be attached to avatar hands and feet; false otherwise
        /// </summary>
        [Tooltip("True if colliders have to be attached to amanda hands and feet; false otherwise")]
        public bool AttachColliders;

        /// <summary>
        /// True to make the system try to make the body of the avatar the most similar possible to the user's body.
        /// False to use avatar prefab as is. Can be used only on UMA avatars (not UMA-like models)
        /// </summary>
        [Tooltip("True to make the system try to make the body of the avatar the most similar possible to the user's body. False to use avatar prefab as is. Can be used only on UMA avatars (not UMA-like models)")]
        public bool CalibratePhysiognomy;

        /// <summary>
        /// Make the avatar skinned mesh to be flagged with the flag UpdateWhenOffscreen, that makes the mesh rendered always, even when not seen from a camera.
        /// The advice is to let this flag on if avatar is used in VR, because of some bugs in UMA bounding box estimates
        /// </summary>
        [Tooltip("Make the avatar skinned mesh to be flagged with the flag UpdateWhenOffscreen, that makes the mesh rendered always, even when not seen from a camera. The advice is to let this flag on if avatar is used in VR, because of some bugs in UMA bounding box estimates")]
        public bool IgnoreBoundsCheck;

        /// <summary>
        /// Make the avatar hands to stay fixed at a zero orientation pose. This is useful to prevent all detection glitches on avatar hands
        /// </summary>
        [Tooltip("Make the avatar hands to stay fixed at a zero orientation pose. This is useful to prevent all detection glitches on avatar feet. Use only if tracking glitches make interaction unusable otherwise")]
        public bool LockHandsPose;

        /// <summary>
        /// Make the avatar feet to stay fixed at a zero orientation pose. This is useful to prevent all detection glitches on avatar feet
        /// </summary>
        [Tooltip("Make the avatar feet to stay fixed at a zero orientation pose. This is useful to prevent all detection glitches on avatar feet. Use only if tracking glitches make interaction unusable otherwise")]
        public bool LockFeetPose;

        #endregion

        #region BodiesAvateeringManager members

        /// <summary>
        /// Adds a body avateerer for a body of interest to the provided game object
        /// </summary>
        /// <param name="avatarGo">Avatar Game Object the avatareer has to be attached to</param>
        /// <param name="bodyId">Unique Body ID</param>
        protected override void AddAvateerer(GameObject avatarGo, ulong bodyId)
        {
            //create a new UMA avatar to follow the body and attach it to the provided gameobject
            avatarGo.SetActive(false); //to launch awake after properties initialization, we freeze the object
            UmaBodyAvatarer umaAvatarer = avatarGo.AddComponent<UmaBodyAvatarer>();
            umaAvatarer.BodyId = bodyId;
            umaAvatarer.AvatarGenerator = this.AvatarGenerator;
            umaAvatarer.TrackPosition = this.TrackPosition;
            umaAvatarer.ShadowsEnabled = this.ShadowsEnabled;
            umaAvatarer.AttachColliders = this.AttachColliders;
            umaAvatarer.CalibratePhysiognomy = this.CalibratePhysiognomy;
            umaAvatarer.IgnoreBoundsCheck = this.IgnoreBoundsCheck;
            umaAvatarer.LockHandsPose = this.LockHandsPose;
            umaAvatarer.LockFeetPose = this.LockFeetPose;

            avatarGo.SetActive(true); //unfreeze the object

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Bodies Uma Manager - Added new UMA avatar for body with ID {0}", bodyId);
            }

        }

        #endregion
    }
}
