namespace ImmotionAR.ImmotionRoom.TrackingEngine.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Model;

    /// <summary>
    ///     Wraps BodyData structure for usage inside a stable marriage matching algorithm
    /// </summary>
    internal class BodyDataForStableMarriage
    {
        /// <summary>
        ///     Actual body data wrapped by this object
        /// </summary>
        internal BodyData BodyData { get; set; }

        /// <summary>
        ///     Current preference array of this object
        /// </summary>
        private BodyDataDistance[] Preferences;

        /// <summary>
        ///     Current Fiance of this object, i.e. the counterpart with which this element is currently engaged (-1 if no fiance)
        /// </summary>
        internal int CurrentFianceId { get; set; }

        /// <summary>
        ///     Next possible fiance we will propose if current marriage (if any) will fail
        /// </summary>
        internal int NextProposingFianceId { get; set; }

        #region Public Properties

        /// <summary>
        ///     Get if element is currently engaged
        /// </summary>
        public bool Engaged
        {
            get { return CurrentFianceId != -1; }
        }

        /// <summary>
        ///     Get current fiance of this element, if any (if not exists, it returns null)
        /// </summary>
        public BodyDataForStableMarriage CurrentFiance
        {
            get
            {
                if (Engaged)
                    return Preferences[CurrentFianceId].BodyDataForMarriage;
                return null;
            }
        }

        /// <summary>
        ///     True if this element has other element to propose to; false if it has not
        /// </summary>
        public bool CanProposeToAnotherOne
        {
            get { return NextProposingFianceId < Preferences.Length; }
        }

        #endregion

        /// <summary>
        ///     Data structure containing a body and the distance of current object with that body
        /// </summary>
        private struct BodyDataDistance
        {
            /// <summary>
            ///     Body
            /// </summary>
            internal BodyDataForStableMarriage BodyDataForMarriage;

            /// <summary>
            ///     Distance from body
            /// </summary>
            internal float Distance;
        }

        /// <summary>
        ///     Create a stable marriage proposer structure (for men and women)
        /// </summary>
        /// <param name="actualBodyData">Actual body data to wrap inside this data struct</param>
        public BodyDataForStableMarriage(BodyData actualBodyData)
        {
            //assign body data and set actual fiance to nothing (-1) and next fiance to propose to id 0
            BodyData = actualBodyData;
            CurrentFianceId = -1;
            NextProposingFianceId = 0;
            Preferences = new BodyDataDistance[0];
        }

        /// <summary>
        ///     Initialize a stable marriage proposer structure (for men and women),
        ///     searching for this object, the preference for each possible mate
        /// </summary>
        /// <param name="possibleFiances">List of all possible body counterparts this object could be matched with</param>
        /// <param name="maximumDistance">
        ///     Maximum distance between this body and a counterpart for a marriage to be considered as
        ///     possible
        /// </param>
        public void Initialize(IList<BodyDataForStableMarriage> possibleFiances, float maximumDistance)
        {
            //calculate distance with all counterparts and sort the list according to this value
            var fiancesWDistances = new List<BodyDataDistance>();

            for (var i = 0; i < possibleFiances.Count; i++)
            {
                var distance = Vector3.Distance(BodyData.StableCentroid, possibleFiances[i].BodyData.StableCentroid);

                if (distance < maximumDistance)
                    fiancesWDistances.Add(new BodyDataDistance {BodyDataForMarriage = possibleFiances[i], Distance = distance});
            }

            //preference will hold all the body datas, in order of preference
            Preferences = fiancesWDistances.OrderBy(fiance => fiance.Distance).ToArray();
        }

        /// <summary>
        ///     Propose to next possible fiancee candidate, if any.
        ///     This function is the one called by men candidates
        /// </summary>
        public void ProposeToNext()
        {
            //if there is still someone to propose to
            if (CanProposeToAnotherOne)
            {
                //try to propose to next, and if it accept the proposal, get engaged
                if (Preferences[NextProposingFianceId].BodyDataForMarriage.EvaluateProposal(this))
                    CurrentFianceId = NextProposingFianceId;

                //anyway, increment counter, so next time we will propose to the next in the list
                NextProposingFianceId++;
            }
        }

        /// <summary>
        ///     Evaluate a proposal of marriage: if it is ok, free from current engagement (if any) and engage with the proposer.
        ///     This function is the one executed by women candidates
        /// </summary>
        /// <param name="bodyDataForStableMarriage">The proposer of the engagement</param>
        /// <returns>True if engagement gets accepted; false oterhwise</returns>
        private bool EvaluateProposal(BodyDataForStableMarriage bodyDataForStableMarriage)
        {
            //find the rank of the proposer in the preference array
            var proposerRankId = Array.FindIndex(Preferences, body => body.BodyDataForMarriage.BodyData.Id == bodyDataForStableMarriage.BodyData.Id);

            //if current element is not engaged, or if the proposer is more preferable than current engagement
            if (!Engaged || proposerRankId < CurrentFianceId)
            {
                //break current engagement
                if (Engaged)
                    Preferences[CurrentFianceId].BodyDataForMarriage.CurrentFianceId = -1;

                //create new engagement
                CurrentFianceId = proposerRankId;
                return true;
            }
                //else, don't care. We are not made to love each other :)
            return false;
        }
    }
}
