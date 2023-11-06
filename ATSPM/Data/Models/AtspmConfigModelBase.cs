using ATSPM.Domain.BaseClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Data.Models
{
    /// <summary>
    /// Base class for configuration context models.
    /// This base includes interfaces for working with user interfaces.
    /// </summary>
    public class AtspmConfigModelBase<T> : ObjectModelBase
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public T Id { get; set; }
    }
}
