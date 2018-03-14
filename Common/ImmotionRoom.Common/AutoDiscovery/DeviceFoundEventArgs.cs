namespace ImmotionAR.ImmotionRoom.AutoDiscovery
{
    using System;

    public class DeviceFoundEventArgs : EventArgs
    {
        public DeviceInfo Info { get; private set; }

        public DeviceFoundEventArgs(DeviceInfo info)
        {
            Info = info;
        }
    }
}
