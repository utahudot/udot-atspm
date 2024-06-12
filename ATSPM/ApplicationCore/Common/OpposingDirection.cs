#region license
// Copyright 2024 Utah Departement of Transportation
// for ApplicationCore - ATSPM.Application.Common/OpposingDirection.cs
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
using ATSPM.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Common
{
    /// <summary>
    /// Used to obtain opposing phase directions
    /// </summary>
    public readonly struct OpposingDirection
    {
        private readonly DirectionTypes _direction;

        public OpposingDirection(DirectionTypes direction) => _direction = direction;

        /// <summary>
        /// Used when converting <see cref="DirectionTypes"/> to <see cref="OpposingDirection"/>
        /// and returns the opposing phase
        /// </summary>
        /// <param name="o"></param>
        public static implicit operator DirectionTypes(OpposingDirection o) => o._direction switch
        {
            DirectionTypes.EB => DirectionTypes.WB,
            DirectionTypes.WB => DirectionTypes.EB,
            DirectionTypes.NB => DirectionTypes.SB,
            DirectionTypes.SB => DirectionTypes.NB,
            DirectionTypes.NE => DirectionTypes.SW,
            DirectionTypes.NW => DirectionTypes.NE,
            DirectionTypes.SE => DirectionTypes.NW,
            DirectionTypes.SW => DirectionTypes.NE,
            _ => DirectionTypes.NA,
        };

        /// <summary>
        /// Used when converting <see cref="OpposingDirection"/> to <see cref="DirectionTypes"/>
        /// and returns the opposing phase
        /// </summary>
        /// <param name="o"></param>
        public static explicit operator OpposingDirection(DirectionTypes d) => d switch
        {
            DirectionTypes.EB => new OpposingDirection(DirectionTypes.WB),
            DirectionTypes.WB => new OpposingDirection(DirectionTypes.EB),
            DirectionTypes.NB => new OpposingDirection(DirectionTypes.SB),
            DirectionTypes.SB => new OpposingDirection(DirectionTypes.NB),
            DirectionTypes.NE => new OpposingDirection(DirectionTypes.SW),
            DirectionTypes.NW => new OpposingDirection(DirectionTypes.NE),
            DirectionTypes.SE => new OpposingDirection(DirectionTypes.NW),
            DirectionTypes.SW => new OpposingDirection(DirectionTypes.NE),
            _ => new OpposingDirection(DirectionTypes.NA),
        };
    }
}
