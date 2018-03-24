namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.PlayerController
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.Calibration;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataSourcesManagement;
    using ImmotionAR.ImmotionRoom.LittleBoots.Avateering;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.HeadsetManagement;
    using ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Tools;
    using Girello;

    /// <summary>
    /// The player controller of the game that uses the ImmotionRoom system
    /// </summary>
    [RequireComponent(typeof(IroomPlayerCalibrator))]
    public partial class IroomPlayerController : MonoBehaviour
    {
        #region Unity public properties

        /// <summary>
        /// True if the system has to detect the movement of the user inside the room, false otherwise
        /// </summary>
        [Tooltip("True if the system has to detect the movement of the user inside the room, false otherwise")]
        public bool TranslationDetection = true;

        /// <summary>
        /// True if the system has to use the walk-in-place locomotion system, false otherwise
        /// </summary>
        [Tooltip("True if the system has to use the walk-in-place locomotion system, false otherwise")]
        public bool WalkingDetection = true;

        /// <summary>
        /// False if the system has to guarantee a pure mapping between the real and the virtual world, making the player
        /// to ignore the physics and the collisions during his movements. True to have FPS videogame behaviour
        /// </summary>
        [Tooltip("False if the system has to guarantee a pure mapping between the real and the virtual world, making the player to ignore the physics and the collisions during his movements. True to have FPS videogame behaviour")]
        public bool FpsBehaviour = true;

        /// <summary>
        /// Flag used only if FpsBehaviour is false.
        /// If this flags is true, room scale is so pure that no kind of collision is detected. If it is false, the controller is guaranteed to stay attached
        /// to the floor
        /// </summary>
        [Tooltip("If this flags is true, room scale is so pure that no kind of collision is detected. If it is false, the controller is guaranteed to stay attached to the floor")]
        public bool RoomScaleIsPure = true;

        /// <summary>
        /// Set to true if you want the system to init avatar position with foot on the floor of the virtual world; false otherwise.
        /// This flag does nothing if Gravity Influence is 0
        /// </summary>
        [Tooltip("Set to true if you want the system to init avatar position with foot on the floor of the virtual world; false otherwise. This flag does nothing if Gravity Influence is 0")]
        public bool InitWithFootOnFloor = true;

        /// <summary>
        /// Additional (unscaled) distance to add after foot on floor initialization, for fine tuning.
        /// </summary>
        [Tooltip("Additional (unscaled) distance to add after foot on floor initialization, for fine tuning")]
        public float InitWithFootOnFloorAdditionalFactor = +0.05f;

        /// <summary>
        /// How much the detected speed on the player affects the actual speed of the walking character
        /// </summary>
        [Tooltip("How much the detected speed on the player affects the actual speed of the walking character")]
        public float DetectedWalkingSpeedMultiplier = 3.0f;

        /// <summary>
        /// How much the gravity of the physics system has to influence the player.
        /// This value is used only if TrespassingDetection is set to true
        /// </summary>
        [Tooltip("How much the gravity of the physics system has to influence the player")]
        public float GravityMultiplier = 1.0f;

        /// <summary>
        /// Value to set the eyes cameras near plane to, in unscaled coordinates. This is useful to tune the things seen in the avatar
        /// </summary>
        [Tooltip("Value to set the eyes cameras near plane to, in unscaled coordinates")]
        public float CamerasNearPlane = 0.075f;

        /// <summary>
        /// Displacement, in eyes cameras coordinate system, from avatar eyes position to actual camera placement. This is useful to tune
        /// visualization of undesired elements of the avatar face
        /// </summary>
        [Tooltip("Displacement, in eyes cameras coordinate system, from avatar eyes position to actual camera placement. This is useful to tune visualization of undesired elements of the avatar face")]
        public Vector3 EyesCameraDisplacement = new Vector3(0, 0, -0.0225f);

        /// <summary>
        /// If true, input from keyboard, mouse and gamepad will be used to move the player. This is useful especially as debug controls.
        /// At the moment the only supported control is keyboard
        /// </summary>
        [Tooltip("If true, input from keyboard, mouse and gamepad will be used to move the player. This is useful especially as debug controls. At the moment the only supported control is keyboard")]
        public bool AllowDebugControls = true;

        /// <summary>
        /// Speed of player when moved using debug controls, in unit/s
        /// </summary>
        [Tooltip("Speed of player when moved using debug controls, in unit/s")]
        public float DebugControlsSpeed = 1.0f;

        #endregion

        #region Private Fields

        /// <summary>
        /// Internal implementation of the class
        /// </summary>
        private IroomPlayerControllerInternal m_internalImplementation;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets if the player is ready for VR or if it is still performing initialization stuff
        /// </summary>
        public bool IsVrReady
        {
            get
            {
                return m_internalImplementation.IsVrReady;
            }
        }

        /// <summary>
        /// Gets the ImmotionRoom/headset calibration data used by this player, or null if calibration is not finished yet
        /// </summary>
        public IroomHeadsetCalibrationData CalibrationData
        {
            get
            {
                return m_internalImplementation.CalibrationData;
            }
        }

        /// <summary>
        /// Gets the last user body read from the ImmotionRoom system, in tracking service frame of reference.
        /// If tracking service is not ready or tracking has been lost, returns null
        /// </summary>
        public TrackingServiceBodyData LastTrackedBody
        {
            get
            {
                return m_internalImplementation.LastTrackedBody;
            }
        }

        /// <summary>
        /// Returns main avateering element of this player. Through this object, all stuff about avatar (like joints positions) can be retrieved
        /// If the system is not ready, the return value is null
        /// </summary>
        public BodyAvatarer MainAvatar
        {
            get
            {
                return m_internalImplementation.MainAvatar;
            }
        }

        /// <summary>
        /// Returns the character controller representing the player
        /// </summary>
        public CharacterController CharController
        {
            get
            {
                return m_internalImplementation.CharController;
            }
        }

        /// <summary>
        /// Returns the headset manager used by the player
        /// </summary>
        public HeadsetManager HmdManager
        {
            get
            {
                return m_internalImplementation.HmdManager;
            }
        }

        /// <summary>
        /// Return data of current player girello
        /// </summary>
        public GirelloData GirelloData
        {
            get
            {
                return m_internalImplementation.GirelloData;
            }
        }

        #endregion

        #region Behaviour methods

        void Awake()
        {
            m_internalImplementation = new IroomPlayerControllerInternal(this);
            m_internalImplementation.Awake();
        }

        void Start()
        {
            m_internalImplementation.Start();
        }

        void OnDestroy()
        {
            m_internalImplementation.OnDestroy();
        }

        void OnLevelWasLoaded(int level)
        {
            m_internalImplementation.OnLevelWasLoaded(level);
        }

        void Update()
        {
            m_internalImplementation.Update();
        }

        void FixedUpdate()
        {
            m_internalImplementation.FixedUpdate();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Resets the player, making it to restart from initial calibration
        /// </summary>
        public void Reset()
        {
            m_internalImplementation.Reset();
        }

        #endregion
    }
}
