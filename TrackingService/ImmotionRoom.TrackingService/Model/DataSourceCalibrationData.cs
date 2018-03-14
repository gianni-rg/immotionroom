namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    using TrackingEngine.Model;

    public class DataSourceCalibrationData
    {
        public Matrix4x4 MasterToWorldCalibrationMatrix { get; set; }
        public Matrix4x4 SlaveToMasterCalibrationMatrices { get; set; }

        public DataSourceCalibrationData()
        {
            MasterToWorldCalibrationMatrix = Matrix4x4.Identity;
            SlaveToMasterCalibrationMatrices = Matrix4x4.Identity;
        }
    }
}