using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.Application.Models
{
    public class GeneralSettings : ApplicationSettings
    {
        public string ImageUrl { get; set; }
        public string ImagePath { get; set; }
        public string ReCaptchaPublicKey { get; set; }
        public string ReCaptchaSecretKey { get; set; }
        public int? RawDataCountLimit { get; set; }
        public int? CycleCompletionSeconds { get; set; }

    }
}
