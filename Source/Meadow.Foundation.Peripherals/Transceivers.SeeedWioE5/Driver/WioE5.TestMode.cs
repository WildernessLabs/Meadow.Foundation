using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Transceivers;

public partial class WioE5
{
    /// <summary>
    /// Switch to P2P TEST mode
    /// </summary>
    public async Task SetTestMode()
    {
        await SendCommand("AT+MODE=TEST");
    }

    /// <summary>
    /// Configure the RF parameters for P2P test mode
    /// </summary>
    public async Task ConfigureRf(
        int frequencyHz = 868_000_000,
        SpreadingFactor sf = SpreadingFactor.SF7,
        Bandwidth bandwidth = Bandwidth.BW125,
        int txPreamble = 8,
        int rxPreamble = 8,
        int powerDbm = 14)
    {
        var cmd = $"AT+TEST=RFCFG,{frequencyHz / 1_000_000},{(int)sf},{(int)bandwidth},{txPreamble},{rxPreamble},{powerDbm}";
        await SendCommand(cmd);
    }

    /// <summary>
    /// Transmit a packet in P2P test mode
    /// </summary>
    /// <param name="data">Payload bytes to send</param>
    public async Task<bool> SendPacket(byte[] data)
    {
        var hex = BitConverter.ToString(data).Replace("-", "");
        var response = await SendCommand($"AT+TEST=TXLRPKT,\"{hex}\"", timeoutMs: 10_000);

        foreach (var line in response)
        {
            if (line.Contains("TX DONE")) return true;
            if (line.Contains("ERROR")) return false;
        }
        return false;
    }

    /// <summary>
    /// Put the module into continuous receive mode. Raises PacketReceived when a packet arrives.
    /// </summary>
    public Task StartReceiving()
    {
        _receiving = true;
        // Write directly — bypasses SendCommand so the command lock stays free
        _port.Write(System.Text.Encoding.ASCII.GetBytes("AT+TEST=RXLRPKT\r\n"));
        return Task.CompletedTask;
    }

    /// <summary>
    /// Exit receive mode
    /// </summary>
    public async Task StopReceiving()
    {
        _receiving = false;
        await SendCommand("AT");
    }
}
