/****************
 * 
 * Copyright (c) 2014-2016 ImmotionAR, a division of Beps Engineering.
 * All rights reserved
 * 
 * See licensing terms of this file in document <Assets folder>\ImmotionRoomUnity\License\LICENSE.TXT
 * 
 ****************/

namespace ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.DataSourcesManagement
{
    using System;
    using System.Collections.Generic;
    using Logger;
    using TrackingService.ControlClient.Model;
    using TrackingService.DataClient;
    using TrackingService.DataClient.Model;
    using UnityEngine;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;

    /// <summary>
    /// Streams scene data from a known source
    /// </summary>
    internal class SceneDataSource : EventBasedMonoBehaviour
    {
        #region Private members

        /// <summary>
        /// Object that can stream scene data from the underlying Tracking service
        /// </summary>
        private TrackingServiceSceneDataClient m_DataStreamerClient;

        /// <summary>
        /// True if this data source is connected to receive data, false otherwise
        /// </summary>
        private bool m_Connected;

        /// <summary>
        ///  Number of SceneDataProvider objects actually connected to this object to read updates. This is to implement
        ///  reference counting of connect operations. 
        ///  When no provider is connected to this object, this object do not connects to the underlying
        ///  service and so bandwith is spared.
        /// </summary>
        private int m_ProvidersConnnectedToThis;

        /// <summary>
        /// Last scene data frame read from the source
        /// </summary>
        private TrackingServiceSceneFrame m_LastReadFrame;

        #endregion

        #region Internal properties

        /// <summary>
        /// Get stream id
        /// </summary>
        internal string StreamId
        {
            get 
            { 
                return m_DataStreamerClient.Id; 
            }
        }

        /// <summary>
        /// Get streaming mode of this stream
        /// </summary>
        internal TrackingServiceSceneDataStreamModes StreamingMode 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// Get last frame read from the source
        /// </summary>
        internal TrackingServiceSceneFrame LastReadFrame
        {
            get 
            { 
                return m_LastReadFrame; 
            }
        }

        /// <summary>
        /// Get last bodies read from the source
        /// </summary>
        internal IList<TrackingServiceBodyData> LastBodies
        {
            get 
            { 
                return m_LastReadFrame.Bodies; 
            }
        }

        /// <summary>
        /// Get timestamp of last read bodies
        /// </summary>
        internal DateTime LastTimeStamp
        {
            get 
            { 
                return m_LastReadFrame.Timestamp; 
            }
        }

        /// <summary>
        /// Get if this data source is connected to receive valid data
        /// </summary>
        internal bool IsConnected
        {
            get
            {
                return m_Connected && m_DataStreamerClient.IsConnected;
            }
        }

        #endregion

        #region Constructors and similar stuff

        /// <summary>
        /// Private default constructor. It does nothing, apart than telling Unity it can't use it.
        /// If you want to construct an object of this type, you should use the factory below
        /// </summary>
        private SceneDataSource()
        {
        }

        /// <summary>
        /// Create a new instance of this streamer behaviour and attaches it to the provided GameObject
        /// </summary>
        /// <param name="instanceGo">GameObject the behaviour has to be assigned to</param>
        /// <param name="dataClient">Client of the scene data streamer this object should encapsulate</param>
        /// <param name="streamingMode">Streaming mode from the data streamer (e.g. raw stream or merged)</param>
        /// <returns>Instance created</returns>
        internal static SceneDataSource CreateInstance(GameObject instanceGo, TrackingServiceSceneDataClient dataClient, TrackingServiceSceneDataStreamModes streamingMode)
        {
            var instance = instanceGo.AddComponent<SceneDataSource>();
            instance.m_DataStreamerClient = dataClient;
            instance.StreamingMode = streamingMode;
            instance.m_LastReadFrame = new TrackingServiceSceneFrame();
            instance.m_Connected = false;
            instance.m_ProvidersConnnectedToThis = 0;

            if (Log.IsDebugEnabled)
            {
                Log.Debug("SceneDataSource on game object [{0}] - Creation", instanceGo);
            }

            return instance;
        }

        /// <summary>
        /// Create a new instance of this streamer behaviour and attaches it to the provided GameObject
        /// </summary>
        /// <param name="instanceGo">GameObject the behaviour has to be assigned to</param>
        /// <param name="sceneStreamerId">Id of the scene streamer we should connect to</param>
        /// <param name="endPointIP">IP of the scene streamer</param>
        /// <param name="portNumber">Port number of the scene streamer</param>
        /// <param name="streamingMode">Streaming mode from the data streamer (e.g. raw stream or merged)</param>
        /// <returns>Instance created</returns>
        internal static SceneDataSource CreateInstance(GameObject instanceGo, string sceneStreamerId, string endPointIP, int portNumber, TrackingServiceSceneDataStreamModes streamingMode)
        {
            return CreateInstance(instanceGo, new TrackingServiceSceneDataClient { Id = sceneStreamerId, IP = endPointIP, Port = portNumber }, streamingMode);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Makes the data source to connect to the underlying service, so that it can begin streaming scene data
        /// </summary>
        private void Connect()
        {
            if (!m_Connected)
            {
                m_DataStreamerClient.DataReady += DataStreamerClient_DataReady; // Register to event
                m_DataStreamerClient.Connect(StreamingMode); // Connect to the underlying service
                m_Connected = true;

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("SceneDataSource [{0}] - Connecting", gameObject.name);
                }
            }
        }

        /// <summary>
        /// Makes the data source to disconnect from the underlying service, so that it can begin streaming scene data
        /// </summary>
        private void Disconnect()
        {
            if (m_Connected)
            {
                m_DataStreamerClient.DataReady -= DataStreamerClient_DataReady; // De-register event
                m_DataStreamerClient.Disconnect(); // Disconnect from the underlying service
                m_Connected = false;

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("SceneDataSource [{0}] - Disconnecting", gameObject.name);
                }
            }
        }

        /// <summary>
        /// Callback for each new scene data frame ready
        /// </summary>
        /// <param name="trackingServiceClient"></param>
        /// <param name="sceneFrameDataArgs"></param>
        private void DataStreamerClient_DataReady(object trackingServiceClient, TrackingServiceSceneFrameReadyEventArgs sceneFrameDataArgs)
        {
            ExecuteOnUI(() =>
            {
                // Uncomment to log for each data frame arrived (i think it's not a great idea, if not for debugging purposes)
                //if (Log.IsDebugEnabled)
                //{
                //    var client = (TrackingServiceSceneDataClient)trackingServiceClient;
                //    Log.Debug("TrackingServiceManager.BodyStreamer[{0}] BodyCount: {1}", client.Id, bodyFrameDataArgs.Frame.Bodies.Count);
                //}

                // Copy internally last read data from the event (so it become synchronous with Unity update system)
                m_LastReadFrame = sceneFrameDataArgs.Frame;
            });
        }

        #endregion

        #region Reference counting methods

        /// <summary>
        /// Increment reference counting, signaling a new connection to this scene data source updates
        /// </summary>
        internal void NewProviderClient()
        {
            // Increment reference counter. If this is the first connection, connect the object to the underlying service
            m_ProvidersConnnectedToThis++;

            if (m_ProvidersConnnectedToThis == 1)
            {
                Connect();
            }

            if (Log.IsDebugEnabled)
            {
                Log.Debug("SceneDataSource [{0}] - Provider Clients are now {1}", gameObject.name, m_ProvidersConnnectedToThis);
            }
        }

        /// <summary>
        /// Decrement reference counting, signaling the end of a connection to this scene data source updates
        /// </summary>
        internal void EndProviderClient()
        {
            // Decrement reference counter. If this is the last connection, disconnect the object from the underlying service
            m_ProvidersConnnectedToThis--;

            if (m_ProvidersConnnectedToThis == 0)
            {
                Disconnect();
            }

            if (Log.IsDebugEnabled)
            {
                Log.Debug("SceneDataSource [{0}] - Provider Clients are now {1}", gameObject.name, m_ProvidersConnnectedToThis);
            }
        }

        #endregion

        #region Behaviour lifetime methods

        /// <summary>
        /// Start this instance
        /// </summary>
        private void Start()
        {
        }

        /// <summary>
        /// Called when this instance is destroyed
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Disconnect();
        }

        /// <summary>
        /// Updates this instance
        /// </summary>
        private new void Update()
        {
            // To make work EventBasedMonobehaviour loop
            base.Update();
        }

        #endregion
    }
}