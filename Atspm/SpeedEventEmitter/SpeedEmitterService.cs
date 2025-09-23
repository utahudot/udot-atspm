// Services/SpeedEmitterService.cs
using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Utah.Udot.Atspm.Infrastructure.Configuration;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Infrastructure.Repositories.ConfigurationRepositories;
using Utah.Udot.Atspm.Repositories.ConfigurationRepositories;

namespace SpeedEventEmitter.Services
{
    public class SpeedEmitterService : BackgroundService
    {
        private readonly ILogger<SpeedEmitterService> _log;
        private readonly IDeviceRepository _repo;
        private readonly string _host;
        private readonly int _port;
        private readonly string _protocol;
        private readonly int _intervalMs;
        private readonly Random _rand;

        public SpeedEmitterService(
            ILogger<SpeedEmitterService> log,
            IDeviceRepository repo,
            IConfiguration config)
        {
            _log = log;
            _repo = repo;
            _host = config["EVENT_LISTENER_HOST"] ?? "eventlistener";
            _port = int.Parse(config["EVENT_LISTENER_PORT"] ?? "10088");
            _protocol = config["EMITTER_PROTOCOL"] ?? "udp";
            _intervalMs = int.Parse(config["EMITTER_INTERVAL_MS"] ?? "100");
            _rand = new Random();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _log.LogInformation(
              "Emitter starting → {Proto} → {Host}:{Port} @ {Ms}ms",
              _protocol, _host, _port, _intervalMs);

            var devices = _repo.GetList()
                .Where(d => d.DeviceType == DeviceTypes.SpeedSensor)
                .ToList();

            if (!devices.Any())
            {
                _log.LogError(
                  "No devices found for DeviceTypes.SpeedSensor – aborting emitter.");
                return;
            }

            _log.LogInformation(
              "Loaded {Count} devices: {Ids}",
              devices.Count,
              string.Join(", ", devices.Select(d => d.DeviceIdentifier)));

            while (!stoppingToken.IsCancellationRequested)
            {
                var device = devices[_rand.Next(devices.Count)];
                var sensorId = device.DeviceIdentifier;
                var mph = _rand.Next(20, 80);
                var kph = (int)(mph * 1.609);
                var buffer = new byte[16];
                buffer[8] = (byte)mph;
                buffer[9] = (byte)kph;
                var idFixed = sensorId.PadRight(6).Substring(0, 6);
                var idBytes = Encoding.ASCII.GetBytes(idFixed);
                Array.Copy(idBytes, 0, buffer, 10, idBytes.Length);

                try
                {
                    if (_protocol.Equals("udp", StringComparison.OrdinalIgnoreCase))
                    {
                        using var udp = new UdpClient();
                        await udp.SendAsync(buffer, buffer.Length, _host, _port);
                    }
                    else
                    {
                        // up to 3 tries for TCP
                        var sent = false;
                        for (int attempt = 1; attempt <= 3 && !sent; attempt++)
                        {
                            try
                            {
                                using var tcp = new TcpClient();
                                await tcp.ConnectAsync(_host, _port);

                                // Shutdown send side after write
                                var stream = tcp.GetStream();
                                await stream.WriteAsync(buffer, 0, buffer.Length, stoppingToken);
                                tcp.Client.Shutdown(SocketShutdown.Send);

                                sent = true;
                            }
                            catch (SocketException ex)
                            {
                                _log.LogWarning(
                                    ex,
                                    "TCP attempt {Attempt} failed for SensorId {Sensor}; retrying in 200ms",
                                    attempt, sensorId);
                                await Task.Delay(200, stoppingToken);
                            }
                        }


                        if (!sent)
                            throw new SocketException((int)SocketError.NotConnected);
                    }

                    _log.LogInformation(
                      "Sent {Protocol} packet [{Sensor}, {Mph}mph/{Kph}kph] at {Time}",
                      _protocol, sensorId, mph, kph, DateTime.UtcNow);
                }
                catch (OperationCanceledException)
                {
                    // graceful shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _log.LogError(
                      ex,
                      "Error sending packet for SensorId {Sensor}; will continue with next device",
                      sensorId);
                }

                try
                {
                    await Task.Delay(_intervalMs, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _log.LogInformation("Emitter stopping.");
        }

    }
}
