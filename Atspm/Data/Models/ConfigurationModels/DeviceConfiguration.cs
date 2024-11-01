﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Models/DeviceConfiguration.cs
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

// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Relationships;

namespace Utah.Udot.Atspm.Data.Models
{
    /// <summary>
    /// Device configuration
    /// </summary>
    public partial class DeviceConfiguration : AtspmConfigModelBase<int>, IRelatedDevices, IRelatedProduct
    {
        /// <summary>
        /// Firmware version
        /// </summary>
        public string Firmware { get; set; }

        /// <summary>
        /// Configuration notes
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Transport protocol for controller logging
        /// </summary>
        public TransportProtocols Protocol { get; set; }

        /// <summary>
        /// Logging communication port
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Path to log directory
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// Log search term to find log in directory
        /// Can be a query string or file extension
        /// </summary>
        public string[] SearchTerms { get; set; }

        /// <summary>
        /// Device connection timeout in milliseconds
        /// </summary>
        public int ConnectionTimeout { get; set; }

        /// <summary>
        /// Device operation timeout in milliseconds
        /// </summary>
        public int OperationTimeout { get; set; }

        /// <summary>
        /// Decoders used to decode events logs
        /// </summary>
        public string[] Decoders { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }

        #region IRelatedProduct

        /// <inheritdoc/>
        public int? ProductId { get; set; }

        /// <inheritdoc/>
        public virtual Product Product { get; set; }

        #endregion

        #region IRelatedDevices

        /// <inheritdoc/>
        public virtual ICollection<Device> Devices { get; set; } = new HashSet<Device>();

        #endregion

        /// <inheritdoc/>
        public override string ToString() => $"{Id} - {Firmware} - {Product}";
    }
}