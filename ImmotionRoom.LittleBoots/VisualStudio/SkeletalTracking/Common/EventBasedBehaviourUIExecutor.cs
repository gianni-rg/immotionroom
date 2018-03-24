using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.Common
{
    /// <summary>
    /// Offers classes that do not inherit from EventBasedMonoBehaviour the ability to execute code on the UI thread
    /// </summary>
    public class EventBasedBehaviourUIExecutor : EventBasedMonoBehaviour
    {
        #region Singleton-like implementation

        /// <summary>
        /// Singleton instance
        /// </summary>
        private static EventBasedBehaviourUIExecutor m_instance = null;

        /// <summary>
        /// Gets a running instance of the EventBasedBehaviourUIExecutor
        /// </summary>
        /// <returns>Instance of the EventBasedBehaviourUIExecutor</returns>
        public static EventBasedBehaviourUIExecutor Instance
        {
            get
            {
                if (m_instance == null)
                {
                    GameObject instanceGo = new GameObject();
                    instanceGo.name = "EventBasedBehaviourUIExecutor";
                    m_instance = instanceGo.AddComponent<EventBasedBehaviourUIExecutor>();
                }

                return m_instance;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Executes a delegate on Unity UI Thread
        /// </summary>
        /// <param name="action">Action method to execute</param>
        public void ExecuteOnUIThread(Action action)
        {
            ExecuteOnUI(action);
        }

        #endregion
    }
}
