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
    using System.Linq;
    using TrackingService.ControlClient.Model;
    using TrackingService.DataClient.Model;

    /// <summary>
    /// Wraps a SceneDataProvider, to obtain data about a specific body
    /// </summary>
    public class BodyDataProvider
    {
        #region Private fields

        /// <summary>
        /// Provider of data about the scene
        /// </summary>
        private SceneDataProvider m_sceneDataProvider;

        /// <summary>
        /// Body ID we are following
        /// </summary>
        private ulong m_trackedBodyId;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the <see cref="SceneDataProvider"/> wrapped by this object
        /// </summary>
        public SceneDataProvider ActualSceneDataProvider
        {
            get
            {
                return m_sceneDataProvider;
            }
        }

        /// <summary>
        /// Get last body value of the desired body
        /// </summary>
        public TrackingServiceBodyData LastBody
        {
            get
            {
                return m_sceneDataProvider.LastBodies.FirstOrDefault(body => body.Id == m_trackedBodyId);
            }
        }
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Construct a scene data provider, given the associated scene data source and the id of the body we should track
        /// </summary>
        /// <param name="dataProvider">Actual scene data provider</param>
        /// <param name="bodyId">Id of the body to track</param>
        public BodyDataProvider(SceneDataProvider dataProvider, ulong bodyId)
        {
            m_sceneDataProvider = dataProvider;
            m_trackedBodyId = bodyId;
        }
        
        #endregion
    }
}