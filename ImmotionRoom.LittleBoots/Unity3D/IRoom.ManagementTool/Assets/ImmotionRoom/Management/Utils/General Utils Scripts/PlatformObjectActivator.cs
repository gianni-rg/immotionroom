namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.Utils
{
    using UnityEngine;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedManager;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using UnityEngine.UI;

    /// <summary>
    /// Activates/Deactivates game objects depending on the current platform
    /// </summary>
    public class PlatformObjectActivator : MonoBehaviour
    {
        /// <summary>
        /// Objects to be activated for mobile platforms
        /// </summary>
        [Tooltip("Objects to be activated for mobile platforms")]
        public GameObject[] MobileObjects;

        /// <summary>
        /// Objects to be activated for standalone platforms
        /// </summary>
        [Tooltip("Objects to be activated for standalone platforms (e.g. PC)")]
        public GameObject[] PcObjects;

        void Awake()
        {
#if UNITY_ANDROID
            foreach (GameObject go in MobileObjects)
                go.SetActive(true);

            foreach (GameObject go in PcObjects)
                go.SetActive(false);
#else
            foreach (GameObject go in MobileObjects)
                go.SetActive(false);

            foreach (GameObject go in PcObjects)
                go.SetActive(true);
#endif
        }

    }

}
