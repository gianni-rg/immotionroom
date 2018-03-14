namespace ImmotionAR.ImmotionRoom.TrackingEngine
{
    // Copyright (c) Microsoft Corporation.  All rights reserved.
    // Some changes by ImmotionAR.

    using System;
    using System.Collections.Generic;
    using Model;

    /// <summary>
    ///     Implementation of a Holt Double Exponential Smoothing filter. The double exponential
    ///     smooths the curve and predicts.  There is also noise jitter removal. And maximum
    ///     prediction bounds.  The parameters are commented in the Init function.
    /// </summary>
    internal class JointsPositionDoubleExponentialFilter2
    {
        /// <summary>
        ///     Minimum confidence for a joint to be considered reliable.
        ///     If a filtered joint has confidence below this threshold and new data is above GoodReliableConfidence threshold, new
        ///     data will become the new filtered value
        /// </summary>
        private const float MinimumReliableConfidence = 0.15f;

        /// <summary>
        ///     Good confidence for a joint to be considered reliable.
        ///     If a filtered joint has confidence below MinimumReliableConfidence threshold and new data is above this threshold,
        ///     new data will become the new filtered value
        /// </summary>
        private const float GoodReliableConfidence = 0.5f;

        /// <summary>
        ///     The previous data.
        /// </summary>
        private FilterDoubleExponentialData[] m_History;

        /// <summary>
        ///     The transform smoothing parameters for this filter.
        /// </summary>
        private TransformSmoothParameters m_SmoothParameters;

        /// <summary>
        ///     Alpha constant for joint confidence running average filtering (0 means full history, 1 full update).
        ///     Confidence is not updated using double exponential filter, but with running average because a simple method is
        ///     enough
        /// </summary>
        private float m_ConfidenceRunAvgAlpha;

        /// <summary>
        ///     True when the filter parameters are initialized.
        /// </summary>
        private bool m_Init;

        /// <summary>
        ///     Initializes a new instance of the <see cref="JointsPositionDoubleExponentialFilter2" /> class.
        /// </summary>
        public JointsPositionDoubleExponentialFilter2()
        {
            m_Init = false;
        }

        /// <summary>
        ///     Initialize the filter with a set of manually specified TransformSmoothParameters.
        /// </summary>
        /// <param name="smoothingValue">Smoothing = [0..1], lower values is closer to the raw data and more noisy.</param>
        /// <param name="correctionValue">Correction = [0..1], higher values correct faster and feel more responsive.</param>
        /// <param name="predictionValue">Prediction = [0..n], how many frames into the future we want to predict.</param>
        /// <param name="jitterRadiusValue">JitterRadius = The deviation distance in m that defines jitter.</param>
        /// <param name="maxDeviationRadiusValue">
        ///     MaxDeviation = The maximum distance in m that filtered positions are allowed to
        ///     deviate from raw data.
        /// </param>
        /// <param name="confidenceRunningAvgAlpha">
        ///     Alpha constant for joint confidence running average filtering (0 means full
        ///     history, 1 full update)
        /// </param>
        public void Init(float smoothingValue = 0.25f, float correctionValue = 0.25f, float predictionValue = 0.25f, float jitterRadiusValue = 0.03f, float maxDeviationRadiusValue = 0.05f, float confidenceRunningAvgAlpha = 0.66f)
        {
            m_SmoothParameters = new TransformSmoothParameters();

            m_SmoothParameters.MaxDeviationRadius = maxDeviationRadiusValue; // Size of the max prediction radius Can snap back to noisy data when too high
            m_SmoothParameters.Smoothing = smoothingValue; // How much soothing will occur.  Will lag when too high
            m_SmoothParameters.Correction = correctionValue; // How much to correct back from prediction.  Can make things springy
            m_SmoothParameters.Prediction = predictionValue; // Amount of prediction into the future to use. Can over shoot when too high
            m_SmoothParameters.JitterRadius = jitterRadiusValue; // Size of the radius where jitter is removed. Can do too much smoothing when too high
            m_ConfidenceRunAvgAlpha = confidenceRunningAvgAlpha;
            Reset();
            m_Init = true;
        }

        /// <summary>
        ///     Initialize the filter with a set of TransformSmoothParameters.
        /// </summary>
        /// <param name="smoothingParameters">The smoothing parameters to filter with.</param>
        /// <param name="confidenceRunningAvgAlpha">
        ///     Alpha constant for joint confidence running average filtering (0 means full
        ///     history, 1 full update)
        /// </param>
        public void Init(TransformSmoothParameters smoothingParameters, float confidenceRunningAvgAlpha)
        {
            m_SmoothParameters = smoothingParameters;
            m_ConfidenceRunAvgAlpha = confidenceRunningAvgAlpha;
            Reset();
            m_Init = true;
        }

        /// <summary>
        ///     Resets the filter to default values.
        /// </summary>
        private void Reset()
        {
            var jointTypeValues = Enum.GetValues(typeof (BodyJointTypes));
            m_History = new FilterDoubleExponentialData[jointTypeValues.Length];
        }

        /// <summary>
        ///     Update the filter with a new frame of data and smooth.
        /// </summary>
        /// <returns>Filtered version of the joint man</returns>
        /// <param name="body">The Skeleton to filter.</param>
        public BodyData UpdateFilter(BodyData body)
        {
            if (null == body)
            {
                return null;
            }

            if (m_Init == false)
            {
                Init(); // Initialize with default parameters                
            }

            var jointTypeValues = Enum.GetValues(typeof (BodyJointTypes));

            var tempSmoothingParams = new TransformSmoothParameters();

            // Check for divide by zero. Use an epsilon of a 10th of a millimeter
            m_SmoothParameters.JitterRadius = Math.Max(0.0001f, m_SmoothParameters.JitterRadius);

            tempSmoothingParams.Smoothing = m_SmoothParameters.Smoothing;
            tempSmoothingParams.Correction = m_SmoothParameters.Correction;
            tempSmoothingParams.Prediction = m_SmoothParameters.Prediction;

            var returnMan = new BodyData(body.Id, new Dictionary<BodyJointTypes, BodyJointData>(BodyJointTypesComparer.Instance), body.DataSources);

            foreach (BodyJointTypes jt in jointTypeValues)
            {
                // If not tracked, we smooth a bit more by using a bigger jitter radius
                // Always filter feet highly as they are so noisy
                if (body.Joints[jt].Confidence < 0.15f)
                {
                    tempSmoothingParams.JitterRadius = 2.0f * m_SmoothParameters.JitterRadius;
                    tempSmoothingParams.MaxDeviationRadius = 2.0f * m_SmoothParameters.MaxDeviationRadius;
                }
                else
                {
                    tempSmoothingParams.JitterRadius = m_SmoothParameters.JitterRadius;
                    tempSmoothingParams.MaxDeviationRadius = m_SmoothParameters.MaxDeviationRadius;
                }

                returnMan.Joints[jt] = FilterJoint(body, jt, tempSmoothingParams);
            }

            return returnMan;
        }

        /// <summary>
        ///     Update the filter for one joint.
        /// </summary>
        /// <returns>Data of the filtered joint</returns>
        /// <param name="bodyData">The Skeleton to filter.</param>
        /// <param name="jt">The Skeleton Joint index to filter.</param>
        /// <param name="smoothingParameters">The Smoothing parameters to apply.</param>
        private BodyJointData FilterJoint(BodyData bodyData, BodyJointTypes jt, TransformSmoothParameters smoothingParameters)
        {
            if (null == bodyData)
            {
                return new BodyJointData(jt);
            }

            var jointIndex = (int) jt;

            Vector3 filteredPosition;
            Vector3 diffvec;
            Vector3 trend;
            float diffVal;

            var rawPosition = bodyData.Joints[jt].Position;
            var prevFilteredPosition = m_History[jointIndex].FilteredPosition;
            var prevTrend = m_History[jointIndex].Trend;
            var prevRawPosition = m_History[jointIndex].RawPosition;

            // If we had low reliable data and new data is highly reliable, reset the filter to take in count the new data
            if (m_History[jointIndex].Confidence < MinimumReliableConfidence && bodyData.Joints[jt].Confidence >= GoodReliableConfidence)
            {
                filteredPosition = rawPosition;
                trend = Vector3.Zero;
            }

            // Initial start values
            if (m_History[jointIndex].FrameCount == 0)
            {
                filteredPosition = rawPosition;
                trend = Vector3.Zero;
            }
            else if (m_History[jointIndex].FrameCount == 1)
            {
                filteredPosition = (rawPosition + prevRawPosition)*0.5f;
                diffvec = filteredPosition - prevFilteredPosition;
                trend = diffvec*smoothingParameters.Correction + prevTrend*(1.0f - smoothingParameters.Correction);
            }
            else
            {
                // First apply jitter filter
                diffvec = rawPosition - prevFilteredPosition;
                diffVal = diffvec.Magnitude;

                if (diffVal <= smoothingParameters.JitterRadius)
                {
                    filteredPosition = rawPosition*diffVal/smoothingParameters.JitterRadius + prevFilteredPosition*(1.0f - diffVal/smoothingParameters.JitterRadius);
                }
                else
                {
                    filteredPosition = rawPosition;
                }

                // Now the double exponential smoothing filter
                filteredPosition = filteredPosition*(1.0f - smoothingParameters.Smoothing) + (prevFilteredPosition + prevTrend)*smoothingParameters.Smoothing;

                diffvec = filteredPosition - prevFilteredPosition;
                trend = diffvec*smoothingParameters.Correction + prevTrend*(1.0f - smoothingParameters.Correction);
            }

            // Predict into the future to reduce latency
            var predictedPosition = filteredPosition + trend*smoothingParameters.Prediction;

            // Check that we are not too far away from raw data
            diffvec = predictedPosition - rawPosition;
            diffVal = diffvec.Magnitude;

            if (diffVal > smoothingParameters.MaxDeviationRadius)
            {
                predictedPosition = predictedPosition*smoothingParameters.MaxDeviationRadius/diffVal + rawPosition*(1.0f - smoothingParameters.MaxDeviationRadius/diffVal);
            }

            // Save the data from this frame            
            m_History[jointIndex].RawPosition = rawPosition;
            m_History[jointIndex].FilteredPosition = filteredPosition;
            m_History[jointIndex].Trend = trend;
            m_History[jointIndex].FrameCount++;

            // Make running average of confidence
            // (if we had reset the filter, just copy new confidence)
            if (m_History[jointIndex].FrameCount == 1 || m_History[jointIndex].Confidence < MinimumReliableConfidence && bodyData.Joints[jt].Confidence >= GoodReliableConfidence)
            {
                m_History[jointIndex].Confidence = bodyData.Joints[jt].Confidence;
            }
            else
            {
                m_History[jointIndex].Confidence = m_History[jointIndex].Confidence*(1 - m_ConfidenceRunAvgAlpha) + bodyData.Joints[jt].Confidence*m_ConfidenceRunAvgAlpha;
            }

            // Calculate filtered data and return them
            var j = new BodyJointData(predictedPosition, m_History[jointIndex].Confidence, jt);

            return j;
        }

        /// <summary>
        ///     Historical Filter Data.
        /// </summary>
        private struct FilterDoubleExponentialData
        {
            /// <summary>
            ///     Gets or sets Historical Position.
            /// </summary>
            public Vector3 RawPosition { get; set; }

            /// <summary>
            ///     Gets or sets Historical Filtered Position.
            /// </summary>
            public Vector3 FilteredPosition { get; set; }

            /// <summary>
            ///     Gets or sets Historical Trend.
            /// </summary>
            public Vector3 Trend { get; set; }

            /// <summary>
            ///     Gets or sets Historical FrameCount.
            /// </summary>
            public uint FrameCount { get; set; }

            /// <summary>
            ///     Confidence of this joint
            /// </summary>
            public float Confidence { get; set; }
        }
    }
}
