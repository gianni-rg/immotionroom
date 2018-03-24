namespace ImmotionAR.ImmotionRoom.LittleBoots.VR.Calibration.DataManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Manages storing of iroom/headset calibration data between different game sessions    
    /// </summary>
    public class CalibrationDataManager
    {
        /// <summary>
        /// Stores data for current online game session.
        /// As online game session we define a game session that goes on until the program gets closed.
        /// After the system gets closed, stored calibration data gets lost
        /// </summary>
        public static IroomHeadsetCalibrationData OnlineSessionCalibrationData;
    }
}
