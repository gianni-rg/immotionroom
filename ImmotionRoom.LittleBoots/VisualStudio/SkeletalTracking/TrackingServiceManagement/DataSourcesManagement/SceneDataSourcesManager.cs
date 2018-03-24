namespace ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using ImmotionAR.ImmotionRoom.TrackingService.ControlClient.Model;
    using UnityEngine;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataSourcesManagement;


    public partial class TrackingServiceManager : EventBasedMonoBehaviour
    {
        /// <summary>
        /// Implements management of <see cref="SceneDataSource"/> objects.
        /// This class is used to manage the data source for <see cref="TrackingServiceManager"/>.
        /// Dispose method should be called when an object of this class is no longer needed
        /// </summary>
        protected internal partial class SceneDataSourcesManager : IDisposable
        {
            #region Private fields

            /// <summary>
            /// Object that represents the root for all BodyDataSource references
            /// </summary>
            private GameObject m_BodyDataSourcesRoot;

            /// <summary>
            /// Current map of data sources id, from ID in byte form, to ID in string form
            /// (byte ID is an incremental number, while string ID is a human readable name)
            /// </summary>
            private Dictionary<byte, string> m_dataSourcesByteIdToStringId;

            #endregion

            #region Constructor and Initializer

            /// <summary>
            /// Creates a <see cref="SceneDataSourcesManager"/> object
            /// </summary>
            /// <param name="parentObject">Unity gameObject that owns this manager. Will be created a child with all data sources references</param>
            /// <param name="managerGameObjectName">Name to assign to the child object of the parentObject, that will hold all data sources references</param>
            internal SceneDataSourcesManager(GameObject parentObject, string managerGameObjectName)
            {
                // Create root body data source child object
                m_BodyDataSourcesRoot = new GameObject();
                m_BodyDataSourcesRoot.name = managerGameObjectName;
                m_BodyDataSourcesRoot.transform.SetParent(parentObject.transform, false);

                //create data struct
                m_dataSourcesByteIdToStringId = new Dictionary<byte, string>();

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("SceneDataSourcesManager - Creating manager with name {0}", managerGameObjectName);
                }
            }

            /// <summary>
            /// Initializes or re-initializes current manager, so that it can handle current data sources managed by
            /// the underlying ImmotionRoom Tracking Services
            /// </summary>
            /// <param name="availableDataStreams">Dictionary that maps each data source string ID to data stream info of that data source</param>
            /// <param name="dataSources">Dictionary that maps each data source string ID to info of that data source</param>
            internal void Initialize(IDictionary<string, TrackingServiceDataStreamerInfo> availableDataStreams, IDictionary<string, TrackingServiceDataSourceInfo> dataSources)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("SceneDataSourcesManager - Initialization begin");
                }

                //clear old data
                Reset();

                //initialize class with new data
                CreateBodyDataSources(availableDataStreams);
                CreateDataSourcesIdMappingsDictionary(dataSources);

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("SceneDataSourcesManager - Initialization end");
                }
            }

            #endregion

            #region Global Management Methods

            /// <summary>
            /// Resets the object to the initial state, deleting all internal data and allocated streams
            /// </summary>
            internal void Reset()
            {
                //clear all
                DestroyBodyDataSources();
                DeleteDataSourcesIdMappingsDictionary();
            }

            #endregion

            #region Data sources management methods

            /// <summary>
            /// Creates a data source unique ID, composing informations about its reference body data streamer and required
            /// streaming mode
            /// </summary>
            /// <param name="SceneDataStreamerInfoId">Reference body data streamer id</param>
            /// <param name="streamingMode">Required streaming mode</param>
            /// <returns>Unique data source ID</returns>
            internal static string MangleBodyDataSourceId(string SceneDataStreamerInfoId, TrackingServiceSceneDataStreamModes streamingMode)
            {
                return string.Format("{0}_{1}", SceneDataStreamerInfoId, streamingMode);
            }

            /// <summary>
            /// Creates a body data source managed by this object, assumed that it does not exists
            /// </summary>
            /// <param name="SceneDataStreamerInfo">
            /// Info about the BodyDataStreamer that can provide streamable data for the body data source
            /// </param>
            /// <param name="streamingMode">Required streaming mode for the body data source</param>
            private void CreateBodyDataSource(TrackingServiceDataStreamerInfo SceneDataStreamerInfo, TrackingServiceSceneDataStreamModes streamingMode)
            {
                // Get body data root game object
                var bodyDataRoot = m_BodyDataSourcesRoot.transform;

                // Create a child object of it, named with a mangling of body streamer id and the required streaming mode
                var bodyDataSourceGo = new GameObject();
                bodyDataSourceGo.name = MangleBodyDataSourceId(SceneDataStreamerInfo.Id, streamingMode);
                bodyDataSourceGo.transform.SetParent(bodyDataRoot.transform, false);

                // Create and add a SceneDataSource object to it
                SceneDataSource.CreateInstance(bodyDataSourceGo, SceneDataStreamerInfo.Id, SceneDataStreamerInfo.StreamEndpoint, SceneDataStreamerInfo.StreamPort, streamingMode);

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("SceneDataSourcesManager - Creating body data source with ID: [{0}] on {1}:{2}", SceneDataStreamerInfo.Id, SceneDataStreamerInfo.StreamEndpoint, SceneDataStreamerInfo.StreamPort);
                }
            }

            /// <summary>
            /// Gets if a body data source associated with the provided body data streamer and streaming mode exists already or not
            /// </summary>
            /// <param name="bodyDataStreamerId">ID of the body data streamer of interest</param>
            /// <param name="streamingMode">Streaming mode of interest</param>
            /// <returns>True if it already exists, false otherwise</returns>
            private bool ExistsBodyDataSource(string bodyDataStreamerId, TrackingServiceSceneDataStreamModes streamingMode)
            {
                // Get if the body data root game object contains a child with the name associated with our request
                return m_BodyDataSourcesRoot.transform.Find(MangleBodyDataSourceId(bodyDataStreamerId, streamingMode));
            }

            /// <summary>
            /// Get a body data source associated with the provided body data streamer and streaming mode
            /// </summary>
            /// <param name="bodyDataStreamerId">ID of the body data streamer of interest</param>
            /// <param name="streamingMode">Streaming mode of interest</param>
            /// <returns>The desired body data source, or null if no such data source exists</returns>
            private SceneDataSource GetBodyDataSource(string bodyDataStreamerId, TrackingServiceSceneDataStreamModes streamingMode)
            {
                // Get if the body data root game object contains a child with the name associated with our request
                var foundDataSource = m_BodyDataSourcesRoot.transform.Find(MangleBodyDataSourceId(bodyDataStreamerId, streamingMode));

                // If it is so, return its corresponding body data source; otherwise return null
                return foundDataSource == null ? null : foundDataSource.GetComponent<SceneDataSource>();
            }

            /// <summary>
            /// (Re)Creates all the body data sources this tracking service manager can manage.
            ///  Body data sources are object capable of connecting with underlying service and get body data in a Unity-friendly way.
            ///  We create a body data source for each possible stream and append it as a child of this object
            /// </summary>
            /// <param name="availableDataStreams">Dictionary that maps each data source string ID to data stream info of that data source</param>
            private void CreateBodyDataSources(IDictionary<string, TrackingServiceDataStreamerInfo> availableDataStreams)
            {
                // Foreach stream we can access to
                foreach (var SceneDataStreamerInfo in availableDataStreams.Values)
                {
                    // Loop its capabilities
                    foreach (var streamingMode in SceneDataStreamerInfo.SupportedStreamModes)
                    {
                        // If a body data source for this kind of stream with this streaming mode does not exist, create it
                        if (!ExistsBodyDataSource(SceneDataStreamerInfo.Id, streamingMode))
                        {
                            CreateBodyDataSource(SceneDataStreamerInfo, streamingMode);
                        }
                    }
                }
            }

            /// <summary>
            /// Destroy all body data source associated with this object, making them disconnecting from the underlying service.
            /// Read <see cref="CreateBodyDataSources" /> comment to better understand what a body data source is
            /// </summary>
            private void DestroyBodyDataSources()
            {
                // Get body data root game object
                var bodyDataRoot = m_BodyDataSourcesRoot.transform;

                // Destroy root game object children; this will auto disconnect them
                foreach (Transform dataSourceTransform in bodyDataRoot)
                {
                    if (dataSourceTransform.GetInstanceID() != bodyDataRoot.GetInstanceID())
                    {
                        if (Log.IsDebugEnabled)
                        {
                            Log.Debug("SceneDataSourcesManager - Destroying body data source [{0}]", dataSourceTransform.gameObject.name);
                        }

                        UnityEngine.Object.Destroy(dataSourceTransform.gameObject);
                    }
                }
            }

            #endregion

            #region Data Sources names management methods

            /// <summary>
            /// Creates mapping of data sources id from byte to string
            /// </summary>
            /// <param name="dataSources">Dictionary that maps each data source string ID to info of that data source</param>
            private void CreateDataSourcesIdMappingsDictionary(IDictionary<string, TrackingServiceDataSourceInfo> dataSources)
            {
                //clear the existing mappings
                m_dataSourcesByteIdToStringId.Clear();

                //create new mappings, associating for each stream info, its byte id with string id
                foreach (TrackingServiceDataSourceInfo streamerInfo in dataSources.Values)
                    m_dataSourcesByteIdToStringId.Add(streamerInfo.UniqueId, streamerInfo.Id);
            }

            /// <summary>
            /// Deletes all mappings of data sources ids from byte to string
            /// </summary>
            private void DeleteDataSourcesIdMappingsDictionary()
            {
                //clear the existing mappings
                m_dataSourcesByteIdToStringId.Clear();
            }

            /// <summary>
            /// Get data source string name associated with the desired unique id
            /// </summary>
            /// <param name="dataSourceUniqueId">byte ID of the data source</param>
            /// <returns>String representation of desired data source</returns>
            internal string GetDataSourceNameFromByteId(byte dataSourceUniqueId)
            {
                return m_dataSourcesByteIdToStringId[dataSourceUniqueId];
            }

            #endregion

            #region Data streams offering management methods

            /// <summary>
            /// Give the caller an object through which can receive the desired scene stream data at each frame.
            /// After the object has been used, the Dispose method should be called on the provided object
            /// If no Dispose gets called on the provided object, the stream gets closed at the end of the program.
            /// For more info, see <see cref="SceneDataProvider" /> class documentation
            /// </summary>
            /// <param name="streamerInfoId">ID of the body data streamer of interest</param>
            /// <param name="streamingMode">Streaming mode of interest</param>
            /// <returns>
            /// Proxy object with properties exposing each frame updated info about scene data streaming, or null if the
            /// required stream does not exists
            /// </returns>
            internal SceneDataProvider StartDataProvider(string streamerInfoId, TrackingServiceSceneDataStreamModes streamingMode)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("SceneDataSourcesManager - Requested data provider for stream [{0}]", MangleBodyDataSourceId(streamerInfoId, streamingMode));
                }

                // See if a body data source with the requested id and streaming mode exists, and if not, return null
                if (!ExistsBodyDataSource(streamerInfoId, streamingMode))
                {
                    if (Log.IsWarnEnabled)
                    {
                        Log.Warning("SceneDataSourcesManager - The requested stream does not exist");
                    }

                    return null;
                }

                // Else, if exists, get it and return a provider wrapper
                
                // Don't forget to add BodyDataSource reference counting
                var dataSource = GetBodyDataSource(streamerInfoId, streamingMode);

                return new SceneDataProvider(dataSource);
            }

            #endregion

            #region IDisposable implementation

            //see http://manski.net/2012/01/idisposable-finalizer-and-suppressfinalize/#combining-dispose-and-finalizer

            /// <summary>
            /// True if the object is already disposed, false otherwise
            /// </summary>
            private bool m_disposed = false;

            /// <summary>
            /// Dispose. You MUST call this when this object is no longer needed (e.g. in the OnDestroy)
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Finalizer
            /// </summary>
            ~SceneDataSourcesManager()
            {
                Dispose(false);
            }

            /// <summary>
            /// Internal disposing method
            /// </summary>
            /// <param name="disposing">True if the method is called from the dispose method, false if it is from the finalizer</param>
            protected virtual void Dispose(bool disposing)
            {
                if (m_disposed)
                    return;

                if (disposing)
                {
                    //delete all the allocated objects. This will close communication with all the underlying objects
                    DestroyBodyDataSources();
                    DeleteDataSourcesIdMappingsDictionary();
                    UnityEngine.Object.Destroy(m_BodyDataSourcesRoot);

                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("SceneDataSourcesManager - Disposed");
                    }
                }

                m_disposed = true;
            }

            #endregion
        }

    }
}
