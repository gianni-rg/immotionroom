namespace ImmotionAR.ImmotionRoom.Helpers.Messaging
{
    using System;
    using System.Reflection;

    // Based on the work contained in MetroMvvm, by Gianni Rosa Gallina
    // Copyright © 2012-2014 Gianni Rosa Gallina

    public class WeakAction
    {
        #region Private fields

        private Action m_StaticAction;

        #endregion

        #region Properties

        protected MethodInfo Method { get; set; }

        public virtual string MethodName
        {
            get
            {
                if (m_StaticAction != null)
                {
                    return m_StaticAction.GetMethodInfo().Name;
                }

                return Method.Name;
            }
        }

        protected WeakReference ActionReference { get; set; }

        protected WeakReference Reference { get; set; }

        public bool IsStatic
        {
            get { return m_StaticAction != null; }
        }

        public virtual bool IsAlive
        {
            get
            {
                if (m_StaticAction == null
                    && Reference == null)
                {
                    return false;
                }

                if (m_StaticAction != null)
                {
                    if (Reference != null)
                    {
                        return Reference.IsAlive;
                    }

                    return true;
                }

                return Reference.IsAlive;
            }
        }

        public object Target
        {
            get
            {
                if (Reference == null)
                {
                    return null;
                }

                return Reference.Target;
            }
        }

        protected object ActionTarget
        {
            get
            {
                if (ActionReference == null)
                {
                    return null;
                }

                return ActionReference.Target;
            }
        }

        #endregion

        #region Constructor

        protected WeakAction()
        {
        }

        public WeakAction(Action action)
            : this(action == null ? null : action.Target, action)
        {
        }

        public WeakAction(object target, Action action)
        {
            if (action.GetMethodInfo().IsStatic)
            {
                m_StaticAction = action;

                if (target != null)
                {
                    Reference = new WeakReference(target);
                }

                return;
            }

            Method = action.GetMethodInfo();

            ActionReference = new WeakReference(action.Target);

            Reference = new WeakReference(target);
        }

        #endregion

        #region Methods

        public void Execute()
        {
            if (m_StaticAction != null)
            {
                m_StaticAction();
                return;
            }

            object actionTarget = ActionTarget;

            if (IsAlive)
            {
                if (Method != null
                    && ActionReference != null
                    && actionTarget != null)
                {
                    Method.Invoke(actionTarget, null);
                }
            }
        }

        public void MarkForDeletion()
        {
            Reference = null;
            ActionReference = null;
            Method = null;
            m_StaticAction = null;
        }

        #endregion
    }
}