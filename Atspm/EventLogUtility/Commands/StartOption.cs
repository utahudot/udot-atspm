using System.CommandLine;

namespace Utah.Udot.ATSPM.EventLogUtility.Commands
{
    public class StartOption : Option<DateTime>
    {
        public StartOption() : base("--start", "Start date/time range")
        {
            IsRequired = true;
            AddAlias("-s");
            AddValidator(r =>
            {
                if (r.GetValueForOption(this) > DateTime.Now)
                    r.ErrorMessage = "Start date must not be greater than current date/time";
            });
        }
    }
}
