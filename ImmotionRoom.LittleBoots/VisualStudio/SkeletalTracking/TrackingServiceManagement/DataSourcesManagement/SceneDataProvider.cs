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
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Logger;
    using System;
    using System.Collections.Generic;
    using TrackingService.ControlClient.Model;
    using TrackingService.DataClient.Model;

    /// <summary>
    /// Provides a scene data source stream to clients.
    /// Internally, this object wraps a reference to a <see cref="SceneDataSource"/> object, hiding it and providing the indirection for reference counting for connecting 
    /// operations (that is actually implemented in <see cref="TrackingServiceManager"/> class).
    /// Use this class to get updated info about the scene as it is seen by a certain data source
    /// </summary>
    public class SceneDataProvider : IDisposable
    {
        #region Private members
        
        /// <summary>
        /// Scene data source
        /// </summary>
        private readonly SceneDataSource m_sceneDataSource;
        
        #endregion
        
        #region Public properties
        
        /// <summary>
        /// Get if the provider is still linked to a valid scene data source and the data source is still reading valid data
        /// </summary>
        public bool IsStillValid
        {
            get
            {
                return !m_disposed && m_sceneDataSource != null && m_sceneDataSource.IsConnected;
            }
        }
        
        /// <summary>
        /// Get stream id
        /// </summary>
        public string StreamId
        {
            get
            {
                return m_sceneDataSource.StreamId;
            }
        }
        
        /// <summary>
        /// Get streaming mode of this stream
        /// </summary>
        public TrackingServiceSceneDataStreamModes StreamingMode
        {
            get
            {
                return m_sceneDataSource.StreamingMode;
            }
        }

        /// <summary>
        /// Get last frame read from the source
        /// </summary>
        public TrackingServiceSceneFrame LastReadFrame
        {
            get
            {
                return m_sceneDataSource.LastReadFrame;
            }
        }

        /// <summary>
        /// Get last bodies read from the source
        /// </summary>
        public IList<TrackingServiceBodyData> LastBodies
        {
            get
            {
                return m_sceneDataSource.LastBodies;
            }
        }
        
        /// <summary>
        /// Get timestamp of last read bodies
        /// </summary>
        public DateTime LastTimeStamp
        {
            get
            {
                return m_sceneDataSource.LastTimeStamp;
            }
        }
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Construct a scene data provider, given the associated scene data source.
        /// This create a linking with the provided data source (incremented reference counting)
        /// </summary>
        /// <param name="dataSource">Valid scene data source</param>
        internal SceneDataProvider(SceneDataSource dataSource)
        {
            m_sceneDataSource = dataSource;
            m_sceneDataSource.NewProviderClient(); //increment reference counting on the data source

            if (Log.IsDebugEnabled)
            {
                Log.Debug("SceneDataProvider - Creating data provider for stream [{0}]", ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.TrackingServiceManager.SceneDataSourcesManager.MangleBodyDataSourceId(StreamId, StreamingMode));
            }
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
        ~SceneDataProvider() 
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
                if (m_sceneDataSource)
                    m_sceneDataSource.EndProviderClient(); //decrement reference counting on the data source

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("SceneDataProvider - Stopping data provider for stream [{0}]", ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.TrackingServiceManagement.TrackingServiceManager.SceneDataSourcesManager.MangleBodyDataSourceId(StreamId, StreamingMode));
                }
            }

            m_disposed = true;
        }

        #endregion
    }
}