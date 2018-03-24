namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Skeletals
{
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Manages Skeletal Avatars that follow users movements
    /// </summary>
    public class BodiesSkeletalsManager : BodiesAvateeringManager
    {
        #region Constants

        /// <summary>
        /// List of available positve colors
        /// </summary>
        protected static readonly Color[] PositiveColors = new Color[] { Color.green, Color.cyan, Color.white, Color.blue };

        /// <summary>
        /// List of available negative colors
        /// </summary>
        protected static readonly Color[] NegativeColors = new Color[] { Color.red, Color.yellow, Color.black, Color.magenta };

        #endregion

        #region Public Unity properties

        /// <summary>
        /// Type of color visualization to give to the managed skeletons
        /// </summary>
        [Tooltip("Colouring mode of the various skeletons managed by this object")]
        public SkeletalsDrawingMode SkeletalDrawingMode;

        /// <summary>
        /// Material to draw the joints with
        /// </summary>
        [Tooltip("The material to draw the joints with")]
        public Material JointsMaterial;

        /// <summary>
        /// Material to draw joints lines with
        /// </summary>
        [Tooltip("The material to draw the limbs (the lines connecting the joints) with")]
        public Material LimbsMaterial;

        /// <summary>
        /// Positive color to be used in the joint drawing.
        /// </summary>
        [Tooltip("Positive color, to be used for joints with 100% confidence")]
        public Color PositiveColor;

        /// <summary>
        /// Negative color to be used in the joint drawing.
        /// This is the one used for low confidence joints
        /// </summary>
        [Tooltip("Negative color, to be used for joints with 0% confidence")]
        public Color NegativeColor;

        /// <summary>
        /// Color to be used to draw the bones of the skeletons
        /// </summary>
        [Tooltip("Limbs color, to be used to draw the skeletons bones")]
        public Color LimbsColor;

        /// <summary>
        /// Radius of the sphere representing each drawn joint
        /// </summary>
        [Tooltip("Radius of the sphere representing each drawn joint")]
        public float JointSphereRadius;

        /// <summary>
        /// Thickness of lines connecting consecutive joints
        /// </summary>
        [Tooltip("Thickness of lines representing the limbs")]
        public float ConnectingLinesThickness;

        /// <summary>
        /// True to add colliders for hands and feet, false otherwise
        /// </summary>
        [Tooltip("True to add colliders for hands and feet, false otherwise")]
        public bool AddColliders;

        #endregion

        #region BodiesAvateeringManager members

        /// <summary>
        /// Adds a body avateerer for a body of interest to the provided game object
        /// </summary>
        /// <param name="avatarGo">Avatar Game Object the avatareer has to be attached to</param>
        /// <param name="bodyId">Unique Body ID</param>
        protected override void AddAvateerer(GameObject avatarGo, ulong bodyId)
        {
            //create a new skeletal avatar to follow the body and attach it to the provided gameobject
            avatarGo.SetActive(false); //to launch awake after properties initialization, we freeze the object
            SkeletalBodyAvaterer skeletalAvatarer = avatarGo.AddComponent<SkeletalBodyAvaterer>();
            skeletalAvatarer.BodyId = bodyId;
            skeletalAvatarer.TrackPosition = this.TrackPosition;
            skeletalAvatarer.ShadowsEnabled = ShadowsEnabled;
            skeletalAvatarer.JointsMaterial = JointsMaterial;
            skeletalAvatarer.LimbsMaterial = LimbsMaterial;
            skeletalAvatarer.LimbsColor = LimbsColor;
            skeletalAvatarer.JointSphereRadius = JointSphereRadius;
            skeletalAvatarer.ConnectingLinesThickness = ConnectingLinesThickness;

            //assign an appropriate color to the new skeleton, depending on user choice
            switch(SkeletalDrawingMode)
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

            avatarGo.SetActive(true); //unfreeze the object

            if (Log.IsDebugEnabled)
            {
                Log.Debug("Bodies Skeletals Manager - Added new Skeletal avatar for body with ID {0}", bodyId);
            }
        }

        #endregion
    }
}
