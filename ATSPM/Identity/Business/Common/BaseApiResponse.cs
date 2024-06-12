#region license
// Copyright 2024 Utah Departement of Transportation
// for Identity - Identity.Business.Common/BaseApiResponse.cs
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
namespace Identity.Business.Common
{
    public class BaseApiResponse
    {
        public BaseApiResponse(int code, string? error)
        {
            Code = code;
            Error = error.Length != 0 ? error : "";
        }
        public int Code { get; set; }
        public string? Error { get; set; }

    }
}