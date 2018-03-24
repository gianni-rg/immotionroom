namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Calibration.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Attaches object to the specified Camera object that has the provided name, changing scale
    /// </summary>
    public class AttachToCamera : MonoBehaviour
    {
        #region Public Unity Properties

        /// <summary>
        /// Name of the object to attach to. If it is left empty, it will attach to the Main Camera
        /// </summary>
        [Tooltip("Name of the object to attach to. If it is left empty, it will attach to the Main Camera")]
        public string ObjectToAttachName;

        /// <summary>
        /// Local position to assign to this object after the attaching
        /// </summary>
        [Tooltip("Local position to assign to this object after the attaching")]
        public Vector3 LocalPosition = Vector3.zero;

        /// <summary>
        /// Local rotation to assign to this object after the attaching
        /// </summary>
        [Tooltip("Local rotation to assign to this object after the attaching")]
        public Quaternion LocalRotation = Quaternion.identity;

        /// <summary>
        /// Local scale to assign to this object after the attaching
        /// </summary>
        [Tooltip("Local scale to assign to this object after the attaching")]
        public Vector3 LocalScale = Vector3.one;

        #endregion

        #region Behaviour methods

        void Start()
        {
            //finds an object with the provided name (or to the main camera)
            GameObject wannaBeFatherGo = (ObjectToAttachName == null || ObjectToAttachName.Length == 0) ? Camera.main.gameObject : GameObject.Find(ObjectToAttachName);

            //if it does not exists, deactivate current object
            if (wannaBeFatherGo == null)
                gameObject.SetActive(false);
            //else, set it as father and assign provided local pose
            else
            {
                transform.SetParent(wannaBeFatherGo.transform, false);
                transform.localPosition = LocalPosition;
                transform.localRotation = LocalRotation;
                transform.localScale = LocalScale;
            }
        }

        #endregion
    }
}
