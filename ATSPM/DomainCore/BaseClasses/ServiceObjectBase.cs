using ATSPM.Domain.Common;
using ATSPM.Domain.Exceptions;
using ATSPM.Domain.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Input;

namespace ATSPM.Domain.BaseClasses
{
    public abstract class ExectuableServiceWithProgressBase<T1, T2, Tp> : ExecutableServiceBase<T1, T2>, IExecutableServiceWithProgress<T1, T2, Tp>
    {
        /// <summary>
        /// Instantiate new service and calls <see cref="ServiceObjectBase.BeginInit"/> if <paramref name="initialize"/> is true
        /// </summary>
        public ExectuableServiceWithProgressBase(bool initialize = false) : base(initialize) { }

        #region IExecutableServiceWithProgress

        ///<inheritdoc/>
        public abstract Task<T2> ExecuteAsync(T1 parameter, IProgress<Tp> progress = null, CancellationToken cancelToken = default);

        ///<inheritdoc/>
        public override async Task<T2> ExecuteAsync(T1 parameter, CancellationToken cancelToken = default)
        {
            return await ExecuteAsync(parameter, cancelToken).ConfigureAwait(false);
        }

        #endregion
    }

    public abstract class ExecutableServiceBase<T1, T2> : ServiceObjectBase, IExecutableService<T1, T2>
    {
        /// <summary>
        /// Instantiate new service and calls <see cref="ServiceObjectBase.BeginInit"/> if <paramref name="initialize"/> is true
        /// </summary>
        public ExecutableServiceBase(bool initialize = false) : base(initialize) { }

        #region IExecuteAsync

        /// <inheritdoc/>
        public event EventHandler CanExecuteChanged;

        /// <inheritdoc/>
        public virtual bool CanExecute(T1 parameter) => true;

        /// <inheritdoc/>
        public abstract Task<T2> ExecuteAsync(T1 parameter, CancellationToken cancelToken = default);

        /// <inheritdoc/>
        Task IExecuteAsync.ExecuteAsync(object parameter)
        {
            if (parameter is T1 p)
                return Task.Run(() => ExecuteAsync(p, default));
            return default;
        }

        /// <inheritdoc/>
        bool ICommand.CanExecute(object parameter)
        {
            if (parameter is T1 p)
                return CanExecute(p);
            return default;
        }

        /// <inheritdoc/>
        void ICommand.Execute(object parameter)
        {
            if (parameter is T1 p)
                Task.Run(() => ExecuteAsync(p, default));
        }

        #endregion
    }

    public abstract class ExecutableServiceWithProgressAsyncBase<T1, T2, Tp> : ExecutableServiceAsyncBase<T1, T2>, IExecutableServiceWithProgressAsync<T1, T2, Tp>
    {
        /// <summary>
        /// Instantiate new service and calls <see cref="ServiceObjectBase.BeginInit"/> if <paramref name="initialize"/> is true
        /// </summary>
        public ExecutableServiceWithProgressAsyncBase(bool initialize = false) : base(initialize) { }

        #region IExecutableServiceWithProgress

        ///<inheritdoc/>
        public abstract IAsyncEnumerable<T2> Execute(T1 parameter, IProgress<Tp> progress = null, CancellationToken cancelToken = default);

        ///<inheritdoc/>
        public override async IAsyncEnumerable<T2> Execute(T1 parameter, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            await foreach (var item in Execute(parameter, default, cancelToken).WithCancellation(cancelToken))
            {
                yield return item;
            }
        }

        #endregion
    }

    public abstract class ExecutableServiceAsyncBase<T1, T2> : ServiceObjectBase, IExecutableServiceAsync<T1, T2>
    {
        /// <summary>
        /// Instantiate new service and calls <see cref="ServiceObjectBase.BeginInit"/> if <paramref name="initialize"/> is true
        /// </summary>
        public ExecutableServiceAsyncBase(bool initialize = false) : base(initialize) { }

        #region IExecuteAsync

        /// <inheritdoc/>
        public event EventHandler CanExecuteChanged;

        /// <inheritdoc/>
        public virtual bool CanExecute(T1 parameter) => true;

        /// <inheritdoc/>
        public abstract IAsyncEnumerable<T2> Execute(T1 parameter, CancellationToken cancelToken = default);

        /// <inheritdoc/>
        bool ICommand.CanExecute(object parameter)
        {
            if (parameter is T1 p)
                return CanExecute(p);
            return false;
        }

        /// <inheritdoc/>
        void ICommand.Execute(object parameter)
        {
            if (parameter is T1 p)
                Task.Run(() => Execute(p, default));
        }

        #endregion
    }

    /// <inheritdoc/>
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
            if (IsInitialized) { IsInitialized = false; }
            else { Initialize(); }
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

        private bool disposedValue;

        ///<inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Used for IDisposable Pattern
        /// </summary>
        /// <param name="disposing">Flag for keeping track of disposed state</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // // TODO: dispose managed state (managed objects)
                }

                // // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // // TODO: set large fields to null
                IsInitialized = false;
                disposedValue = true;
            }
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

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ServiceObjectBase()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        
    }
}
