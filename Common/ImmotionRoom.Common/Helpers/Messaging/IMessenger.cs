namespace ImmotionAR.ImmotionRoom.Helpers.Messaging
{
    // Based on the work contained in MetroMvvm, by Gianni Rosa Gallina
    // Copyright © 2012-2014 Gianni Rosa Gallina
    
    using System;
    
    public interface IMessenger
    {
        void Register<TMessage>(object recipient, Action<TMessage> action);

        void Register<TMessage>(object recipient, object token, Action<TMessage> action);

        void Register<TMessage>(object recipient, object token, bool receiveDerivedMessagesToo, Action<TMessage> action);

        void Register<TMessage>(object recipient, bool receiveDerivedMessagesToo, Action<TMessage> action);

        void Send<TMessage>(TMessage message);

        void Send<TMessage, TTarget>(TMessage message);

        void Send<TMessage>(TMessage message, object token);
        
        void Unregister(object recipient);

        void Unregister<TMessage>(object recipient);

        void Unregister<TMessage>(object recipient, object token);

        void Unregister<TMessage>(object recipient, Action<TMessage> action);

        void Unregister<TMessage>(object recipient, object token, Action<TMessage> action);
    }
}