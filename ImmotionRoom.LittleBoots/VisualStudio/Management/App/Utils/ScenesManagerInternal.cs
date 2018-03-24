namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.Common;
    using UnityEngine.Events;

    /// <summary>
    /// Manages scenes transitions inside the Management Tool
    /// </summary>
    public partial class ScenesManager : MonoBehaviour
    {

        /// <summary>
        /// Contains the actual definition of the ScenesManager, for obfuscation purposes
        /// </summary>
        private class ScenesManagerInternal
        {            
            #region Private fields

            /// <summary>
            /// Stack of scene navigated by the player.
            /// Element 0 is the TOS
            /// </summary>
            private List<string> m_scenesStack;

            /// <summary>
            /// -1 if we are not in Configuration Wizard mode;
            /// Otherwise set the number of scenes present in the stack when the wizard mode was started
            /// </summary>
            private int m_wizardStackCount = -1;

            /// <summary>
            /// True if back button use is currently allowed, false otherwise
            /// </summary>
            private bool m_backButtonAllowed = true;

            /// <summary>
            /// Callback to be called when the back button gets pressed
            /// </summary>
            private BackButtonCallback m_backButtonDelegate;

            #endregion

            #region Behaviour lifetime methods

            internal void Awake()
            {
                m_scenesStack = new List<string>();

                //push current scene onto the stack
                PushCurrentSceneOntoStack();

                //init back button callback to return to previous scene
                m_backButtonDelegate = (obj) => { PopScene(); };
            }

            internal void Update()
            {
                //pop to previous scene if pressed ESC (back button on Android), or BACKSPACE, but only if it is allowed to use back button
                if (m_backButtonDelegate != null && m_backButtonAllowed && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace)))
                    m_backButtonDelegate(new object());
            }

            #endregion

            #region Private Methods

            /// <summary>
            /// Asks Unity to load a new scene (wraps unity call).
            /// Communicate the analytics manager the scene change.
            /// Restore back button behaviour to Return to previous scene
            /// </summary>
            /// <param name="sceneName">Name of the scene to be loaded</param>
            private void LoadScene(string sceneName)
            {
                m_backButtonDelegate = (obj) => { PopScene(); }; //init back button callback to return to previous scene
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
                AnalyticsManager.Instance.SceneEnter(sceneName);
            }

            /// <summary>
            /// Push current screen onto the scenes stack
            /// </summary>
            private void PushCurrentSceneOntoStack()
            {
                m_scenesStack.Insert(0, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                AnalyticsManager.Instance.SceneEnter(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }

            /// <summary>
            /// Push a scene name onto the scenes stack
            /// </summary>
            /// <param name="sceneName">Scene name to push</param>
            private void PushSceneOntoStack(string sceneName)
            {
                m_scenesStack.Insert(0, sceneName);
            }

            /// <summary>
            /// Pop scene name onto the top of the stack
            /// </summary>
            /// <returns>element popped from the tos or null if the stack is empty</returns>
            private string PopSceneFromStack()
            {
                if (m_scenesStack.Count == 0)
                {
                    return null;
                }
                else
                {
                    string tos = m_scenesStack[0];
                    m_scenesStack.RemoveAt(0);

                    return tos;
                }
            }

            /// <summary>
            /// Gets the string onto the top of the scenes stack
            /// </summary>
            /// <returns>The scenes onto the top of the scenes stack, or null if the stack is empty</returns>
            private string GetTosSceneInStack()
            {
                if (m_scenesStack.Count == 0)
                {
                    return null;
                }
                else
                {
                    return m_scenesStack[0];
                }
            }

            #endregion

            #region Internal Scene-management Methods

            /// <summary>
            /// Get if current scene is the root scene
            /// </summary>
            /// <returns>True if it is the root scene (it has no father); false otherwise</returns>
            internal bool IsRoot()
            {
                return m_scenesStack.Count == 1;
            }

            /// <summary>
            /// Makes the app to change the current active scene.
            /// The new scene name gets pushed onto the stack
            /// </summary>
            /// <param name="sceneName">Scene to switch to</param>
            internal void GoToScene(string sceneName)
            {
                PushSceneOntoStack(sceneName);
                LoadScene(sceneName);
            }

            /// <summary>
            /// Makes the app to change the current active scene.
            /// The new scene name gets pushed onto the stack, while the current one DOES NOT get recorded
            /// </summary>
            /// <param name="sceneName">Scene to switch to</param>
            internal void GoToSceneAndForget(string sceneName)
            {
                PopSceneFromStack();
                PushSceneOntoStack(sceneName);
                LoadScene(sceneName);
            }

            /// <summary>
            /// Makes the app to return to the previous scene, that will be loaded from scratch
            /// The old scene name gets popped from the stack
            /// </summary>
            internal void PopScene()
            {
                //pop a scene
                PopSceneFromStack();

                //gets scenes in top of the stack
                string tosScene = GetTosSceneInStack();

                //if the popped one was the last scene in the stack, exit the program
                if (tosScene == null)
                    Application.Quit();
                //else, return to previous screen
                else
                {
                    //if we are in wizard mode and have popped until the exit from the wizard mode, exit it
                    if (m_wizardStackCount != -1 && m_scenesStack.Count <= m_wizardStackCount)
                        m_wizardStackCount = -1;

                    LoadScene(tosScene);
                }
            }

            #endregion

            #region Internal Configuration-wizard Methods

            /// <summary>
            /// If the app is not in wizard mode, pop current scene and return to previous;
            //  Else, if we are during a wizard setup, go to the provided scene
            /// </summary>
            /// <param name="sceneName">Scene to switch to, if we are in wizard mode</param>
            internal void PopOrNextInWizard(string sceneName)
            {
                //if we are not in wizard mode, pop current scene and return to previous;
                //else, go to the provided scene
                if (m_wizardStackCount == -1)
                    PopScene();
                else
                    GoToScene(sceneName);
            }

            /// <summary>
            /// Makes the app enter to a scene, to start a wizard configuration.
            /// Notifies the Analytics Manager about this new mode
            /// </summary>
            /// <param name="sceneName">Scene to switch to, to start wizard mode</param>
            internal void StartWizard(string sceneName)
            {
                AnalyticsManager.Instance.ConfigurationWizardStarted();
                m_wizardStackCount = m_scenesStack.Count;
                GoToScene(sceneName);
            }

            /// <summary>
            /// Stops wizard mode, popping out from the stack all scenes until we return to initial one from which we started the wizard.
            /// If we are not in wizard mode, counts as a single pop
            /// </summary>
            internal void StopWizard()
            {
                //if we are not in wizard mode, simply pop
                if (m_wizardStackCount == -1)
                    PopScene();
                //else, if we are in wizard mode, rollback to the beginning
                else
                {
                    //remove all wizard scenes
                    while (m_wizardStackCount < m_scenesStack.Count)
                        PopSceneFromStack();

                    //reset wizard mode
                    m_wizardStackCount = -1;

                    //re-load the pre-wizard scene
                    //(we use this syntax because GoToScene performs a Push operation)
                    GoToScene(PopSceneFromStack());
                }
            }

            #endregion

            #region Back Button Management methods

            /// <summary>
            /// Set new back button state: true if back button is allowed to go to previous scene, false otherwise.
            /// </summary>
            /// <param name="state">New back button allowance state</param>
            internal void SetBackButtonEnabledState(bool state)
            {
                m_backButtonAllowed = state;
            }

            /// <summary>
            /// Sets the method to be called when the back button gets pressed.
            /// The default value is PopScene(), i.e. the program returns to previous scene.
            /// Notice that this behaviour gets reset when a new scene gets loaded
            /// </summary>
            /// <param name="callback">The callback that has to be executed when back button gets pressed</param>
            internal void SetBackButtonBehaviour(BackButtonCallback callback)
            {
                m_backButtonDelegate = callback;
            }

            #endregion
        }

    }

}
