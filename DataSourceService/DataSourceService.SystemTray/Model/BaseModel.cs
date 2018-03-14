namespace ImmotionAR.ImmotionRoom.DataSourceService.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class BaseModel : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Private methods

        private void RaisePropertyChanged<T>([CallerMemberName] string propertyName = null, T oldValue = default(T), T newValue = default(T), bool broadcast = false)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("This method cannot be called with an empty string", "propertyName");
            }

            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(propertyName);
        }

        protected bool Set<T>(ref T field, T newValue = default(T), [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            var oldValue = field;
            field = newValue;

            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(propertyName, oldValue, field);

            return true;
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
