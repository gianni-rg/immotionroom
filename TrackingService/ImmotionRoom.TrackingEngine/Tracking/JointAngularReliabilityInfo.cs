using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImmotionAR.ImmotionRoom.TrackingEngine.Model;

namespace ImmotionAR.ImmotionRoom.TrackingEngine.Tracking
{
    /// <summary>
    /// Defines reliability of joint tracking wrt player orientation in the XZ plane as seen by a tracking box
    /// </summary>
    internal struct JointAngularReliabilityInfo
    {
        /// <summary>
        /// The center of the angular intervals. This should be the body angle with maximum reliability confidence
        /// </summary>
        public float CenterAngleInterval { get; set; }

        /// <summary>
        /// Extents (i.e. half of the total size) of the interval, in radians, of body orientation in the XZ plane wrt a certain tracking box to consider the reliability of that tracking box to be 1.0 wrt this joint.
        /// </summary>
        public float FullConfidenceAngleIntervalExtents { get; set; }

        /// <summary>
        /// Extents (i.e. half of the total size) of the interval, in radians, of body orientation in the XZ plane wrt a certain tracking box to consider the reliability of that tracking box to be > 0.0 wrt this joint.
        /// Outside this interval, data about this joint gathered from that tracking box will be discarded.
        /// </summary>
        public float ConfidenceAngleIntervalExtents { get; set; }

        /// <summary>
        /// Constructor with full initialization
        /// </summary>
        /// <param name="centerAngleInterval">Center of tracking intervals</param>
        /// <param name="fullConfidenceAngleIntervalExtents">Extents of full confidence interval</param>
        /// <param name="confidenceAngleIntervalExtents">Extents of reliable tracking interval</param>
        public JointAngularReliabilityInfo(float centerAngleInterval, float fullConfidenceAngleIntervalExtents, float confidenceAngleIntervalExtents) :
            this()
        {
            CenterAngleInterval = centerAngleInterval;
            FullConfidenceAngleIntervalExtents = fullConfidenceAngleIntervalExtents;
            ConfidenceAngleIntervalExtents = confidenceAngleIntervalExtents;
        }

        /// <summary>
        /// Compute reliability confidence of this joint wrt the body orientation
        /// </summary>
        /// <param name="bodyOrientation">Orientation of body to consider.</param>
        /// <returns>Reliability confidence</returns>
        public float ComputeReliabilityConfidence(float bodyOrientation)
        {
            const float highThresh1 = 30, highThresh2 = 65;
            const float midThresh1 = 83, midThresh2 = 119;
            //const float highThresh1 = 25, highThresh2 = 55;
            //const float midThresh1 = 95, midThresh2 = 130;
            if (CenterAngleInterval == 15 * MathConstants.Deg2Rad) // left
            {
                float degOrientation = bodyOrientation * MathConstants.Rad2Deg;

                if (degOrientation > -highThresh1 && degOrientation < highThresh2)
                    return 1;
                else if (degOrientation > -midThresh1 && degOrientation < -highThresh1)
                    return (degOrientation - -midThresh1) / (-highThresh1 - -midThresh1);
                else if (degOrientation > highThresh2 && degOrientation < midThresh2)
                    return (degOrientation - midThresh2) / (highThresh2 - midThresh2);
                else
                    return 0;
            }
            else if (CenterAngleInterval == -15 * MathConstants.Deg2Rad) // right
            {

                float degOrientation = bodyOrientation * MathConstants.Rad2Deg;

                if (degOrientation > -highThresh2 && degOrientation < highThresh1)
                    return 1;
                else if (degOrientation > highThresh1 && degOrientation < midThresh1)
                    return (degOrientation - midThresh1) / (highThresh1 - midThresh1);
                else if (degOrientation > -midThresh2 && degOrientation < -highThresh2)
                    return (degOrientation - -midThresh2) / (-highThresh2 - -midThresh2);
                else
                    return 0;
            }

            //abs distance from the center angle
            float absOrientation = Math.Abs(bodyOrientation - CenterAngleInterval);

            //if we are in full confidence range
            if (absOrientation < FullConfidenceAngleIntervalExtents)
                return 1.0f;
            //else, if we are in standard confidence range, return a value ranging from 0 to 1, where 0 is when the angle is ConfidenceAngleIntervalExtents
            else if (absOrientation < ConfidenceAngleIntervalExtents)
                return (absOrientation - ConfidenceAngleIntervalExtents) / (FullConfidenceAngleIntervalExtents - ConfidenceAngleIntervalExtents);
            //else, we have no confidence
            else
                return 0.0f;
        }
    }
}
