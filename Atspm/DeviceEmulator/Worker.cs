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

            var devices = new List<DeviceDefinition>();

            // FTP devices
            var ftpCount = int.TryParse(_config["FTP_Count"], out var fc) ? fc : 0;
            //var ftpCount = int.TryParse(Environment.GetEnvironmentVariable("FTP_COUNT"), out var fc) ? fc : 0;
            for (int i = 1; i <= ftpCount; i++)
            {
                devices.Add(new DeviceDefinition
                {
                    DeviceIdentifier = $"FTP-{i:D3}",
                    Protocol = "ftp",
                    IpAddress = _config["FTP_IP"] ?? "127.0.0.1",
                    Port = 21,
                    LogDirectory = Path.Combine(_config["FTP_LOG_BASE"] ?? "/files", $"FTP-{i:D3}"),
                    UseCompression = false
                });
            }

            // SFTP devices
            var sftpCount = int.TryParse(_config["SFTP_COUNT"], out var sc) ? sc : 0;
            for (int i = 1; i <= sftpCount; i++)
            {
                devices.Add(new DeviceDefinition
                {
                    DeviceIdentifier = $"SFTP-{i:D3}",
                    Protocol = "sftp",
                    IpAddress = _config["SFTP_IP"] ?? "127.0.0.1",
                    Port = 22,
                    LogDirectory = Path.Combine(_config["SFTP_LOG_BASE"] ?? "/data", $"SFTP-{i:D3}"),
                    UseCompression = false
                });
            }

            // HTTP devices
            var httpCount = int.TryParse(_config["HTTP_COUNT"], out var hc) ? hc : 0;
            for (int i = 1; i <= httpCount; i++)
            {
                devices.Add(new DeviceDefinition
                {
                    DeviceIdentifier = $"HTTP-{i:D3}",
                    Protocol = "http",
                    IpAddress = "127.0.0.1",
                    Port = 80,
                    LogDirectory = Path.Combine(_config["HTTP_LOG_BASE"] ?? "/http", $"HTTP-{i:D3}"),
                    UseCompression = false
                });
            }

            if (!devices.Any())
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