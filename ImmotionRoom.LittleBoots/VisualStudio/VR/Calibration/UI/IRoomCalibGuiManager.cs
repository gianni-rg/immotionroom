namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Calibration.UI
{
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.SupportStruct;
    using ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Interface for generic managers of graphical interfaces for Iroom/headset calibration GUI
    /// </summary>
    public interface IRoomCalibGuiManager
    {
        /// <summary>
        /// Resets the calibration GUI used by this instance
        /// </summary>
        void ResetGui();

        /// <summary>
        /// Asks the manager to show the gui
        /// </summary>
        void Activate();

        /// <summary>
        /// Asks the manager to hide the gui
        /// </summary>
        void Deactivate();

        /// <summary>
        /// Informs the gui manager that the calibration process has entered a new status
        /// </summary>
        /// <param name="newCalibrationStatus">New calibration status</param>
        void GoToStatus(IroomCalibratorStatus newCalibrationStatus);

        /// <summary>
        /// Asks the manager to show the user that he's performing very well
        /// </summary>
        void ShowVeryGoodMessage();

        /// <summary>
        /// Informs the manager that a process that is going on inside current state and that can be represented by a progress bar,
        /// has reached a certain progress value
        /// </summary>
        /// <param name="progressValue">New progress value in range [0, 1]</param>
        void ShowCurrentStateProgressBarValue(float progressValue);

        /// <summary>
        /// Asks the manager to initialize all the gui for "waiting for a calibrating user" stage, using the provided info.
        /// </summary>
        /// <param name="trackingEnvironment">Tracking environment inside which tracking happens</param>
        void InitWaitingBodyTrackingInfo(TrackingServiceEnv trackingEnvironment);
    
        /// <summary>
        /// Informs the manager about the most probable user body during "waiting for a calibrating user" stage.
        /// </summary>
        /// <param name="body">Body of the most tracked user</param>
        /// <param name="totalTrackingBoxes">Number of tracking boxes required to track the user</param>
        void ShowWaitingBodyTrackingInfo(TrackingServiceBodyData body, int totalTrackingBoxes);

        /// <summary>
        /// Informs the manager about the orientation that the user should face during the "rotate to origin" stage.
        /// </summary>
        /// <param name="direction">0 if player has correct orientation, positive value if he has to turn right, negative value if he has to turn left</param>
        void ShowCalibratingBodyOrientationsDir(float direction);

        /// <summary>
        /// Make the gui to fade away after the process has completed, so the game scene can be displayed
        /// </summary>
        /// <param name="fadingPercent">Percent of fading in the range [0, 1], where 0 is full opaque and 1 is full transparent</param>
        void MakeGuiFade(float fadingPercent);
    }
}
