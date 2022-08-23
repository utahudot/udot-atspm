using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Enums
{
    public enum AgencyTypes
    {
        [Display(Name = "Unknown", Order = 0)]
        NA = 0,
        [Display(Name = "Academics", Order = 0)]
        Academics = 1,
        [Display(Name = "City Government", Order = 1)]
        CityGovernment = 2,
        [Display(Name = "Consultant", Order = 2)]
        Consultant = 3,
        [Display(Name = "County Government", Order = 3)]
        CountyGovernment = 4,
        [Display(Name = "Federal Government", Order = 4)]
        FederalGovernment = 5,
        [Display(Name = "MPO", Order = 5)]
        MPO = 6,
        [Display(Name = "State Government", Order = 6)]
        StateGovernment = 7,
        [Display(Name = "Other", Order = 7)]
        Other = 8
    }
}
