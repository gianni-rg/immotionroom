namespace ImmotionAR.ImmotionRoom.TrackingEngine.Model
{
    using System.Collections.Concurrent;
    
    public class CalibrationSettings
    {
        public bool CalibrationDone { get; set; }

        public Matrix4x4 MasterToWorldCalibrationMatrix { get; set; }

        public ConcurrentDictionary<string, Matrix4x4> SlaveToMasterCalibrationMatrices { get; set; }

        public CalibrationSettings()
        {
            MasterToWorldCalibrationMatrix = Matrix4x4.Identity;
            SlaveToMasterCalibrationMatrices = new ConcurrentDictionary<string, Matrix4x4>();
        }
    }
}