namespace ImmotionAR.ImmotionRoom.TrackingService.ControlApi
{
    using System.Collections.Generic;
    using DataClient.Model;
    using Model;

    public static class Mappers
    {
        public static Model.Command ToModel(this ControlClient.Model.Command webModel)
        {
            if (webModel == null)
            {
                return null;
            }

            var model = new Model.Command
            {
                RequestId = webModel.RequestId,
                CommandType = (CommandType) webModel.CommandType,
                Timestamp = webModel.Timestamp,
            };

            foreach (var itemKeyValue in webModel.Data)
            {
                model.Data.Add(new KeyValuePair<string, object>(itemKeyValue.Key, itemKeyValue.Value));
            }

            return model;
        }

        public static ControlClient.Model.CommandResult<object> ToWebModel(this CommandResult<object> model)
        {
            if (model == null)
            {
                return null;
            }

            var webModel = new ControlClient.Model.CommandResult<object>
            {
                RequestId = model.RequestId,
                Data = model.Data,
                Read = model.Read,
                Timestamp = model.Timestamp,
            };

            return webModel;
        }

        public static CalibrationParameters ToModel(this ControlClient.Model.CalibrationParameters webModel)
        {
            if (webModel == null)
            {
                return null;
            }

            var model = new CalibrationParameters
            {
                AdditionalMasterYRotation = webModel.AdditionalMasterYRotation,
                CalibratingUserHeight = webModel.CalibratingUserHeight,
                CalibrateSlavesUsingCentroids = webModel.CalibrateSlavesUsingCentroids,
                LastButNthValidMatrix = webModel.LastButNthValidMatrix,
                DataSource1 = webModel.DataSource1,
                DataSource2 = webModel.DataSource2,
                Step = (TrackingServiceCalibrationSteps)webModel.Step,
            };

            return model;
        }

        public static AutoDiscoveryParameters ToModel(this ControlClient.Model.AutoDiscoveryParameters webModel)
        {
            if (webModel == null)
            {
                return null;
            }

            var model = new AutoDiscoveryParameters
            {
                ClearMasterDataSource = webModel.ClearMasterDataSource,
                ClearCalibrationData = webModel.ClearCalibrationData,
            };

            return model;
        }

        public static TrackingSessionConfiguration ToModel(this ControlClient.Model.TrackingSessionConfiguration webModel)
        {
            if (webModel == null)
            {
                return null;
            }
            
            var model = new TrackingSessionConfiguration
            {
                DataSourceTrackingSettings = new TrackingSessionDataSourceConfiguration
                {
                    BodyClippingEdgesEnabled = webModel.DataSourceTrackingSettings.BodyClippingEdgesEnabled,
                    HandsStatusEnabled = webModel.DataSourceTrackingSettings.HandsStatusEnabled,
                    TrackJointRotation = webModel.DataSourceTrackingSettings.TrackJointRotation,
                },
            };

            return model;
        }
    }
}
