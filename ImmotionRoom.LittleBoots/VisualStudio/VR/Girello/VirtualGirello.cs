namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Girello
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Globalization;
    using System.IO;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Tools;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement;
    using System;

    /// <summary>
    /// Draws a bounding box around the virtual player to let him know where the physical boundaries of the game area are.
    /// The more the player will be near its boundaries, the more the box will become opaque
    /// </summary>
    public partial class TrackingServiceVirtualGirello : MonoBehaviour
    {
        #region Public Unity properties

        /// <summary>
        /// True to draw the box lines and grids, false otherwise
        /// </summary>
        [Tooltip("True to draw the box lines and grids, false otherwise. Should remain true, unless the cage causes some problems")]
        public bool DrawBox = true;

        /// <summary>
        /// Player that must stay inside the girello. If it is null, the object will look for the first valid CharacterController.
        /// If even this one does not exist, the girello will disable itself
        /// </summary>
        [Tooltip("Player that must stay inside the girello. If it is null, the object will look for the first valid CharacterController. If even this one does not exist, the girello will disable itself")]
        public Collider Player;

        /// <summary>
        /// The color to draw the box with 
        /// (will be full transparent when user is inside the boundaries and will become the more opaque the more the user will go
        /// near the boundaries)
        /// </summary>
        [Tooltip("The color to draw the box with. Only B, G, R values will be used (alpha is depenedent on user distance from the bounds)")]
        public Color BoxColor = Color.red;

        /// <summary>
        /// Thickness of the box lines to draw
        /// </summary>
        [Tooltip("Thickness of the box lines to draw")]
        public float BoxLinesThickness = 22.5f;

        /// <summary>
        /// Thickness of the box grid lines to draw, in unscaled coordinates
        /// </summary>
        [Tooltip("Thickness of the box grid lines to draw, in unscaled coordinates")]
        public float BoxGridLinesThickness = 14.5f;

        /// <summary>
        /// Number (horizontal or vertical) of interleaving grid lines 
        /// (and so number of flashing arrows for each drawn line)
        /// </summary>
        [Tooltip("Number (horizontal or vertical) of intermediate grid lines")]
        public int GridLinesNumber = 3;

        /// <summary>
        /// Line material used by this object to draw lines
        /// </summary>
        [Tooltip("Material used to draw box lines")]
        public Material LinesMaterial;

        /// <summary>
        /// True to draw flashing arrows, false otherwise
        /// </summary>
        [Tooltip("Specifies if flashing arrows have to be drawn")]
        public bool DrawArrows = true;

        /// <summary>
        /// Minimum distance, between inner and outer bounds, normalized in the range [0, 1], from which flashing arrows have to be shown
        /// </summary>
        [Tooltip("Minimum distance, between inner and outer bounds, normalized in the range [0, 1], from which flashing arrows have to be shown")]
        public float MinShowArrowDistance = 0.35f;

        /// <summary>
        /// Game object used to draw the arrows. Must represent an arrow pointing towards up direction, with origin at the base of the object
        /// </summary>
        [Tooltip("Game object used to draw the arrows. Must represent an arrow pointing towards up direction, with origin at the base of the object")]
        public GameObject ArrowObject;

        /// <summary>
        /// Disable verification of lower bound check, that is often useless for VR applications
        /// </summary>
        [Tooltip("Disable verification of lower bound check, that is often useless (and sometimes problematic) for VR applications")]
        public bool DisableLowerBound = true;

        #endregion

        #region Private Fields

        /// <summary>
        /// Internal implementation of the class
        /// </summary>
        private TrackingServiceVirtualGirelloInternal m_internalImplementation;

        #endregion

        #region Behaviour methods

        void Awake()
        {
            m_internalImplementation = new TrackingServiceVirtualGirelloInternal(this);
        }

        // Use this for initialization
        void Start()
        {
            m_internalImplementation.Start();      
        }

        void OnDestroy()
        {
            m_internalImplementation.OnDestroy();
        }

        // Update is called once per frame
        void Update()
        {
            m_internalImplementation.Update();
        }

        /// <summary>
        /// Called on object rendering
        /// </summary>
        private void OnRenderObject()
        {
            m_internalImplementation.OnRenderObject();
        }

        private void OnDrawGizmos()
        {
            if(m_internalImplementation != null)
                m_internalImplementation.OnDrawGizmos();
        }

        #endregion
    }

}