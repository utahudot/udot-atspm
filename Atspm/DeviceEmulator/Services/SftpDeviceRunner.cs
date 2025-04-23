using DeviceEmulator.Models;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace DeviceEmulator.Services
{
    public class SftpDeviceRunner : IDeviceProtocolRunner
    {
        private readonly DeviceDefinition _device;
        private readonly ILogger _logger;
        private readonly string _deviceDirectory;

        public SftpDeviceRunner(DeviceDefinition device, ILogger logger)
        {
            _device = device;
            _logger = logger;
            _deviceDirectory = Path.Combine("data", "sftp", _device.DeviceIdentifier);

            Directory.CreateDirectory(_deviceDirectory);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("SFTP device emulator started for {DeviceId} on port {Port}", _device.DeviceIdentifier, _device.Port);
            return Task.CompletedTask;
        }

        public async Task GenerateLogAsync()
        {
            var timestamp = DateTime.UtcNow;
            var extension = _device.UseCompression ? ".datZ" : ".dat";
            var fileName = $"{_device.DeviceIdentifier}_{_device.IpAddress.Replace('.', '_')}_{timestamp:yyyy_MM_dd_HHmm}{extension}";
            var filePath = Path.Combine(_deviceDirectory, fileName);

            byte[] mockData = GenerateBinaryLog();

            if (_device.UseCompression)
            {
                using var fs = new FileStream(filePath, FileMode.Create);
                using var gzip = new GZipStream(fs, CompressionLevel.Optimal);
                await gzip.WriteAsync(mockData, 0, mockData.Length);
            }
            else
            {
                await File.WriteAllBytesAsync(filePath, mockData);
            }

            _logger.LogInformation("SFTP device {DeviceId} wrote binary log file: {FileName}", _device.DeviceIdentifier, fileName);
        }

        private byte[] GenerateBinaryLog()
        {
            var buffer = new byte[1024];
            new Random().NextBytes(buffer);
            return buffer;
        }
    }
}
