using ATSPM.Application.Configuration;
using ATSPM.EventLogUtility.Commands;
using System;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSPM.EventLogUtility.CommandBinders
{
    public class ExtractConfigurationBinder : BinderBase<EventLogExtractConfiguration>
    {
        private readonly Option<string> _fileCommandOption;

        private readonly DateCommandOption _dateOption;

        private readonly SignalIncludeCommandOption _includeOption;

        private readonly SignalExcludeCommandOption _excludeOption;

        private readonly PathCommandOption _pathCommandOption;

        public ExtractConfigurationBinder(
            Option<string> fileCommandOption,
            DateCommandOption dateOption,
            SignalIncludeCommandOption includeOption,
            SignalExcludeCommandOption excludeOption,
            PathCommandOption pathCommandOption)
        {
            _fileCommandOption = fileCommandOption;
            _dateOption = dateOption;
            _includeOption = includeOption;
            _excludeOption = excludeOption;
            _pathCommandOption = pathCommandOption;
        }

        protected override EventLogExtractConfiguration GetBoundValue(BindingContext bindingContext)
        {
            return new EventLogExtractConfiguration()
            {
                FileFormat = bindingContext.ParseResult.GetValueForOption(_fileCommandOption),
                Dates = bindingContext.ParseResult.GetValueForOption(_dateOption),
                Included = bindingContext.ParseResult.GetValueForOption(_includeOption),
                Excluded = bindingContext.ParseResult.GetValueForOption(_excludeOption),
                Path = bindingContext.ParseResult.GetValueForOption(_pathCommandOption)
            };
        }
    }
}
