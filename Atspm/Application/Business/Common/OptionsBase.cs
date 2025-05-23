﻿#region license
// Copyright 2025 Utah Departement of Transportation
// for Application - Utah.Udot.Atspm.Business.Common/OptionsBase.cs
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

namespace Utah.Udot.Atspm.Business.Common
{
    public class OptionsBase
    {
        private DateTime _start;
        private DateTime _end;
        public DateTime Start
        {
            get
            {
                return _start;
            }
            set
            {
                _start = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
            }
        }
        public DateTime End
        {
            get
            {
                return _end;
            }
            set
            {
                _end = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
            }
        }
        public string LocationIdentifier { get; set; }
    }
}