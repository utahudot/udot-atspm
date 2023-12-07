using ATSPM.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ATSPM.Application.Analysis.ReportServices
{
    /// <summary>
    /// Base for report services
    /// </summary>
    /// <typeparam name="Tin"></typeparam>
    /// <typeparam name="Tout"></typeparam>
    public abstract class ReportServiceBase<Tin, Tout> : IReportService<Tin, Tout>
    {
        #region IExecuteWithProgress

        /// <inheritdoc/>
        public event EventHandler? CanExecuteChanged;

        /// <inheritdoc/>
        public abstract Task<Tout> ExecuteAsync(Tin parameter, IProgress<int> progress = null, CancellationToken cancelToken = default);

        /// <inheritdoc/>
        public virtual bool CanExecute(Tin parameter)
        {
            var context = new ValidationContext(parameter);
            var validationResults = new List<ValidationResult>();

            return Validator.TryValidateObject(parameter, context, validationResults, true);
        }

        /// <inheritdoc/>
        public async Task<Tout> ExecuteAsync(Tin parameter, CancellationToken cancelToken = default)
        {
            return await ExecuteAsync(parameter, default, cancelToken);
        }

        Task IExecuteAsync.ExecuteAsync(object parameter)
        {
            if (parameter is Tin p)
                Task.Run(() => ExecuteAsync(p, default, default));

            return Task.CompletedTask;
        }

        bool ICommand.CanExecute(object parameter)
        {
            if (parameter is Tin p)
                return CanExecute(p);
            return false;
        }

        void ICommand.Execute(object parameter)
        {
            if (parameter is Tin p)
                Task.Run(() => ExecuteAsync(p, default, default));
        }

        #endregion
    }
}
