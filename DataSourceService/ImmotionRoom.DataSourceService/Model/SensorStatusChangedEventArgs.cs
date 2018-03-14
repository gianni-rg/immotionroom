namespace ImmotionAR.ImmotionRoom.DataSourceService.Model
{
    using System;

    public class SensorStatusChangedEventArgs : EventArgs
    {
        public bool IsActive { get; private set; }

        public SensorStatusChangedEventArgs(bool isActive)
        {
            IsActive = isActive;
        }
    }
}
