﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Services/ReportServiceBase.cs
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

using System.ComponentModel.DataAnnotations;
using System.Windows.Input;

namespace Utah.Udot.Atspm.Services
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
        public event EventHandler CanExecuteChanged;

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
