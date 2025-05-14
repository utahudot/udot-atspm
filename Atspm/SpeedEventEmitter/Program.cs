using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

var host = Environment.GetEnvironmentVariable("EVENT_LISTENER_HOST") ?? "eventlistener";
var port = int.Parse(Environment.GetEnvironmentVariable("EVENT_LISTENER_PORT") ?? "10088");
var protocol = Environment.GetEnvironmentVariable("EMITTER_PROTOCOL") ?? "udp";
var interval = int.Parse(Environment.GetEnvironmentVariable("EMITTER_INTERVAL_MS") ?? "100");
var rand = new Random();

Console.WriteLine($"Emitter starting → {protocol.ToUpper()} → {host}:{port} @ {interval}ms");

while (true)
{
    // build a CSV: LocationIdentifier,Ticks,DetectorId,Mph,Kph
    var tick = DateTime.UtcNow.Ticks;
    var detector = $"D{rand.Next(1, 100):D2}";
    var mph = rand.Next(20, 80);
    var kph = (int)(mph * 1.609);
    var msg = $"Loc1,{tick},{detector},{mph},{kph}";
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
