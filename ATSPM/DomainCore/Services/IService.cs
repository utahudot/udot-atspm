using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ATSPM.Domain.Services
{
    /// <summary>
    /// <c>Service Definition</c> For services implementing:
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
    public interface IService : INotifyPropertyChanged, INotifyPropertyChanging, ISupportInitializeNotification, IDisposable { }


    /// <summary>
    /// <c>Service Definition</c> For executable services implementing:
    /// <list type="table">
    /// 
    /// <item>
    /// <term><see cref="IExecuteAsync"/></term>
    /// <description>Defines an async command or process with input and ouput parameters which can conditionally be executed.</description>
    /// </item>
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
    public interface IExecutableService<Tin, Tout> : IService, IExecuteAsync<Tin, Tout> { }


    /// <summary>
    /// <c>Service Definition</c> For executable services with progress implementing:
    /// <list type="table">
    /// 
    /// <item>
    /// <term><see cref="IExecuteAsyncWithProgress{Tin, Tout, Tp}"/></term>
    /// <description>Defines an async command or operation with input and ouput parameters which can conditionally be executed and use IProgress.</description>
    /// </item>
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
    public interface IExecutableServiceWithProgress<Tin, Tout, Tp> : IService, IExecuteAsyncWithProgress<Tin, Tout, Tp> { }


    /// <summary>
    /// <c>Service Definition</c> For <see cref="IAsyncEnumerable{T}"/> executable services implementing:
    /// <list type="table">
    /// 
    /// <item>
    /// <term><see cref="IExecute"/></term>
    /// <description>Defines a command with input and ouput parameters which can conditionally be executed.</description>
    /// </item>
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
    public interface IExecutableServiceAsync<Tin, Tout> : IService, IExecute<Tin, IAsyncEnumerable<Tout>> { }

    /// <summary>
    /// <c>Service Definition</c> For <see cref="IAsyncEnumerable{T}"/> executable services with progress implementing:
    /// <list type="table">
    /// 
    /// <item>
    /// <term><see cref="IExecuteWithProgress{Tin, Tout, Tp}"/></term>
    /// <description>Defines a command with input and ouput parameters which can conditionally be executed and use IProgress.</description>
    /// </item>
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
    public interface IExecutableServiceWithProgressAsync<Tin, Tout, Tp> : IService, IExecuteWithProgress<Tin, IAsyncEnumerable<Tout>, Tp> { }
}
