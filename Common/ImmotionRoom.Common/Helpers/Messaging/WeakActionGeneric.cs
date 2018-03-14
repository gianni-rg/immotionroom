namespace ImmotionAR.ImmotionRoom.Helpers.Messaging
{
    using System;
    using System.Reflection;

    // Based on the work contained in MetroMvvm, by Gianni Rosa Gallina
    // Copyright © 2012-2014 Gianni Rosa Gallina

    public class WeakAction<T> : WeakAction, IExecuteWithObject
    {
        #region Private fields

        private Action<T> m_StaticAction;

        #endregion

        #region Properties

        public override string MethodName
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

        public override bool IsAlive
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

        #endregion

        #region Constructor

        public WeakAction(Action<T> action)
            : this(action == null ? null : action.Target, action)
        {
        }

        public WeakAction(object target, Action<T> action)
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

        public new void Execute()
        {
            Execute(default(T));
        }

        public void Execute(T parameter)
        {
            if (m_StaticAction != null)
            {
                m_StaticAction(parameter);
                return;
            }

            object actionTarget = ActionTarget;

            if (IsAlive)
            {
                if (Method != null
                    && ActionReference != null
                    && actionTarget != null)
                {
                    Method.Invoke(
                        actionTarget,
                        new object[]
                        {
                            parameter
                        });
                }
            }
        }

        public void ExecuteWithObject(object parameter)
        {
            var parameterCasted = (T) parameter;
            Execute(parameterCasted);
        }

        public new void MarkForDeletion()
        {
            m_StaticAction = null;
            base.MarkForDeletion();
        }

        #endregion
    }
}