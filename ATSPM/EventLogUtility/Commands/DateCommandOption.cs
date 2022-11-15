using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace ATSPM.EventLogUtility.Commands
{
    public class DateCommandOption : Option<IEnumerable<DateTime>>
    {
        public DateCommandOption() : base("--date", "List of dates in dd/mm/yyyy format")
        {
            //IsRequired = true;
            AllowMultipleArgumentsPerToken = true;
            AddAlias("-d");
            AddValidator(r =>
            {
                if (r.GetValueForOption(this).Any(a => a > DateTime.Now))
                    r.ErrorMessage = "Date must not be greater than current date";
            });
        }
    }
}
