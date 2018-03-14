namespace ImmotionAR.ImmotionRoom.Helpers.Messaging
{
    using System;
    using System.Reflection;

    // Based on the work contained in MetroMvvm, by Gianni Rosa Gallina
    // Copyright © 2012-2014 Gianni Rosa Gallina

    public class WeakFunc<TResult>
    {
        #region Private fields

        private Func<TResult> m_StaticFunc;

        #endregion

        #region Properties

        protected MethodInfo Method { get; set; }

        public bool IsStatic
        {
            get { return m_StaticFunc != null; }
        }

        public virtual string MethodName
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

        protected WeakReference FuncReference { get; set; }

        protected WeakReference Reference { get; set; }

        protected virtual bool IsAlive
        {
            get
            {
                if (m_StaticFunc == null
                    && Reference == null)
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

        protected object FuncTarget
        {
            get
            {
                if (FuncReference == null)
                {
                    return null;
                }

                return FuncReference.Target;
            }
        }

        #endregion

        #region Constructor

        protected WeakFunc()
        {
        }

        public WeakFunc(Func<TResult> func) : this(func == null ? null : func.Target, func)
        {
        }

        private WeakFunc(object target, Func<TResult> func)
        {
            if (func.GetMethodInfo().IsStatic)
            {
                m_StaticFunc = func;

                if (target != null)
                {
                    // Keep a reference to the target to control the
                    // WeakAction's lifetime.
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

        public TResult Execute()
        {
            if (m_StaticFunc != null)
            {
                return m_StaticFunc();
            }

            object funcTarget = FuncTarget;

            if (IsAlive)
            {
                if (Method != null
                    && FuncReference != null
                    && funcTarget != null)
                {
                    return (TResult) Method.Invoke(funcTarget, null);
                }
            }

            return default(TResult);
        }

        #endregion

        #region Protected methods

        protected void MarkForDeletion()
        {
            Reference = null;
            FuncReference = null;
            Method = null;
            m_StaticFunc = null;
        }

        #endregion
    }
}