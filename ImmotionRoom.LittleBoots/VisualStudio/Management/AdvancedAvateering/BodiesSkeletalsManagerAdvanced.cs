namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedAvateering
{
    using ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Skeletals;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedManager;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Manages Skeletal Avatars that follow users movements.
    /// This skeletal Avatars manager can work for avatars in tracking, diagnostic and calibration modes
    /// </summary>
    public class BodiesSkeletalsManagerAdvanced : BodiesSkeletalsManager
    {
        #region Public Unity Properties

        /// <summary>
        /// ID of the stream whose skeletons are to be shown
        /// </summary>
        [Tooltip("ID of the data stream whose skeletons are to be shown")]
        public string SceneStreamerInfoId;

        /// <summary>
        /// Streaming mode of the skeletons that are to be shown (e.g. world transform skeletons vs master transform skeletons)
        /// </summary>
        [Tooltip("Which kind of scene data stream modes are to be asked from the tracking service (e.g. world transform skeletons vs master transform skeletons)")]
        public TrackingServiceSceneDataStreamModes SceneStreamingMode;

        /// <summary>
        /// GameObjects to activate or deactivate when one of the skeletons reaches the kinect tracking area limits.
        /// When kinect communicate that one of the skeletons is near the tracking area borders, the object corresponding to the 
        /// trespassing side gets activated. When everything is fine, the object gets deactivated.
        /// Gameobject order is left, top, right, bottom.
        /// Can be null
        /// </summary>
        [Tooltip("GameObjects to activate or deactivate when one of the skeletons reaches the kinect tracking area limits. Gameobject order is left, top, right, bottom.")]
        public GameObject[] RedAlerts;

        #endregion

        #region Behaviour methods

        protected override void Update()
        {
            //re-set to false all the red-alert objects (the single skeletons will put it to false when they'll cross the borders)
            if(RedAlerts != null)
                foreach (GameObject redAlert in RedAlerts)
                    redAlert.SetActive(false);

            base.Update();
        }

        #endregion

        #region BodiesAvateeringManager Methods

        /// <summary>
        /// Adds a body avateerer for a body of interest to the provided game object
        /// </summary>
        /// <param name="avatarGo">Avatar Game Object the avatareer has to be attached to</param>
        /// <param name="bodyId">Unique Body ID</param>
        protected override void AddAvateerer(GameObject avatarGo, ulong bodyId)
        {
            //create a new skeletal avatar to follow the body and attach it to the provided gameobject
            avatarGo.SetActive(false); //to launch awake after properties initialization, we freeze the object
            SkeletalBodyAvatererAdvanced skeletalAvatarer = avatarGo.AddComponent<SkeletalBodyAvatererAdvanced>();
            skeletalAvatarer.BodyId = bodyId;
            skeletalAvatarer.SceneStreamerInfoId = SceneStreamerInfoId;
            skeletalAvatarer.SceneStreamingMode = SceneStreamingMode;
            skeletalAvatarer.TrackPosition = this.TrackPosition;
            skeletalAvatarer.ShadowsEnabled = ShadowsEnabled;
            skeletalAvatarer.JointsMaterial = JointsMaterial;
            skeletalAvatarer.LimbsMaterial = LimbsMaterial;
            skeletalAvatarer.LimbsColor = LimbsColor;
            skeletalAvatarer.JointSphereRadius = JointSphereRadius;
            skeletalAvatarer.ConnectingLinesThickness = ConnectingLinesThickness;
            skeletalAvatarer.AddColliders = AddColliders;

            //assign an appropriate color to the new skeleton, depending on user choice
            switch (SkeletalDrawingMode)
            {
                //user provided values
                case SkeletalsDrawingMode.Standard:
                    skeletalAvatarer.PositiveColor = PositiveColors[0];
                    skeletalAvatarer.NegativeColor = NegativeColors[0];
                    break;

                //green-red
                case SkeletalsDrawingMode.FixedColors:
                    skeletalAvatarer.PositiveColor = PositiveColor;
                    skeletalAvatarer.NegativeColor = NegativeColor;
                    break;

                //random color pair
                case SkeletalsDrawingMode.RandomPresetsColor:
                    {
                        int randIdx = UnityEngine.Random.Range(0, PositiveColors.Length);
                        skeletalAvatarer.PositiveColor = PositiveColors[randIdx];
                        skeletalAvatarer.NegativeColor = NegativeColors[randIdx];
                    }
                    break;

                //random colors inside each set
                case SkeletalsDrawingMode.RandomColor:
                    {
                        int randIdx = UnityEngine.Random.Range(0, PositiveColors.Length);
                        skeletalAvatarer.PositiveColor = PositiveColors[randIdx];
                        randIdx = UnityEngine.Random.Range(0, PositiveColors.Length);
                        skeletalAvatarer.NegativeColor = NegativeColors[randIdx];
                    }
                    break;

                default:
                    throw new Exception("WTF?");
            }

            skeletalAvatarer.RedAlerts = RedAlerts;

            avatarGo.SetActive(true); //unfreeze the object

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Bodies Skeletals Manager Advanced - Added new Skeletal avatar for body with ID {0}", bodyId);
            }
        }

        /// <summary>
        /// Connect to tracking service and initialize the scene data provider
        /// </summary>
        /// <returns></returns>
        protected override IEnumerator TrackingServiceConnect()
        {
            //TODO: SAREBBE MEGLIO SE TUTTO CIO' FUNZIONASSE AD EVENTI INVECE CHE CON IL POLLING... E' DA CORREGGERE OVUNQUE
            //wait for tracking service connection and tracking
            while (!TrackingServiceManagerAdvanced.Instance.IsStreamingSkeletons)
                yield return new WaitForSeconds(0.1f);

            //create the body provider, waiting for it to begin
            while ((m_sceneData = TrackingServiceManagerAdvanced.Instance.StartSceneDataProvider(SceneStreamerInfoId, SceneStreamingMode)) == null)
                yield return new WaitForSeconds(0.1f);

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Bodies Avateering Manager Advanced - Connected to Tracking Service and Initialized");
            }

            yield break;
        }
        #endregion
    }
}
