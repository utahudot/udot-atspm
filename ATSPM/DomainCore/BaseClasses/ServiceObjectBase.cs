#region license
// Copyright 2024 Utah Departement of Transportation
// for DomainCore - ATSPM.Domain.BaseClasses/ServiceObjectBase.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using ATSPM.Domain.Common;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ATSPM.Domain.BaseClasses
{

    /// <inheritdoc cref="IExecutableServiceWithProgress{Tin, Tout, Tp}"/>
    public abstract class ExecutableServiceWithProgressBase<Tin, Tout, Tp> : ExecutableServiceBase<Tin, Tout>, IExecutableServiceWithProgress<Tin, Tout, Tp>
    {
        /// <inheritdoc/>
        public ExecutableServiceWithProgressBase(bool initialize = false) : base(initialize) { }

        #region IExecutableServiceWithProgress

        ///<inheritdoc/>
        public abstract Task<Tout> ExecuteAsync(Tin parameter, IProgress<Tp> progress = null, CancellationToken cancelToken = default);

        ///<inheritdoc/>
        public override async Task<Tout> ExecuteAsync(Tin parameter, CancellationToken cancelToken = default)
        {
            return await ExecuteAsync(parameter, cancelToken).ConfigureAwait(false);
        }

        #endregion
    }

    /// <inheritdoc cref="IExecutableService{Tin, Tout}"/>
    public abstract class ExecutableServiceBase<Tin, Tout> : ServiceObjectBase, IExecutableService<Tin, Tout>
    {
        /// <inheritdoc/>
        public ExecutableServiceBase(bool initialize = false) : base(initialize) { }

        #region IExecuteAsync

        /// <inheritdoc/>
        public event EventHandler CanExecuteChanged;

        /// <inheritdoc/>
        public virtual bool CanExecute(Tin parameter) => IsInitialized;

        /// <inheritdoc/>
        public abstract Task<Tout> ExecuteAsync(Tin parameter, CancellationToken cancelToken = default);

        /// <inheritdoc/>
        Task IExecuteAsync.ExecuteAsync(object parameter)
        {
            if (parameter is Tin p)
                return Task.Run(() => ExecuteAsync(p, default));
            return default;
        }

        /// <inheritdoc/>
        bool ICommand.CanExecute(object parameter)
        {
            if (parameter is Tin p)
                return CanExecute(p);
            return default;
        }

        /// <inheritdoc/>
        void ICommand.Execute(object parameter)
        {
            if (parameter is Tin p)
                Task.Run(() => ExecuteAsync(p, default));
        }

        #endregion
    }

    /// <inheritdoc cref="IExecutableServiceWithProgressAsync{Tin, Tout, Tp}"/>
    public abstract class ExecutableServiceWithProgressAsyncBase<Tin, Tout, Tp> : ExecutableServiceAsyncBase<Tin, Tout>, IExecutableServiceWithProgressAsync<Tin, Tout, Tp>
    {
        /// <inheritdoc/>
        public ExecutableServiceWithProgressAsyncBase(bool initialize = false) : base(initialize) { }

        #region IExecutableServiceWithProgress

        ///<inheritdoc/>
        public abstract IAsyncEnumerable<Tout> Execute(Tin parameter, IProgress<Tp> progress = null, CancellationToken cancelToken = default);

        ///<inheritdoc/>
        public override async IAsyncEnumerable<Tout> Execute(Tin parameter, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            await foreach (var item in Execute(parameter, default, cancelToken).WithCancellation(cancelToken))
            {
                yield return item;
            }
        }

        #endregion
    }

    /// <inheritdoc cref="IExecutableServiceAsync{Tin, Tout}"/>
    public abstract class ExecutableServiceAsyncBase<Tin, Tout> : ServiceObjectBase, IExecutableServiceAsync<Tin, Tout>
    {
        /// <inheritdoc/>
        public ExecutableServiceAsyncBase(bool initialize = false) : base(initialize) { }

        #region IExecuteAsync

        /// <inheritdoc/>
        public event EventHandler CanExecuteChanged;

        /// <inheritdoc/>
        public virtual bool CanExecute(Tin parameter) => IsInitialized;

        /// <inheritdoc/>
        public abstract IAsyncEnumerable<Tout> Execute(Tin parameter, CancellationToken cancelToken = default);

        /// <inheritdoc/>
        bool ICommand.CanExecute(object parameter)
        {
            if (parameter is Tin p)
                return CanExecute(p);
            return false;
        }

        /// <inheritdoc/>
        void ICommand.Execute(object parameter)
        {
            if (parameter is Tin p)
                Task.Run(() => Execute(p, default));
        }

        #endregion
    }

    /// <inheritdoc cref="IService"/>
    public abstract class ServiceObjectBase : ObservableObjectBase, IService
    {
        /// <summary>
        /// Instantiate new service and calls <see cref="BeginInit"/> if <paramref name="initialize"/> is true
        /// </summary>
        public ServiceObjectBase(bool initialize = false)
        {
            if (initialize)
            {
                BeginInit();
            }
        }

        /// <summary>
        /// Initialize service
        /// </summary>
        /// <remarks>Constructor calls <see cref="BeginInit"/> and initializes on instantiation.</remarks>
        public virtual Task Initialize()
        {
            //initialize complete
            //EndInit();

            return Task.CompletedTask;
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

        ///<inheritdoc/>
        public event EventHandler Initialized;

        private bool _isInitialized;

        //[Newtonsoft.Json.JsonIgnore]
        ///<inheritdoc/>
        public bool IsInitialized
        {
            get { return _isInitialized; }
            protected set
            {
                _isInitialized = value;
                RaisePropertyChanged(nameof(IsInitialized));
                if (_isInitialized) 
                {
                    disposedValue = false;
                    RaiseInitialized(); 
                }
            }
        }

        ///<inheritdoc/>
        public void BeginInit()
        {
            IsInitialized = false;
            Task.Run(() => Initialize().ContinueWith(t => EndInit()));
        }

        ///<inheritdoc/>
        public void EndInit()
        {
            //clear changes
            //AcceptChanges();

            //raise flag
            IsInitialized = true;
        }

        #endregion

        #region IDisposable

        protected bool disposedValue;

        ///<inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);

            //show that the service is not Initialized
            IsInitialized = false;
        }

        /// <summary>
        /// Used for IDisposable Pattern
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly or indirectly by a user's code.
        /// Managed and unmanaged resources can be disposed.
        /// If disposing equals false, the method has been called by the runtime from inside the finalizer and you should not reference other objects.
        /// Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">Flag for keeping track of disposed state</param>
        protected void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!disposedValue)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    DisposeManagedCode();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                DisposeUnManagedCode();

                // Note disposing has been done.
                disposedValue = true;
            }
        }

        /// <summary>
        /// Custom implementation of the IDisposable Pattern.
        /// Override to dispose of Managed Code.
        /// </summary>
        protected virtual void DisposeManagedCode() { }

        /// <summary>
        /// Custom implementation of the IDisposable Pattern.
        /// Override to dispose of Un-Managed Code.
        /// </summary>
        protected virtual void DisposeUnManagedCode() { }

        /// <summary>
        /// Use C# finalizer syntax for finalization code.
        /// This finalizer will run only if the Dispose method does not get called.
        /// It gives your base class the opportunity to finalize.
        /// Do not provide finalizer in types derived from this class.
        /// </summary>
        ~ServiceObjectBase()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(disposing: false) is optimal in terms of
            // readability and maintainability.
            Dispose(disposing: false);
        }

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
