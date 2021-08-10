using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ATSPM.Domain.BaseClasses
{
    public abstract class ServiceObjectBase : ObservableObjectBase, ISupportInitializeNotification, IDisposable
    {
        public ServiceObjectBase()
        {
            //intialize object
            BeginInit();
        }

        public virtual void Initialize()
        {
            //initialize complete
            EndInit();
        }

        protected override bool Set<T>(ref T currentValue, T newValue, IEqualityComparer<T> comparer, [CallerMemberName] string propertyName = "")
        {
            //check IEquatable<T>
            if (currentValue is IEquatable<T> equate && equate.Equals(newValue)) { return false; }

            //see if value has changed
            if (comparer.Equals(currentValue, newValue)) { return false; }

            //raise property changing
            if (IsInitialized) { RaisePropertyChanging(propertyName); }

            //IsChanged
            //if (IsInitialized) { AddChange(propertyName, currentValue); }

            //update value
            currentValue = newValue;

            //raise property changed
            if (IsInitialized) { RaisePropertyChanged(propertyName); }

            return true;
        }

        #region ISupportInitializeNotification

        public event EventHandler Initialized;

        private bool _isInitialized;
        //[Newtonsoft.Json.JsonIgnore]
        public bool IsInitialized
        {
            get { return _isInitialized; }
            private set
            {
                _isInitialized = value;
                RaisePropertyChanged(nameof(IsInitialized));
                if (_isInitialized) { RaiseInitialized(); }
            }
        }

        public void BeginInit()
        {
            if (IsInitialized) { IsInitialized = false; }
            else { Initialize(); }
        }

        public void EndInit()
        {
            //clear changes
            //AcceptChanges();

            //raise flag
            IsInitialized = true;
        }

        #endregion

        #region IDisposable

        public abstract void Dispose();

        #endregion

        protected void RaiseInitialized()
        {
            RaisePropertyChanged(string.Empty);

            Initialized?.Invoke(this, new EventArgs());
        }
    }
}
