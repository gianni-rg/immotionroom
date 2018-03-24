namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine.Analytics;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.Common;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;

    /// <summary>
    /// Manages analytics function of this program
    /// </summary>
    public partial class AnalyticsManager : MonoBehaviour
    {
        #region Private data

        /// <summary>
        /// Actual object performing the operation, for obfuscation purposes
        /// </summary>
        AnalyticsManagerInternal m_internalImplementation;

        #endregion

        #region Behaviour methods

        void Awake()
        {
            m_internalImplementation = new AnalyticsManagerInternal();
            m_internalImplementation.Awake();
        }

        void OnDestroy()
        {
            m_internalImplementation.OnDestroy();
        }

        void Update()
        {
            m_internalImplementation.Update();
        }

        #endregion

        #region Usage analytics methods

        /// <summary>
        /// Signal the analytics manager we've entered in a new game scene, exiting from current one
        /// </summary>
        /// <param name="sceneName">Name of the scene we're entering into. If it is null, we're entering no scene (i.e. exiting the program)</param>
        public void SceneEnter(string sceneName)
        {
            m_internalImplementation.SceneEnter(sceneName);
        }

        /// <summary>
        /// Signal the analytics manager we've entered in configuration wizard mode
        /// </summary>
        public void ConfigurationWizardStarted()
        {
            m_internalImplementation.ConfigurationWizardStarted();
        }

        #endregion

        //TODO: TUTTE QUESTE IMPLEMENTAZIONI DELL'ANALYTICS MANAGER SONO SBAGLIATE... DOVRESTI SOSTITUIRLE CON LE STESSE DEL
        //TRACKINGSERVICEMANAGER

        #region Singleton-like implementation

        /// <summary>
        /// Gets the first running instance of the Analytics Manager. If it does not exists, creates an instance of the 
        /// <see cref="AnalyticsManager"/> class
        /// </summary>
        /// <returns>Instance of the AnalyticsManager</returns>
        public static AnalyticsManager Instance
        {
            get
            {
                // Search an object of type AnalyticsManager. If we find it, return it.
                // Otherwise, let's create a new gameobject, add a AnalyticsManager to it and return it.
                var instance = FindObjectOfType<AnalyticsManager>();

                if (instance != null)
                {
                    return instance;
                }

                var instanceGo = new GameObject();
                instanceGo.name = "Analytics Manager";
                instanceGo.AddComponent<DoNotDestroy>(); //this object should persist for the whole program

                instance = instanceGo.AddComponent<AnalyticsManager>();

                return instance;
            }
        }

        #endregion
    }

}
