namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Humanoid
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Performs avateering operations for a humanoid avatar
    /// </summary>
    public abstract class HumanoidAvatarer : Avatarer
    {
        #region Protected fields

        /// <summary>
        /// Actual humanoid avatar object
        /// </summary>
        protected GameObject m_avatarModel;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="avatarModel">Avatar game object that has to be moved according to player movements</param>
        protected HumanoidAvatarer(GameObject avatarModel) :
            base()
        {
            m_avatarModel = avatarModel;
        }

        #endregion
    }
}
