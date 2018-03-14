using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImmotionAR.ImmotionRoom.TrackingEngine.Tracking
{
    /// <summary>
    /// Holds data about a temporary marriage between a tracking box body and a merged body.
    /// The marriage is temporary until it it seen that it is stable over time... at that point the t-box body becomes part of the bodies merged to form that merged body
    /// </summary>
    internal struct TemporaryMarriage
    {
        /// <summary>
        /// True if a matching merging body has been found; otherwise false
        /// </summary>
        public bool FoundMergedBody;

        /// <summary>
        /// Id of the merged body this marriage is referred to
        /// </summary>
        public ulong MergedBodyId;

        /// <summary>
        /// Time since the marriage is stable
        /// </summary>
        public double Time;
    }
}
