namespace ImmotionAR.ImmotionRoom.Helpers.Messaging
{
    using System;
    using System.Reflection;

    // Based on the work contained in MetroMvvm, by Gianni Rosa Gallina
    // Copyright © 2012-2014 Gianni Rosa Gallina

    public class WeakFunc<T, TResult> : WeakFunc<TResult>, IExecuteWithObjectAndResult
    {
        #region Private fields

        private Func<T, TResult> m_StaticFunc;

        #endregion

        #region Properties

        public override string MethodName
        {
            get
            {
                if (m_StaticFunc != null)
                {
                    return m_StaticFunc.GetMethodInfo().Name;
                }

                return Method.Name;
            }
        }

        protected override bool IsAlive
        {
            get
            {
                if (m_StaticFunc == null &&
                    Reference == null)
                {
                    return false;
                }

                if (m_StaticFunc != null)
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

        public WeakFunc(Func<T, TResult> func)
            : this(func == null ? null : func.Target, func)
        {
        }


        public WeakFunc(object target, Func<T, TResult> func)
        {
            if (func.GetMethodInfo().IsStatic)
            {
                m_StaticFunc = func;

                if (target != null)
                {
                    Reference = new WeakReference(target);
                }

                return;
            }

            Method = func.GetMethodInfo();

            FuncReference = new WeakReference(func.Target);

            Reference = new WeakReference(target);
        }

        #endregion

        #region Methods

        public new TResult Execute()
        {
            return Execute(default(T));
        }

        public object ExecuteWithObject(object parameter)
        {
            var parameterCasted = (T) parameter;
            return Execute(parameterCasted);
        }

        public new void MarkForDeletion()
        {
            m_StaticFunc = null;
            base.MarkForDeletion();
        }

        #endregion

        #region Private methods

        private TResult Execute(T parameter)
        {
            if (m_StaticFunc != null)
            {
                return m_StaticFunc(parameter);
            }

            var funcTarget = FuncTarget;

            if (IsAlive)
            {
                if (Method != null &&
                    FuncReference != null &&
                    funcTarget != null)
                {
                    return (TResult) Method.Invoke(
                        funcTarget,
                        new object[]
                        {
                            parameter
                        });
                }
            }

            return default(TResult);
        }

        #endregion
    }
}