#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Repositories.ConfigurationRepositories/MeasureOptionsEFRepository.cs
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
using ATSPM.Application.Repositories.ConfigurationRepositories;
using ATSPM.Data;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ATSPM.Infrastructure.Repositories.ConfigurationRepositories
{
    ///<inheritdoc cref="IMeasureOptionsRepository"/>
    public class MeasureOptionsEFRepository : ATSPMRepositoryEFBase<MeasureOption>, IMeasureOptionsRepository
    {
        /// <inheritdoc/>
        public MeasureOptionsEFRepository(ConfigContext db, ILogger<MeasureOptionsEFRepository> log) : base(db, log) { }

        #region Overrides

        /// <inheritdoc/>
        public override IQueryable<MeasureOption> GetList()
        {
            return base.GetList().Include(i => i.MeasureType);
        }

        #endregion

        #region IMeasureOptionsRepository

        //[Obsolete("Use GetList instead")]
        //IReadOnlyList<MeasureOption> GetAll();

        //[Obsolete("Use GetList instead")]
        //IQueryable<string> GetListOfMeasures();

        //[Obsolete("Use Lookup instead")]
        //IReadOnlyList<MeasureOption> GetMeasureDefaults(string chart);

        //[Obsolete("Use GetList and ToDictionary instead")]
        //Dictionary<string, string> GetAllAsDictionary();

        //[Obsolete("Use GetList and ToDictionary instead")]
        //Dictionary<string, string> GetMeasureDefaultsAsDictionary(string chart);

        //[Obsolete("Use Update in the BaseClass")]
        //void Update(MeasureOption option);

        #endregion
    }
}
