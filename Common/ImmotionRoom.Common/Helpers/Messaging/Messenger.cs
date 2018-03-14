namespace ImmotionAR.ImmotionRoom.Helpers.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    // Based on the work contained in MetroMvvm, by Gianni Rosa Gallina
    // Copyright © 2012-2014 Gianni Rosa Gallina

    public class Messenger : IMessenger
    {
        #region Private fields

        private bool m_IsCleanupRegistered;
        private static readonly object RegisterLock = new object();
        private Dictionary<Type, List<WeakActionAndToken>> m_RecipientsOfSubclassesAction;
        private Dictionary<Type, List<WeakActionAndToken>> m_RecipientsStrictAction;

        #endregion

        #region Methods

        public void Register<TMessage>(object recipient, Action<TMessage> action)
        {
            Register(recipient, null, false, action);
        }

        public void Register<TMessage>(object recipient, bool receiveDerivedMessagesToo, Action<TMessage> action)
        {
            Register(recipient, null, receiveDerivedMessagesToo, action);
        }

        public void Register<TMessage>(object recipient, object token, Action<TMessage> action)
        {
            Register(recipient, token, false, action);
        }

        public void Send<TMessage>(TMessage message)
        {
            SendToTargetOrType(message, null, null);
        }

        public void Send<TMessage, TTarget>(TMessage message)
        {
            SendToTargetOrType(message, typeof (TTarget), null);
        }

        public void Send<TMessage>(TMessage message, object token)
        {
            SendToTargetOrType(message, null, token);
        }
        
        public void Unregister(object recipient)
        {
            UnregisterFromLists(recipient, m_RecipientsOfSubclassesAction);
            UnregisterFromLists(recipient, m_RecipientsStrictAction);
        }

        public void Unregister<TMessage>(object recipient)
        {
            Unregister<TMessage>(recipient, null, null);
        }

        public void Unregister<TMessage>(object recipient, object token)
        {
            Unregister<TMessage>(recipient, token, null);
        }

        public void Unregister<TMessage>(object recipient, Action<TMessage> action)
        {
            Unregister(recipient, null, action);
        }

        public void Register<TMessage>(object recipient, object token, bool receiveDerivedMessagesToo, Action<TMessage> action)
        {
            lock (RegisterLock)
            {
                Type messageType = typeof (TMessage);

                Dictionary<Type, List<WeakActionAndToken>> recipients;

                if (receiveDerivedMessagesToo)
                {
                    if (m_RecipientsOfSubclassesAction == null)
                    {
                        m_RecipientsOfSubclassesAction = new Dictionary<Type, List<WeakActionAndToken>>();
                    }

                    recipients = m_RecipientsOfSubclassesAction;
                }
                else
                {
                    if (m_RecipientsStrictAction == null)
                    {
                        m_RecipientsStrictAction = new Dictionary<Type, List<WeakActionAndToken>>();
                    }

                    recipients = m_RecipientsStrictAction;
                }

                lock (recipients)
                {
                    List<WeakActionAndToken> list;

                    if (!recipients.ContainsKey(messageType))
                    {
                        list = new List<WeakActionAndToken>();
                        recipients.Add(messageType, list);
                    }
                    else
                    {
                        list = recipients[messageType];
                    }

                    var weakAction = new WeakAction<TMessage>(recipient, action);
                    var item = new WeakActionAndToken
                    {
                        Action = weakAction,
                        Token = token
                    };
                    list.Add(item);
                }
            }

            Cleanup();
        }

        public void Unregister<TMessage>(object recipient, object token, Action<TMessage> action)
        {
            UnregisterFromLists(recipient, token, action, m_RecipientsStrictAction);
            UnregisterFromLists(recipient, token, action, m_RecipientsOfSubclassesAction);
            Cleanup();
        }

        public void RequestCleanup()
        {
            if (!m_IsCleanupRegistered)
            {
                Action cleanupAction = Cleanup;

                cleanupAction();

                m_IsCleanupRegistered = true;
            }
        }

        #endregion

        #region Private methods

        private static void CleanupList(IDictionary<Type, List<WeakActionAndToken>> lists)
        {
            if (lists == null)
            {
                return;
            }

            lock (lists)
            {
                var listsToRemove = new List<Type>();
                foreach (var list in lists)
                {
                    List<WeakActionAndToken> recipientsToRemove = list.Value
                        .Where(item => item.Action == null || !item.Action.IsAlive)
                        .ToList();

                    foreach (WeakActionAndToken recipient in recipientsToRemove)
                    {
                        list.Value.Remove(recipient);
                    }

                    if (list.Value.Count == 0)
                    {
                        listsToRemove.Add(list.Key);
                    }
                }

                foreach (Type key in listsToRemove)
                {
                    lists.Remove(key);
                }
            }
        }

        private static bool Implements(Type instanceType, Type interfaceType)
        {
            if (interfaceType == null ||
                instanceType == null)
            {
                return false;
            }

            IEnumerable<Type> interfaces = instanceType.GetTypeInfo().ImplementedInterfaces;

            return interfaces.Any(currentInterface => currentInterface == interfaceType);
        }

        private static void SendToList<TMessage>(TMessage message, IEnumerable<WeakActionAndToken> weakActionsAndTokens, Type messageTargetType, object token)
        {
            if (weakActionsAndTokens != null)
            {
                // Clone to protect from people registering in a "receive message" method
                // Correction Messaging BL0004.007
                List<WeakActionAndToken> list = weakActionsAndTokens.ToList();
                List<WeakActionAndToken> listClone = list.Take(list.Count).ToList();

                foreach (WeakActionAndToken item in listClone)
                {
                    var executeAction = item.Action as IExecuteWithObject;

                    if (executeAction != null &&
                        item.Action.IsAlive &&
                        item.Action.Target != null &&
                        (messageTargetType == null ||
                         item.Action.Target.GetType() == messageTargetType || messageTargetType.GetTypeInfo().IsAssignableFrom(item.Action.Target.GetType().GetTypeInfo())) &&
                        ((item.Token == null && token == null) ||
                         item.Token != null && item.Token.Equals(token)))
                    {
                        executeAction.ExecuteWithObject(message);
                    }
                }
            }
        }

        private static void UnregisterFromLists(object recipient, Dictionary<Type, List<WeakActionAndToken>> lists)
        {
            if (recipient == null ||
                lists == null ||
                lists.Count == 0)
            {
                return;
            }

            lock (lists)
            {
                foreach (Type messageType in lists.Keys)
                {
                    foreach (WeakActionAndToken item in lists[messageType])
                    {
                        var weakAction = (IExecuteWithObject) item.Action;

                        if (weakAction != null &&
                            recipient == weakAction.Target)
                        {
                            weakAction.MarkForDeletion();
                        }
                    }
                }
            }
        }

        private static void UnregisterFromLists<TMessage>(object recipient, object token, Action<TMessage> action, Dictionary<Type, List<WeakActionAndToken>> lists)
        {
            Type messageType = typeof (TMessage);

            if (recipient == null ||
                lists == null ||
                lists.Count == 0 ||
                !lists.ContainsKey(messageType))
            {
                return;
            }

            lock (lists)
            {
                foreach (WeakActionAndToken item in lists[messageType])
                {
                    var weakActionCasted = item.Action as WeakAction<TMessage>;

                    if (weakActionCasted != null && recipient == weakActionCasted.Target && (action == null || action.GetMethodInfo().Name == weakActionCasted.MethodName) && (token == null || token.Equals(item.Token)))
                    {
                        item.Action.MarkForDeletion();
                    }
                }
            }
        }

        private void Cleanup()
        {
            CleanupList(m_RecipientsOfSubclassesAction);
            CleanupList(m_RecipientsStrictAction);
            m_IsCleanupRegistered = false;
        }

        private void SendToTargetOrType<TMessage>(TMessage message, Type messageTargetType, object token)
        {
            Type messageType = typeof (TMessage);

            if (m_RecipientsOfSubclassesAction != null)
            {
                // Clone to protect from people registering in a "receive message" method
                // Correction Messaging BL0008.002
                List<Type> listClone = m_RecipientsOfSubclassesAction.Keys.Take(m_RecipientsOfSubclassesAction.Count).ToList();

                foreach (Type type in listClone)
                {
                    List<WeakActionAndToken> list = null;

                    if (messageType == type || messageType.GetTypeInfo().IsSubclassOf(type) || type.GetTypeInfo().IsAssignableFrom(messageType.GetTypeInfo()))
                    {
                        lock (m_RecipientsOfSubclassesAction)
                        {
                            list = m_RecipientsOfSubclassesAction[type].Take(m_RecipientsOfSubclassesAction[type].Count).ToList();
                        }
                    }

                    SendToList(message, list, messageTargetType, token);
                }
            }

            if (m_RecipientsStrictAction != null)
            {
                List<WeakActionAndToken> list = null;

                lock (m_RecipientsStrictAction)
                {
                    if (m_RecipientsStrictAction.ContainsKey(messageType))
                    {
                        list = m_RecipientsStrictAction[messageType]
                            .Take(m_RecipientsStrictAction[messageType].Count)
                            .ToList();
                    }
                }

                if (list != null)
                {
                    SendToList(message, list, messageTargetType, token);
                }
            }

            RequestCleanup();
        }

        #endregion

        #region Nested type: WeakActionAndToken

        private struct WeakActionAndToken
        {
            public WeakAction Action;

            public object Token;
        }

        #endregion
    }
}