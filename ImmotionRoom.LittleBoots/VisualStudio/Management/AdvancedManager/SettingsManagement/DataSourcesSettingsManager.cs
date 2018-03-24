namespace ImmotionAR.ImmotionRoom.LittleBoots.Management.AdvancedManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement;
using ImmotionAR.ImmotionRoom.LittleBoots.Management._3dparties;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using ImmotionAR.ImmotionRoom.AutoDiscovery.Model;
    using UnityEngine;

    public partial class TrackingServiceManagerAdvanced : TrackingServiceManager
    {

        /// <summary>
        /// Stores Data Sources settings, across multiple game sessions.
        /// These settings regard how the underlying data source services can be contacted
        /// </summary>
        protected internal partial class DataSourcesSettingsManager
        {
            #region Constants

            private const string DataSourcesIdSettingName = "ImmotionRoom.DataSources.Id";
            private const string DataSourcesControlApiEndpointSettingName = "ImmotionRoom.DataSources.ControlApiEndpoint";
            private const string DataSourcesControlApiPortSettingName = "ImmotionRoom.DataSources.ControlApiPort";
            private const string DataSourcesDataStreamerEndpointSettingName = "ImmotionRoom.DataSources.DataStreamerEndpoint";
            private const string DataSourcesDataStreamerPortSettingName = "ImmotionRoom.DataSources.DataStreamerPort";

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
                    return PlayerPrefs.HasKey(DataSourcesIdSettingName) &&
                            PlayerPrefs.HasKey(DataSourcesControlApiEndpointSettingName) &&
                            PlayerPrefs.HasKey(DataSourcesControlApiPortSettingName) &&
                            PlayerPrefs.HasKey(DataSourcesDataStreamerEndpointSettingName) &&
                            PlayerPrefs.HasKey(DataSourcesDataStreamerPortSettingName);
                }
            }

            /// <summary>
            /// Gets stored DataSources infos or null if this datum is not stored
            /// </summary>
            internal Dictionary<string, DataSourceItem> DataSources
            {
                get
                {
                    //get settings from the PlayerPrefs (to see how they have been serialized, have a look to the underlying set method)

                    if (!HasSavedSettings)
                        return null;

                    //get data about the data sources as array from the player preferences
                    string[][] stringSettings = new string[3][];
                    int[][] intSettings = new int[2][];

                    //now that we have arrays, we can use PlayerPrefsX methods to save them
                    stringSettings[0] = PlayerPrefsX.GetStringArray(DataSourcesIdSettingName);
                    stringSettings[1] = PlayerPrefsX.GetStringArray(DataSourcesControlApiEndpointSettingName);
                    stringSettings[2] = PlayerPrefsX.GetStringArray(DataSourcesDataStreamerEndpointSettingName);
                    intSettings[0] = PlayerPrefsX.GetIntArray(DataSourcesControlApiPortSettingName);
                    intSettings[1] = PlayerPrefsX.GetIntArray(DataSourcesDataStreamerPortSettingName);

                    //check consistency
                    if (stringSettings[0].Length != stringSettings[1].Length ||
                        stringSettings[1].Length != stringSettings[2].Length ||
                        stringSettings[2].Length != intSettings[0].Length ||
                        intSettings[0].Length != intSettings[1].Length)
                        return null;

                    //use the data to fill a dictionary and return it
                    Dictionary<string, DataSourceItem> dataSources = new Dictionary<string, DataSourceItem>();

                    for (int i = 0; i < stringSettings[0].Length; i++)
                        dataSources[stringSettings[0][i]] = new DataSourceItem() { Id = stringSettings[0][i], ControlApiEndpoint = stringSettings[1][i], ControlApiPort = intSettings[0][i], DataStreamerEndpoint = stringSettings[2][i], DataStreamerPort = intSettings[1][i] };

                    return dataSources;
                }
                private set
                {
                    if (value == null)
                        return;

                    //set new settings (remember that for each data source we have 3 strings and 2 ints, i.e. the port numbers)
                    //First string value is DataSource Id
                    //Second string value is DataSource ControlApiEndpoint
                    //Third string value is DataSource DataStreamerEndpoint
                    //First int value is DataSource ControlApiPort
                    //Second int value is DataSource DataStreamerPort
                    string[][] stringSettings = new string[3][];
                    int[][] intSettings = new int[2][];
                    int idx = 0;

                    for (int i = 0; i < 3; i++)
                        stringSettings[i] = new string[value.Count];

                    for (int i = 0; i < 2; i++)
                        intSettings[i] = new int[value.Count];

                    //get the values of the data sources and put them into the arrays
                    foreach (DataSourceItem dataSourceItem in value.Values)
                    {
                        stringSettings[0][idx] = dataSourceItem.Id;
                        stringSettings[1][idx] = dataSourceItem.ControlApiEndpoint;
                        stringSettings[2][idx] = dataSourceItem.DataStreamerEndpoint;
                        intSettings[0][idx] = dataSourceItem.ControlApiPort;
                        intSettings[1][idx] = dataSourceItem.DataStreamerPort;
                        idx++;
                    }

                    //now that we have arrays, we can use PlayerPrefsX methods to save them
                    PlayerPrefsX.SetStringArray(DataSourcesIdSettingName, stringSettings[0]);
                    PlayerPrefsX.SetStringArray(DataSourcesControlApiEndpointSettingName, stringSettings[1]);
                    PlayerPrefsX.SetStringArray(DataSourcesDataStreamerEndpointSettingName, stringSettings[2]);
                    PlayerPrefsX.SetIntArray(DataSourcesControlApiPortSettingName, intSettings[0]);
                    PlayerPrefsX.SetIntArray(DataSourcesDataStreamerPortSettingName, intSettings[1]);
                }
            }

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            internal DataSourcesSettingsManager()
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
                    Log.Debug("DataSourcesSettingsManager - Loaded settings with result code: {0}", (HasSavedSettings ? "OK" : "FAIL"));
                }
            }

            /// <summary>
            /// Initializes the manager, deleting old settings and using the new provided ones
            /// </summary>
            /// <param name="dataSources">The data sources to be saved inside the settings</param>
            internal void Initialize(Dictionary<string, DataSourceItem> dataSources)
            {
                //set the provided settings
                DataSources = dataSources;

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("DataSourcesSettingsManager - Set new settings for all known Data Sources");
                }
            }

            /// <summary>
            /// Clear all permanent settings
            /// </summary>
            internal void Clear()
            {
                //remove all Player Prefs
                PlayerPrefs.DeleteKey(DataSourcesIdSettingName);
                PlayerPrefs.DeleteKey(DataSourcesControlApiEndpointSettingName);
                PlayerPrefs.DeleteKey(DataSourcesControlApiPortSettingName);
                PlayerPrefs.DeleteKey(DataSourcesDataStreamerEndpointSettingName);
                PlayerPrefs.DeleteKey(DataSourcesDataStreamerPortSettingName);

                this.Save();

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("DataSourcesSettingsManager - Cleared preferences");
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
                    Log.Debug("DataSourcesSettingsManager - Saved preferences");
                }
            }

            #endregion

        }
    }

}
