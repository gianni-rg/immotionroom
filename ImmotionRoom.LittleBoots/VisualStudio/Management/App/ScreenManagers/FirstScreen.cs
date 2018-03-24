namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.ScreenManagers
{
    using UnityEngine;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedManager;
    using UnityEngine.UI;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils;

    /// <summary>
    /// Manages FirstScreen scene behaviour
    /// </summary>
    public class FirstScreen : MonoBehaviour
    {
        void Start()
        {
            //disable button to go to skeleton scene if we failed to connect or if we don't have a master data source or the system is not calibrated(we can't show bodies if the system is not connected & configured!)
            if (!TrackingServiceManagerAdvanced.Instance.IsConnected || TrackingServiceManagerAdvanced.Instance.TrackingServiceInfo.MasterDataSourceID == null
                || TrackingServiceManagerAdvanced.Instance.TrackingServiceInfo.IsCalibrated == false)
                GameObject.Find("Skeletons Scene Button").GetComponent<Button>().interactable = false;
        }

        /// <summary>
        /// Triggered when the main menu button gets clicked
        /// </summary>
        public void OnMainMenuButtonClicked()
        {
            ScenesManager.Instance.GoToScene("MainMenu");
        }

        /// <summary>
        /// Triggered when the "go to Skeletons Scene" button gets clicked
        /// </summary>
        public void OnSkeletonsSceneButtonClicked()
        {
            ScenesManager.Instance.GoToScene("Merging");
        }

        /// <summary>
        /// Triggered when the configuration wizard button gets clicked
        /// </summary>
        public void OnConfigurationWizardButtonClicked()
        {
            ScenesManager.Instance.StartWizard("NetworkReconfig");
        }
    }

}