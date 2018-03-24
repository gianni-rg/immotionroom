namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Humanoid.Amanda
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;

    /// <summary>
    /// Manages Amanda Avatars that follow users movements
    /// </summary>
    public class BodiesAmandaManager : BodiesAvateeringManager
    {
        #region Unity public properties

        /// <summary>
        /// Reference to Amanda prefab
        /// </summary>
        [Tooltip("Reference to Amanda prefab")]
        public GameObject AmandaModel;

        /// <summary>
        /// True if colliders have to attached to amanda hands and feet; false otherwise
        /// </summary>
        [Tooltip("True if colliders have to attached to amanda hands and feet; false otherwise")]
        public bool AttachColliders;

        /// <summary>
        /// Reference to Amanda Android Material
        /// </summary>
        [Tooltip("Reference to Amanda material to be used on Android (PC one is too heavy). Leave null if substitution is not required")]
        public Material AndroidAmandaMaterial;

        #endregion

        #region BodiesAvateeringManager members

        /// <summary>
        /// Adds a body avateerer for a body of interest to the provided game object
        /// </summary>
        /// <param name="avatarGo">Avatar Game Object the avatareer has to be attached to</param>
        /// <param name="bodyId">Unique Body ID</param>
        protected override void AddAvateerer(GameObject avatarGo, ulong bodyId)
        {
            //create a new amanda avatar to follow the body and attach it to the provided gameobject
            avatarGo.SetActive(false); //to launch awake after properties initialization, we freeze the object
            AmandaBodyAvatarer amandaAvatarer = avatarGo.AddComponent<AmandaBodyAvatarer>();
            amandaAvatarer.BodyId = bodyId;
            amandaAvatarer.AmandaModel = this.AmandaModel;
            amandaAvatarer.TrackPosition = this.TrackPosition;
            amandaAvatarer.ShadowsEnabled = this.ShadowsEnabled;
            amandaAvatarer.AttachColliders = this.AttachColliders;
            amandaAvatarer.AndroidAmandaMaterial = this.AndroidAmandaMaterial;
            avatarGo.SetActive(true); //unfreeze the object

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Bodies Amanda Manager - Added new Amanda avatar for body with ID {0}", bodyId);
            }
            
        }

        #endregion
    }
}
