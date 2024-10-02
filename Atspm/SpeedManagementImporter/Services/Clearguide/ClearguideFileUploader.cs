using CsvHelper;
using CsvHelper.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks.Dataflow;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Common;
using Utah.Udot.Atspm.Data.Models.SpeedManagementModels.Config;
using Utah.Udot.Atspm.Repositories.SpeedManagementRepositories;

namespace SpeedManagementImporter.Services.Clearguide
{
    public class ClearguideFileUploader : IFileUploader
    {
        private readonly ISegmentEntityRepository segmentEntityRepository;
        private readonly ITempDataRepository tempDataRepository;
        private readonly ISegmentRepository segmentRepository;
        static readonly int sourceId = 3;
        static readonly int batchSize = 10000;

        public ClearguideFileUploader(ISegmentEntityRepository segmentEntityRepository, ITempDataRepository tempDataRepository, ISegmentRepository segmentRepository)
        {
            this.segmentEntityRepository = segmentEntityRepository;
            this.tempDataRepository = tempDataRepository;
            this.segmentRepository = segmentRepository;
        }

        public async Task FileUploaderAsync(string filePath, List<string>? providedSegments)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<Segment> segments = new List<Segment>();
            if (providedSegments != null && providedSegments.Count > 0)
            {
                var segmentIds = providedSegments.Select(s => Guid.Parse(s)).ToList();
                segments = await segmentRepository.GetSegmentsDetailsWithEntity(segmentIds);
            }
            else
            {
                segments = segmentRepository.AllSegmentsWithEntity(sourceId);
            }

            List<SegmentEntityWithSpeed> segmentEntities = segments.SelectMany(segment => segment.Entities.Select(entity => new SegmentEntityWithSpeed
            {
                SpeedLimit = segment.SpeedLimit,
                EntityId = entity.EntityId,
                SourceId = entity.SourceId,
                SegmentId = entity.SegmentId,
                EntityType = entity.EntityType,
                Length = entity.Length,
            })).ToList();

            var distinctEntityIds = segmentEntities
                                    .Select(e => e.EntityId)
                                    .Distinct()
                                    .ToList();
            var entityIdSet = new HashSet<long>(distinctEntityIds);

            await UploadFileAsync(entityIdSet, filePath);
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            Console.WriteLine($"Done adding data. It took {ts.TotalSeconds} Seconds. Here is the milliseconds {ts}");
            // ADD a messaging service to let downloader service know that it is ready to download the data into hourly speeds
        }

        //Private Methods//
        private async Task UploadFileAsync(HashSet<long> entityIdSet, string filePath)
        {
            var settings = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount // or set to a specific value
            };

            //Parse through file
            var manyParseLineBlock = new TransformManyBlock<string, TempData>(filePath => ParseCsvFile(filePath, entityIdSet), settings);

            //Create batch block
            var batchBlock = new BatchBlock<TempData>(batchSize);

            // Create the ActionBlock to handle saving data in batches asynchronously
            var saveBatchBlock = new ActionBlock<IEnumerable<TempData>>(
                SaveTempDataAsync, settings);

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            manyParseLineBlock.LinkTo(batchBlock, linkOptions);
            batchBlock.LinkTo(saveBatchBlock, linkOptions);

            await manyParseLineBlock.SendAsync(filePath);
            manyParseLineBlock.Complete();
            // Await the completion of the ActionBlock
            await saveBatchBlock.Completion;
        }

        private IEnumerable<TempData> ParseCsvFile(string filePath, HashSet<long> entityIdSet)
        {
            using var reader = new StreamReader(filePath);
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

            // Register the TempDataMap
            csvReader.Context.RegisterClassMap<TempDataMap>();

            // Read the header
            csvReader.Read();
            csvReader.ReadHeader();
            var headerIndices = GetHeaderIndices(csvReader);

            while (csvReader.Read())
            {
                // Extract the current line as a string
                var line = csvReader.Parser.RawRecord;
                yield return ParseCsvLine(line, entityIdSet, headerIndices);
            }
        }

        private Dictionary<string, int> GetHeaderIndices(CsvReader csvReader)
        {
            var headerIndices = new Dictionary<string, int>();
            for (int i = 0; i < csvReader.HeaderRecord.Length; i++)
            {
                headerIndices[csvReader.HeaderRecord[i]] = i;
            }
            return headerIndices;
        }

        // Method to parse a CSV line and map it to TempData
        private static TempData ParseCsvLine(string line, HashSet<long> entityIdSet, Dictionary<string, int> headerIndices)
        {
            using var reader = new StringReader(line);
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

            var sourceIdIndex = headerIndices["source_id"];
            var binStartTimeIndex = headerIndices["local_timestamp"];
            var avgIndex = headerIndices["avg_speed_mph"];
            var freeflowIndex = headerIndices["freeflow_mph"];

            csvReader.Read();
            var sourceIdString = csvReader.GetField(sourceIdIndex);

            if (long.TryParse(sourceIdString, NumberStyles.Any, CultureInfo.InvariantCulture, out long sourceId))
            {
                if (entityIdSet.Contains(sourceId))
                {
                    // Parse binStartTime as DateTime
                    if (DateTime.TryParse(csvReader.GetField(binStartTimeIndex), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime binStartTime))
                    {
                        // Parse avg as double
                        if (double.TryParse(csvReader.GetField(avgIndex), NumberStyles.Any, CultureInfo.InvariantCulture, out double avg))
                        {
                            if (double.TryParse(csvReader.GetField(freeflowIndex), NumberStyles.Any, CultureInfo.InvariantCulture, out double freeflow))
                            {
                                return new TempData
                                {
                                    BinStartTime = binStartTime,
                                    Average = avg,
                                    EntityId = sourceId,
                                    FilledIn = avg == freeflow
                                };
                            }
                            else
                            {
                                Console.WriteLine($"Failed to parse flow speed: {csvReader.GetField(freeflowIndex)}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Failed to parse average speed: {csvReader.GetField(avgIndex)}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to parse bin start time: {csvReader.GetField(binStartTimeIndex)}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Failed to parse source_id: {sourceIdString}");
            }

            return null; // Return null if parsing fails or no matching entity is found
        }

        // Method to save TempData in batches
        private async Task SaveTempDataAsync(IEnumerable<TempData> tempData)
        {
            if (tempData.Any())
            {
                //clean up tempData by removing any nulls
                var sendData = tempData.Where(x => x != null);
                await tempDataRepository.AddRangeAsync(sendData);
                Console.WriteLine($"Performed save for {sendData.Count()} number of lines");
            }
        }
    }

    public class TempDataMap : ClassMap<TempData>
    {
        public TempDataMap()
        {
            Map(m => m.EntityId).Name("source_id");
            Map(m => m.BinStartTime).Name("local_timestamp");
            Map(m => m.Average).Name("avg_speed_mph");
            Map(m => m.FilledIn).Name("freeflow_mph");
        }
    }
}
