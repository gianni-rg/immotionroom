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
        #region Delegate definitions

        /// <summary>
        /// Delegate definition for callback to be called when the back button gets pressed
        /// </summary>
        /// <param name="param">Generic params of the method. Currently not used.</param>
        public delegate void BackButtonCallback(object param);

        #endregion

        #region Private Fields

        /// <summary>
        /// Internal implementation of the class
        /// </summary>
        ScenesManagerInternal m_internalImplementation;

        #endregion

        #region Behaviour lifetime methods

        void Awake()
        {
            m_internalImplementation = new ScenesManagerInternal();
            m_internalImplementation.Awake();
        }

        void Update()
        {
            m_internalImplementation.Update();
        }

        #endregion

        #region Public Scene-management Methods
        
        /// <summary>
        /// Get if current scene is the root scene
        /// </summary>
        /// <returns>True if it is the root scene (it has no father); false otherwise</returns>
        public bool IsRoot()
        {
            return m_internalImplementation.IsRoot();
        }

        /// <summary>
        /// Makes the app to change the current active scene.
        /// The new scene name gets pushed onto the stack
        /// </summary>
        /// <param name="sceneName">Scene to switch to</param>
        public void GoToScene(string sceneName)
        {
            m_internalImplementation.GoToScene(sceneName);
        }

        /// <summary>
        /// Makes the app to change the current active scene.
        /// The new scene name gets pushed onto the stack, while the current one DOES NOT get recorded
        /// </summary>
        /// <param name="sceneName">Scene to switch to</param>
        public void GoToSceneAndForget(string sceneName)
        {
            m_internalImplementation.GoToSceneAndForget(sceneName);
        }

        /// <summary>
        /// Makes the app to return to the previous scene, that will be loaded from scratch
        /// The old scene name gets popped from the stack
        /// </summary>
        public void PopScene()
        {
            m_internalImplementation.PopScene();
        }

        #endregion

        #region Public Configuration-wizard Methods

        /// <summary>
        /// If the app is not in wizard mode, pop current scene and return to previous;
        //  Else, if we are during a wizard setup, go to the provided scene
        /// </summary>
        /// <param name="sceneName">Scene to switch to, if we are in wizard mode</param>
        public void PopOrNextInWizard(string sceneName)
        {
            m_internalImplementation.PopOrNextInWizard(sceneName);
        }

        /// <summary>
        /// Makes the app enter to a scene, to start a wizard configuration.
        /// Notifies the Analytics Manager about this new mode
        /// </summary>
        /// <param name="sceneName">Scene to switch to, to start wizard mode</param>
        public void StartWizard(string sceneName)
        {
            m_internalImplementation.StartWizard(sceneName);
        }

        /// <summary>
        /// Stops wizard mode, popping out from the stack all scenes until we return to initial one from which we started the wizard.
        /// If we are not in wizard mode, counts as a single pop
        /// </summary>
        public void StopWizard()
        {
            m_internalImplementation.StopWizard();
        }

        #endregion

        #region Back Button Management methods

        /// <summary>
        /// Set new back button state: true if back button is allowed to go to previous scene, false otherwise.
        /// </summary>
        /// <param name="state">New back button allowance state</param>
        public void SetBackButtonEnabledState(bool state)
        {
            m_internalImplementation.SetBackButtonEnabledState(state);
        }

        /// <summary>
        /// Sets the method to be called when the back button gets pressed.
        /// The default value is PopScene(), i.e. the program returns to previous scene.
        /// Notice that this behaviour gets reset when a new scene gets loaded
        /// </summary>
        /// <param name="callback">The callback that has to be executed when back button gets pressed</param>
        public void SetBackButtonBehaviour(BackButtonCallback callback)
        {
            m_internalImplementation.SetBackButtonBehaviour(callback);
        }

        #endregion

        #region Singleton-like implementation

        /// <summary>
        /// Gets the first running instance of the ScenesManager. If it does not exists, creates an instance of the 
        /// <see cref="ScenesManager"/> class
        /// </summary>
        /// <returns>Instance of the ScenesManager</returns>
        public static ScenesManager Instance
        {
            get
            {
                // Search an object of type ScenesManager. If we find it, return it.
                // Otherwise, let's create a new gameobject, add a ScenesManager to it and return it.
                var instance = FindObjectOfType<ScenesManager>();

                if (instance != null)
                {
                    return instance;
                }

                var instanceGo = new GameObject();
                instanceGo.name = "Scenes Manager";
                instanceGo.AddComponent<DoNotDestroy>(); //this object should persist for the whole program

                instance = instanceGo.AddComponent<ScenesManager>();
      
                return instance;
            }
        }

        #endregion
    }

}
