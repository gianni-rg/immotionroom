namespace ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement
{
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    public partial class TrackingServiceManager : EventBasedMonoBehaviour
    {

        /// <summary>
        /// Stores <see cref="TrackingServiceManager"/> settings, across multiple game sessions.
        /// These settings regard how the underlying tracking service can be contacted
        /// </summary>
        protected internal partial class TrackingServiceSettingsManager
        {
            #region Constants

            private const string TrackingServiceIdSettingName = "ImmotionRoom.TrackingService.Id";
            private const string TrackingServiceControlApiEndpointSettingName = "ImmotionRoom.TrackingService.ControlApiEndpoint";
            private const string TrackingServiceControlApiPortSettingName = "ImmotionRoom.TrackingService.ControlApiPort";

            #endregion

            #region Internal Properties

            /// <summary>
            /// True if there are saved settings from the last sessions, false otherwise
            /// </summary>
            internal bool HasSavedSettings
            {
                get
                {
                    //check if we've saved some keys in the PlayerPrefs
                    return TrackingServiceId != String.Empty && TrackingServiceControlApiPort != -1;
                }
            }

            /// <summary>
            /// Gets stored TrackingService id, or Empty String if this datum is not stored
            /// </summary>
            internal string TrackingServiceId
            {
                get
                {
                    return PlayerPrefs.GetString(TrackingServiceIdSettingName, "");
                }
                private set
                {
                    PlayerPrefs.SetString(TrackingServiceIdSettingName, value);
                }
            }

            /// <summary>
            /// Gets stored TrackingService control API endpoint, or Empty String if this datum is not stored
            /// </summary>
            internal string TrackingServiceControlApiEndpoint
            {
                get
                {
                    return PlayerPrefs.GetString(TrackingServiceControlApiEndpointSettingName, "");
                }
                private set
                {
                    PlayerPrefs.SetString(TrackingServiceControlApiEndpointSettingName, value);
                }
            }

            /// <summary>
            /// Gets stored TrackingService control API port, or -1 if this datum is not stored
            /// </summary>
            internal int TrackingServiceControlApiPort
            {
                get
                {
                    return PlayerPrefs.GetInt(TrackingServiceControlApiPortSettingName, -1);
                }
                private set
                {
                    PlayerPrefs.SetInt(TrackingServiceControlApiPortSettingName, value);
                }
            }

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            internal TrackingServiceSettingsManager()
            {
            }

            #endregion

            #region Settings Management

            /// <summary>
            /// Initializes the manager, making it trying to restore the data saved in the previous sessions
            /// </summary>
            internal void Initialize()
            {
                //PlayerPrefs loads automatically, so we simply do nothing

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("TrackingServiceSettingsManager - Loaded settings with result code: {0}", (HasSavedSettings ? "OK" : "FAIL"));
                }
            }

            /// <summary>
            /// Initializes the manager, deleting old settings and using the new provided ones
            /// </summary>
            /// <param name="trackingServiceId">Tracking Service ID</param>
            /// <param name="trackingServiceControlApiEndpoint">TrackingService Control API IP Address</param>
            /// <param name="trackingServiceControlApiPort">TrackingService Control API IP Port</param>
            public void Initialize(string trackingServiceId, string trackingServiceControlApiEndpoint, int trackingServiceControlApiPort)
            {
                //set new settings
                TrackingServiceId = trackingServiceId;
                TrackingServiceControlApiEndpoint = trackingServiceControlApiEndpoint;
                TrackingServiceControlApiPort = trackingServiceControlApiPort;

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("TrackingServiceSettingsManager - Set new settings for Tracking Service {0}", trackingServiceId);
                }
            }

            /// <summary>
            /// Clear all permanent settings
            /// </summary>
            internal void Clear()
            {
                //remove all Player Prefs
                PlayerPrefs.DeleteKey(TrackingServiceIdSettingName);
                PlayerPrefs.DeleteKey(TrackingServiceControlApiEndpointSettingName);
                PlayerPrefs.DeleteKey(TrackingServiceControlApiPortSettingName);

                this.Save();

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("TrackingServiceSettingsManager - Cleared preferences");
                }
            }

            /// <summary>
            /// Save all settings permanently, so they can be restored across different sessions
            /// </summary>
            internal void Save()
            {
                //Save data in PlayerPref
                PlayerPrefs.Save();

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("TrackingServiceSettingsManager - Saved preferences");
                }
            }

            #endregion

        }

    }
}
