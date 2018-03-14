namespace ImmotionAR.ImmotionRoom.TrackingEngine.Tracking
{
    using System;
    using System.Collections.Generic;
    using Interfaces;
    using Logger;
    using Model;
    using Walking;

    /// <summary>
    ///     DataSources people tracker: tracks multiple people using multiple DataSources
    /// </summary>
    public class DataSourcesPeopleTracker
    {
        /// <summary>
        ///     The element that finds matches between bodies present across different DataSources
        /// </summary>
        private readonly BodyMarrier m_BodiesMatcher;

        /// <summary>
        ///     The calibration data for the tracking system
        /// </summary>
        private readonly CalibrationSettings m_CalibrationData;

        /// <summary>
        ///     Gestures detector for Walking
        /// </summary>
        private readonly IDictionary<ulong, IPlayerWalkingDetector> m_PlayerWalkingDetectors;

        /// <summary>
        ///     Detected Gestures for each body
        /// </summary>
        private readonly IDictionary<ulong, IDictionary<BodyGestureTypes, BodyGesture>> m_PlayerGestures;

        private readonly WalkingDetectionConfiguration m_WalkingDetectionConfiguration;
        
        #region Public properties

        /// <summary>
        ///     Gets the persons tracked by the object
        /// </summary>
        /// <value>The people tracked by the object</value>
        public BodyData[] People
        {
            get
            {
                // Get all people tracked, as merging bodies, and return them applying the master DataSource to world transformation
                // GIANNI: this was in the old code. NOW, we apply the required transformation in the BodyData Streamer
                var retVal = new BodyData[m_BodiesMatcher.MergedBodies.Count];

                for (var i = 0; i < m_BodiesMatcher.MergedBodies.Count; i++)
                {
                    // Check if return value is of type BodyData or not (MergingBodyPro returns BodyData data, MergingBody simple BodyData data)
                    // and act accordingly in returning the data
                    var lastFilteredManAsTracked = m_BodiesMatcher.MergedBodies[i].LastFilteredMan;
                    retVal[i] = new BodyData(lastFilteredManAsTracked, Matrix4x4.Identity, lastFilteredManAsTracked.DataSources, m_PlayerGestures[lastFilteredManAsTracked.Id]);
                }

                return retVal;
            }
        }

        ///// <summary>
        /////     Gets the Walking Gestures tracked by the object
        ///// </summary>
        //internal PlayerWalkingDetection[] WalkingGestures
        //{
        //    get
        //    {
        //        // Get all detected walking gestures for all tracked people
        //        var retVal = new PlayerWalkingDetection[m_PlayerWalkingDetectors.Count];
        //        int i = 0;
        //        foreach (var playerWalkingDetector in m_PlayerWalkingDetectors)
        //        {
                    
        //            m_PlayerGestures[playerWalkingDetector.Key][BodyGestureTypes.Walking] = playerWalkingDetector.Value.CurrentDetection;
        //            i++;
        //        }

        //        return retVal;
        //    }
        //}

        #endregion

        public DataSourcesPeopleTracker(IBodyDataProvider bodyDataProvider, CalibrationSettings calibrationData, WalkingDetectionConfiguration walkingDetectionConfiguration)
        {
            m_CalibrationData = calibrationData;
            m_BodiesMatcher = new BodyMarrier(bodyDataProvider);
            m_WalkingDetectionConfiguration = walkingDetectionConfiguration;
            m_PlayerWalkingDetectors = new Dictionary<ulong, IPlayerWalkingDetector>();
            m_PlayerGestures = new Dictionary<ulong, IDictionary<BodyGestureTypes, BodyGesture>>();
        }

        /// <summary>
        ///     Update this instance for each rendering frame
        /// </summary>
        public void Update(double deltaTime, TimeSpan incrementalTime)
        {
            m_BodiesMatcher.Update(deltaTime, m_CalibrationData);
            
            // Detect walking gestures 
            var previousBodyIds = new HashSet<ulong>(m_PlayerWalkingDetectors.Keys);
            foreach (var body in m_BodiesMatcher.MergedBodies)
            {
                previousBodyIds.Remove(body.Id);

                if (!m_PlayerWalkingDetectors.ContainsKey(body.Id))
                {
                    m_PlayerWalkingDetectors.Add(body.Id, PlayerWalkingDetectorFactory.CreateDetector(m_WalkingDetectionConfiguration.WalkingDetector, m_WalkingDetectionConfiguration.Parameters));
                }
                
                m_PlayerWalkingDetectors[body.Id].UpdateDetection(incrementalTime, body.LastFilteredMan);

                if (!m_PlayerGestures.ContainsKey(body.Id))
                {
                    m_PlayerGestures.Add(body.Id, new Dictionary<BodyGestureTypes, BodyGesture>());
                }
                
                m_PlayerGestures[body.Id][BodyGestureTypes.Walking] = m_PlayerWalkingDetectors[body.Id].CurrentDetection;
            }

            // Remove no-more-tracked bodies
            foreach (var bodyId in previousBodyIds)
            {
                m_PlayerWalkingDetectors.Remove(bodyId);
                m_PlayerGestures[bodyId].Clear();
                m_PlayerGestures.Remove(bodyId);
            }
        }
    }
}