using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

var host = Environment.GetEnvironmentVariable("EVENT_LISTENER_HOST") ?? "eventlistener";
var port = int.Parse(Environment.GetEnvironmentVariable("EVENT_LISTENER_PORT") ?? "10088");
var protocol = Environment.GetEnvironmentVariable("EMITTER_PROTOCOL") ?? "udp";
var interval = int.Parse(Environment.GetEnvironmentVariable("EMITTER_INTERVAL_MS") ?? "100");

// allow either a single sensor or a comma-separated list
var rawSensors = (Environment.GetEnvironmentVariable("EMITTER_SENSORS") ?? "D01")
                   .Split(',', StringSplitOptions.RemoveEmptyEntries);
var rand = new Random();

Console.WriteLine($"Emitter starting → {protocol.ToUpper()} → {host}:{port} @ {interval}ms");
Console.WriteLine($"Using sensors: {string.Join(", ", rawSensors)}");

while (true)
{
    // pick a random sensor from the list
    var sensorId = rawSensors[rand.Next(rawSensors.Length)];
    var tick = DateTime.UtcNow.Ticks;

    // raw packet only needs SensorId,Ticks,Mph,Kph
    var mph = rand.Next(20, 80);
    var kph = (int)(mph * 1.609);
    var msg = $"{sensorId},{tick},{mph},{kph}";
    var data = Encoding.UTF8.GetBytes(msg);

    if (protocol.Equals("udp", StringComparison.OrdinalIgnoreCase))
    {
        using var udp = new UdpClient();
        await udp.SendAsync(data, data.Length, host, port);
    }
    else
    {
        using var tcp = new TcpClient();
        await tcp.ConnectAsync(host, port);
        await tcp.GetStream().WriteAsync(data, 0, data.Length);
    }

    await Task.Delay(interval);
}
