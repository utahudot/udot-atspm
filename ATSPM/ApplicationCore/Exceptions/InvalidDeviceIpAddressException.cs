#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Exceptions/InvalidDeviceIpAddressException.cs
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

using ATSPM.Data.Models;

#nullable enable
namespace ATSPM.Application.Exceptions
{
    /// <summary>
    /// Thrown when a <see cref="Device.Ipaddress"/> is either invalid or can't be reached 
    /// </summary>
    public class InvalidDeviceIpAddressException : AtspmException
    {
        /// <inheritdoc/>
        public InvalidDeviceIpAddressException(Device device) : base($"{device.Ipaddress} is either invalid or can't be reached")
        {
            Device = device;
        }

        /// <summary>
        /// <see cref="Device"/> that exception was thrown for
        /// </summary>
        public Device Device { get; private set; }
    }
}
