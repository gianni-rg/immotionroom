namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Watermarking
{
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
        #region Private fields

        /// <summary>
        /// Internal implementation of the class
        /// </summary>
        private GameWatermarkInternal m_internalImplementation;

        #endregion

        #region Behaviour methods

        void Awake()
        {
            m_internalImplementation = new GameWatermarkInternal(this);
        }

        void Update()
        {
            m_internalImplementation.Update();
        }

        void Start()
        {
            m_internalImplementation.Start();
        }

        #endregion

        #region Watermarking methods

        /// <summary>
        /// Create an object that puts a watermark in front of user's eyes, deleting previous one existing in the scene, if any
        /// </summary>
        internal static void CreateInstance()
        {
            GameWatermarkInternal.CreateInstance();
        }

        /// <summary>
        /// Checks that this watermark object is exactly how it was created, so that a writing is in front of user's eyes. 
        /// If the check is not successful, return false
        /// </summary>
        /// <returns>True if watermark is still ok; false if it has been altered</returns>
        internal bool Check()
        {
            return m_internalImplementation.Check();
        }

        #endregion
    }
}
