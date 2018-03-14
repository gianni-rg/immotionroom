namespace ImmotionAR.ImmotionRoom.DataSourceService.Interfaces
{
    using Model;

    public interface ICommandProcessor
    {
        void Start();
        void Stop();

        void EnqueueCommand(Command command);
    }
}
