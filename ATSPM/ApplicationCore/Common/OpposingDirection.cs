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
