namespace ImmotionAR.ImmotionRoom.DataSourceSensor.Kinect1
{
    using System;
    using DataSourceService.Model;
    using Logger;
    using Microsoft.Kinect;
    using Protocol;

    internal class DepthStreamProcessor : BaseStreamProcessor<SensorVideoStreamFrame, DepthImageFrame, DataSourceImageDataAvailableEventArgs>
    {
        #region Private fields

        private byte[] m_Image;

        /// <summary>
        /// Map depth range to byte range
        /// </summary>
        private const int MapDepthToByte = 8000 / 256;
        #endregion

        #region Constructor

        internal DepthStreamProcessor(TrackingSessionConfiguration trackingConfiguration) : base(LoggerService.GetLogger<DepthStreamProcessor>(), trackingConfiguration)
        {
        }

        #endregion

        #region Event Handlers

        protected override void ProcessData()
        {
            if (Data != null)
            {
                OnDataAvailable(Data);
            }
        }

        internal override void SetData(DepthImageFrame frame)
        {
            Data = MapToSensorDepthStreamFrameEntity(frame);
        }

        #endregion

        #region Private methods

        private void OnDataAvailable(SensorVideoStreamFrame data)
        {
            base.OnDataAvailable(new DataSourceImageDataAvailableEventArgs(data));
        }

        private SensorVideoStreamFrame MapToSensorDepthStreamFrameEntity(DepthImageFrame frame)
        {
            var pixelDataLength = frame.Width*frame.Height*4;
            
            if (m_Image == null || m_Image.Length < pixelDataLength)
            {
                m_Image = new byte[pixelDataLength];
            }
            
            // The fastest way to process the body index data is to directly access the underlying buffer

            // Verify data and write the color data to the display bitmap
            if (frame.Width * frame.Height == frame.PixelDataLength / frame.BytesPerPixel)
            {
                // Note: In order to see the full range of depth (including the less reliable far field depth)
                // we are setting maxDepth to the extreme potential depth threshold
                ushort maxDepth = ushort.MaxValue;

                // If you wish to filter by reliable depth distance, uncomment the following line:
                // maxDepth = frame.DepthMaxReliableDistance

                ProcessDepthFrameData(frame.GetRawPixelData(), frame, frame.PixelDataLength, frame.MinDepth, maxDepth);
            }
            
          
            var sensorData = new SensorVideoStreamFrame
            {
                RelativeTime = frame.Timestamp,
                Width = frame.Width,
                Height = frame.Height,
                Depth = 4,
                RawFrameData = m_Image
            };


            return sensorData;
        }

        /// <summary>
        /// Directly accesses the underlying image buffer of the DepthFrame to 
        /// create a displayable bitmap.
        /// This function requires the /unsafe compiler option as we make use of direct
        /// access to the native memory pointed to by the depthFrameData pointer.
        /// </summary>
        /// <param name="depthFrameData">Pointer to the DepthFrame image data</param>
        /// <param name="frameDescription"></param>
        /// <param name="depthFrameDataSize">Size of the DepthFrame image data</param>
        /// <param name="minDepth">The minimum reliable depth value for the frame</param>
        /// <param name="maxDepth">The maximum reliable depth value for the frame</param>
        private unsafe void ProcessDepthFrameData(DepthImagePixel[] frameData, DepthImageFrame frameDescription, int depthFrameDataSize, int minDepth, int maxDepth)
        {
            // convert depth to a visual representation
            int colorIndex = 0;
            for (int i = 0; i < (depthFrameDataSize / frameDescription.BytesPerPixel); ++i)
            {
                // Get the depth for this pixel
                short depth = frameData[i].Depth;

                // To convert to a byte, we're mapping the depth value to the byte range.
                // Values outside the reliable depth range are mapped to 0 (black).
                byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth / MapDepthToByte : 0);
                m_Image[colorIndex++] = intensity; 
                m_Image[colorIndex++] = intensity;
                m_Image[colorIndex++] = intensity;

                colorIndex++;
            }
        }
        #endregion
    }
}
