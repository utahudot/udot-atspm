//using ATSPM.Data;
//using ATSPM.Data.Models;
//using ATSPM.Data.Models.AggregationModels;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using System.CommandLine;
//using System.CommandLine.NamingConventionBinder;

//public class CreateSpeedManagementAggregations : Command
//{
//    private readonly ILogger<CreateSpeedManagementAggregations> logger;
//    private readonly IServiceProvider serviceProvider;

//    public CreateSpeedManagementAggregations(ILogger<CreateSpeedManagementAggregations> logger, IServiceProvider serviceProvider) : base("aggregation-test", "Create Aggregations command")
//    {
//        var startDateOption = new Option<DateTime?>("--start", "Start");
//        startDateOption.IsRequired = false; // Make the option not required
//        AddOption(startDateOption);

//        var endDateOption = new Option<DateTime?>("--end", "End");
//        endDateOption.IsRequired = false; // Make the option not required
//        AddOption(endDateOption);

//        Handler = CommandHandler.Create<DateTime?, DateTime?>(ExecuteAsync);
//        this.logger = logger;
//        this.serviceProvider = serviceProvider;
//    }

//    private async Task ExecuteAsync(DateTime? start, DateTime? end)
//    {
//        IConfiguration config = new ConfigurationBuilder()
//            .SetBasePath(Directory.GetCurrentDirectory())
//            .AddJsonFile("appsettings.json")
//            .Build();

//        var random = new Random();
//        try
//        {
//            var locations = Enumerable.Range(1, 6000).Select(x => x.ToString()).ToList();

//            var localAggregations = new List<CompressedAggregations<ClearGuideAggregation>>();
//            foreach (var locationIdentifier in locations)
//            {
//                for (var monthStart = start.Value.Date; monthStart < end.Value.Date; monthStart = monthStart.AddMonths(1))
//                {
//                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);
//                    if (monthEnd > end.Value.Date)
//                        monthEnd = end.Value.Date;

//                    var speeds = new List<ClearGuideAggregation>();
//                    for (var date = monthStart; date <= monthEnd; date = date.AddHours(1))
//                    {
//                        speeds.Add(new ClearGuideAggregation()
//                        {
//                            LocationIdentifier = locationIdentifier,
//                            Start = date,
//                            End = date.AddHours(1),
//                            ConfidenceId = random.Next(1, 10),
//                            Average = random.Next(50, 100),
//                            FifteenthSpeed = random.Next(50, 100),
//                            EightyFifthSpeed = random.Next(50, 100),
//                            NinetyFifthSpeed = random.Next(50, 100),
//                            NinetyNinthSpeed = random.Next(50, 100),
//                            Violation = random.Next(0, 20),
//                            Flow = random.Next(100, 500)
//                        });
//                    }
//                    localAggregations.Add(new CompressedAggregations<ClearGuideAggregation>
//                    {
//                        ArchiveDate = DateOnly.FromDateTime(monthStart),
//                        LocationIdentifier = locationIdentifier,
//                        Data = speeds
//                    });
//                }
//                await SaveAggregationsToDatabase(localAggregations);
//            }

//        }
//        catch (Exception ex)
//        {
//            logger.LogError($"Error inserting data: {ex.Message}");
//        }

//        Console.WriteLine("Execution completed.");
//    }



//    private async Task SaveAggregationsToDatabase(
//        List<CompressedAggregations<ClearGuideAggregation>> speedAggregations
//        )
//    {
//        using (var scope = serviceProvider.CreateScope())
//        {
//            var context = scope.ServiceProvider.GetService<AggregationContext>();

//            if (context != null)
//            {
//                foreach (var aggregation in speedAggregations)
//                {
//                    context.CompressedAggregations.Add(aggregation);
//                }
//                context.SaveChanges();
//            }
//        }
//        speedAggregations.Clear();
//    }
//}
