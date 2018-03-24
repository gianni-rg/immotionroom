namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.ScreenManagers
{
    using UnityEngine;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedManager;
    using UnityEngine.UI;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils;

    /// <summary>
    /// Manages MainMenu scene behaviour
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        void Start()
        {
            //if we failed to connect disable all buttons except the one to perform network discovery & reconfig
            //(we can't do anything if the system is not connected & configured!)
            if (!TrackingServiceManagerAdvanced.Instance.IsConnected)
            {
                GameObject.Find("Data Source View Button").GetComponent<Button>().interactable = false;
                GameObject.Find("Calibrate Button").GetComponent<Button>().interactable = false;
                GameObject.Find("Skeleton Scene Button").GetComponent<Button>().interactable = false;
                GameObject.Find("Girello Scene Button").GetComponent<Button>().interactable = false;
                GameObject.Find("Set Master Button").GetComponent<Button>().interactable = false;
                GameObject.Find("Reboot Button").GetComponent<Button>().interactable = false;
                GameObject.Find("System Info button").GetComponent<Button>().interactable = false;    
            }
            //else, if we are connected, but we have not a master data source, disable all buttons that serve to perfom some kind of tracking
            //(we can't calibrate or get skeletons without a master data source)
            else if (TrackingServiceManagerAdvanced.Instance.TrackingServiceInfo.MasterDataSourceID == null)
            {
                GameObject.Find("Calibrate Button").GetComponent<Button>().interactable = false;
                GameObject.Find("Skeleton Scene Button").GetComponent<Button>().interactable = false;
                GameObject.Find("Girello Scene Button").GetComponent<Button>().interactable = false;
            }
            //else, if we are connected, but not calibrated, disable all tracking functionalities
            else if (TrackingServiceManagerAdvanced.Instance.TrackingServiceInfo.IsCalibrated == false)
            {
                GameObject.Find("Skeleton Scene Button").GetComponent<Button>().interactable = false;
                GameObject.Find("Girello Scene Button").GetComponent<Button>().interactable = false;
            }
                
        }

        /// <summary>
        /// Triggered when the configuration wizard button gets clicked
        /// </summary>
        public void OnConfigurationWizardButtonClicked()
        {
            ScenesManager.Instance.StartWizard("NetworkReconfig");
        }

        /// <summary>
        /// Triggered when the "data sources visualization" button gets clicked
        /// </summary>
        public void OnDataSourcesVisualizationButtonClicked()
        {
            ScenesManager.Instance.GoToScene("VisualizerScene");
        }

        /// <summary>
        /// Triggered when the "calibration" button gets clicked
        /// </summary>
        public void OnCalibrationButtonClicked()
        {
            ScenesManager.Instance.GoToScene("Calibration");
        }

        /// <summary>
        /// Triggered when the "go to Skeletons Scene" button gets clicked
        /// </summary>
        public void OnSkeletonsSceneButtonClicked()
        {
            ScenesManager.Instance.GoToScene("Merging");
        }

        /// <summary>
        /// Triggered when the "girello configuration" button gets clicked
        /// </summary>
        public void OnGirelloConfigurationButtonClicked()
        {
            ScenesManager.Instance.GoToScene("GirelloConfiguration");
        }

        /// <summary>
        /// Triggered when the "system reconfiguration" button gets clicked
        /// </summary>
        public void OnSystemReconfigurationButtonClicked()
        {
            ScenesManager.Instance.GoToScene("NetworkReconfig");
        }

        /// <summary>
        /// Triggered when the "set master data source" button gets clicked
        /// </summary>
        public void OnSetMasterDataSourceButtonClicked()
        {
            ScenesManager.Instance.GoToScene("MasterSetting");
        }

        /// <summary>
        /// Triggered when the "reboot data source" button gets clicked
        /// </summary>
        public void OnRebootDataSourceButtonClicked()
        {
            ScenesManager.Instance.GoToScene("Reboot");
        }

        /// <summary>
        /// Triggered when the "system info" button gets clicked
        /// </summary>
        public void OnSystemInfoButtonClicked()
        {
            ScenesManager.Instance.GoToScene("SystemInfo");
        }
  
    }

}