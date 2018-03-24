using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Uma
{
    /// <summary>
    /// Interface for communication of Uma dll to Actual UMA package for matching of physical features of avatar
    /// </summary>
    public interface IUmaPhysioMatchingBridge
    {
        /// <summary>
        /// True if the avatar is a UMA avatar, false if it is only a UMA-compliant avatar
        /// </summary>
        bool IsUmaAvatar
        {
            get;
        }
         
        /// <summary>
        /// Gets the IsShapeDirty flag on UMA avatars (always false for UMA compliant avatar)
        /// </summary>
        bool IsShapeDirty
        {
            get;
        }

        /// <summary>
        /// Performs a matching step for the avatar body physio matching process, incrementing sliders of the UMA Dna of the required step.
        /// This method should do nothing for UMA compliant avatars
        /// </summary>
        /// <param name="featureID">Feature for which the slider has to be moved</param>
        /// <param name="incrementStep">Increment of the slider to apply</param>
        /// <returns>New value of the slider (that is clamped in the range [0, 1]</returns>
        float MatchingFeatureStepForUma(int featureID, float incrementStep);
    }
}
