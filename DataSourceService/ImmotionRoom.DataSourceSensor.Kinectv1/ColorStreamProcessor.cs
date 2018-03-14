namespace ImmotionAR.ImmotionRoom.DataSourceSensor.Kinect1
{
    using System;
    using DataSourceService.Model;
    using Logger;
    using Microsoft.Kinect;
    using Protocol;

    internal class ColorStreamProcessor : BaseStreamProcessor<SensorVideoStreamFrame, ColorImageFrame, DataSourceImageDataAvailableEventArgs>
    {
        #region Private fields

        private byte[] m_Image;

        #endregion

        #region Constructor

        internal ColorStreamProcessor(TrackingSessionConfiguration trackingConfiguration) : base(LoggerService.GetLogger<ColorStreamProcessor>(), trackingConfiguration)
        {
        }

        #endregion

        #region Methods

        internal override void SetData(ColorImageFrame frame)
        {
            Data = MapToSensorColorStreamFrameEntity(frame);
        }

        #endregion

        #region Private methods

        protected override void ProcessData()
        {
            if (Data != null)
            {
                OnDataAvailable(Data);
            }
        }

        private void OnDataAvailable(SensorVideoStreamFrame data)
        {
            base.OnDataAvailable(new DataSourceImageDataAvailableEventArgs(data));
        }

        private SensorVideoStreamFrame MapToSensorColorStreamFrameEntity(ColorImageFrame frame)
        {
            var pixelDataLength = frame.BytesPerPixel * frame.PixelDataLength;

            if (m_Image == null || m_Image.Length < pixelDataLength)
            {
                m_Image = new byte[pixelDataLength];
            }

            frame.CopyPixelDataTo(m_Image);

            var sensorData = new SensorVideoStreamFrame
            {
                RelativeTime = frame.Timestamp,
                Width = frame.Width,
                Height = frame.Height,
                Depth = frame.BytesPerPixel,
                RawFrameData = m_Image
            };


            return sensorData;
        }

        #endregion
    }
}
