namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.DataSourcesManagement
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Manages <see cref="Bounds"/> object, allowing operations on them like changing limits and drawing them
    /// </summary>
    public partial class BoundsManager : MonoBehaviour
    {
        #region Public Unity Fields

        /// <summary>
        /// Center of the bounding box to manage
        /// </summary>
        [Tooltip("Bounding Box center")]
        public Vector3 BoundsCenter;

        /// <summary>
        /// Extents of the bounding box to manage
        /// </summary>
        [Tooltip("Bounding Box size (the half of the size)")]
        public Vector3 BoundsExtents;

        /// <summary>
        /// Material to draw the bounding box contours with
        /// </summary>
        [Tooltip("Material to draw the bounding box contours with")]
        public Material BoundsLinesMaterial;

        /// <summary>
        /// Color into which draw the bounding element 
        /// </summary>
        [Tooltip("Color to draw the bounding box contours with")]
        public Color BoundsLinesColor;

        /// <summary>
        /// Thickness of drawn lines
        /// </summary>
        [Tooltip("Thickness of drawn lines")]
        public float BoundsLinesThickness = 0.071f;

        #endregion

        #region Private Fields

        /// <summary>
        /// Internal implementation of the class
        /// </summary>
        BoundsManagerInternal m_internalImplementation;

        #endregion

        #region Behaviour Methods

        void Awake()
        {
            m_internalImplementation = new BoundsManagerInternal(this);
        }

        void Start()
        {
            m_internalImplementation.Start();
        }

        void Update()
        {
            m_internalImplementation.Update();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Set a new boundary limit for the managed bounding box.
        /// The provided point becomes the new limit for the bounding element, changing the present limit specified as parameter
        /// </summary>
        /// <param name="newLimitType">Limit to change (e.g. if front limit is specified, only z value will get affected)</param>
        /// <param name="newLimit">Point to use as new limit</param>
        public void SetNewBoundLimit(BoundsGrowingType newLimitType, Vector3 newLimit)
        {
            m_internalImplementation.SetNewBoundLimit(newLimitType, newLimit);               
        }
        
        /// <summary>
        /// Set bounding box extents so that the box is null
        /// </summary>
        public void SetNullLimits()
        {
            m_internalImplementation.SetNullLimits();
        }

        #endregion
    }
}
