namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils
{
    using UnityEngine;
    using System.Collections;

    /// <summary>
    /// Gives the Camera component of current gameobject the ability to be moved using fingers on a touch screen:
    /// If the camera is perspective, pinch gesture will zoom in/out, while swipe gesture will make the camera rotate inside an ideal sphere
    /// If the camera is ortographic, pinch gesture will zoom in/out, while swipe gesture will translate the camera left/right/up/down
    /// If the script is executed on PC, keyboard arrow keys will be used to substitute swipe and +/- keys will be used to zoom
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public partial class MoveCameraWithFingers : MonoBehaviour
    {
        #region Public Unity Properties

        /// <summary>
        /// UI Control inside which the gesture inputs affect the camera pose
        /// </summary>
        [Tooltip("UI Control inside which the gesture inputs affect the camera pose")]
        public RectTransform CameraControlRectangle;

        /// <summary>
        /// The rate of change of zoom (camera forward / backward) in perspective mode.
        /// </summary>
        [Tooltip("Zoom factor in perspective mode: how much the pinch gesture affects the zoom of the camera, if it is a perspective one")]
        public float PerspectiveCameraZoomSpeed = 6.5f;

        /// <summary>
        /// The rate of change of the zoom (FOV smaller / greater) in orthographic mode
        /// </summary>
        [Tooltip("Zoom factor in ortographic mode: how much the pinch gesture affects the zoom of the camera, if it is a ortographic one")]
        public float OrthoCameraZoomSpeed = 3.5f;

        /// <summary>
        /// The rate of change of the camera position in perspective mode.
        /// </summary>
        [Tooltip("Translation factor in perspective mode: how much the swipe gesture affects the position of the camera, if it is a perspective one")]
        public float PerspectiveCameraSpeed = 0.4f;

        /// <summary>
        /// The rate of change of the camera position in ortographic mode.
        /// </summary>
        [Tooltip("Translation factor in ortographic mode: how much the swipe gesture affects the position of the camera, if it is a ortographic one")]
        public float OrthoCameraSpeed = 0.4f;

        /// <summary>
        /// The lookat point of the camera
        /// </summary>
        [Tooltip("Center of the rotational sphere, i.e. the look-at point of the camera, if it is a perspective one")]
        public Vector3 PerspectiveSphereCenter = Vector3.zero;

        /// <summary>
        /// Difference of speed from touch to keyboard input 
        /// (touch is more sensible)
        /// </summary>
        [Tooltip("Difference of speed from touch to keyboard input ")]
        public float TouchFromKeyboardSpeedFactor = 0.0115f;

        #endregion

        #region Private Fields

        /// <summary>
        /// Internal implementation of the class
        /// </summary>
        MoveCameraWithFingersInternal m_internalImplementation;

        #endregion

        #region Behaviour Methods

        void Awake()
        {
            m_internalImplementation = new MoveCameraWithFingersInternal(this);
        }

        // Use this for initialization
        void Start()
        {         
            m_internalImplementation.Start();
        }

        // Update is called once per frame
        void Update()
        {
            if (Application.platform == RuntimePlatform.Android)
                m_internalImplementation.UpdateMobile();
            else
                m_internalImplementation.Update();
        }

        #endregion

    }

}