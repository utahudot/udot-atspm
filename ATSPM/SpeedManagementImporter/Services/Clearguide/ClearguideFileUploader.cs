using ATSPM.Application.Repositories.SpeedManagementRepositories;
using ATSPM.Data.Models.SpeedManagement.Common;
using ATSPM.Data.Models.SpeedManagementConfigModels;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Threading.Tasks.Dataflow;

namespace SpeedManagementImporter.Services.Clearguide
{
    public class ClearguideFileUploader : IFileUploader
    {
        private readonly ISegmentEntityRepository segmentEntityRepository;
        private readonly ITempDataRepository tempDataRepository;
        static readonly int sourceId = 3;
        static readonly int batchSize = 10000;

        public ClearguideFileUploader(ISegmentEntityRepository segmentEntityRepository, ITempDataRepository tempDataRepository)
        {
            this.segmentEntityRepository = segmentEntityRepository;
            this.tempDataRepository = tempDataRepository;
        }

        public async Task FileUploaderAsync(string filePath)
        {
            List<SegmentEntityWithSpeed> segmentEntities = await segmentEntityRepository.GetEntitiesWithSpeedForSourceId(sourceId);

            await UploadFileAsync(segmentEntities, filePath);
            Console.WriteLine("Done adding data");
            // ADD a messaging service to let downloader service know that it is ready to download the data into hourly speeds
        }

        private async Task UploadFileAsync(List<SegmentEntityWithSpeed> segmentEntities, string filePath)
        {
            var settings = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 10,
            };

            //Transform Many block should take in filepath
            //mnany block takes in the csv reader

            //Parse through file
            var manyParseLineBlock = new TransformManyBlock<string, TempData>(filePath => ParseCsvFile(filePath, segmentEntities), settings);

            //Create batch block
            var batchBlock = new BatchBlock<TempData>(batchSize);

            // Create the ActionBlock to handle saving data in batches asynchronously
            var saveBatchBlock = new ActionBlock<IEnumerable<TempData>>(
                SaveTempDataAsync, settings);

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            manyParseLineBlock.LinkTo(batchBlock, linkOptions);
            batchBlock.LinkTo(saveBatchBlock, linkOptions);

            await manyParseLineBlock.SendAsync(filePath);

            // Await the completion of the ActionBlock
            await saveBatchBlock.Completion;
        }

        private IEnumerable<TempData> ParseCsvFile(string filePath, List<SegmentEntityWithSpeed> segmentEntities)
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
                yield return ParseCsvLine(line, segmentEntities, headerIndices);
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
        private static TempData ParseCsvLine(string line, List<SegmentEntityWithSpeed> segmentEntities, Dictionary<string, int> headerIndices)
        {
            using var reader = new StringReader(line);
            using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

            var sourceIdIndex = headerIndices["source_id"];
            var binStartTimeIndex = headerIndices["local_timestamp"];
            var avgIndex = headerIndices["avg_speed_mph"];

            csvReader.Read();
            var sourceIdString = csvReader.GetField(sourceIdIndex);

            if (long.TryParse(sourceIdString, NumberStyles.Any, CultureInfo.InvariantCulture, out long sourceId))
            {
                var segmentEntity = segmentEntities.FirstOrDefault(e => e.EntityId == sourceId);
                if (segmentEntity != null)
                {
                    // Parse binStartTime as DateTime
                    if (DateTime.TryParse(csvReader.GetField(binStartTimeIndex), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime binStartTime))
                    {
                        // Parse avg as double
                        if (double.TryParse(csvReader.GetField(avgIndex), NumberStyles.Any, CultureInfo.InvariantCulture, out double avg))
                        {
                            return new TempData
                            {
                                BinStartTime = binStartTime,
                                Average = avg,
                                EntityId = segmentEntity.EntityId
                            };
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
                Console.WriteLine($"Going to perform save for {sendData.Count()} number of lines");
                await tempDataRepository.AddRangeAsync(sendData);
                Console.WriteLine("performed save");
            }
            //if (tempData != null)
            //{
            //    data.Add(tempData);
            //    if (data.Count >= batchSize - 1)
            //    {
            //        List<TempData> batch;
            //        lock (data)
            //        {
            //            batch = data.ToList();
            //            data.Clear();

            //            GC.Collect();
            //        }

            //        await tempDataRepository.AddRangeAsync(batch);
            //        Console.WriteLine($"Added {batch.Count} rows");
            //    }
            //}
        }
    }

    public class TempDataMap : ClassMap<TempData>
    {
        public TempDataMap()
        {
            Map(m => m.EntityId).Name("source_id");
            Map(m => m.BinStartTime).Name("local_timestamp");
            Map(m => m.Average).Name("avg_speed_mph");
        }
    }
}
