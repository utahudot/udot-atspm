using ControllerLogger.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ControllerLogger.Domain.BaseClasses
{
    public abstract class ObservableObjectBase : INotifyPropertyChanged, INotifyPropertyChanging
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region INotifyPropertyChanging

        public event PropertyChangingEventHandler PropertyChanging;

        #endregion

        protected virtual bool Set<T>(ref T currentValue, T newValue, [CallerMemberName] string propertyName = "")
        {
            return Set(ref currentValue, newValue, new LambdaEqualityComparer<T>((x, y) => Equals(x, y)), propertyName);
        }

        protected virtual bool Set<T>(ref T currentValue, T newValue, IEqualityComparer<T> comparer, [CallerMemberName] string propertyName = "")
        {
            //check IEquatable<T>
            if (currentValue is IEquatable<T> equate && equate.Equals(newValue)) { return false; }

            //see if value has changed
            if (comparer.Equals(currentValue, newValue)) { return false; }

            //raise property changing
            RaisePropertyChanging(propertyName);

            //update value
            currentValue = newValue;

            //raise property changed
            RaisePropertyChanged(propertyName);

            return true;
        }

        public virtual void RaisePropertyChanging([CallerMemberName] string propertyName = null)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        public virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
