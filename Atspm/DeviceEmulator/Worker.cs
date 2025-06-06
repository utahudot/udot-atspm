#region license
// Copyright 2025 Utah Departement of Transportation
// for DeviceEmulator - DeviceEmulator/Worker.cs
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
using DeviceEmulator.Services;
using System.Text.Json;

namespace DeviceEmulator
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _config;

        public Worker(ILogger<Worker> logger, IServiceProvider services, IConfiguration config)
        {
            _logger = logger;
            _services = services;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Device Emulator starting...");

            var deviceConfigPath = Environment.GetEnvironmentVariable("DEVICE_CONFIG_PATH") ?? "devices.json";
            if (!File.Exists(deviceConfigPath))
            {
                _logger.LogError("Device config file not found at {Path}", deviceConfigPath);
                return;
            }

            var devices = JsonSerializer.Deserialize<List<DeviceDefinition>>(await File.ReadAllTextAsync(deviceConfigPath, stoppingToken));
            if (devices == null || devices.Count == 0)
            {
                _logger.LogWarning("No devices configured for emulation.");
                return;
            }

            var runners = new List<IDeviceProtocolRunner>();

            foreach (var device in devices)
            {
                IDeviceProtocolRunner? runner = device.Protocol.ToLower() switch
                {
                    "http" => new HttpXmlDeviceRunner(device, _logger),
                    "ftp" => new FtpDeviceRunner(device, _logger, _config),
                    "sftp" => new SftpDeviceRunner(device, _logger, _config),

                    // Add other protocol cases here later
                    _ => null
                };

                if (runner != null)
                {
                    runners.Add(runner);
                    await runner.StartAsync(stoppingToken);
                }
                else
                {
                    _logger.LogWarning("Unsupported or unhandled device: {DeviceId}", device.DeviceIdentifier);
                }
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var runner in runners)
                {
                    await runner.GenerateLogAsync();
                }

                var intervalMinutes = int.TryParse(Environment.GetEnvironmentVariable("LOG_INTERVAL_MINUTES"), out var mins)
                    ? mins : 15;

                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);

            }

            _logger.LogInformation("Device Emulator shutting down...");
        }
    }
}
