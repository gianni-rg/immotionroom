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
        /// <summary>
        /// Shows a waiting / processing rotating gear if the user has to wait for the program to perform something.
        /// If wait state is true, all scene buttons get disabled
        /// </summary>
        private partial class WaitManagerInternal
        {
            #region Private fields

            /// <summary>
            /// The Wait Manager that contains this object
            /// </summary>
            private WaitManager m_waitManager;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="waitManager">Enclosing instance, whose code has to be implemented</param>
            internal WaitManagerInternal(WaitManager waitManager)
            {
                m_waitManager = waitManager;
            }

            #endregion

            #region Behaviour methods

            internal void Start()
            {
                m_waitManager.GetComponent<Image>().enabled = m_waitManager.m_waitingState;
            }

            // Update is called once per frame
            internal void Update()
            {
                //if we have to show the rotating gear
                if (m_waitManager.m_waitingState)
                {
                    //rotate it each frame
                    m_waitManager.transform.localRotation *= Quaternion.AngleAxis(m_waitManager.RotationSpeed * Time.deltaTime, Vector3.forward);
                }
            }

            #endregion

            #region Internal Methods

            /// <summary>
            /// Gets or sets if the program is in waiting state and the gizmo has to be shown
            /// </summary>
            /// <param name="value">Value to set for the waiting state</param>
            internal void SetWaitingState(bool value)
            {
                if (m_waitManager.m_waitingState != value)
                {
                    m_waitManager.m_waitingState = value;
                    m_waitManager.GetComponent<Image>().enabled = value; //remember to show/hide the gear image if we're changing the state of the program

                    //enable/disable all buttons
                    Selectable[] buttons = FindObjectsOfType<Selectable>(); //selectable is base class of button and toggle
                    foreach (Selectable button in buttons)
                        button.interactable = !value;

                    //enable/disable back buttons behaviour
                    ScenesManager.Instance.SetBackButtonEnabledState(!value);
                }
            }

            #endregion
        }
    }
}
