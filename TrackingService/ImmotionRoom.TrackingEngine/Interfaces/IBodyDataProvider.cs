namespace ImmotionAR.ImmotionRoom.TrackingEngine.Interfaces
{
    using System.Collections.Generic;
    using Model;

    public interface IBodyDataProvider
    {
        void Update(double deltaTime);

        IDictionary<string, SceneFrame> DataSources { get; }
        IDictionary<string, byte> DataSourceMapping { get; }
    }
}
