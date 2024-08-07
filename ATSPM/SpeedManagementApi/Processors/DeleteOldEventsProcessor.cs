using ATSPM.Data.Models.SpeedManagement.MonthlyAggregation;
using ATSPM.Infrastructure.Services.SpeedManagementServices;
using System.Threading.Tasks.Dataflow;

namespace SpeedManagementApi.Processors
{
    public class DeleteOldEventsProcessor
    {
        private readonly MonthlyAggregationService monthlyAggregationService;

        public DeleteOldEventsProcessor(MonthlyAggregationService monthlyAggregationService)
        {
            this.monthlyAggregationService = monthlyAggregationService;
        }

        public async Task DeleteOldEvents()
        {
            var settings = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = 1,
            };

            //Here is the list of tasks
            var expiredEvents = new TransformManyBlock<object, MonthlyAggregation>(input => AllAggregationsOverTimePeriodAsync(), settings);
            var deleteBlock = new ActionBlock<MonthlyAggregation>(monthlyAggregationService.DeleteMonthlyAggregation, settings);

            DataflowLinkOptions linkOptions = new DataflowLinkOptions() { PropagateCompletion = true };

            //Link the workflow
            expiredEvents.LinkTo(deleteBlock, linkOptions);

            //Start the workflow
            await expiredEvents.SendAsync(expiredEvents);
            expiredEvents.Complete();

            await deleteBlock.Completion;
        }

        private IEnumerable<MonthlyAggregation> AllAggregationsOverTimePeriodAsync()
        {
            var list = monthlyAggregationService.AllAggregationsOverTimePeriodAsync().Result;
            foreach (var item in list)
            {
                yield return item;
            }
        }


    }
}
