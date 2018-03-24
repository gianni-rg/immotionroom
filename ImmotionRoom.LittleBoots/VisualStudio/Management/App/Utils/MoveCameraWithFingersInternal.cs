namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils
{
    using ImmotionAR.ImmotionRoom.LittleBoots.Management._3dparties;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Gives the Camera component of current gameobject the ability to be moved using fingers on a touch screen:
    /// If the camera is perspective, pinch gesture will zoom in/out, while swipe gesture will make the camera rotate inside an ideal sphere
    /// If the camera is ortographic, pinch gesture will zoom in/out, while swipe gesture will translate the camera left/right/up/down
    /// If the script is executed on PC, keyboard arrow keys will be used to substitute swipe and +/- keys will be used to zoom
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public partial class MoveCameraWithFingers : MonoBehaviour
    {

        /// <summary>
        /// Provides the internal (obfuscable) behaviour to Unity class MoveCameraWithFingers
        /// </summary>
        private class MoveCameraWithFingersInternal
        {
            #region Private fields

            /// <summary>
            /// Camera object this script refers to
            /// </summary>
            private Camera m_camera;
      
            /// <summary>
            /// The Behaviour that contains this object
            /// </summary>
            private MoveCameraWithFingers m_wrappingBehaviour;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor with full initialization
            /// </summary>
            /// <param name="wrappingBehaviour">Behaviour wrapping this object</param>
            internal MoveCameraWithFingersInternal(MoveCameraWithFingers wrappingBehaviour)
            {
                m_wrappingBehaviour = wrappingBehaviour;
            }

            #endregion

            #region Behaviour Methods

            // Use this for initialization
            internal void Start()
            {
                //save reference to this camera
                m_camera = m_wrappingBehaviour.GetComponent<Camera>();
            }

            // Update is called once per frame
            internal void Update()
            {
                //update the zoom of the camera
                ZoomUpdatePC();

                //update the movement of the camera
                MoveUpdatePC();
            }

            // Update is called once per frame (Version for mobile)
            internal void UpdateMobile()
            {
                //update the zoom of the camera
                ZoomUpdateMobile();

                //update the movement of the camera
                MoveUpdateMobile();
            }

            #endregion

            #region Zoom methods

            /// <summary>
            /// Performs update of the behaviour, concerning the update of the zoom factor of current camera.
            /// Version for PC systems
            /// </summary>
            private void ZoomUpdatePC()
            {
                //if the mouse pointer isn't inside the provided rectangle, do nothing
                if (!RectTransformUtility.RectangleContainsScreenPoint(m_wrappingBehaviour.CameraControlRectangle, Input.mousePosition, null))
                    return;

                //use + key to zooom in, - key to zoom out
                float zoomFactor = Time.deltaTime * ((Input.GetKey(KeyCode.Equals) ? 1 : 0) - (Input.GetKey(KeyCode.Minus) ? 1 : 0));

                ZoomCamera(zoomFactor);
            }

            /// <summary>
            /// Performs update of the behaviour, concerning the update of the zoom factor of current camera.
            /// Version for Mobile systems
            /// </summary>
            private void ZoomUpdateMobile()
            {
                //code from https://unity3d.com/learn/tutorials/modules/beginner/platform-specific/pinch-zoom

                // If there are two touches on the device...
                if (Input.touchCount == 2)
                {
                    // Store both touches.
                    Touch touchZero = Input.GetTouch(0);
                    Touch touchOne = Input.GetTouch(1);

                    //if at least one of them is outside the area of interest, do nothing; otherwise, perform the zoom
                    if (!RectTransformUtility.RectangleContainsScreenPoint(m_wrappingBehaviour.CameraControlRectangle, touchZero.position, null) ||
                        !RectTransformUtility.RectangleContainsScreenPoint(m_wrappingBehaviour.CameraControlRectangle, touchOne.position, null))
                        return;

                    // Find the position in the previous frame of each touch.
                    Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                    Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                    // Find the magnitude of the vector (the distance) between the touches in each frame.
                    float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                    float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                    // Find the difference in the distances between each frame.
                    float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                    ZoomCamera(m_wrappingBehaviour.TouchFromKeyboardSpeedFactor * -deltaMagnitudeDiff);
                }

            }
            /// <summary>
            /// Zoom current controlled camera, changing its position.
            /// </summary>
            /// <param name="zoomFactor">zoom factor to apply (how to change current zoom)</param>
            private void ZoomCamera(float zoomFactor)
            {
                //if the camera is ortho, change its FOV
                if (m_camera.orthographic)
                {
                    // ... change the orthographic size based on the change in distance between the touches.
                    m_camera.orthographicSize -= zoomFactor * m_wrappingBehaviour.OrthoCameraZoomSpeed;

                    // make sure the orthographic size never drops below zero.
                    m_camera.orthographicSize = Mathf.Max(m_camera.orthographicSize, 0.1f);
                }
                //else, if it is perspective, move it forward/backward... FOV change is weird for them
                else
                {
                    //change camera position (forward, backward) depending on the zoom factor and the type of camera
                    //(be careful to not go too near to sphere center, or camera angle determination becomes unstable)
                    if(zoomFactor < 0 || Vector3.Distance(m_camera.transform.position, m_wrappingBehaviour.PerspectiveSphereCenter) > 0.1f)
                        m_camera.transform.position += zoomFactor * m_wrappingBehaviour.PerspectiveCameraZoomSpeed * m_camera.transform.forward;
                }
            }

            #endregion

            #region Move methods

            /// <summary>
            /// Performs update of the behaviour, concerning the update of the movement of current camera.
            /// Version for PC systems
            /// </summary>
            private void MoveUpdatePC()
            {
                //if the mouse pointer isn't inside the provided rectangle, do nothing
                if (!RectTransformUtility.RectangleContainsScreenPoint(m_wrappingBehaviour.CameraControlRectangle, Input.mousePosition, null))
                    return;

                //use Arrow keys to move
                Vector2 moveFactor = Time.deltaTime * new Vector2(
                    (Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.LeftArrow) ? 1 : 0),
                    (Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.DownArrow) ? 1 : 0)
                    );

                MoveCamera(moveFactor);
            }

            /// <summary>
            /// Performs update of the behaviour, concerning the update of the movement of current camera.
            /// Version for Mobile systems
            /// </summary>
            private void MoveUpdateMobile()
            {
                //code from https://unity3d.com/learn/tutorials/modules/beginner/platform-specific/pinch-zoom

                // If there are is one touch on the device...
                if (Input.touchCount == 1)
                {
                    // get it
                    Touch touch = Input.GetTouch(0);

                    //if it is outside the area of interest, do nothing; otherwise, perform the move operation
                    if (!RectTransformUtility.RectangleContainsScreenPoint(m_wrappingBehaviour.CameraControlRectangle, touch.position, null))
                        return;

                    // use the difference from previous frame to move the camera
                    MoveCamera(m_wrappingBehaviour.TouchFromKeyboardSpeedFactor * touch.deltaPosition);
                }
            }

            /// <summary>
            /// Move current controlled camera, changing its position
            /// </summary>
            /// <param name="moveFactor">move factor to apply (how to change current camera position)</param>
            private void MoveCamera(Vector2 moveFactor)
            {
                //if the camera is ortho, change its position moving left, right, up, down
                if (m_camera.orthographic)
                {
                    m_camera.transform.position += m_camera.transform.rotation * new Vector3(moveFactor.x, moveFactor.y, 0) * m_wrappingBehaviour.OrthoCameraZoomSpeed;
                }
                //else, if it is perspective, move it around a sphere centered in the origin
                else
                {
                    //transform position in polar coordinates
                    SphericalCoordinates polarCameraCoords = new SphericalCoordinates(m_wrappingBehaviour.transform.position - m_wrappingBehaviour.PerspectiveSphereCenter, 0.01f, 100f, 0f, Mathf.PI * 2, 0, +Mathf.PI / 2.2f);

                    //increment polar angle using x and elevation using y component of moveFactor
                    polarCameraCoords.RotatePolarAngle(moveFactor.x * m_wrappingBehaviour.PerspectiveCameraSpeed);
                    polarCameraCoords.RotateElevationAngle(moveFactor.y * m_wrappingBehaviour.PerspectiveCameraSpeed);

                    //re-set position using polar
                    m_wrappingBehaviour.transform.position = polarCameraCoords.toCartesian + m_wrappingBehaviour.PerspectiveSphereCenter;

                    //look at the origin
                    m_wrappingBehaviour.transform.LookAt(m_wrappingBehaviour.PerspectiveSphereCenter);
                }
            }

            #endregion
        }
    }

}
