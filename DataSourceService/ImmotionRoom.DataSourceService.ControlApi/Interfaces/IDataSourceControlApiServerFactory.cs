namespace ImmotionAR.ImmotionRoom.DataSourceService.ControlApi.Interfaces
{
    using Model;

    public interface IDataSourceControlApiServerFactory
    {
        IDataSourceControlApiServer Create(DataSourceConfiguration configuration);
    }
}
