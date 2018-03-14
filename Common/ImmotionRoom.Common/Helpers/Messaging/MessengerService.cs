namespace ImmotionAR.ImmotionRoom.Helpers.Messaging
{
    // Based on the work contained in MetroMvvm, by Gianni Rosa Gallina
    // Copyright © 2012-2014 Gianni Rosa Gallina

    public static class MessengerService
    {
        private static readonly object LockObj = new object();

        private static IMessenger m_Messenger;

        public static IMessenger Messenger
        {
            get
            {
                if (m_Messenger == null)
                {
                    lock (LockObj)
                    {
                        if (m_Messenger == null)
                        {
                            m_Messenger = new Messenger();
                        }
                    }
                }

                return m_Messenger;
            }

            set
            {
                lock (LockObj)
                {
                    m_Messenger = value;
                }
            }
        }
    }
}