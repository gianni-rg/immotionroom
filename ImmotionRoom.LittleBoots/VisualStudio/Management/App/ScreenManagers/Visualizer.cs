namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.App.ScreenManagers
{
    using UnityEngine;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedManager;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using UnityEngine.UI;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedAvateering;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataSourcesManagement;
    using ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Tools;
    using ImmotionAR.ImmotionRoom.LittleBoots.Management.App.Utils._3dparties;

    /// <summary>
    /// Manages Visualizer scene behaviour
    /// </summary>
    public partial class Visualizer : MonoBehaviour
    {
        #region Public Unity Properties

        /// <summary>
        /// The global visualization panel in the screen (the panel inside which all child panels showing all cameras view will be added as children)
        /// </summary>
        [Tooltip("Visualization panels root")]
        public AutoGridLayout VisualizationPanel;

        /// <summary>
        /// The child panel used to show the visualization of a single data source
        /// </summary>
        [Tooltip("The child panel used to show the visualization of a single data source")]
        public GameObject CameraChildPanel;

        /// <summary>
        /// Object to be used to show the different skeletons during the calibration process
        /// </summary>
        [Tooltip("Object to be used to show the different skeletons during the calibration process")]
        public BodiesSkeletalsManagerAdvanced BodyDrawerPrefab;

        /// <summary>
        /// Joints material to draw skeletal joints with
        /// </summary>
        [Tooltip("Joints material to draw skeletal joints with")]
        public Material SkeletalJointsMaterial;

        /// <summary>
        /// Bones material to draw skeletal bones with
        /// </summary>
        [Tooltip("Bones material to draw skeletal bones with")]
        public Material SkeletalBonesMaterial;

        #endregion

        #region Private Fields

        /// <summary>
        /// Internal implementation of the class
        /// </summary>
        private VisualizerInternal m_internalImplementation;

        #endregion

        #region Behaviour methods

        void Awake()
        {
            m_internalImplementation = new VisualizerInternal(this);
        }

        void Start()
        {
            m_internalImplementation.Start();
        }

        void OnDestroy()
        {
            m_internalImplementation.OnDestroy();
        }

        //uncomment for debugging purposes
        //int inc_val = 0;
        //internal void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.A))
        //        OnDataSourceToggleValueChanged("a" + (inc_val++), true);
        //    else if (Input.GetKeyDown(KeyCode.B))
        //        OnDataSourceToggleValueChanged("a" + (inc_val--), false);
        //}

        #endregion

        #region Buttons events methods

        /// <summary>
        /// Triggered when the OK button gets clicked
        /// </summary>
        public void OnOkButtonClicked()
        {
            m_internalImplementation.OnOkButtonClicked();
        }

        /// <summary>
        /// Callback called each time one of the toggle button of the data sources changes its status
        /// </summary>
        /// <param name="dataSourceName">Label of the toggle button (corresponds to a Data Source name)</param>
        /// <param name="newStatus">New status of the toggle</param>
        private void OnDataSourceToggleValueChanged(string dataSourceName, bool newStatus)
        {
            m_internalImplementation.OnDataSourceToggleValueChanged(dataSourceName, newStatus);
        }

        #endregion

    }

}