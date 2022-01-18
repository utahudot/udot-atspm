using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ATSPM.Domain.BaseClasses
{
    /// <summary>
    /// <c>ServiceObjectBase</c> For services implementing:
    /// <list type="table">
    /// 
    /// <item>
    /// <term><see cref="INotifyPropertyChanged"/></term>
    /// <description>Notifies clients that a property value has changed.</description>
    /// </item>
    /// 
    /// <item>
    /// <term><see cref="INotifyPropertyChanging"/></term>
    /// <description>Notifies clients that a property value is changing.</description>
    /// </item>
    /// 
    /// <item>
    /// <term><see cref="ISupportInitializeNotification"/></term>
    /// <description>Allows coordination of initialization for a component and its dependent properties.</description>
    /// </item>
    /// 
    /// <item>
    /// <term><see cref="IDisposable"/></term>
    /// <description>Provides a mechanism for releasing unmanaged resources.</description>
    /// </item>
    /// 
    /// </list>
    /// </summary>
    public abstract class ServiceObjectBase : ObservableObjectBase, ISupportInitializeNotification, IDisposable
    {
        /// <summary>
        /// Instantiate new service and calls <see cref="BeginInit"/>
        /// </summary>
        public ServiceObjectBase()
        {
            //intialize object
            BeginInit();
        }

        /// <summary>
        /// Initialize service
        /// </summary>
        /// <remarks>Constructor calls <see cref="BeginInit"/> and initializes on instantiation.</remarks>
        public virtual void Initialize()
        {
            //initialize complete
            EndInit();
        }

        /// <summary>
        /// Sets a properties value and raises the <see cref="ObservableObjectBase.PropertyChanging"/> and <see cref="ObservableObjectBase.PropertyChanged"/> events if <paramref name="newValue"/> != <paramref name="currentValue"/>.
        /// </summary>
        /// <remarks>Overriden from <see cref="ObservableObjectBase"/> to check for validation errors and change tracking.</remarks>
        protected override bool Set<T>(ref T currentValue, T newValue, IEqualityComparer<T> comparer, [CallerMemberName] string propertyName = "")
        {
            //check IEquatable<T>
            if (currentValue is IEquatable<T> equate && equate.Equals(newValue)) { return false; }

            //see if value has changed
            if (comparer.Equals(currentValue, newValue)) { return false; }

            //raise property changing
            if (IsInitialized) { RaisePropertyChanging(propertyName); }

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

        /// <summary>
        /// Raise <see cref="Initialized"/> when initialization is complete.
        /// </summary>
        protected void RaiseInitialized()
        {
            RaisePropertyChanged(string.Empty);

            Initialized?.Invoke(this, new EventArgs());
        }
    }
}
