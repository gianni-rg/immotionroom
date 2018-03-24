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
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.Watermarking;
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.Calibration.DataManagement;
    using Girello;

    /// <summary>
    /// The player controller of the game that uses the ImmotionRoom system
    /// </summary>
    [RequireComponent(typeof(IroomPlayerCalibrator))]
    public partial class IroomPlayerController : MonoBehaviour
    {
        /// <summary>
        /// Contains the actual implementation of the IroomPlayerController, for obfuscation purposes
        /// </summary>
        private class IroomPlayerControllerInternal
        {
            #region Constants definition

            /// <summary>
            /// Coherence threshold between the detected walking direction and the direction the user is facing with the HMD, in degrees of angle.
            /// If the two directions are too different, the detected walking speed ges reversed
            /// </summary>
            private const float OrientationCheckThreshold = 120;

            /// <summary>
            /// Ratio between character controller height and relative ch. controller step offset
            /// </summary>
            private const float CharacterControllerStepOffsetHeightFactor = 0.16666f;

            #endregion

            #region Private fields

            /// <summary>
            /// Object responsible for player calibration
            /// </summary>
            private IroomPlayerCalibrator m_calibrator;

            /// <summary>
            /// Provider of tracking data
            /// </summary>
            private SceneDataProvider m_sceneDataProvider;

            /// <summary>
            /// Provider of body data of current detected player
            /// </summary>
            private BodyDataProvider m_playerBodyDataProvider;

            /// <summary>
            /// Transform of the child gameobject representing the frame of reference of ImmotionRoom system
            /// </summary>
            private Transform m_kinectFrameOfReference;

            /// <summary>
            /// Transform of the child gameobject representing the frame of reference of the associated headset
            /// </summary>
            private Transform m_headsetFrameOfReference;

            /// <summary>
            /// Initial local position of headset frame of reference (so that it can be restored)
            /// </summary>
            private Vector3 m_headsetInitialPos;

            /// <summary>
            /// Reference of the character controller managed by this object
            /// </summary>
            private CharacterController m_characterController;

            /// <summary>
            /// Reference to the body avaterers managed by this object.
            /// First one in array will be considered as the master one (the others are mainly for debugging purposes)
            /// </summary>
            private BodyAvatarer[] m_bodyAvatarers;

            /// <summary>
            /// Manager of current headset
            /// </summary>
            private HeadsetManager m_headsetManager;

            /// <summary>
            /// Cameras of the headset object (may be two cameras, one for each eye; or one for both eyes; or three... depending from the headset)
            /// </summary>
            private Camera[] m_headsetCameras;

            /// <summary>
            /// For each member of the array m_headsetCameras specifies if this camera represents an eye of the player or not
            /// (some headsets add spurious cameras for the rendering)
            /// </summary>
            private bool[] m_headsetCamerasIsEye;

            /// <summary>
            /// Last position of the Root Joint of the avatar, as seen in last call to FixedUpdate.
            /// This value is used to align the character controller to the translation of the avatar, so to perform the feature of
            /// non-trespassing of the avatar collider with external obstacles during room-scale translation
            /// </summary>
            private Vector3 m_lastAvatarPosition;

            /// <summary>
            /// Last position of the Character controller, as seen in last call to FixedUpdate.
            /// This value is used to let external elements to move the player controller, leaving everything coherent
            /// </summary>
            private Vector3 m_lastCharacterControllerPosition;

            /// <summary>
            /// Speed of the character due to gravity influence at the last frame
            /// </summary>
            private Vector3 m_lastGravitySpeed;

            /// <summary>
            /// True if the system is ready and we're ready for VR, false otherwise (e.g. we're still calibrating)
            /// </summary>
            private bool m_systemReady;

            /// <summary>
            /// The TrackingServiceVirtualGirello object that contains this object
            /// </summary>
            private IroomPlayerController m_enclosingInstance;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="enclosingInstance">Enclosing instance, whose code has to be implemented</param>
            internal IroomPlayerControllerInternal(IroomPlayerController enclosingInstance)
            {
                m_enclosingInstance = enclosingInstance;
            }

            #endregion

            #region Internal Properties

            /// <summary>
            /// Gets if the player is ready for VR or if it is still performing initialization stuff
            /// </summary>
            internal bool IsVrReady
            {
                get
                {
                    return m_systemReady;
                }
            }

            /// <summary>
            /// Gets the ImmotionRoom/headset calibration data used by this player, or null if calibration is not finished yet
            /// </summary>
            internal IroomHeadsetCalibrationData CalibrationData
            {
                get
                {
                    if (m_systemReady)
                        return m_calibrator.CalibrationData;
                    else
                        return null;
                }
            }

            /// <summary>
            /// Gets the last user body read from the ImmotionRoom system, in tracking service frame of reference.
            /// If tracking service is not ready or tracking has been lost, returns null
            /// </summary>
            internal TrackingServiceBodyData LastTrackedBody
            {
                get
                {
                    if (m_systemReady && m_sceneDataProvider.IsStillValid)
                        return m_playerBodyDataProvider.LastBody;
                    else
                        return null;
                }
            }

            /// <summary>
            /// Returns main avateering element of this player. Through this object, all stuff about avatar (like joints positions) can be retrieved
            /// If the system is not ready, the return value is null
            /// </summary>
            internal BodyAvatarer MainAvatar
            {
                get
                {
                    if (m_systemReady)
                        return m_bodyAvatarers[0];
                    else
                        return null;
                }
            }

            /// <summary>
            /// Returns the character controller representing the player
            /// </summary>
            internal CharacterController CharController
            {
                get
                {
                    return m_characterController;
                }
            }

            /// <summary>
            /// Returns the headset manager used by the player
            /// </summary>
            internal HeadsetManager HmdManager
            {
                get
                {
                    return m_headsetManager;
                }
            }

            /// <summary>
            /// Return data of current player girello.
            /// If the system is not ready, the return value is null
            /// </summary>
            internal GirelloData GirelloData
            {
                get
                {
                    if (m_systemReady)
                    {
                        //get data about girello from tracking service
                        Vector3 originalCenter = TrackingServiceManagerBasic.Instance.TrackingServiceEnvironment.SceneDescriptor.GameArea.Center.ToVector3();
                        Vector3 originalSize = TrackingServiceManagerBasic.Instance.TrackingServiceEnvironment.SceneDescriptor.GameArea.Size.ToVector3();

                        //convert this data to Unity world coordinates using the transform of the avatar
                        return new GirelloData()
                        {
                            Center = m_enclosingInstance.transform.GetChild(0).TransformPoint(originalCenter),
                            Size = new Vector3(m_enclosingInstance.transform.GetChild(0).lossyScale.x * originalSize.x,
                                               m_enclosingInstance.transform.GetChild(0).lossyScale.y * originalSize.y,
                                               m_enclosingInstance.transform.GetChild(0).lossyScale.z * originalSize.z),
                            Rotation = m_enclosingInstance.transform.GetChild(0).rotation
                        };
                    }
                    else
                        return null;                    
                }
            }

            #endregion

            #region Behaviour methods

            internal void Awake()
            {
                //get references of most used objects
                m_calibrator = m_enclosingInstance.GetComponent<IroomPlayerCalibrator>();
                m_kinectFrameOfReference = m_enclosingInstance.transform.Find("IRoom system frame of reference");
                m_headsetFrameOfReference = m_enclosingInstance.transform.Find("Headset frame of reference");
                m_characterController = m_enclosingInstance.transform.GetComponentInChildren<CharacterController>();
                m_bodyAvatarers = m_enclosingInstance.transform.GetComponentsInChildren<BodyAvatarer>();
                m_headsetManager = m_enclosingInstance.GetComponent<HeadsetManager>();
                m_headsetCameras = m_headsetFrameOfReference.GetComponentsInChildren<Camera>(true);
                m_headsetCamerasIsEye = new bool[m_headsetCameras.Length];

                //non-eye cameras are thing for rendering stuff. Usually they have children cameras that actually represent the eye
                //so, if a camera has child cameras, just ignore it.
                for (int i = 0; i < m_headsetCameras.Length; i++)
                    if (m_headsetCameras[i].transform.GetComponentsInChildren<Camera>(true).Length == 1) //only the parent one
                        m_headsetCamerasIsEye[i] = true;
                    else
                        m_headsetCamerasIsEye[i] = false;

                //save initial position of headset frame of reference
                m_headsetInitialPos = m_headsetFrameOfReference.localPosition;

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("ImmotionRoom player controller - awaken");
                }
            }

            internal void Start()
            {
                //Launch controller initialization
                m_enclosingInstance.StartCoroutine(Initialization());

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("ImmotionRoom player controller - started");
                }
            }

            internal void OnDestroy()
            {
                m_enclosingInstance.StopAllCoroutines();

                if (m_sceneDataProvider != null && m_sceneDataProvider.IsStillValid)
                    m_sceneDataProvider.Dispose();
            }

            internal void OnLevelWasLoaded(int level)
            {
#if !VR_WATERMARK_FREE
                //add watermark object. This is useful if the same player controller goes through different scenes (e.g. thanks to DoNotDestroyOnLoad)
                //to ensure that a watermark survives
                GameWatermark.CreateInstance();
#endif
            }

            internal void Update()
            {
                //if the system is calibrated and ready
                if (m_systemReady)
                {
                    //if we have tracking issues or the body we're tracking does not exist anymore, reset system and exit
                    if (!m_sceneDataProvider.IsStillValid || m_playerBodyDataProvider.LastBody == null)
                    {
                        if(Log.IsDebugEnabled)
                        {
                            Log.Debug("IroomPlayerController - User body lost");
                        }

                        Reset();
                        return;
                    }

                    //else, if everything is up and running

                    //inject hmd orientation to the avatars
                    foreach (BodyAvatarer avatar in m_bodyAvatarers)
                    {
                        avatar.InjectedJointPoses[TrackingService.DataClient.Model.TrackingServiceBodyJointTypes.Neck] = m_headsetManager.OrientationInGame;
                    }

                    //set the headset eyes where the avatar eyes are. First compute mean position of the eyes cameras, then put the mean position of the cameras
                    //at the mean position of the avatar eyes (moving it by an offset provided by the user, for fine tunings)
                    //(remember to consider only eye cameras and not supporting cameras, too)
                    Vector3 meanCameraPos = Vector3.zero;
                    int eyeCamerasNum = 0;

                    for (int i = 0; i < m_headsetCameras.Length; i++ )
                        if (m_headsetCamerasIsEye[i])
                        {
                            meanCameraPos += m_headsetCameras[i].transform.position;
                            eyeCamerasNum++;
                        }

                    meanCameraPos /= eyeCamerasNum;

                    m_headsetFrameOfReference.position += m_bodyAvatarers[0].BodyEyesCameraPosition - meanCameraPos + m_headsetCameras[0].transform.TransformVector(m_enclosingInstance.EyesCameraDisplacement); //we can use m_headsetCameras[0] because surely both eyes will have the same rotation and scaling. And usually non-eye, supporting cameras have same rotation-scale of the eye cameras
                }

            }

            internal void FixedUpdate()
            {
                //if we are not calibrated or have not a valid body, just do nothing
                if (!m_systemReady || !m_sceneDataProvider.IsStillValid || m_playerBodyDataProvider.LastBody == null)
                    return;
                
                //if character controller has moved since last call, it means that someone from the outside is moving the player.
                //So, move avatar to make everything coherent
                Vector3 chControllerExternalMov = m_characterController.transform.position - m_lastCharacterControllerPosition;
                m_kinectFrameOfReference.position += chControllerExternalMov;

                //calculate displacement of the avatar since last execution of fixed update on the XZ plane.
                //This is the translation of the avatar due to movement of the player inside the room (room-scale),
                //that the body avatarer directly applies to the avatar (but we have not applied to the player controller yet).
                //We're forcing the character controller to be centered at the body root joint of the avatar
                //(Remember that with last line we moved the kinect frame of reference, so we had to remove this offset from current avatar position)
                Vector3 avatarDisp = m_bodyAvatarers[0].BodyRootJoint.position - chControllerExternalMov - m_lastAvatarPosition;

                //if translation detection was not required, move the kinect frame of reference to compensate for avatar movement.
                //This will make the avatar to appear fixed, while the girello will be coherent with new user position in the real world
                if (!m_enclosingInstance.TranslationDetection)
                    m_kinectFrameOfReference.position -= avatarDisp;
                //else, if translation detection was requested
                else
                {
                    //IN FPS BEHAVIOUR, we ignore Y value: if player will jump in the real world, the collider will stay still. This is to avoid incoherence on physics: 
                    //if the player jumps, while he's jumping, he has gravity in the real world that makes him fall down, but in the meantime he also has
                    //Unity gravity that will make him to fall down at twice the speed... .
                    //For different reasons, we do the same if Room scale is pure (basically, otherwise player can't jump or crouch)
                    if (m_enclosingInstance.FpsBehaviour || !m_enclosingInstance.RoomScaleIsPure)
                        avatarDisp.y = 0;

                    //as we did before, try to move the player controller according to this translation
                    this.Move(avatarDisp, true);
                }

                //if player controller has FPS behaviour
                if (m_enclosingInstance.FpsBehaviour)
                {
                    //the requested movement for this frame is the sum of: the gravity influence, the input influence (mouse, keyboard, gamepad... usually for debuggin purposes),
                    //the walk-in-place movement
                    Vector3 movementForThisFrameGravity = GetGravityMovementAmount();
                    Vector3 movementForThisFrameWalk = GetInputMovementAmount() +
                                                       GetWalkingMovementAmount();

                    //move the player according to the calculated movement amount for this iteration
                    RaycastHit hitInfo;
                    Physics.SphereCast(m_characterController.transform.position, m_characterController.radius, Vector3.down, out hitInfo,
                                       m_characterController.height * m_enclosingInstance.transform.lossyScale.y / 2f);
                    Vector3 actualMovementForThisFrameWalk = Vector3.ProjectOnPlane(movementForThisFrameWalk, hitInfo.normal);

                    this.Move(actualMovementForThisFrameWalk + movementForThisFrameGravity);
                }
                //else, if it has room-scale behaviour
                else
                {
                    //the requested movement for this frame is the sum of: the input influence (mouse, keyboard, gamepad... usually for debugging purposes),
                    //the walk-in-place movement
                    Vector3 movementForThisFrameWalk = GetInputMovementAmount() +
                                                       GetWalkingMovementAmount();

                    //if the user has requested that the controller stays attached to the floor
                    if (!m_enclosingInstance.RoomScaleIsPure)
                    {
                        //find the floor below the controller
                        RaycastHit hitInfo;
                        Physics.SphereCast(m_characterController.transform.position, m_characterController.radius, Vector3.down, out hitInfo,
                                           m_characterController.height * m_enclosingInstance.transform.lossyScale.y / 2f + 0.1f);

                        //move the controller so that it stays on the floor
                        Vector3 floorStay = (hitInfo.point.y - (m_characterController.transform.position.y - m_characterController.height * m_enclosingInstance.transform.lossyScale.y / 2f)) * Vector3.up;

                        this.Move(floorStay);
                    }

                    this.Move(movementForThisFrameWalk);
                }

                //save position of root joint of the avatar, for trespassing detection of next frame
                m_lastAvatarPosition = m_bodyAvatarers[0].BodyRootJoint.position;

                //save last known position of character controller
                m_lastCharacterControllerPosition = m_characterController.transform.position;
            }

            #endregion

            #region Internal methods

            /// <summary>
            /// Resets the player, making it to restart from initial calibration
            /// </summary>
            internal void Reset()
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("ImmotionRoom player controller - Received Reset request");
                }

                //wipe everything (including saved calibration data)
                m_enclosingInstance.StopAllCoroutines();
                m_systemReady = false;
                CalibrationDataManager.OnlineSessionCalibrationData = null;

                //trigger re-initialization
                m_calibrator.ReCalibrate();
                m_enclosingInstance.StartCoroutine(Initialization());
            }

            #endregion

            #region Initialization stuff

            /// <summary>
            /// Initialize the whole system, making it to connect to the tracking service and start CalibrationInitialization method
            /// to perform the calibration of VR with ImmotionRoom system
            /// </summary>
            /// <returns></returns>
            private IEnumerator Initialization()
            {
                //let all Start methods to perform (otherwise in HeadsetManager the method InitForIRoom may be executed before Start())
                yield return 0;

                //init headset
                m_headsetManager.InitForIRoom();

                //move the headset frame of reference far far away, so that the calibration canvas isn't disturbed by the environment
                m_headsetFrameOfReference.localPosition = 500 * Vector3.down;

                //deactivate the avatarers and reset them, so they can attach to a new body
                foreach (BodyAvatarer avatar in m_bodyAvatarers)
                {
                    avatar.enabled = false;
                }

                //wait for tracking service connection, then get a scene provider, to read user's skeleton data
                while (!TrackingServiceManagerBasic.Instance.IsTracking)
                    yield return new WaitForSeconds(0.1f);

                m_sceneDataProvider = null;

                while ((m_sceneDataProvider = TrackingServiceManagerBasic.Instance.StartSceneDataProvider()) == null)
                    yield return new WaitForSeconds(0.1f);

                yield return 0;

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("ImmotionRoom player controller - Connected with Scene Provider");
                }

                //start system calibration
                m_enclosingInstance.StartCoroutine(CalibrationInitialization());

            }

            /// <summary>
            /// Wait until the calibration object performs its calibration and then initializes avatars and other stuff of the player
            /// </summary>
            /// <returns></returns>
            private IEnumerator CalibrationInitialization()
            {
                //wait for done calibration
                while (!m_calibrator.CalibrationDone)
                {
                    yield return new WaitForSeconds(0.1f);
                }

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("ImmotionRoom player controller - Calibration done!");
                }

                //get body provider of calibrated body, that is, the user
                m_playerBodyDataProvider = new BodyDataProvider(m_sceneDataProvider, m_calibrator.CalibrationData.UserBodyId);

                //change Kinect frame of reference position, so that avatar center now stays in zero position (in local coordinates) on XZ plane
                //use spine mid for this purpose
                m_kinectFrameOfReference.transform.localPosition = m_calibrator.CalibrationData.CalibrationRotationMatrix.MultiplyPoint3x4(
                                                                               new Vector3(-m_playerBodyDataProvider.LastBody.Joints[TrackingService.DataClient.Model.TrackingServiceBodyJointTypes.SpineMid].Position.X,
                                                                               0,
                                                                               m_playerBodyDataProvider.LastBody.Joints[TrackingService.DataClient.Model.TrackingServiceBodyJointTypes.SpineMid].Position.Z));

                //rotate kinect frame of reference following found calibration
                m_kinectFrameOfReference.transform.localRotation = m_calibrator.CalibrationData.CalibrationRotationMatrix.ToQuaternion();

                //say to avatars which is the found calibrated body and re-enable them
                foreach (BodyAvatarer avatar in m_bodyAvatarers)
                {
                    avatar.SetNewBodyId(m_calibrator.CalibrationData.UserBodyId);
                    avatar.TrackPosition = true; //force track position to always true: we will handle the request to not track movements in FixedUpdate method
                    avatar.enabled = true;
                }

                //wait until main avatar object is fully constructed and initialized with user pose (should need one frame or two)
                while (!m_bodyAvatarers[0].InitializedWithBodyData)
                {
                    yield return 0;
                }

                //set correct height to character controller (and adjust step offset accordingly)
                m_characterController.height = m_calibrator.CalibrationData.UserHeight;
                m_characterController.stepOffset = m_characterController.height * CharacterControllerStepOffsetHeightFactor * m_enclosingInstance.transform.lossyScale.y;

                //move the character controller, so that it stays where the avatar is. We add a little translation on the y component to make it fall to the ground
                //in the next lines
                m_characterController.transform.position = m_bodyAvatarers[0].BodyRootJoint.position + new Vector3(0, 0.25f, 0);

                //wait for the physics system to move the character controller. This is useful if the player controller is initially colliding with 
                //another object: the physics system will move it so that it now stays in a valid position.
                //In the meantime make the character controller to go in a valid position where its lower bound touches the floor
                while (!m_characterController.isGrounded)
                {
                    m_characterController.Move(2 * Physics.gravity * Time.fixedDeltaTime);
                    yield return new WaitForFixedUpdate();
                }

                //move the avatar so that it stays in a position coherent with the one of the character controller on the XZ plane.
                //Check how much we have moved the character controller to move it away from colliding objects and apply the same translation to
                //the avatar system
                Vector3 characterControllerAdjustmentTranslation = m_characterController.transform.position - m_bodyAvatarers[0].BodyRootJoint.position;
                characterControllerAdjustmentTranslation.y = 0; //ignore y position, we have used it to land the controller on the floor
                characterControllerAdjustmentTranslation.Scale(new Vector3(1 / m_enclosingInstance.transform.lossyScale.x, 1 / m_enclosingInstance.transform.lossyScale.y, 1 / m_enclosingInstance.transform.lossyScale.z));
                characterControllerAdjustmentTranslation = Quaternion.Inverse(m_enclosingInstance.transform.rotation) * characterControllerAdjustmentTranslation;

                m_kinectFrameOfReference.transform.localPosition += characterControllerAdjustmentTranslation;

                //if that flag has been checked, put feets on the floor. To do this, we put avatar ankles where collider lower bounds is
                //(usually character controller floats a bit above the floor, so it is ideal to put ankles there)
                //(actually we don't put ankles, but a mean point between ankles and foot)
                if (m_enclosingInstance.InitWithFootOnFloor)
                {
                    float feetMeanYPos = (m_bodyAvatarers[0].GetJointTransform(TrackingServiceBodyJointTypes.AnkleLeft).position.y +
                                          m_bodyAvatarers[0].GetJointTransform(TrackingServiceBodyJointTypes.AnkleRight).position.y +
                                          m_bodyAvatarers[0].GetJointTransform(TrackingServiceBodyJointTypes.FootLeft).position.y +
                                          m_bodyAvatarers[0].GetJointTransform(TrackingServiceBodyJointTypes.FootRight).position.y) * 0.25f;
                    float chControllerLowerBound = m_characterController.transform.position.y - 0.5f * m_characterController.height * m_enclosingInstance.transform.lossyScale.y;

                    m_kinectFrameOfReference.transform.localPosition += new Vector3(0, (chControllerLowerBound - feetMeanYPos) / m_enclosingInstance.transform.lossyScale.y + m_enclosingInstance.InitWithFootOnFloorAdditionalFactor, 0);
                }

                //initialize last avatar position, used for non-trespassing behaviour in the fixed update method
                m_lastAvatarPosition = m_bodyAvatarers[0].BodyRootJoint.position;

                //initialize last known character position
                m_lastCharacterControllerPosition = m_characterController.transform.position;

                //init gravity influence speed to zero
                m_lastGravitySpeed = Vector3.zero;

                //restore headset position
                m_headsetFrameOfReference.localPosition = m_headsetInitialPos;

                //re-read camera references: some plugins (like OSVR one) adds cameras after a bit of initialization, so re-check camera references you've saved at startup
                m_headsetCameras = m_headsetFrameOfReference.GetComponentsInChildren<Camera>(true);
                m_headsetCamerasIsEye = new bool[m_headsetCameras.Length];

                //non-eye cameras are thing for rendering stuff. Usually they have children cameras that actually represent the eye
                //so, if a camera has child cameras, just ignore it.
                for (int i = 0; i < m_headsetCameras.Length; i++)
                    if (m_headsetCameras[i].transform.GetComponentsInChildren<Camera>(true).Length == 1) //only the parent one
                        m_headsetCamerasIsEye[i] = true;
                    else
                        m_headsetCamerasIsEye[i] = false;

                //change cameras near plane to provided user value, taking in count new scale of avatar. This prevents the camera to see internal parts of the avatar
                //when avatar is too big
                foreach (Camera camera in m_headsetCameras)
                    camera.nearClipPlane = m_enclosingInstance.CamerasNearPlane * (m_enclosingInstance.transform.lossyScale.x + m_enclosingInstance.transform.lossyScale.y + m_enclosingInstance.transform.lossyScale.z) / 3;

                //start watermarking
                m_enclosingInstance.StartCoroutine(WatermarkingLoop());

                //we-re now ready for VR!
                m_calibrator.SignalPlayerReady();
                m_systemReady = true;

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("ImmotionRoom player controller - Completely initialized! Happy VR!");
                }

                yield break;
            }

            #endregion

            #region Movement methods

            /// <summary>
            /// Try to move the player according to the provided distance.
            /// The player can move by the provided length or by a smaller measure, because the player may have collided with an obstacle
            /// </summary>
            /// <param name="movementAmount">The translation the player has to perform</param>
            /// <param name="avateeringMovement">True if the movement comes from an avateering operation that has to be transmitted to the character controller (e.g. room-scale); false if it is the opposite (e.g. walk-scale)</param>
            private void Move(Vector3 movementAmount, bool avateeringMovement = false)
            {
                //save present position of the character controller (we'll use it later)
                Vector3 characterPosition = m_characterController.transform.localPosition;

                //get the movement in local scale amount (if we have to move by 2 meters and local scale is 2, consider movement of 1 meter (the object
                //scaling will make it result a 2m movement))
                //consider also that if the player is rotated, we have to undo that rotation too
                Vector3 lossyScaleInverse = new Vector3(1 / m_enclosingInstance.transform.lossyScale.x, 1 / m_enclosingInstance.transform.lossyScale.y, 1 / m_enclosingInstance.transform.lossyScale.z);
                Vector3 unscaledMovementAmount = movementAmount;
                unscaledMovementAmount.Scale(lossyScaleInverse);
                unscaledMovementAmount = Quaternion.Inverse(m_enclosingInstance.transform.rotation) * unscaledMovementAmount;

                //if this is a controller with pure room-scale behaviour
                if (!m_enclosingInstance.FpsBehaviour)
                {
                    //move the character controller transform by the required amount.
                    //Notice that moving the transform and not using the Move method, the controller does not check for collisions
                    m_characterController.transform.localPosition += unscaledMovementAmount;

                    //IF THE MOVEMENT COMES FROM A CHARACTER CONTROLLER OPERATION,
                    //move the kinect frame of reference by the value of actual movement of the controller. 
                    //This way, we mantain coherence between the character controller and the ImmotionRoom frame of reference
                    //(i.e. the avatar and the girello).
                    //This means that if the player has moved 1m forward because of the walking detection, the avatar gets moved
                    //1m ahead, too (because we move avateering frame of reference)
                    //IF THE MOVEMENT COMES FROM AN AVATEERING OPERATION,
                    //do nothing, because the avatar has already performed its translation 
                    if (!avateeringMovement)
                        m_kinectFrameOfReference.localPosition += unscaledMovementAmount;
                }
                //else, if this is a player controller with FPS behaviour
                else
                {
                    //try to move the player controller by the required calculated amount
                    m_characterController.Move(movementAmount);

                    //get how much the player actually moved: if he hit an obstacle, he didn't move by the provided length, but by a
                    //smaller distance. So, get the actual movement subtracting actual position by the saved value.
                    //if this movement comes from an avateering translation (room-scale) and obstacles detection for room-scale have been
                    //disable, ignore this mechanic and just consider as we moved by the required amount
                    Vector3 actualMovementForThisFrame = m_characterController.transform.localPosition - characterPosition;

                    //IF THE MOVEMENT COMES FROM A CHARACTER CONTROLLER OPERATION,
                    //move the kinect frame of reference by the value of actual movement of the controller. 
                    //This way, we mantain coherence between the character controller and the ImmotionRoom frame of reference
                    //(i.e. the avatar and the girello).
                    //This means that if the player has moved 1m forward because of the walking detection, the avatar gets moved
                    //1m ahead, too (because we move avateering frame of reference)
                    //IF THE MOVEMENT COMES FROM AN AVATEERING OPERATION,
                    //moves the kinect frame of reference by the amount of movement that we thought that should happen and that didn't happen.
                    //This means that if the avateering system has detected a translation of 1m forward and has moved the avatar 1m forward,
                    //but the character controller detected a collision and so the avatar can move only by 0.3m forward because of a wall,
                    //we have to move the kinect frame of reference by 0.7m backward, so that to mantain coherence between the two systems
                    m_kinectFrameOfReference.localPosition += actualMovementForThisFrame - (avateeringMovement ? unscaledMovementAmount : Vector3.zero);
                }
            }

            /// <summary>
            /// Get the amount of movement of the player coming from the gravity influence inside the game
            /// </summary>
            /// <returns>How much the player has to move due to gravity effect</returns>
            private Vector3 GetGravityMovementAmount()
            {
                if (m_characterController.isGrounded)
                {
                    m_lastGravitySpeed = Vector3.zero;

                    return Vector3.down * Time.fixedDeltaTime;
                }
                else
                {
                    m_lastGravitySpeed += Physics.gravity * m_enclosingInstance.GravityMultiplier * Time.fixedDeltaTime;
                    return m_lastGravitySpeed * Time.fixedDeltaTime;
                }


            }

            /// <summary>
            /// Get the amount of movement of the player coming from mouse, keyboard and gamepad
            /// </summary>
            /// <returns>How much the player has to move due to user's input</returns>
            private Vector3 GetInputMovementAmount()
            {
                //if debug controls are enabled
                if (m_enclosingInstance.AllowDebugControls)
                {
                    //get speed direction from keyboard
                    Vector2 speed = Vector2.zero;

                    if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                        speed.x -= 1;

                    if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                        speed.x += 1;

                    if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
                        speed.y += 1;

                    if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
                        speed.y -= 1;

                    //normalize speed and multiply by the required modulus
                    speed = speed.normalized * m_enclosingInstance.DebugControlsSpeed;

                    //calculate movement for this time step
                    Vector3 movementAmount = new Vector3(speed.x * Time.fixedDeltaTime, 0, speed.y * Time.fixedDeltaTime);
                    movementAmount = m_enclosingInstance.transform.rotation * movementAmount; //consider global player rotation
                    movementAmount.Scale(m_enclosingInstance.transform.lossyScale); //consider global player scale

                    return movementAmount;
                }
                //else, if debug controls are disabled, return no movement
                else
                    return Vector3.zero;
            }

            /// <summary>
            /// Get the amount of movement of the player coming from the walking gesture detection (walk-scale behaviour)
            /// </summary>
            /// <returns>How much the player has to move due to user walking</returns>
            private Vector3 GetWalkingMovementAmount()
            {
                //init the amount to zero
                Vector3 movementAmount = Vector3.zero;

                //if walking is required, use walk-in-place locomotion
                if (m_enclosingInstance.WalkingDetection)
                {
                    //add kinect player speed (we negate the z component, because kinect reference system is right handed, while Unity one is left handed).
                    //notice that we have to consider that walking speed detected by kinect is in the tracking system world coordinate system, while here we are in Oculus coordinate system
                    //notice that we check that walking direction is coherent with hmd orientation (we must not go backward!)
                    Quaternion headPoseFromOvr = m_headsetManager.OrientationInGame;
                    Vector3 ovrOrientation = headPoseFromOvr * Vector3.forward;

                    if (m_playerBodyDataProvider.LastBody.Gestures.ContainsKey(TrackingServiceBodyGestureTypes.Walking))
                    {
                        Vector3 detectedSpeed = ((TrackingServiceWalkingGesture)m_playerBodyDataProvider.LastBody.Gestures[TrackingServiceBodyGestureTypes.Walking]).EstimatedWalkSpeed.ToVector3();
                        detectedSpeed.z = -detectedSpeed.z; //because of left-handedness
                        movementAmount = m_calibrator.CalibrationData.CalibrationRotationMatrix.MultiplyVector(detectedSpeed);
                    }

                    //Log.Debug("Walking speed" + movementAmount);

                    //check that walking direction is coherent to where the user is looking
                    //(if player is looking forward and the walking detection says to walk backwards, it is coherent, so reverse detected walking speed)
                    if (Mathf.Abs(UnityUtilities.SignedVector2Angle(new Vector2(ovrOrientation.x, ovrOrientation.z), new Vector2(movementAmount.x, movementAmount.z))) > OrientationCheckThreshold)
                        movementAmount = -movementAmount;

                    movementAmount = m_enclosingInstance.DetectedWalkingSpeedMultiplier * movementAmount * Time.fixedDeltaTime;
                    movementAmount = m_enclosingInstance.transform.rotation * movementAmount; //consider global player rotation
                    movementAmount.Scale(m_enclosingInstance.transform.lossyScale); //consider global player scale
                }

                //return detected movement
                return movementAmount;
            }

            #endregion

            #region Watermarking methods

            /// <summary>
            /// Adds a watermark to what the player see, if the user has not paid to remove watermarking
            /// </summary>
            private IEnumerator WatermarkingLoop()
            {
#if !VR_WATERMARK_FREE

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("IroomPlayerController - Watermark check ENABLED");
                }

                //add watermark object
                GameWatermark.CreateInstance();

                yield return 0; //wait for watermark creation
                yield return 0;

                //while forever
                while(true)
                {
                    //get watermark object
                    GameWatermark watermarkCamera = FindObjectOfType<GameWatermark>();

                    //if it does not exist or it has been altered, reset everything and throw exception
                    if(watermarkCamera == null || !watermarkCamera.Check())
                    {
                        if(Log.IsErrorEnabled)
                        {
                            Log.Error("Watermark has been altered. You are not authorized to do this!");
                        }

                        Reset();

                        throw new Exception("Watermark has been altered. You are not authorized to do this!");
                    }

                    //wait a second for the next iteration
                    yield return new WaitForSeconds(1.0f);
                }
                
#else
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("IroomPlayerController - Watermark check disabled");
                }

                yield break;
#endif
            }

            #endregion
        }

    }
}

