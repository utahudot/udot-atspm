using System.CommandLine;

namespace Utah.Udot.ATSPM.EventLogUtility.Commands
{
    public class EndOption : Option<DateTime>
    {
        public EndOption() : base("--end", "End date/time range")
        {
            IsRequired = true;
            AddAlias("-e");
            AddValidator(r =>
            {
                if (r.GetValueForOption(this) > DateTime.Now)
                    r.ErrorMessage = "End date must not be greater than current date/time";
            });
        }
    }
}
