using Meadow.Modbus;

namespace Meadow.Foundation.Sensors.Environmental.GroPoint;

/// <summary>
/// Represents a GroPoint GPLP2625-S-3 soil moisture sensor without temperature sensors.
/// </summary>
public class Gplp2625_S_3 : Gplp2625
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Gplp2625_S_3"/> class.
    /// </summary>
    /// <param name="modbusClient">The Modbus RTU client used for communication.</param>
    /// <param name="modbusAddress">The Modbus address of the device. Defaults to <see cref="Gplp2625.DefaultModbusAddress"/>.</param>
    public Gplp2625_S_3(
        ModbusRtuClient modbusClient,
        byte modbusAddress = DefaultModbusAddress)
        : base(modbusClient, 3, 0, modbusAddress)
    {
    }
}
