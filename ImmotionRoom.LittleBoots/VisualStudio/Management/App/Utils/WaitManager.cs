namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils
{
    using UnityEngine;
    using System.Collections;
    using UnityEngine.UI;

    /// <summary>
    /// Shows a waiting / processing rotating gear if the user has to wait for the program to perform something.
    /// If wait state is true, all scene buttons get disabled
    /// </summary>
    public partial class WaitManager : MonoBehaviour
    {
        #region Unity exposed data

        /// <summary>
        /// Rotation speed of the gizmo, in degrees / s
        /// </summary>
        [Tooltip("Rotation speed of the gizmo, in degrees / s")]
        public float RotationSpeed = 60.0f;

        /// <summary>
        /// True if the program is in waiting state and the gizmo has to be shown, false otherwise
        /// </summary>
        [Tooltip("True if the program is in waiting state and the gizmo has to be shown, false otherwise")]
        public bool m_waitingState = true; // GIANNI, made public for obfuscation issues

        #endregion

        #region Private Fields

        /// <summary>
        /// Internal implementation of the class
        /// </summary>
        WaitManagerInternal m_internalImplementation;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets if the program is in waiting state and the gizmo has to be shown
        /// </summary>
        public bool WaitingState
        {
            get
            {
                return m_waitingState;
            }
            set
            {
                m_internalImplementation.SetWaitingState(value);
            }
        }

        #endregion

        #region Behaviour methods

        void Awake()
        {
            m_internalImplementation = new WaitManagerInternal(this);
        }

        void Start()
        {
            m_internalImplementation.Start();
        }

        // Update is called once per frame
        void Update()
        {
            m_internalImplementation.Update();
        }

        #endregion
    }

}