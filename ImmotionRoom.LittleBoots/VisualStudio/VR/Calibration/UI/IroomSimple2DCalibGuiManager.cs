namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Calibration.UI
{
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.SupportStruct;
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.Localization;
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.Localization.UI;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Manages the graphical interface of the standard Iroom/headset calibration GUI
    /// </summary>
    public partial class IroomSimple2DCalibGuiManager : MonoBehaviour, 
        IRoomCalibGuiManager
    {
        #region Public Unity Properties

        /// <summary>
        /// Camera used to display the GUI. If it is null, the object will be considered to be rendered using main camera
        /// </summary>
        [Tooltip("Camera used to display the GUI. It must have an appropriate depth value, so it writes its contents above the other game camera. If it is null, the object will be considered to be rendered using main camera")]
        public Camera GuiCamera;

        /// <summary>
        /// Root game object of all calibration stages panels
        /// </summary>
        [Tooltip("Root game object of all calibration stages panels")]
        public GameObject PanelRoot;

        /// <summary>
        /// Additional background image panel, useful if GuiCamera is null, to set the correct background of calibration UI
        /// </summary>
        [Tooltip("Additional background image panel, useful if GuiCamera is null, to set the correct background of calibration UI")]
        public Image PanelBackground;

        /// <summary>
        /// Prefab used to display the tracking data sources in "waiting for calibrating body" stage
        /// </summary>
        [Tooltip("Prefab used to display the tracking data sources in \"waiting for calibrating body\" stage")]
        public GameObject PallinPrefab;

        /// <summary>
        /// Sprite image used to signal a non tracking data source during waiting stage
        /// </summary>
        [Tooltip("Sprite image used to signal an error or something wrong in each stage")]
        public Sprite NoSprite;

        /// <summary>
        /// Sprite image used to signal a tracking data source during waiting stage
        /// </summary>
        [Tooltip("Sprite image used to signal a successful operation in each stage")]
        public Sprite OkSprite;

        /// <summary>
        /// Sprite image used to signal to rotate towards left in body rotation stage
        /// </summary>
        [Tooltip("Sprite image used to signal to rotate towards left in body rotation stage")]
        public Sprite DirectionLeftSprite;

        /// <summary>
        /// Sprite image used to signal to rotate towards right in body rotation stage
        /// </summary>
        [Tooltip("Sprite image used to signal to rotate towards right in body rotation stage")]
        public Sprite DirectionRightSprite;

        #endregion

        #region Private Fields

        /// <summary>
        /// Internal implementation of the class
        /// </summary>
        private IroomSimple2DCalibGuiManagerInternal m_internalImplementation;

        #endregion

        #region Behaviour methods

        void Awake()
        {
            m_internalImplementation = new IroomSimple2DCalibGuiManagerInternal(this);
        }

        void Start()
        {
            m_internalImplementation.Start();
        }

        #endregion

        #region IRoomCalibGuiManager Members

        /// <summary>
        /// Resets the calibration GUI used by this instance
        /// </summary>
        public void ResetGui()
        {
            m_internalImplementation.ResetGui();
        }

        /// <summary>
        /// Asks the manager to show the gui
        /// </summary>
        public void Activate()
        {
            m_internalImplementation.Activate();
        }

        /// <summary>
        /// Asks the manager to hide the gui
        /// </summary>
        public void Deactivate()
        {
            m_internalImplementation.Deactivate();
        }

        /// <summary>
        /// Informs the gui manager that the calibration process has entered a new status
        /// </summary>
        /// <param name="newCalibrationStatus">New calibration status</param>
        public void GoToStatus(IroomCalibratorStatus newCalibrationStatus)
        {
            m_internalImplementation.GoToStatus(newCalibrationStatus);
        }

        /// <summary>
        /// Asks the manager to show the user that he's performing very well
        /// </summary>
        public void ShowVeryGoodMessage()
        {
            m_internalImplementation.ShowVeryGoodMessage();
        }

        /// <summary>
        /// Informs the manager that a process that is going on inside current state and that can be represented by a progress bar,
        /// has reached a certain progress value
        /// </summary>
        /// <param name="progressValue">New progress value in range [0, 1]</param>
        public void ShowCurrentStateProgressBarValue(float progressValue)
        {
            m_internalImplementation.ShowCurrentStateProgressBarValue(progressValue);
        }

        /// <summary>
        /// Asks the manager to initialize all the gui for "waiting for a calibrating user" stage, using the provided info.
        /// </summary>
        /// <param name="trackingEnvironment">Tracking environment inside which tracking happens</param>
        public void InitWaitingBodyTrackingInfo(TrackingServiceEnv trackingEnvironment)
        {
            m_internalImplementation.InitWaitingBodyTrackingInfo(trackingEnvironment);
        }

        /// <summary>
        /// Informs the manager about the most probable user body during "waiting for a calibrating user" stage.
        /// </summary>
        /// <param name="body">Body of the most tracked user</param>
        /// <param name="totalTrackingBoxes">Number of tracking boxes required to track the user</param>
        public void ShowWaitingBodyTrackingInfo(TrackingServiceBodyData body, int totalTrackingBoxes)
        {
            m_internalImplementation.ShowWaitingBodyTrackingInfo(body, totalTrackingBoxes);
        }

        /// <summary>
        /// Informs the manager about the orientation that the user should face during the "rotate to origin" stage.
        /// </summary>
        /// <param name="direction">0 if player has correct orientation, positive value if he has to turn left, negative value if he has to turn right</param>
        public void ShowCalibratingBodyOrientationsDir(float direction)
        {
            m_internalImplementation.ShowCalibratingBodyOrientationsDir(direction);
        }

        /// <summary>
        /// Make the gui to fade away after the process has completed, so the game scene can be displayed
        /// </summary>
        /// <param name="fadingPercent">Percent of fading in the range [0, 1], where 0 is full opaque and 1 is full transparent</param>
        public void MakeGuiFade(float fadingPercent)
        {
            m_internalImplementation.MakeGuiFade(fadingPercent);
        }

        #endregion
    }
}

