namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Watermarking
{
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.HeadsetManagement;
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.PlayerController;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Manages a Watermark object put in front of user eyes
    /// </summary>
    public partial class GameWatermark : MonoBehaviour
    {
        /// <summary>
        /// Contains the actual implementation of the GameWatermark, for obfuscation purposes
        /// </summary>
        private class GameWatermarkInternal
        {
            #region Private fields

            /// <summary>
            /// Material the font renderer has at creation of this object
            /// </summary>
            private Material m_initialFontMaterial;

            /// <summary>
            /// Shader the font renderer has at creation of this object
            /// </summary>
            private Shader m_initialFontShader;

            /// <summary>
            /// Texture the font renderer has at creation of this object
            /// </summary>
            private Texture m_initialFontTexture;

            /// <summary>
            /// The GameWatermark object that contains this object
            /// </summary>
            private GameWatermark m_enclosingInstance;

            /// <summary>
            /// Position we set for the watermark at the last frame
            /// </summary>
            private Vector3 m_lastFramePosition;

            /// <summary>
            /// Rotation we set for the watermark at the last frame
            /// </summary>
            private Quaternion m_lastFrameRotation;

            /// <summary>
            /// Scale we set for the watermark at the last frame
            /// </summary>
            private Vector3 m_lastFrameScale;

            /// <summary>
            /// True if the user has moved the pose of the watermark, false otherwise
            /// </summary>
            private bool m_poseCorrupted;

            /// <summary>
            /// Reference to the headset manager of the player controller
            /// </summary>
            private HeadsetManager m_headsetManager;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="enclosingInstance">Enclosing instance, whose code has to be implemented</param>
            internal GameWatermarkInternal(GameWatermark enclosingInstance)
            {
                m_enclosingInstance = enclosingInstance;
            }

            #endregion

            #region Behaviour methods

            internal void Start()
            {
                //save reference to initial material data, for future checks
                m_initialFontMaterial = m_enclosingInstance.transform.GetChild(0).GetComponent<Renderer>().material;
                m_initialFontShader = m_initialFontMaterial.shader;
                m_initialFontTexture = m_initialFontMaterial.mainTexture;
                m_poseCorrupted = false;
                m_headsetManager = FindObjectOfType<IroomPlayerController>().GetComponent<HeadsetManager>();
            }

            internal void Update()
            {
                //check that no one has changed its pose since last frame
                if (m_enclosingInstance.transform.position != m_lastFramePosition ||
                    m_enclosingInstance.transform.rotation != m_lastFrameRotation ||
                    m_enclosingInstance.transform.localScale != m_lastFrameScale)
                    m_poseCorrupted = true;

                //set its pose so that it is seen by the cameras
                m_lastFrameRotation = m_enclosingInstance.transform.rotation = m_headsetManager.OrientationInGame;
                m_lastFramePosition = m_enclosingInstance.transform.position = m_headsetManager.PositionInGame;
                m_lastFrameScale = m_enclosingInstance.transform.localScale = m_headsetManager.transform.localScale;
            }

            #endregion

            #region Watermarking methods

            /// <summary>
            /// Create an object that puts a watermark in front of user's eyes, deleting previous one existing in the scene, if any
            /// </summary>
            internal static void CreateInstance()
            {
                //delete old instance, if any
                if (FindObjectOfType<GameWatermark>())
                    Destroy(FindObjectOfType<GameWatermark>().gameObject);

                //create a new object, far far away
                GameObject watermarkGo = new GameObject("Watermarking");
                watermarkGo.transform.position = 30000 * Vector3.one;
                GameWatermark createdInstance = watermarkGo.AddComponent<GameWatermark>();

                ////add an ortho camera to it, with depth bigger than the one of the main camera
                //GameObject watermarkCameraGo = new GameObject("Camera");
                //watermarkCameraGo.transform.SetParent(watermarkGo.transform, false);
                //Camera watermarkCamera = watermarkCameraGo.AddComponent<Camera>();
                //watermarkCamera.clearFlags = CameraClearFlags.Depth;
                //watermarkCamera.orthographic = true;
                //watermarkCamera.orthographicSize = 5;
                //watermarkCamera.nearClipPlane = 0.1f;
                //watermarkCamera.farClipPlane = 5.0f;
                //watermarkCamera.depth = Camera.main.depth + 1;

                //add a text with written Made with ImmotionRoom to it
                GameObject watermarkTextGo = new GameObject("Text");
                watermarkTextGo.transform.SetParent(watermarkGo.transform, false);
                watermarkTextGo.transform.localPosition = new Vector3(-1.67f, -2.61f, 3.58f);
                watermarkTextGo.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                TextMesh watermarkText = watermarkTextGo.AddComponent<TextMesh>();
                watermarkText.text = "Powered by ImmotionRoom";
                watermarkText.fontSize = 36;
                watermarkText.color = new Color(0, 0.5f, 1.0f, 0.245f);

                //save last set data
                createdInstance.m_internalImplementation.m_lastFramePosition = watermarkGo.transform.position;
                createdInstance.m_internalImplementation.m_lastFrameRotation = watermarkGo.transform.rotation;
                createdInstance.m_internalImplementation.m_lastFrameScale = watermarkGo.transform.localScale;

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("WatermarkCamera - Created");
                }
            }

            /// <summary>
            /// Checks that this watermark object is exactly how it was created, so that a writing is in front of user's eyes. 
            /// If the check is not successful, return false
            /// </summary>
            /// <returns>True if watermark is still ok; false if it has been altered</returns>
            internal bool Check()
            {
                //check if this watermark object exists and is active
                if (!m_enclosingInstance.isActiveAndEnabled)
                    return false;

                //it must have no father
                GameObject waterMarkGo = m_enclosingInstance.gameObject;

                if (waterMarkGo.transform.parent != null)
                    return false;

                //user must not have moved it
                if (m_poseCorrupted)
                    return false;

                //it must have 1 active descendant (the text)

                if (waterMarkGo.transform.childCount != 1)
                    return false;

                if (!waterMarkGo.transform.GetChild(0).gameObject.activeInHierarchy)
                    return false;

                //get first child (the camera)
                //GameObject watermarkCameraGo = waterMarkGo.transform.GetChild(0).gameObject;

                ////check it has an active camera
                //Camera watermarkCamera = watermarkCameraGo.GetComponent<Camera>();

                //if (!watermarkCamera.enabled)
                //    return false;

                ////camera has to have precise characteristics
                //if (watermarkCamera.clearFlags != CameraClearFlags.Depth ||
                //    watermarkCamera.cullingMask != -1 /*everything*/ ||
                //    watermarkCamera.orthographic != true ||
                //    watermarkCamera.orthographicSize != 5 ||
                //    watermarkCamera.nearClipPlane != 0.1f ||
                //    watermarkCamera.farClipPlane != 5.0f ||
                //    watermarkCamera.rect.width != 1 || watermarkCamera.rect.height != 1 || watermarkCamera.rect.x != 0 || watermarkCamera.rect.y != 0 ||
                //    watermarkCamera.depth <= Camera.main.depth ||
                //    watermarkCamera.targetTexture != null)
                //    return false;

                //get second child (the text)
                GameObject watermarkTextGo = waterMarkGo.transform.GetChild(0).gameObject;

                //check if second child transform is ok
                if (watermarkTextGo.transform.localPosition.x != -1.67f || watermarkTextGo.transform.localPosition.y != -2.61f || watermarkTextGo.transform.localPosition.z != 3.58f ||
                    watermarkTextGo.transform.localRotation != Quaternion.identity ||
                    watermarkTextGo.transform.localScale.x != 0.05f || watermarkTextGo.transform.localScale.y != 0.05f || watermarkTextGo.transform.localScale.z != 0.05f)
                    return false;

                //check it has an active text and renderer
                TextMesh watermarkText = watermarkTextGo.GetComponent<TextMesh>();
                Renderer watermarkTextRenderer = watermarkTextGo.GetComponent<Renderer>();

                if (!watermarkTextRenderer.enabled)
                    return false;

                //check if renderer has default text renderer
                if (watermarkTextRenderer.materials.Length != 1 || watermarkTextRenderer.material != m_initialFontMaterial || watermarkTextRenderer.material.shader != m_initialFontShader ||
                    watermarkTextRenderer.material.mainTexture != m_initialFontTexture || watermarkTextRenderer.material.color.a != 1.0f || watermarkTextRenderer.material.mainTextureOffset != Vector2.zero || watermarkTextRenderer.material.mainTextureScale != Vector2.one)
                    return false;

                //text has to have precise characteristics
                if (watermarkText.text != "Powered by ImmotionRoom" ||
                    watermarkText.offsetZ != 0 ||
                    watermarkText.characterSize != 1 ||
                    watermarkText.lineSpacing != 1 ||
                    watermarkText.anchor != TextAnchor.UpperLeft ||
                    watermarkText.alignment != TextAlignment.Left ||
                    watermarkText.tabSize != 4 ||
                    watermarkText.fontSize != 36 ||
                    watermarkText.fontStyle != FontStyle.Normal ||
                    watermarkText.richText != true ||
                    watermarkText.font.name != "Arial" ||
                    watermarkText.color.a < 0.22f)
                    return false;

                //let's prevent someone to change layers so that the watermark can't be seen by active cameras
                Camera[] cameras = FindObjectsOfType<Camera>();
                bool foundCompatibleCamera = false;
                foreach(Camera camera in cameras)
                {
                    if ((camera.cullingMask & (1 << watermarkTextGo.layer)) != 0)
                    {
                        foundCompatibleCamera = true;
                        break;
                    }
                }

                if (!foundCompatibleCamera)
                    return false;                

                //if we're here, everything is alright
                return true;
            }

            #endregion
        }

    }
}
