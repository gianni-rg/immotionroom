namespace ImmotionAR.ImmotionRoom.TrackingService.Interfaces
{
    using Model;

    public interface ICommandProcessor
    {
        void Start();
        void Stop();

        void EnqueueCommand(Command command);
    }
}
