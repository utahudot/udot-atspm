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
                MaxDegreeOfParallelism = 10,
            };

            //Here is the list of tasks
            var expiredEvents = new TransformManyBlock<object, MonthlyAggregation>(async _ =>
            {
                var result = await monthlyAggregationService.AllAggregationsOverTimePeriodAsync();
                return result;
            }, settings);
            var deleteBlock = new ActionBlock<MonthlyAggregation>(monthlyAggregationService.DeleteMonthlyAggregation, settings);

            DataflowLinkOptions linkOptions = new DataflowLinkOptions() { PropagateCompletion = true };

            //Link the workflow
            expiredEvents.LinkTo(deleteBlock, linkOptions);

            //Start the workflow
            await expiredEvents.SendAsync(expiredEvents);
            expiredEvents.Complete();

            await deleteBlock.Completion;
        }


    }
}
