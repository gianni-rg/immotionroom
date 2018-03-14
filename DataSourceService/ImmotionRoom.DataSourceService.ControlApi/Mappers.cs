namespace ImmotionAR.ImmotionRoom.DataSourceService.ControlApi
{
    using System.Collections.Generic;

    public static class Mappers
    {
        public static Model.Command ToModel(this DataSource.ControlClient.Model.Command webModel)
        {
            if (webModel == null)
            {
                return null;
            }

            var model = new Model.Command
            {
                RequestId = webModel.RequestId,
                CommandType = (Model.CommandType) webModel.CommandType,
                Timestamp = webModel.Timestamp,
            };

            foreach (var itemKeyValue in webModel.Data)
            {
                model.Data.Add(new KeyValuePair<string, object>(itemKeyValue.Key, itemKeyValue.Value));
            }

            return model;
        }

        public static DataSource.ControlClient.Model.CommandResult<object> ToWebModel(this Model.CommandResult<object> model)
        {
            if (model == null)
            {
                return null;
            }

            var webModel = new DataSource.ControlClient.Model.CommandResult<object>
            {
                RequestId = model.RequestId,
                Data = model.Data,
                Read = model.Read,
                Timestamp = model.Timestamp,
            };

            return webModel;
        }

        public static Model.TrackingSessionConfiguration ToModel(this DataSource.ControlClient.Model.TrackingSessionConfiguration webModel)
        {
            if (webModel == null)
            {
                return null;
            }

            var model = new Model.TrackingSessionConfiguration
            {
                BodyClippingEdgesEnabled = webModel.BodyClippingEdgesEnabled,
                TrackHandsStatus = webModel.HandsStatusEnabled,
                TrackJointRotation = webModel.TrackJointRotation,
            };
            
            return model;
        }
    }
}
