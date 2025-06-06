#region license
// Copyright 2025 Utah Departement of Transportation
// for DeviceEmulator - DeviceEmulator.Services/SftpDeviceRunner.cs
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

using DeviceEmulator.Models;
using System.Text;

namespace DeviceEmulator.Services
{
    public class SftpDeviceRunner : IDeviceProtocolRunner
    {
        private readonly DeviceDefinition _device;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly string _deviceDirectory;

        public SftpDeviceRunner(DeviceDefinition device, ILogger logger, IConfiguration config)
        {
            _device = device;
            _logger = logger;
            _config = config;
            _deviceDirectory = Path.Combine("data", _device.DeviceIdentifier);

            Directory.CreateDirectory(_deviceDirectory);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("SFTP device emulator started for {DeviceId} on port {Port}", _device.DeviceIdentifier, _device.Port);
            return Task.CompletedTask;
        }

        public async Task GenerateLogAsync()
        {
            var sb = new StringBuilder();

            var startTime = DateTime.Now;
            sb.Append(startTime.ToString("G").PadRight(20)); // 20-character timestamp header

            sb.AppendLine("Version: 5.0");
            sb.AppendLine("Resource: TEST");
            sb.AppendLine("Intersection: 9999");
            sb.AppendLine("IP: 127.0.0.1");
            sb.AppendLine("MAC: 00:11:22:33:44:55");
            sb.AppendLine("Controller data log beginning: " + startTime.ToString("G"));
            sb.AppendLine("Phases in use: 2,4,6");

            var headerBytes = Encoding.ASCII.GetBytes(sb.ToString());

            var random = new Random();
            var logBytes = new List<byte>();
            int rowsToCreatePerFile = _config.GetValue<int>("DeviceEmulator:RowsToCreatePerFile", 1000);

            for (int i = 0; i < rowsToCreatePerFile; i++)
            {
                byte eventCode = (byte)random.Next(0, 201);
                byte eventParam = (byte)random.Next(0, 201);
                ushort offset = (ushort)(i * 10);

                logBytes.Add(eventCode);
                logBytes.Add(eventParam);
                logBytes.AddRange(BitConverter.GetBytes(offset)); // little endian
            }

            var fullLog = headerBytes.Concat(logBytes).ToArray();

            var timestamp = DateTime.Now.ToString("yyyy_MM_dd_HHmm");
            var fileName = $"{_device.DeviceIdentifier}_127_0_0_1_{timestamp}.dat";
            var fullPath = Path.Combine(_deviceDirectory, fileName);

            Directory.CreateDirectory(_deviceDirectory);
            await File.WriteAllBytesAsync(fullPath, fullLog);

            _logger.LogInformation($"[SFTP Emulator] Wrote {rowsToCreatePerFile} events to: {fileName}");
        }
    }
}
