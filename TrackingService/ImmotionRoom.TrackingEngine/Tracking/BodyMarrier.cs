namespace ImmotionAR.ImmotionRoom.TrackingEngine.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Helpers;
    using Interfaces;
    using Model;
    using Tools;

    /// <summary>
    ///     Find matches of same human bodies seen by different DataSources.
    ///     Its purpose is find the BodyDatas, in different DataSources, that correspond to the same physical persons and to
    ///     create and update the persons object, that represent the physical people in front of the system
    /// </summary>
    internal class BodyMarrier
    {
        /// <summary>
        ///     Maximum distance between two bodies to be considered mergeable
        /// </summary>
        private const float MaxMergingBodiesDistance = 0.45f;

        /// <summary>
        ///     Time of stable body matching for a temporary marriage to be considered as reliable
        /// </summary>
        private const float TemporaryMarriagesStableTime = 0.3f;

        /// <summary>
        ///     The filter parameters
        /// </summary>
        private static readonly TransformSmoothParameters m_FilterParams = new TransformSmoothParameters(0.5f, 0.75f, 1.0f, 0.13f, 0.25f);

        //private static readonly TransformSmoothParameters m_FilterParams = new TransformSmoothParameters(0.6f, 0.4f, 1.0f, 0.07f, 0.35f);
        ////DIRTY

        /// <summary>
        ///     The filter parameters to specify running average update for the merged body confidence
        /// </summary>
        private static readonly float m_FilterRunningAvgConfidence = 0.66f;

        /// <summary>
        ///     ID assigned to last identified merging body
        /// </summary>
        private ulong m_CurrentBodyId;

        /// <summary>
        ///     The merging bodies currently managed by this object
        /// </summary>
        private readonly List<BaseMergingBody> m_Bodies;

        /// <summary>
        ///     DataSources that are the actual source of body data
        /// </summary>
        private readonly IBodyDataProvider m_BodyDataProvider;

        /// <summary>
        ///     Array that, for each DataSources, memorizes which MergingBody that DataSource is providing data to.
        ///     This is useful so the same DataSource can't provide two bodies into the same MergingBody, to remove untracked
        ///     bodies
        ///     and much more :)
        ///     The array has one entry for each DataSource.
        ///     Every array value is a map with two sub-values:
        ///     - a ulong id key of the BodyData that the DataSources read
        ///     - a value of the MergingBody which the joint man belongs to
        /// </summary>
        private readonly Dictionary<string, Dictionary<ulong, BaseMergingBody>> m_BodiesOfDataSources;

        /// <summary>
        ///     Temporary marriages found by this body marrier. They are the associations between a tracking box raw body and a
        ///     merging body that have been found, but are not
        ///     still considered as stable. When the associations become stable, they are put inside m_BodiesOfDataSources data
        ///     struct.
        ///     Notice that this dictionary holds also the newly found bodies for which no matching body gets found.
        /// </summary>
        private readonly Dictionary<string, Dictionary<ulong, TemporaryMarriage>> m_TemporaryMarriagesOfDataSources;

        #region Public properties

        /// <summary>
        ///     Gets the persons tracked by the object
        /// </summary>
        /// <value>The merged people</value>
        public List<BaseMergingBody> MergedBodies
        {
            get { return m_Bodies; }
        }

        #endregion

        /// <summary>
        ///     Initializes a new instance of the <see cref="BodyMarrier" /> class.
        /// </summary>
        /// <param name="bodyDataProvider">Source DataSource from which read the body data from</param>
        public BodyMarrier(IBodyDataProvider bodyDataProvider)
        {
            m_CurrentBodyId = 0;
            m_BodyDataProvider = bodyDataProvider;
            m_Bodies = new List<BaseMergingBody>();
            m_BodiesOfDataSources = new Dictionary<string, Dictionary<ulong, BaseMergingBody>>(StringComparer.OrdinalIgnoreCase);
            m_TemporaryMarriagesOfDataSources = new Dictionary<string, Dictionary<ulong, TemporaryMarriage>>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Update this instance, reading informations from already updated DataSource sources
        /// </summary>
        public void Update(double deltaTime, CalibrationSettings settings)
        {
            // For each DataSource
            for (var i = 0; i < m_BodyDataProvider.DataSources.Count; i++)
            {
                // Add to the internal collection of DataSources if not already present
                // This should happen at start only... 
                var dataSource = m_BodyDataProvider.DataSources.ElementAt(i);
                if (!m_BodiesOfDataSources.ContainsKey(dataSource.Key))
                {
                    m_BodiesOfDataSources.Add(dataSource.Key, new Dictionary<ulong, BaseMergingBody>());
                    m_TemporaryMarriagesOfDataSources.Add(dataSource.Key, new Dictionary<ulong, TemporaryMarriage>());
                }

                // Take all tracked bodies and their IDs
                var dataSourceMen = dataSource.Value.Bodies;
                IList<ulong> dataSourceMenIDs = dataSourceMen.Select(body => body.Id).ToList(); //LINQ

                //take all IDs of the merged bodies this data source is providing a tracked body to
                IList<ulong> providedMergedMenIDs = m_BodiesOfDataSources[dataSource.Key].Keys.ToList();

                //see if the bodies tracked by this data source are exactly the same we had already assigned to some merged body in the last
                //frames. If it is so, we can just update all the bodies (this should happen 90% of the frames)
                if (dataSourceMenIDs.ScrambledEquals(providedMergedMenIDs))
                {
                    //clear list of temporary marriages, if any.
                    //If we are here, for this data source we should have no pending marriages, so delete stale entries 
                    //(they could be caused by glitch bodies that are appeared in one frame and disappeared the frame after, so the
                    // algorithm didn't manage to assign them to any merging body)
                    if (m_TemporaryMarriagesOfDataSources[dataSource.Key].Count > 0)
                        m_TemporaryMarriagesOfDataSources[dataSource.Key].Clear();

                    // Cycle through the bodies 
                    foreach (var man in dataSourceMen)
                    {
                        var masterTransform = Matrix4x4.Identity;
                        if (settings.SlaveToMasterCalibrationMatrices.ContainsKey(dataSource.Key))
                        {
                            masterTransform = settings.SlaveToMasterCalibrationMatrices[dataSource.Key];
                        }

                        var masterTransformedMan = new BodyData(man, masterTransform, man.DataSources);

                        //update it with new data
                        UpdateExistingBody(m_BodiesOfDataSources[dataSource.Key][masterTransformedMan.Id], m_BodyDataProvider.DataSourceMapping[dataSource.Key], masterTransformedMan);
                    }
                }
                //else, we have in this frame, for this data source a (new) non-assigned body or an assigned body that exists no more.
                //Notice that "new" body does not mean necessarily that it is new in this frame... it means that it is still un-assigned
                //(maybe it has a marriage that is not reached the stable time)
                else
                {
                    var deletedItems = providedMergedMenIDs.Except(dataSourceMenIDs).ToList();
                    var newItems = dataSourceMenIDs.Except(providedMergedMenIDs).ToList();

                    //create array of data source men, transformed in master data source frame of reference (so that they can be merged in a common frame of reference)
                    IList<BodyData> masterTransformedDataSourceMen = new List<BodyData>();

                    foreach (var man in dataSourceMen)
                    {
                        var masterTransform = Matrix4x4.Identity;
                        if (settings.SlaveToMasterCalibrationMatrices.ContainsKey(dataSource.Key))
                        {
                            masterTransform = settings.SlaveToMasterCalibrationMatrices[dataSource.Key];
                        }

                        masterTransformedDataSourceMen.Add(new BodyData(man, masterTransform, man.DataSources));
                    }

                    //this structure will hold the IDs of the bodies read from this data source that must not be used to update the merging bodies.
                    //It is useful because the method RefactorBodiesUsingStableMarriages will already call Update on some merging bodies using
                    //new body data, so we must NOT call Update again for these bodies
                    var toNotUpdateBodyDataSourcesId = new HashSet<ulong>();

                    //check if we have assigned bodies that exists no more, and delete them from everywhere
                    if (deletedItems.Any())
                        //if we had in previous frame a body tracked by this DataSource that it is not still present in current frame,
                        //delete its reference from this class
                        foreach (var id in deletedItems)
                        {
                            m_BodiesOfDataSources[dataSource.Key][id].RemoveBody(id, m_BodyDataProvider.DataSourceMapping[dataSource.Key]);
                            m_BodiesOfDataSources[dataSource.Key].Remove(id);
                        }

                    //check if we have new unassigned bodies and if so, launch the stable marriages algorithm to match them with 
                    //the merged bodies.
                    //Notice that we try to re-match ALL the tracked bodies with the merged bodies, because the new body could be a previously
                    //not seen body that has to match with a particular merged body (so present matches could change)
                    if (newItems.Any())
                    {
                        //perform stable marriages algorithm
                        BodyDataForStableMarriage[] mergedBodiesMarriages;
                        BodyDataForStableMarriage[] dataSourceBodiesMarriages;

                        BodyAndMergedBodiesStableMarriages(masterTransformedDataSourceMen, out mergedBodiesMarriages, out dataSourceBodiesMarriages);

                        //use stable marriages results to update current merged bodies detection
                        RefactorBodiesUsingStableMarriages(deltaTime, dataSource.Key, dataSourceBodiesMarriages, toNotUpdateBodyDataSourcesId);
                    }

                    //we can now update merging bodies that have not be touched by body refactoring

                    // Cycle through the source bodies 
                    foreach (var masterTransformedMan in masterTransformedDataSourceMen)
                    {
                        //update its corresponding merging body with new data, if any.
                        //Notice that if its ID is in the blacklist of already updated body, we do nothing
                        if (m_BodiesOfDataSources[dataSource.Key].ContainsKey(masterTransformedMan.Id) && !toNotUpdateBodyDataSourcesId.Contains(masterTransformedMan.Id))
                            UpdateExistingBody(m_BodiesOfDataSources[dataSource.Key][masterTransformedMan.Id], m_BodyDataProvider.DataSourceMapping[dataSource.Key], masterTransformedMan);
                    }
                }
            }

            //delete stale entries of merging bodies
            UpdateRemoveStaleEntries();

            //after all, merge bodies for this frame
            foreach (var mergingBody in m_Bodies)
                mergingBody.Merge();
        }

        /// <summary>
        ///     Refactor all bodies assignments using new stable marriages results
        /// </summary>
        /// <param name="deltaTime">Time delta since last call</param>
        /// <param name="dataSourceId">ID of the data source of interest</param>
        /// <param name="dataSourceBodiesMarriages">Results of stable marriages operation, seen from data source bodies perspective</param>
        /// <param name="toNotUpdateBodyDataSourcesId">
        ///     Data structure into which put the IDs of the body for which the
        ///     AddUpdateBody on the corresponding MergingBody has already been called
        /// </param>
        private void RefactorBodiesUsingStableMarriages(double deltaTime, string dataSourceId, BodyDataForStableMarriage[] dataSourceBodiesMarriages, HashSet<ulong> toNotUpdateBodyDataSourcesId)
        {
            //Ok, in this method we should distinguish 2 kinds of marriage (my name assigment has not been very smart):
            //- body marriages refers to the results of the stable marriages algorithm between data source body and merged bodies
            //- temporary marriages refers to the matching of the source body with merging body that must remain stable for some seconds
            //  before being considered as acceptable and then make the strong assignement, passing that source body to the body merger

            //for each data source body (with its relative marriage informations)
            foreach (var dataSourceBodyMarriage in dataSourceBodiesMarriages)
            {
                //if we already have this body inside the temporary marriages list
                if (m_TemporaryMarriagesOfDataSources[dataSourceId].ContainsKey(dataSourceBodyMarriage.BodyData.Id))
                {
                    //get its current temporary marriage
                    var temporaryMarriage = m_TemporaryMarriagesOfDataSources[dataSourceId][dataSourceBodyMarriage.BodyData.Id];

                    //if we have found again the same matching, increment matching time. If time is enough, convert this marriage to a body merging
                    if ((temporaryMarriage.FoundMergedBody == false && !dataSourceBodyMarriage.Engaged) || (temporaryMarriage.FoundMergedBody && dataSourceBodyMarriage.Engaged && dataSourceBodyMarriage.CurrentFiance.BodyData.Id == temporaryMarriage.MergedBodyId))
                    {
                        m_TemporaryMarriagesOfDataSources[dataSourceId][dataSourceBodyMarriage.BodyData.Id] = new TemporaryMarriage {FoundMergedBody = temporaryMarriage.FoundMergedBody, MergedBodyId = temporaryMarriage.MergedBodyId, Time = temporaryMarriage.Time + deltaTime};

                        if (temporaryMarriage.Time + deltaTime >= TemporaryMarriagesStableTime)
                        {
                            //delete any previous association of this source body with a merging body, if different from current one
                            if (m_BodiesOfDataSources[dataSourceId].ContainsKey(dataSourceBodyMarriage.BodyData.Id))
                            {
                                if (!(dataSourceBodyMarriage.Engaged && dataSourceBodyMarriage.CurrentFiance.BodyData.Id == m_BodiesOfDataSources[dataSourceId][dataSourceBodyMarriage.BodyData.Id].Id))
                                    m_BodiesOfDataSources[dataSourceId][dataSourceBodyMarriage.BodyData.Id].RemoveBody(dataSourceBodyMarriage.BodyData.Id, m_BodyDataProvider.DataSourceMapping[dataSourceId]);
                            }

                            //if at least a matching merging body has been found, add the newly found body to it.
                            //if another source body was assigned to it, remove that association
                            if (dataSourceBodyMarriage.Engaged)
                            {
                                //find the merging body corresponding to the marriage (we have only a bodydata, not a MergingBody structure)
                                var mergedBody = m_Bodies.Find(body => body.Id == dataSourceBodyMarriage.CurrentFiance.BodyData.Id);

                                //look if we have any source body previously assigned to that merging body (different from current one, but from the same data source)
                                var bodyDataSourcePair = m_BodiesOfDataSources[dataSourceId].FirstOrDefault(keyValuePair => keyValuePair.Value.Id == mergedBody.Id && keyValuePair.Key != dataSourceBodyMarriage.BodyData.Id);

                                //if it is so, remove that assignment
                                if (bodyDataSourcePair.Value != null)
                                {
                                    m_BodiesOfDataSources[dataSourceId][bodyDataSourcePair.Key].RemoveBody(bodyDataSourcePair.Key, m_BodyDataProvider.DataSourceMapping[dataSourceId]);
                                    m_BodiesOfDataSources[dataSourceId].Remove(bodyDataSourcePair.Key);
                                }

                                //add new assignment of this data source body with newly found merging body.
                                //Notice that we could be here even if we are re-assigning a source body to a merging body it was already assigned to. In this case, the following
                                //lines are harmless and only update the body
                                mergedBody.AddUpdateBody(dataSourceBodyMarriage.BodyData, m_BodyDataProvider.DataSourceMapping[dataSourceId]);
                                m_BodiesOfDataSources[dataSourceId][dataSourceBodyMarriage.BodyData.Id] = mergedBody;
                                toNotUpdateBodyDataSourcesId.Add(dataSourceBodyMarriage.BodyData.Id);
                            }
                            //else, create a new merging body for this body
                            else
                            {
                                m_Bodies.Add(new MergingBodyReliable(++m_CurrentBodyId, dataSourceBodyMarriage.BodyData, m_BodyDataProvider.DataSourceMapping[dataSourceId], m_FilterParams, m_FilterRunningAvgConfidence));
                                //m_Bodies.Add(new MergingBodyPro(++m_CurrentBodyId, man, m_FilterParams));
                                m_BodiesOfDataSources[dataSourceId][dataSourceBodyMarriage.BodyData.Id] = m_Bodies.Last();
                                toNotUpdateBodyDataSourcesId.Add(dataSourceBodyMarriage.BodyData.Id);
                            }

                            //remove temporary marriage entry
                            m_TemporaryMarriagesOfDataSources[dataSourceId].Remove(dataSourceBodyMarriage.BodyData.Id);
                        }
                    }
                    //else, if we have found another matching, reset current temporary marriage with new matching
                    else
                    {
                        m_TemporaryMarriagesOfDataSources[dataSourceId][dataSourceBodyMarriage.BodyData.Id] = new TemporaryMarriage {FoundMergedBody = dataSourceBodyMarriage.Engaged, MergedBodyId = dataSourceBodyMarriage.Engaged ? dataSourceBodyMarriage.CurrentFiance.BodyData.Id : 0, Time = 0};
                    }
                }
                //else, if we have never seen this body before for a definitive marriage
                else
                {
                    //if at least a matching merging body has been found, add the newly found temporary marriage
                    if (dataSourceBodyMarriage.Engaged)
                    {
                        m_TemporaryMarriagesOfDataSources[dataSourceId][dataSourceBodyMarriage.BodyData.Id] = new TemporaryMarriage {FoundMergedBody = true, MergedBodyId = dataSourceBodyMarriage.CurrentFiance.BodyData.Id, Time = 0};
                    }
                    //else, add a temporary marriage with no matching
                    else
                    {
                        m_TemporaryMarriagesOfDataSources[dataSourceId][dataSourceBodyMarriage.BodyData.Id] = new TemporaryMarriage {FoundMergedBody = false, MergedBodyId = 0, Time = 0};
                    }
                }
            }
        }

        /// <summary>
        ///     Performs stable marriage algorithm to match data source bodies of current frame with merged bodies of last frame
        /// </summary>
        /// <param name="dataSourceMen">BodyData read from a particular data source</param>
        /// <param name="mergedBodiesAsMen">Data structure that will hold stable marriage situation for merged bodies</param>
        /// <param name="dataSourceBodiesAsWomen">Data structure that will hold stable marriage situation data source bodies</param>
        private void BodyAndMergedBodiesStableMarriages(IList<BodyData> dataSourceMen, out BodyDataForStableMarriage[] mergedBodiesAsMen,
            out BodyDataForStableMarriage[] dataSourceBodiesAsWomen)
        {
            //slightly different version of original algorithm specified at https://en.wikipedia.org/wiki/Stable_marriage_problem
            //to consider that too distant bodies have to be discarded. 
            //Notice that men are represented by merged bodies, because algorithm is optimal for men. This means that each merged body
            //will get the unassigned body nearest to it

            //create arrays for men and women of the marriages, and fill them with data from merged bodies and from kinect source
            mergedBodiesAsMen = new BodyDataForStableMarriage[m_Bodies.Count];
            dataSourceBodiesAsWomen = new BodyDataForStableMarriage[dataSourceMen.Count];

            for (var i = 0; i < mergedBodiesAsMen.Length; i++)
            {
                mergedBodiesAsMen[i] = new BodyDataForStableMarriage(m_Bodies[i].LastFilteredMan);
            }

            for (var i = 0; i < dataSourceBodiesAsWomen.Length; i++)
            {
                dataSourceBodiesAsWomen[i] = new BodyDataForStableMarriage(dataSourceMen[i]);
            }

            //now make everyone calculate its preference array with its possible mate

            for (var i = 0; i < mergedBodiesAsMen.Length; i++)
            {
                mergedBodiesAsMen[i].Initialize(dataSourceBodiesAsWomen, MaxMergingBodiesDistance);
            }

            for (var i = 0; i < dataSourceBodiesAsWomen.Length; i++)
            {
                dataSourceBodiesAsWomen[i].Initialize(mergedBodiesAsMen, MaxMergingBodiesDistance);
            }

            //start stable marriages algorithm
            var freeMenThatCanPropose = true; //true if there are still free men that have to be paired and that have still a woman to propose to

            //while there is at least a free man
            while (freeMenThatCanPropose)
            {
                //cycle through men
                for (var i = 0; i < mergedBodiesAsMen.Length; i++)
                {
                    //if this man is free
                    if (!mergedBodiesAsMen[i].Engaged)
                    {
                        //make it propose to a woman
                        mergedBodiesAsMen[i].ProposeToNext();
                    }
                }

                //see if we have some free men left after this iteration
                freeMenThatCanPropose = Array.FindIndex(mergedBodiesAsMen, bodyForMarriage => !bodyForMarriage.Engaged && bodyForMarriage.CanProposeToAnotherOne) >= 0;
            }
        }

        /// <summary>
        ///     Updates an existing body with new data from a read body
        /// </summary>
        /// <param name="mergingBody">Merging body</param>
        /// <param name="bodySourceId">ID of the tracking source that provided the man</param>
        /// <param name="man">Read body</param>
        private static void UpdateExistingBody(BaseMergingBody mergingBody, byte bodySourceId, BodyData man)
        {
            mergingBody.AddUpdateBody(man, bodySourceId);
        }

        /// <summary>
        ///     Updates this object, removing the stale entries, i.e. the merging bodies that have no more bodies to merge
        /// </summary>
        private void UpdateRemoveStaleEntries()
        {
            //remove all merging bodies that have a number of body to merge that is 0
            for (var i = m_Bodies.Count - 1; i >= 0; i--)
                if (m_Bodies.ElementAt(i).MergingBodiesNumber < 1)
                    m_Bodies.RemoveAt(i);
        }
    }
}