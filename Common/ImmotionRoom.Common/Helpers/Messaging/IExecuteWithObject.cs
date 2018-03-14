namespace ImmotionAR.ImmotionRoom.Helpers.Messaging
{
    // Based on the work contained in MetroMvvm, by Gianni Rosa Gallina
    // Copyright © 2012-2014 Gianni Rosa Gallina

    public interface IExecuteWithObject
    {
        object Target { get; }

        void ExecuteWithObject(object parameter);

        void MarkForDeletion();
    }
}