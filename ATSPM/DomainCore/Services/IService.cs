using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Domain.Services
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
    public interface IService : INotifyPropertyChanged, INotifyPropertyChanging, ISupportInitializeNotification, IDisposable { }

    public interface IExecutableService<Tin, Tout> : IService, IExecuteAsync<Tin, Tout> { }

    public interface IExecutableServiceWithProgress<Tin, Tout, Tp> : IService, IExecuteAsyncWithProgress<Tin, Tout, Tp> { }

    public interface IExecutableServiceAsync<Tin, Tout> : IService, IExecute<Tin, IAsyncEnumerable<Tout>> { }

    public interface IExecutableServiceWithProgressAsync<Tin, Tout, Tp> : IService, IExecuteWithProgress<Tin, IAsyncEnumerable<Tout>, Tp> { }
}
