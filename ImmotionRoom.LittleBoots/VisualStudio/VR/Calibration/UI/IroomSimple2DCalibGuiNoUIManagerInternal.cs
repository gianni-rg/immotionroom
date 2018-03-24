namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Calibration.UI
{
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.SupportStruct;
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.Localization;
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.Localization.Graphics;
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
    public partial class IroomSimple2DCalibGuiNoUIManager : MonoBehaviour,
        IRoomCalibGuiManager
    {
        /// <summary>
        /// Contains the actual implementation of the IroomSimple2DCalibGuiNoUIManager, for obfuscation purposes
        /// </summary>
        private class IroomSimple2DCalibGuiNoUIManagerInternal
        {

            #region Constants definition

            /// <summary>
            /// Different background colors for different states
            /// </summary>
            private static readonly Dictionary<IroomCalibratorStatus, Color> CanvasColors = new Dictionary<IroomCalibratorStatus, Color>()
            {
                {IroomCalibratorStatus.None,                                new Color(00f / 255, 000f / 255, 000f / 255, 255f / 255)},
                {IroomCalibratorStatus.WaitingForBody,                      new Color(33f / 255, 150f / 255, 255f / 255, 255f / 255)},
                {IroomCalibratorStatus.RotatingBodyToOrigin,                new Color(33f / 255, 180f / 255, 205f / 255, 255f / 255)},
                //{IroomCalibratorStatus.BodyStandingStill,                   new Color(33f / 255, 255f / 255, 200f / 255, 255f / 255)},
                {IroomCalibratorStatus.BodyStandingStill,                   new Color(33f / 255, 200f / 255, 140f / 255, 255f / 255)},
                {IroomCalibratorStatus.InitializingWithBody,                new Color(33f / 255, 200f / 255, 140f / 255, 255f / 255)},
                {IroomCalibratorStatus.Calibrating,                         new Color(33f / 255, 200f / 255, 140f / 255, 255f / 255)},
                {IroomCalibratorStatus.Done,                                new Color(33f / 255, 255f / 255, 128f / 255, 255f / 255)}
            };

            #endregion

            #region Private fields

            /// <summary>
            /// Handy reference to the title text 
            /// </summary>
            private LanguageManagerLocalizedTextMesh m_titleText;

            /// <summary>
            /// Handy reference to the subtitle text 
            /// </summary>
            private LanguageManagerLocalizedTextMesh m_subtitleText;

            /// <summary>
            /// Handy reference to the subtitle text mesh
            /// </summary>
            private TextMesh m_subtitleTextMesh;

            /// <summary>
            /// Handy reference to the OTTIMO (VERY GOOD) text gameobject
            /// </summary>
            private GameObject m_veryGoodObject;

            /// <summary>
            /// Handy reference to current active stage canvas 
            /// </summary>
            private GameObject m_currentStageCanvas;

            /// <summary>
            /// Current calibration status
            /// </summary>
            private IroomCalibratorStatus m_currentStatus;

            /// <summary>
            /// The IroomSimple2DCalibGuiNoUIManager object that contains this object
            /// </summary>
            private IroomSimple2DCalibGuiNoUIManager m_enclosingInstance;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="enclosingInstance">Enclosing instance, whose code has to be implemented</param>
            internal IroomSimple2DCalibGuiNoUIManagerInternal(IroomSimple2DCalibGuiNoUIManager enclosingInstance)
            {
                m_enclosingInstance = enclosingInstance;
            }

            #endregion

            #region Behaviour methods

            internal void Start()
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("IroomSimple2DCalibGuiNoUIManager - Start called");
                }

                //get most used references for later uses
                m_titleText = m_enclosingInstance.PanelRoot.transform.GetChild(0).GetChild(0).GetComponent<LanguageManagerLocalizedTextMesh>();
                m_subtitleText = m_enclosingInstance.PanelRoot.transform.GetChild(0).GetChild(1).GetComponent<LanguageManagerLocalizedTextMesh>();
                m_subtitleTextMesh = m_enclosingInstance.PanelRoot.transform.GetChild(0).GetChild(1).GetComponent<TextMesh>();
                m_veryGoodObject = m_enclosingInstance.PanelRoot.transform.GetChild(5).gameObject;
                m_currentStageCanvas = null; //no initial canvas to be shown
                m_currentStatus = IroomCalibratorStatus.None;
                ResetGui();
            }

            #endregion

            #region IRoomCalibGuiManager Members

            /// <summary>
            /// Resets the calibration GUI used by this instance
            /// </summary>
            internal void ResetGui()
            {
                ResetGuiPrivate();

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("IroomSimple2DCalibGuiNoUIManager - Reset Gui called");
                }
            }

            /// <summary>
            /// Asks the manager to show the gui
            /// </summary>
            internal void Activate()
            {
                ActivatePrivate();

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("IroomSimple2DCalibGuiNoUIManager - Activate called");
                }
            }

            /// <summary>
            /// Asks the manager to hide the gui
            /// </summary>
            internal void Deactivate()
            {
                DeactivatePrivate();

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("IroomSimple2DCalibGuiNoUIManager - Deactivate called");
                }
            }

            /// <summary>
            /// Informs the gui manager that the calibration process has entered a new status
            /// </summary>
            /// <param name="newCalibrationStatus">New calibration status</param>
            internal void GoToStatus(IroomCalibratorStatus newCalibrationStatus)
            {
                GoToStatusPrivate(newCalibrationStatus);

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("IroomSimple2DCalibGuiNoUIManager - Asked status change. New status is {0}", newCalibrationStatus.ToString());
                }
            }

            /// <summary>
            /// Asks the manager to show the user that he's performing very well
            /// </summary>
            internal void ShowVeryGoodMessage()
            {
                ShowVeryGoodMessagePrivate();
            }

            /// <summary>
            /// Informs the manager that a process that is going on inside current state and that can be represented by a progress bar,
            /// has reached a certain progress value
            /// </summary>
            /// <param name="progressValue">New progress value in range [0, 1]</param>
            internal void ShowCurrentStateProgressBarValue(float progressValue)
            {
                ShowCurrentStateProgressBarValuePrivate(progressValue);
            }

            /// <summary>
            /// Asks the manager to initialize all the gui for "waiting for a calibrating user" stage, using the provided info.
            /// </summary>
            /// <param name="trackingEnvironment">Tracking environment inside which tracking happens</param>
            internal void InitWaitingBodyTrackingInfo(TrackingServiceEnv trackingEnvironment)
            {
                InitWaitingBodyTrackingInfoPrivate(trackingEnvironment);
            }

            /// <summary>
            /// Informs the manager about the most probable user body during "waiting for a calibrating user" stage.
            /// </summary>
            /// <param name="body">Body of the most tracked user</param>
            /// <param name="totalTrackingBoxes">Number of tracking boxes required to track the user</param>
            internal void ShowWaitingBodyTrackingInfo(TrackingServiceBodyData body, int totalTrackingBoxes)
            {
                ShowWaitingBodyTrackingInfoPrivate(body, totalTrackingBoxes);
            }

            /// <summary>
            /// Informs the manager about the orientation that the user should face during the "rotate to origin" stage.
            /// </summary>
            /// <param name="direction">0 if player has correct orientation, positive value if he has to turn left, negative value if he has to turn right</param>
            internal void ShowCalibratingBodyOrientationsDir(float direction)
            {
                ShowCalibratingBodyOrientationsDirPrivate(direction);
            }

            /// <summary>
            /// Make the gui to fade away after the process has completed, so the game scene can be displayed
            /// </summary>
            /// <param name="fadingPercent">Percent of fading in the range [0, 1], where 0 is full opaque and 1 is full transparent</param>
            internal void MakeGuiFade(float fadingPercent)
            {
                MakeGuiFadePrivate(fadingPercent);
            }

            #endregion

            #region Private GUI Management methods

            /// <summary>
            /// Resets the calibration GUI used by this instance
            /// </summary>
            private void ResetGuiPrivate()
            {
                //reset to initial stage
                m_currentStatus = IroomCalibratorStatus.None;

                //set initial canvas title
                m_titleText.KeyString = "Wait";
                m_subtitleText.KeyString = "NoConnection";

                //show logo
                m_enclosingInstance.PanelRoot.transform.GetChild(0).GetChild(2).gameObject.SetActive(true);

                //make background color black, using camera or background image
                if (m_enclosingInstance.PanelBackground != null)
                    m_enclosingInstance.PanelBackground.sharedMaterial.color = Color.black;

                //init last status canvas text to full opaque color
                TextMesh captionWriting = m_enclosingInstance.PanelRoot.transform.GetChild(4).GetChild(0).GetComponent<TextMesh>();
                captionWriting.color = new Color(captionWriting.color.r, captionWriting.color.g, captionWriting.color.b, 1);

                //disable all canvases, but show title
                ShowCanvases(-1);
            }

            /// <summary>
            /// Asks the manager to show the gui
            /// </summary>
            private void ActivatePrivate()
            {
                m_enclosingInstance.PanelRoot.transform.parent.gameObject.SetActive(true);
            }

            /// <summary>
            /// Asks the manager to hide the gui
            /// </summary>
            private void DeactivatePrivate()
            {
                m_enclosingInstance.PanelRoot.transform.parent.gameObject.SetActive(false);
            }

            /// <summary>
            /// Informs the gui manager that the calibration process has entered a new status
            /// </summary>
            /// <param name="newCalibrationStatus">New calibration status</param>
            private void GoToStatusPrivate(IroomCalibratorStatus newCalibrationStatus)
            {
                //set current status
                m_currentStatus = newCalibrationStatus;

                //set appropriate background, changing camera clear color or background image
                if (CanvasColors.ContainsKey(m_currentStatus))
                {
                    if (m_enclosingInstance.PanelBackground != null)
                        m_enclosingInstance.PanelBackground.sharedMaterial.color = CanvasColors[m_currentStatus];
                }

                //make appropriate initializations actions depending on the new status
                switch (m_currentStatus)
                {
                    case IroomCalibratorStatus.WaitingForBody:

                        //hide logo
                        m_enclosingInstance.PanelRoot.transform.GetChild(0).GetChild(2).gameObject.SetActive(false);

                        //show and set appropriate canvas, with appropriate title and subtitle
                        ShowCanvases(1);
                        m_titleText.KeyString = "Initialization";
                        m_subtitleTextMesh.text = LanguageManager.Instance.GetLocalizedString("InitStep") + "1"; //can't use StringKey because we want to add the number to the localized string

                        break;

                    case IroomCalibratorStatus.RotatingBodyToOrigin:

                        //show and set appropriate canvas, with appropriate title and subtitle
                        ShowCanvases(2);
                        m_subtitleTextMesh.text = LanguageManager.Instance.GetLocalizedString("InitStep") + "2"; //can't use StringKey because we want to add the number to the localized string

                        break;

                    case IroomCalibratorStatus.BodyStandingStill:

                        //show and set appropriate canvas, with appropriate title and subtitle
                        ShowCanvases(3);
                        m_subtitleTextMesh.text = LanguageManager.Instance.GetLocalizedString("InitStep") + "3"; //can't use StringKey because we want to add the number to the localized string

                        //set progress bar to 0
                        Transform progressBarTransf = m_currentStageCanvas.transform.GetChild(1).GetChild(0);
                        progressBarTransf.localPosition = new Vector3(0, progressBarTransf.localPosition.y, progressBarTransf.localPosition.z);
                        progressBarTransf.localScale = new Vector3(0, progressBarTransf.localScale.y, progressBarTransf.localScale.z);

                        break;

                    case IroomCalibratorStatus.Calibrating:

                        //say to camera that it has not to clear screen anymore, so the fading can happen
                        //(if calibration camera clears the screen, we can't see the main camera output).
                        //if we have not a camera, but a background, make it disappear, because the fading will be performed by
                        //the final message canvas
                        if (m_enclosingInstance.PanelBackground != null)
                            m_enclosingInstance.PanelBackground.sharedMaterial.color = new Color(0, 0, 0, 0);

                        //show and set appropriate canvas, with appropriate title and subtitle
                        ShowCanvases(4, false, false);
                        m_subtitleTextMesh.text = LanguageManager.Instance.GetLocalizedString("InitStep") + "4"; //can't use StringKey because we want to add the number to the localized string

                        break;
                }
            }

            /// <summary>
            /// Asks the manager to show the user that he's performing very well
            /// </summary>
            private void ShowVeryGoodMessagePrivate()
            {
                //show OTTIMO writing
                m_veryGoodObject.SetActive(true);
            }

            /// <summary>
            /// Informs the manager that a process that is going on inside current state and that can be represented by a progress bar,
            /// has reached a certain progress value
            /// </summary>
            /// <param name="progressValue">New progress value in range [0, 1]</param>
            private void ShowCurrentStateProgressBarValuePrivate(float progressValue)
            {
                //if we are in stage that we're waiting for current body to stay still
                if (m_currentStatus == IroomCalibratorStatus.InitializingWithBody)
                {
                    //get the rectangle image of the progress bar and assign it a scale and a color (from red to green) to show current progress
                    Transform progressBarTransf = m_currentStageCanvas.transform.GetChild(1).GetChild(0);
                    progressBarTransf.localScale = new Vector3(progressValue, progressBarTransf.localScale.y, progressBarTransf.localScale.z);
                    progressBarTransf.GetComponent<Renderer>().material.color = new Color(1 - progressValue, progressValue, 0.1f);

                    //move the progress bar on the x axis, so that it stays fixed on the left (left pos is the position of the father)
                    progressBarTransf.localPosition = new Vector3(progressValue / 2, progressBarTransf.localPosition.y, progressBarTransf.localPosition.z);
                }
            }

            /// <summary>
            /// Asks the manager to initialize all the gui for "waiting for a calibrating user" stage, using the provided info.
            /// </summary>
            /// <param name="trackingEnvironment">Tracking environment inside which tracking happens</param>
            private void InitWaitingBodyTrackingInfoPrivate(TrackingServiceEnv trackingEnvironment)
            {
                //get root of objects showing red/green statuses for various data sources
                Transform pallinsRoot = m_enclosingInstance.PanelRoot.transform.GetChild(1).GetChild(1);

                //clear all existing symbols (pallins)
                foreach (Transform pallin in pallinsRoot)
                {
                    UnityEngine.Object.Destroy(pallin.gameObject);
                }

                //create the required number of pallins, with the correct name
                for (int i = 0; i < trackingEnvironment.MinDataSourcesForPlayer; i++)
                {
                    GameObject pallin = UnityEngine.Object.Instantiate<GameObject>(m_enclosingInstance.PallinPrefab);
                    pallin.transform.SetParent(pallinsRoot, false);
                    pallin.name = trackingEnvironment.DataSources[i].ToString(); //giving it this name will come handy later
                    pallin.transform.GetChild(1).GetComponent<TextMesh>().text = TrackingServiceManagerBasic.Instance.GetDataSourceNameFromByteId(trackingEnvironment.DataSources[i]);
                }

            }

            /// <summary>
            /// Informs the manager about the most probable user body during "waiting for a calibrating user" stage.
            /// </summary>
            /// <param name="body">Body of the most tracked user</param>
            /// <param name="totalTrackingBoxes">Number of tracking boxes required to track the user</param>
            private void ShowWaitingBodyTrackingInfoPrivate(TrackingServiceBodyData body, int totalTrackingBoxes)
            {
                //get root of objects showing red/green statuses for various data sources
                Transform pallinsRoot = m_currentStageCanvas.transform.GetChild(1);

                //make semaphore circles green for tracking data sources and red otherwise.
                //So loop for every children object of the pallins root
                foreach (Transform pallin in pallinsRoot)

                    if (pallin.GetInstanceID() != pallinsRoot.GetInstanceID())
                    {
                        //get tracking box id from child name. It may happen that object still contains spurious objects from initial
                        //configuration (objects inserted into the editor, still not destroyed by InitWaitingBodyTrackingInfoPrivate, who
                        //calls a Destroy, that gets executed at next frame after first execution of this method), hence the check
                        byte trackingBoxId;
                        bool parsable = Byte.TryParse(pallin.name, out trackingBoxId);

                        //if this data source is tracking the body
                        if (body != null && parsable && body.DataSources.Contains(trackingBoxId))
                        {
                            pallin.GetChild(0).GetComponent<SpriteRenderer>().sprite = m_enclosingInstance.OkSprite;
                        }
                        //else
                        else
                        {
                            pallin.GetChild(0).GetComponent<SpriteRenderer>().sprite = m_enclosingInstance.NoSprite;
                        }
                    }
            }

            /// <summary>
            /// Informs the manager about the orientation that the user should face during the "rotate to origin" stage.
            /// </summary>
            /// <param name="direction">0 if player has correct orientation, positive value if he has to turn left, negative value if he has to turn right</param>
            private void ShowCalibratingBodyOrientationsDirPrivate(float direction)
            {
                if (direction == 0)
                {
                    m_currentStageCanvas.transform.GetChild(2).GetComponent<LanguageManagerLocalizedTextMesh>().KeyString = "InitNowStop";
                    m_currentStageCanvas.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = m_enclosingInstance.OkSprite;
                }
                else if (direction < 0)
                {
                    m_currentStageCanvas.transform.GetChild(2).GetComponent<LanguageManagerLocalizedTextMesh>().KeyString = "InitRotatRight";
                    m_currentStageCanvas.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = m_enclosingInstance.DirectionRightSprite;
                }
                else
                {
                    m_currentStageCanvas.transform.GetChild(2).GetComponent<LanguageManagerLocalizedTextMesh>().KeyString = "InitRotatLeft";
                    m_currentStageCanvas.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = m_enclosingInstance.DirectionLeftSprite;
                }
            }

            /// <summary>
            /// Make the gui to fade away after the process has completed, so the game scene can be displayed
            /// </summary>
            /// <param name="fadingPercent">Percent of fading in the range [0, 1], where 0 is full opaque and 1 is full transparent</param>
            private void MakeGuiFadePrivate(float fadingPercent)
            {
                //make a fading effect on the panel and its text
                m_enclosingInstance.PanelBackground.sharedMaterial.color = new Color(m_enclosingInstance.PanelBackground.sharedMaterial.color.r, m_enclosingInstance.PanelBackground.sharedMaterial.color.g, m_enclosingInstance.PanelBackground.sharedMaterial.color.b, 1 - fadingPercent);
                TextMesh captionWriting = m_currentStageCanvas.transform.GetChild(0).GetComponent<TextMesh>();
                captionWriting.color = new Color(captionWriting.color.r, captionWriting.color.g, captionWriting.color.b, 1 - fadingPercent);
            }

            #endregion

            #region Private Helpers

            /// <summary>
            /// Show the desired canvas inside the GUI, disabling all the others.
            /// It may be specified if header and footer canvases should be disabled, as well.
            /// The method also set the m_currentStageCanvas to point to the active canvas object
            /// </summary>
            /// <param name="canvasNum">ID number of the canvas that should remain active. Use -1 to disable all canvases</param>
            /// <param name="showHeader">True to show header canvas, false otherwise</param>
            /// <param name="showVeryGood">True to show footer canvas, false otherwise</param>
            private void ShowCanvases(int canvasNum, bool showHeader = true, bool showVeryGood = false)
            {
                //disable all canvases
                foreach (Transform canvas in m_enclosingInstance.PanelRoot.transform)
                    if (canvas.GetInstanceID() != m_enclosingInstance.PanelRoot.GetInstanceID())
                        canvas.gameObject.SetActive(false);

                //if was not requested to disable all
                if (canvasNum > 0)
                {
                    //enable required one
                    m_enclosingInstance.PanelRoot.transform.GetChild(canvasNum).gameObject.SetActive(true);

                    //set current stage canvas to the active canvas ID
                    m_currentStageCanvas = m_enclosingInstance.PanelRoot.transform.GetChild(canvasNum).gameObject;
                }

                //enable header, if required
                m_enclosingInstance.PanelRoot.transform.GetChild(0).gameObject.SetActive(showHeader);

                //enable "very good" footer, if required
                m_enclosingInstance.PanelRoot.transform.GetChild(5).gameObject.SetActive(showVeryGood);
            }

            #endregion
        }

    }

}
