#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Models/CompressedDataBase.cs
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

using Utah.Udot.Atspm.Data.Interfaces;
using Utah.Udot.Atspm.Data.Models.EventLogModels;
using Utah.Udot.NetStandardToolkit.Common;

#nullable disable

namespace Utah.Udot.Atspm.Data.Models
{
    /// <summary>
    /// Base for compressed database table models
    /// </summary>
    public abstract class CompressedDataBase : StartEndRange, ILocationLayer
    {
        private IEnumerable<ILocationLayer> data;

        ///<inheritdoc/>
        public string LocationIdentifier { get; set; }

        /// <summary>
        /// Model type that is used in the compressed <see cref="Data"/>
        /// </summary>
        public Type DataType { get; set; }

        /// <summary>
        /// Compressed data, ovverride or use <c>new</c> in derrived class for specific type
        /// </summary>
        public virtual IEnumerable<ILocationLayer> Data
        {
            get
            {
                if (data != null)
                {
                    foreach (var d in data)
                    {
                        d.LocationIdentifier = LocationIdentifier;
                    }
                }
                else
                {
                    data = new List<ILocationLayer>();
                }

                return data?.ToList();
            }
            set => data = value;
        }
    }

    /// <summary>
    /// Base for compressed events database table models
    /// </summary>
    public abstract class CompressedEventLogBase : CompressedDataBase
    {
        /// <summary>
        /// Id of the device the logs came from
        /// </summary>
        public int DeviceId { get; set; }

        ///<inheritdoc cref="CompressedDataBase.Data"/>
        public new IEnumerable<EventLogModelBase> Data
        {
            get => base.Data.Cast<EventLogModelBase>().ToList();
            set => base.Data = value;
        }

        public override string ToString()
        {
            return $"{this.LocationIdentifier} - {this.Start} - {this.End} - {this.DeviceId} - {this.DataType.Name} - {this.Data?.Count()} - {this.DataType.Name} | min: {this.Data?.Min(m => m.Timestamp)} max: {this.Data?.Max(m => m.Timestamp)}";
        }
    }

    /// <summary>
    /// Generic type to use when compressing <see cref="EventLogModelBase"/> objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CompressedEventLogs<T> : CompressedEventLogBase where T : EventLogModelBase
    {
        ///<inheritdoc cref="CompressedDataBase.Data"/>
        public new ICollection<T> Data
        {
            get => base.Data.Cast<T>().ToHashSet().ToList();
            set => base.Data = value.Cast<T>().ToHashSet().ToList();
        }
    }

    /// <summary>
    /// Base for compressed aggregation database table models
    /// </summary>
    public abstract class CompressedAggregationBase : CompressedDataBase
    {
        ///<inheritdoc cref="CompressedDataBase.Data"/>
        public new IEnumerable<AggregationModelBase> Data
        {
            get => base.Data.Cast<AggregationModelBase>().ToList();
            set => base.Data = value;
        }

        public override string ToString()
        {
            return $"{this.LocationIdentifier} - {this.Start} - {this.End} - {this.DataType.Name} - {this.Data?.Count()} - {this.DataType.Name}";
        }
    }

    /// <summary>
    /// Generic type to use when compressing <see cref="AggregationModelBase"/> objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CompressedAggregations<T> : CompressedAggregationBase where T : AggregationModelBase
    {
        ///<inheritdoc cref="CompressedDataBase.Data"/>
        public new ICollection<T> Data
        {
            get => base.Data.Cast<T>().ToHashSet().ToList();
            set => base.Data = value.Cast<T>().ToHashSet().ToList();
        }

        public override string ToString()
        {
            return $"{this.LocationIdentifier} - {this.Start} - {this.End} - {this.DataType.Name} - {this.Data?.Count()} - {this.DataType.Name}";
        }
    }
}
