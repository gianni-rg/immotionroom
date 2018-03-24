namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils
{
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using UnityEngine.Analytics;

    /// <summary>
    /// Manages analytics function of this program
    /// </summary>
    public partial class AnalyticsManager : MonoBehaviour
    {

        /// <summary>
        /// Contains the actual definition of the AnalyticsManager, for obfuscation purposes
        /// </summary>
        private class AnalyticsManagerInternal
        {
            #region Constant definitions

            /// <summary>
            /// Constant of event name regarding entering in a new game scene
            /// </summary>
            const string SceneEnteringEventName = "SceneEnter";

            /// <summary>
            /// Constant of event name regarding entering in a new game scene
            /// </summary>
            const string SceneLeavingEventName = "SceneExit";

            /// <summary>
            /// Constant of event name regarding entering in a new game scene
            /// </summary>
            const string SceneNameAttributeName = "SceneName";

            /// <summary>
            /// Constant of event name regarding entering in a new game scene
            /// </summary>
            const string SceneTimeAttributeName = "SceneTime";

            /// <summary>
            /// Constant of event name regarding entering in wizard mode
            /// </summary>
            const string WizardStartedEventName = "WizardStart";

            #endregion

            #region Private fields

            /// <summary>
            /// Name of current scene (the last name passed to SceneEnter)
            /// </summary>
            private string m_currentSceneName;

            /// <summary>
            /// Time spent from last call to SceneEnter
            /// </summary>
            private float m_currentSceneTime = 0;

            #endregion

            #region Behaviour methods

            internal void Awake()
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("AnalyticsManager - Awaken", m_currentSceneName, m_currentSceneTime);
                }
            }

            internal void OnDestroy()
            {
                OnDestroyPrivate();
            }

            internal void Update()
            {
                UpdatePrivate();
            }

            #endregion

            #region Usage analytics methods

            /// <summary>
            /// Signal the analytics manager we've entered in a new game scene, exiting from current one
            /// </summary>
            /// <param name="sceneName">Name of the scene we're entering into. If it is null, we're entering no scene (i.e. exiting the program)</param>
            internal void SceneEnter(string sceneName)
            {
                SceneEnterPrivate(sceneName);
            }

            /// <summary>
            /// Signal the analytics manager we've entered in configuration wizard mode
            /// </summary>
            internal void ConfigurationWizardStarted()
            {
                ConfigurationWizardStartedPrivate();
            }

            #endregion

            #region Private methods

            private void OnDestroyPrivate()
            {
                SceneEnter(null); //signal we're exiting from current scene (if we're here, the program is surely quitting)
            }

            private void UpdatePrivate()
            {
                //increment current scene time, if any
                if (m_currentSceneName != null)
                    m_currentSceneTime += Time.deltaTime;
            }

            /// <summary>
            /// Signal the analytics manager we've entered in a new game scene, exiting from current one
            /// </summary>
            /// <param name="sceneName">Name of the scene we're entering into. If it is null, we're entering no scene (i.e. exiting the program)</param>
            private void SceneEnterPrivate(string sceneName)
            {
                //communicate we're exiting from current scene, if any
                if (m_currentSceneName != null)
                {
                    Analytics.CustomEvent(SceneLeavingEventName, new Dictionary<string, object>()
                    {
                        {SceneNameAttributeName, m_currentSceneName},
                        {SceneTimeAttributeName, m_currentSceneTime}
                    });

                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("AnalyticsManager - Exiting from scene {0}, after {1} seconds", m_currentSceneName, m_currentSceneTime);
                    }
                }

                //set new scene data
                m_currentSceneName = sceneName;
                m_currentSceneTime = 0;

                //communicate we're starting a new scene, if any
                if (sceneName != null)
                {
                    Analytics.CustomEvent(SceneEnteringEventName, new Dictionary<string, object>()
                    {
                        {SceneNameAttributeName, sceneName}
                    });


                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("AnalyticsManager - Entering new scene {0}", sceneName);
                    }
                }
            }

            /// <summary>
            /// Signal the analytics manager we've entered in configuration wizard mode
            /// </summary>
            private void ConfigurationWizardStartedPrivate()
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("AnalyticsManager - Configuration Wizard started");
                }

                Analytics.CustomEvent(WizardStartedEventName, null);
            }

            #endregion
        }

    }

}