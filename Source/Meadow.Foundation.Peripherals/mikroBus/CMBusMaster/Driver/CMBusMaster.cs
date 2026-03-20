using Meadow.Foundation.MBus;
using Meadow.Hardware;

namespace Meadow.Foundation.mikroBUS.Sensors.MBus;

/// <summary>
/// Represents a mikroBUS M-Bus Master Click board
/// </summary>
public class CMBusMaster : MBusSerialServer
{
    /// <summary>
    /// Creates a new CMBusMaster object using a serial port
    /// </summary>
    public CMBusMaster(ISerialPort port)
        : base(port)
    { }
}