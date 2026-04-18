using Meadow.Foundation.Transceivers;
using System.Text;

var portName = args.Length > 0 ? args[0] : "/dev/ttyUSB0";
Console.WriteLine($"Connecting to Wio-E5 on {portName}...");

var serialPort = new DesktopSerialMessagePort(portName, 9600, Encoding.ASCII.GetBytes("\r\n"));
using var radio = new WioE5(serialPort);

// --- Initialize ---
var ok = await radio.Initialize();
Console.WriteLine($"Initialize: {(ok ? "OK" : "FAILED")}");
if (!ok) return;

var version = await radio.GetVersion();
Console.WriteLine($"Firmware:   {version}");

var ids = await radio.GetIds();
Console.WriteLine($"DevEui:     {ids.DevEui}");

// --- Switch to P2P test mode ---
await radio.SetTestMode();
Console.WriteLine("Mode:       TEST");

await radio.ConfigureRf(
    frequencyHz: 868_000_000,
    sf: SpreadingFactor.SF7,
    bandwidth: Bandwidth.BW125,
    powerDbm: 14);
Console.WriteLine("RF:         868MHz SF7 BW125 14dBm\n");

radio.PacketReceived += (s, pkt) =>
{
    var text = Encoding.ASCII.GetString(pkt.Payload);
    Console.WriteLine($"  RX: \"{text}\"  RSSI:{pkt.Rssi}dBm  SNR:{pkt.Snr}dB");
};

// --- Mirror loop: TX, then listen for reply, repeat ---
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); };

while (!cts.IsCancellationRequested)
{
    Console.WriteLine("Sending packet...");
    var sent = await radio.SendPacket(Encoding.ASCII.GetBytes("Hello from Desktop"));
    Console.WriteLine($"TX: {(sent ? "DONE" : "FAILED")}");

    Console.WriteLine("Listening for reply...");
    await radio.StartReceiving();
    await Task.Delay(5000, cts.Token).ContinueWith(_ => { });
    await radio.StopReceiving();
}

Console.WriteLine("Done.");
