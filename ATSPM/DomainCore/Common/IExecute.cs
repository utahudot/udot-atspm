using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ATSPM.Domain.Common
{
    public interface IExecuteAsync : ICommand
    {
        Task ExecuteAsync(object parameter);
    }
    
    public interface IExecute : ICommand
    {
    }

    public interface IExecute<Tin, Tout> : IExecute
    {
        bool CanExecute(Tin parameter);

        Tout Execute(Tin parameter, CancellationToken cancelToken = default);

        Tout Execute<T>(Tin parameter, CancellationToken cancelToken = default, IProgress<T> progress = default);
    }

    public interface IExecuteAsync<Tin, Tout> : IExecuteAsync
    {
        bool CanExecute(Tin parameter);

        Task<Tout> ExecuteAsync(Tin parameter, CancellationToken cancelToken = default);
    }

    public interface IExecuteAsyncWithProgress<Tin, Tout, Tp> : IExecuteAsync<Tin, Tout>
    {
        Task<Tout> ExecuteAsync(Tin parameter, CancellationToken cancelToken = default, IProgress<Tp> progress = default);
    }
}
