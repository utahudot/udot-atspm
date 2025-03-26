#region license
// Copyright 2024 Utah Departement of Transportation
// for ConfigApi - ATSPM.ConfigApi.Models/ApproachDto.cs
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


#region license
// Copyright 2024 Utah Departement of Transportation
// for ConfigApi - ATSPM.ConfigApi.Models/ApproachDto.cs
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

using Utah.Udot.Atspm.Data.Enums;

namespace Utah.Udot.Atspm.ValueObjects
{
    public class DeviceConfigurationDto
    {
        public int? Id { get; set; }
        public string Firmware { get; set; }
        public string Notes { get; set; }
        public TransportProtocols Protocol { get; set; }
        public int Port { get; set; }
        public string Directory { get; set; }
        public string[] SearchTerms { get; set; }
        public int ConnectionTimeout { get; set; }
        public int OperationTimeout { get; set; }
        public string[] Decoders { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int? ProductId { get; set; }
        public Product Product { get; set; }
    }
}
