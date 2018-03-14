namespace ImmotionAR.ImmotionRoom.DataSourceSensor.Kinect2
{
    using System;
    using DataSourceService.Model;
    using Logger;
    using Microsoft.Kinect;
    using Protocol;

    internal class ColorStreamProcessor : BaseStreamProcessor<SensorVideoStreamFrame, ColorFrame, DataSourceImageDataAvailableEventArgs>
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

        internal override void SetData(ColorFrame frame)
        {
            Data = MapToSensorColorStreamFrameEntity(frame);
        }

        #endregion

        #region Private methods

        protected override void ProcessData()
        {
            if (Data != null)
            {
                // The camera is either running at 30 fps (33.33ms interval) or 15 fps
                // (66.66ms interval), in case of low lighting conditions.
                // In order to keep a steady frame rate we duplicate frames when we
                // are running at 15 fps.
                //
                // See: https://social.msdn.microsoft.com/Forums/en-US/7e65c48f-f9f3-4725-91b8-25fa9d347450/kinect-v2-video-framerate?forum=kinectv2sdk

                //double fps = 1.0 / frame.ColorCameraSettings.FrameInterval.TotalSeconds;

                OnDataAvailable(Data);

                //if(fps<29)
                //{
                //    OnDataAvailable(convertedFrame);
                //}
            }
        }

        private void OnDataAvailable(SensorVideoStreamFrame data)
        {
            base.OnDataAvailable(new DataSourceImageDataAvailableEventArgs(data));
        }

        private SensorVideoStreamFrame MapToSensorColorStreamFrameEntity(ColorFrame frame)
        {
            var format = frame.CreateFrameDescription(ColorImageFormat.Rgba);
            var pixelDataLength = format.BytesPerPixel*format.LengthInPixels;

            if (m_Image == null || m_Image.Length < pixelDataLength)
            {
                m_Image = new byte[pixelDataLength];
            }

            frame.CopyConvertedFrameDataToArray(m_Image, ColorImageFormat.Rgba);

            var sensorData = new SensorVideoStreamFrame
            {
                RelativeTime = frame.RelativeTime.Ticks,
                Width = format.Width,
                Height = format.Height,
                Depth = (int) format.BytesPerPixel,
                RawFrameData = m_Image
            };


            return sensorData;
        }

        #endregion
    }
}
