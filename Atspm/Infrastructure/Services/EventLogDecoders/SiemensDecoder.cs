#region license
// Copyright 2024 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services.EventLogDecoders/MaxtimeToIndianaDecoder.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Globalization;
using Utah.Udot.Atspm.Data.Models.EventLogModels;

namespace Utah.Udot.Atspm.Infrastructure.Services.EventLogDecoders
{
    /// <inheritdoc/>
    public class SiemensDecoder : EventLogDecoderBase<IndianaEvent>
    {
        private readonly IConfiguration configuration;

        public SiemensDecoder(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        #region Properties

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override IEnumerable<IndianaEvent> Decode(Device device, Stream stream, CancellationToken cancelToken = default)
        {
            cancelToken.ThrowIfCancellationRequested();

            if (device == null)
                throw new ArgumentNullException(nameof(device), "Device can not be null");

            if (stream?.Length == 0)
                throw new InvalidDataException("Stream is empty");

            string locationIdentifider = device.Location.LocationIdentifier;
            return ProcessDatFile(locationIdentifider, stream, cancelToken);
        }

        private List<IndianaEvent> ProcessDatFile(string locationIdentifider, Stream datStream, CancellationToken cancelToken = default)
        {
            // Step 1: Save the Stream to a Temporary .dat File
            string baseFileName = Path.GetTempFileName();
            var datFilePath = baseFileName + ".dat";
            var csvFilePath = baseFileName + ".csv";

            using (var fileStream = new FileStream(datFilePath, FileMode.Create, FileAccess.Write))
            {
                datStream.CopyTo(fileStream);
            }

            // Step 2: Execute External Process
            string csvOutput = RunPerfLogTranslate(datFilePath, csvFilePath);

            // Step 3: Convert CSV to JSON
            List<IndianaEvent> jsonOutput = ConvertCsvToJson(locationIdentifider, csvOutput, cancelToken);

            // Clean up the temporary .dat file
            File.Delete(datFilePath);
            File.Delete(csvFilePath);

            return jsonOutput;
        }

        private List<IndianaEvent> ConvertCsvToJson(string locationIdentifier, string csvOutput, CancellationToken cancelToken = default)
        {
            using (var reader = new StringReader(csvOutput))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = new List<IndianaEvent>();

                // Skip the first row (IP address)
                if (reader.Peek() != -1)
                {
                    reader.ReadLine(); // Discard the first line
                }
                csv.Context.RegisterClassMap<SiemensToIndianaEventMap>();

                foreach (var record in csv.GetRecords<IndianaEvent>())
                {
                    cancelToken.ThrowIfCancellationRequested();

                    record.LocationIdentifier = locationIdentifier;
                    records.Add(record);
                }

                return records;
            }
        }


        private class SiemensToIndianaEventMap : ClassMap<IndianaEvent>
        {
            public SiemensToIndianaEventMap()
            {
                Map(m => m.Timestamp).Index(0);
                Map(m => m.EventCode).Index(1);
                Map(m => m.EventParam).Index(2);
            }
        }


        private string RunPerfLogTranslate(string datFilePath, string csvFilePath)
        {
            string exePath = configuration["EventLogUtility:SiemensDecoder"];

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = $"PerfLogTranslate -i \"{datFilePath}\"", // Quotes to handle spaces
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!File.Exists(csvFilePath))
                {
                    throw new FileNotFoundException("CSV file was not generated.", csvFilePath);
                }

                // Read and return the CSV file content
                return File.ReadAllText(csvFilePath);
            }
        }

        #endregion
    }
}