using Meadow.Modbus;

namespace Meadow.Foundation.Sensors.Environmental.GroPoint;

/// <summary>
/// Represents a GroPoint GPLP2625-S-T-3 soil moisture and temperature sensor.
/// </summary>
public class Gplp2625_S_T_3 : Gplp2625
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Gplp2625_S_T_3"/> class.
    /// </summary>
    /// <param name="modbusClient">The Modbus RTU client used for communication.</param>
    /// <param name="modbusAddress">The Modbus address of the device. Defaults to <see cref="Gplp2625.DefaultModbusAddress"/>.</param>
    public Gplp2625_S_T_3(
        ModbusRtuClient modbusClient,
        byte modbusAddress = DefaultModbusAddress)
        : base(modbusClient, 3, 6, modbusAddress)
    {
    }
}
