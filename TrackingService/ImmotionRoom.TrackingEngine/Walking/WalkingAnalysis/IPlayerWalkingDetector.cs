namespace ImmotionAR.ImmotionRoom.TrackingEngine.Walking
{
    using System;
    using System.Collections.Generic;
    using Model;

    /// <summary>
    ///     Interface for player walking gesture detection objects
    /// </summary>
    internal interface IPlayerWalkingDetector : IDebugDictionarable<Dictionary<string, Dictionary<string, string>>>
    {
        /// <summary>
        ///     Get last analysis result of this object about user walking gesture
        /// </summary>
        PlayerWalkingDetection CurrentDetection { get; }

        /// <summary>
        ///     Perform new detection of walking movement, because new joint data is arrived.
        ///     It is advised to call this function at a very regular interval
        /// </summary>
        /// <param name="timestamp">Time since a common reference event, like the start of the program</param>
        /// <param name="body">New body joint data</param>
        void UpdateDetection(TimeSpan timestamp, BodyData body);

        void LoadSettings(Dictionary<string, string> runtimeParameters);
    }
}
