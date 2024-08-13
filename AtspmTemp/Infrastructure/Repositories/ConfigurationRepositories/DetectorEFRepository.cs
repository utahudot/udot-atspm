﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - ATSPM.Infrastructure.Repositories.ConfigurationRepositories/DetectorEFRepository.cs
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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Utah.Udot.Atspm.Common.EqualityComparers;
using Utah.Udot.Atspm.Data;
using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.Infrastructure.Repositories.ConfigurationRepositories
{
    ///<inheritdoc cref="IDetectorRepository"/>
    public class DetectorEFRepository : ATSPMRepositoryEFBase<Detector>, IDetectorRepository
    {
        /// <inheritdoc/>
        public DetectorEFRepository(ConfigContext db, ILogger<DetectorEFRepository> log) : base(db, log) { }

        #region Overrides

        /// <inheritdoc/>
        public override IQueryable<Detector> GetList()
        {
            return base.GetList()
                .Include(i => i.Approach);
            //.Include(i => i.DetectionHardware)
            //.Include(i => i.LaneType)
            //.Include(i => i.MovementType)
            //.Include(i => i.DetectorComments)
            //.Include(i => i.DetectionTypes);
        }

        /// <inheritdoc/>
        protected override void UpdateCollections(Detector oldItem, CollectionEntry oldCollection, Detector newItem, CollectionEntry newCollection)
        {
            switch (oldCollection.Metadata.Name)
            {
                case "DetectionTypes":
                    {
                        var remove = oldItem.DetectionTypes.Except(newItem.DetectionTypes, new ConfigEntityIdComparer<DetectionType, DetectionTypes>());
                        var add = newItem.DetectionTypes.Except(oldItem.DetectionTypes, new ConfigEntityIdComparer<DetectionType, DetectionTypes>());

                        foreach (var r in remove)
                        {
                            oldItem.DetectionTypes.Remove(r);
                        }

                        foreach (var a in add)
                        {
                            oldItem.DetectionTypes.Add(_db.Find<DetectionType>(a.Id));
                        }

                        break;
                    }
                default:
                    {
                        base.UpdateCollections(oldItem, oldCollection, newItem, newCollection);

                        break;
                    }
            }
        }

        #endregion

        #region IDetectorRepository

        #endregion
    }
}
