namespace ImmotionAR.ImmotionRoom.TrackingEngine.Walking
{
    using System;
    using System.Collections.Generic;
    using Model;
    
    /// <summary>
    ///     Factory to create PlayerWalkingDetectors
    /// </summary>
    internal static class PlayerWalkingDetectorFactory
    {
        public static IPlayerWalkingDetector CreateDetector(PlayerWalkingDetectorTypes detectorType, Dictionary<string, string> runtimeParameters)
        {
            switch (detectorType)
            {
                case PlayerWalkingDetectorTypes.KnaivePlayerWalkingDetector:
                    return CreateKnaivePlayerWalkingDetector(runtimeParameters);

                default:
                    throw new ArgumentOutOfRangeException("detectorType", string.Format("'{0}' unsupported", detectorType));
            }            
        }

        private static IPlayerWalkingDetector CreateKnaivePlayerWalkingDetector(Dictionary<string, string> runtimeParameters)
        {
            // DEFAULT BUILT-IN VALUES
            /////////////////////////////////////////////////////////////////////////
            
            KnaivePlayerWalkingDetectorParams detectorParams = new KnaivePlayerWalkingDetectorParams
            {
                KneeDetectionParams = new KneeWalkingDetectorParams
                {
                    StillToRisingThreshold = 0.156f,
                    AnyStateToFallingThreshold = 0.20f,
                    AnyStateToStillThreshold = 0.122f,
                    StillAngleThreshold = 12.5f * MathConstants.Deg2Rad,
                    TimeToTriggerMovement = 0.042f,
                    TimeToTriggerStillness = 0.066f,
                    FallingToRisingSpeedMultiplier = 1.5f,
                    AlmostStillSpeed = 0.25f,
                    RisingAngleTolerance = 1.06f,
                    SpikeNoiseThreshold = 3.5f,
                    TriggerToSpeedMultiplier = 0.2145f,
                    UseAcceleration = false,
                    EstimatedFrameRate = 60
                },
                StillResetTime = 1.8f,
                WalkingAngleRunningAvgAlpha = 0.8f,
                WalkingMagnitudeRunningAvgAlpha = 0.22f,
                WalkingAngleEstimationType = WalkingDirectionEstimator.Shoulders,
                PlayerMovementDetectionThresh = 0.0145f,
                PlayerMovementDetectionTimeThreshold = 0.055f,
                PlayerMovementDetectionRunningAvgAlpha = 0.215f

            //    KneeDetectionParams = new KneeWalkingDetectorParams
            //    {
            //        StillToRisingThreshold = 0.156f,
            //        AnyStateToFallingThreshold = 0.20f,
            //        AnyStateToStillThreshold = 0.122f,
            //        StillAngleThreshold = 15f * MathConstants.Deg2Rad,
            //        TimeToTriggerMovement = 0.042f,
            //        TimeToTriggerStillness = 0.056f,
            //        FallingToRisingSpeedMultiplier = 1.5f,
            //        AlmostStillSpeed = 0.25f,
            //        RisingAngleTolerance = 1.06f,
            //        SpikeNoiseThreshold = 3.5f,
            //        TriggerToSpeedMultiplier = 0.1845f,
            //        UseAcceleration = false,
            //        EstimatedFrameRate = 60
            //    },
            //    StillResetTime = 1.8f,
            //    WalkingAngleRunningAvgAlpha = 0.8f,
            //    WalkingMagnitudeRunningAvgAlpha = 0.22f,
            //    WalkingAngleEstimationType = WalkingDirectionEstimator.Shoulders,
            //    PlayerMovementDetectionThresh = 0.0145f,
            //    PlayerMovementDetectionTimeThreshold = 0.055f,
            //    PlayerMovementDetectionRunningAvgAlpha = 0.215f
            };

            // Overrides default values with external runtime settings (if specified)
            /////////////////////////////////////////////////////////////////////////
            
            float tempValue;
            if (runtimeParameters.ContainsKey(KnaivePlayerWalkingDetectorSettings.Knee_StillToRisingThreshold_Key) && float.TryParse(runtimeParameters[KnaivePlayerWalkingDetectorSettings.Knee_StillToRisingThreshold_Key], out tempValue))
            {
                detectorParams.KneeDetectionParams.StillToRisingThreshold = tempValue;
            }
            
            if (runtimeParameters.ContainsKey(KnaivePlayerWalkingDetectorSettings.Knee_AnyStateToFallingThreshold_Key) && float.TryParse(runtimeParameters[KnaivePlayerWalkingDetectorSettings.Knee_AnyStateToFallingThreshold_Key], out tempValue))
            {
                detectorParams.KneeDetectionParams.AnyStateToFallingThreshold = tempValue;
            }

            if (runtimeParameters.ContainsKey(KnaivePlayerWalkingDetectorSettings.Knee_AnyStateToStillThreshold_Key) && float.TryParse(runtimeParameters[KnaivePlayerWalkingDetectorSettings.Knee_AnyStateToStillThreshold_Key], out tempValue))
            {
                detectorParams.KneeDetectionParams.AnyStateToStillThreshold = tempValue;
            }

            if (runtimeParameters.ContainsKey(KnaivePlayerWalkingDetectorSettings.Knee_StillAngleThreshold_Key) && float.TryParse(runtimeParameters[KnaivePlayerWalkingDetectorSettings.Knee_StillAngleThreshold_Key], out tempValue))
            {
                detectorParams.KneeDetectionParams.StillAngleThreshold = tempValue;
            }

            if (runtimeParameters.ContainsKey(KnaivePlayerWalkingDetectorSettings.Knee_TimeToTriggerMovement_Key) && float.TryParse(runtimeParameters[KnaivePlayerWalkingDetectorSettings.Knee_TimeToTriggerMovement_Key], out tempValue))
            {
                detectorParams.KneeDetectionParams.TimeToTriggerMovement = tempValue;
            }

            if (runtimeParameters.ContainsKey(KnaivePlayerWalkingDetectorSettings.Knee_TimeToTriggerStillness_Key) && float.TryParse(runtimeParameters[KnaivePlayerWalkingDetectorSettings.Knee_TimeToTriggerStillness_Key], out tempValue))
            {
                detectorParams.KneeDetectionParams.TimeToTriggerStillness = tempValue;
            }

            if (runtimeParameters.ContainsKey(KnaivePlayerWalkingDetectorSettings.Knee_FallingToRisingSpeedMultiplier_Key) && float.TryParse(runtimeParameters[KnaivePlayerWalkingDetectorSettings.Knee_FallingToRisingSpeedMultiplier_Key], out tempValue))
            {
                detectorParams.KneeDetectionParams.FallingToRisingSpeedMultiplier = tempValue;
            }

            if (runtimeParameters.ContainsKey(KnaivePlayerWalkingDetectorSettings.Knee_AlmostStillSpeed_Key) && float.TryParse(runtimeParameters[KnaivePlayerWalkingDetectorSettings.Knee_AlmostStillSpeed_Key], out tempValue))
            {
                detectorParams.KneeDetectionParams.AlmostStillSpeed = tempValue;
            }

            if (runtimeParameters.ContainsKey(KnaivePlayerWalkingDetectorSettings.Knee_RisingAngleTolerance_Key) && float.TryParse(runtimeParameters[KnaivePlayerWalkingDetectorSettings.Knee_RisingAngleTolerance_Key], out tempValue))
            {
                detectorParams.KneeDetectionParams.RisingAngleTolerance = tempValue;
            }

            if (runtimeParameters.ContainsKey(KnaivePlayerWalkingDetectorSettings.Knee_SpikeNoiseThreshold_Key) && float.TryParse(runtimeParameters[KnaivePlayerWalkingDetectorSettings.Knee_SpikeNoiseThreshold_Key], out tempValue))
            {
                detectorParams.KneeDetectionParams.SpikeNoiseThreshold = tempValue;
            }

            if (runtimeParameters.ContainsKey(KnaivePlayerWalkingDetectorSettings.Knee_TriggerToSpeedMultiplier_Key) && float.TryParse(runtimeParameters[KnaivePlayerWalkingDetectorSettings.Knee_TriggerToSpeedMultiplier_Key], out tempValue))
            {
                detectorParams.KneeDetectionParams.TriggerToSpeedMultiplier = tempValue;
            }

            bool tempBoolValue;
            if (runtimeParameters.ContainsKey(KnaivePlayerWalkingDetectorSettings.Knee_UseAcceleration_Key) && bool.TryParse(runtimeParameters[KnaivePlayerWalkingDetectorSettings.Knee_UseAcceleration_Key], out tempBoolValue))
            {
                detectorParams.KneeDetectionParams.UseAcceleration = tempBoolValue;
            }

            int tempIntValue;
            if (runtimeParameters.ContainsKey(KnaivePlayerWalkingDetectorSettings.Knee_EstimatedFrameRate_Key) && int.TryParse(runtimeParameters[KnaivePlayerWalkingDetectorSettings.Knee_EstimatedFrameRate_Key], out tempIntValue))
            {
                detectorParams.KneeDetectionParams.EstimatedFrameRate = tempIntValue;
            }

            if (runtimeParameters.ContainsKey(KnaivePlayerWalkingDetectorSettings.StillResetTime_Key) && float.TryParse(runtimeParameters[KnaivePlayerWalkingDetectorSettings.StillResetTime_Key], out tempValue))
            {
                detectorParams.StillResetTime = tempValue;
            }

            if (runtimeParameters.ContainsKey(KnaivePlayerWalkingDetectorSettings.WalkingAngleRunningAvgAlpha_Key) && float.TryParse(runtimeParameters[KnaivePlayerWalkingDetectorSettings.WalkingAngleRunningAvgAlpha_Key], out tempValue))
            {
                detectorParams.WalkingAngleRunningAvgAlpha = tempValue;
            }

            if (runtimeParameters.ContainsKey(KnaivePlayerWalkingDetectorSettings.WalkingMagnitudeRunningAvgAlpha_Key) && float.TryParse(runtimeParameters[KnaivePlayerWalkingDetectorSettings.WalkingMagnitudeRunningAvgAlpha_Key], out tempValue))
            {
                detectorParams.WalkingMagnitudeRunningAvgAlpha = tempValue;
            }

            WalkingDirectionEstimator estimatorType;
            if (runtimeParameters.ContainsKey(KnaivePlayerWalkingDetectorSettings.WalkingAngleEstimationType_Key) && Enum.TryParse(runtimeParameters[KnaivePlayerWalkingDetectorSettings.WalkingAngleEstimationType_Key], out estimatorType))
            {
                detectorParams.WalkingAngleEstimationType = estimatorType;
            }

            if (runtimeParameters.ContainsKey(KnaivePlayerWalkingDetectorSettings.PlayerMovementDetectionThresh_Key) && float.TryParse(runtimeParameters[KnaivePlayerWalkingDetectorSettings.PlayerMovementDetectionThresh_Key], out tempValue))
            {
                detectorParams.PlayerMovementDetectionThresh = tempValue;
            }

            if (runtimeParameters.ContainsKey(KnaivePlayerWalkingDetectorSettings.PlayerMovementDetectionTimeThreshold_Key) && float.TryParse(runtimeParameters[KnaivePlayerWalkingDetectorSettings.PlayerMovementDetectionTimeThreshold_Key], out tempValue))
            {
                detectorParams.PlayerMovementDetectionTimeThreshold = tempValue;
            }

            if (runtimeParameters.ContainsKey(KnaivePlayerWalkingDetectorSettings.PlayerMovementDetectionRunningAvgAlpha_Key) && float.TryParse(runtimeParameters[KnaivePlayerWalkingDetectorSettings.PlayerMovementDetectionRunningAvgAlpha_Key], out tempValue))
            {
                detectorParams.PlayerMovementDetectionRunningAvgAlpha = tempValue;
            }

            return new KnaivePlayerWalkingDetector(detectorParams);
        }
    }
}
