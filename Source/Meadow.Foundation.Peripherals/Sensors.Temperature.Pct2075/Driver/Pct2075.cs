using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature;

/// <summary>
/// Represents a PCT2075 temperature sensor
/// </summary>
public partial class Pct2075 : PollingSensorBase<Units.Temperature>, ITemperatureSensor, II2cPeripheral
{
    private II2cCommunications _comms;

    /// <inheritdoc/>
    public Units.Temperature? Temperature { get; private set; }

    /// <inheritdoc/>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    /// <summary>
    /// Initializes a new instance of the Pct2075 class.
    /// </summary>
    /// <param name="bus">The I2C bus to which the sensor is connected.</param>
    /// <param name="address">The I2C address of the sensor (optional, default is Addresses.Default).</param>
    public Pct2075(II2cBus bus, Addresses address = Addresses.Default)
    {
        _comms = new I2cCommunications(bus, (byte)address);
    }

    /// <inheritdoc/>
    protected override Task<Units.Temperature> ReadSensor()
    {
        var tempRegister = _comms.ReadRegisterAsUShort((byte)Registers.Temp, ByteOrder.BigEndian);
        Units.Temperature result;
        if ((tempRegister & (1 << 15)) != 0)
        {
            // negative temp
            var t = tempRegister >> 5; // shift
            t = (~t & 0b111111111) + 1; // invert, mask to 9-bits, add 1 (9-bit twos complement)
            result = (-1 * t * 0.125).Celsius();
        }
        else
        {
            result = ((tempRegister >> 5) * 0.125).Celsius();
        }
        Temperature = result;
        return Task.FromResult(result);
    }

    /// <summary>
    /// Sets the interrupt temperatures for the PCT2075 sensor.
    /// </summary>
    /// <param name="interruptOnTemperature">The temperature threshold that triggers the interrupt.</param>
    /// <param name="interruptOffTemperature">The temperature threshold that deactivates the interrupt.</param>
    /// <param name="interruptActiveHigh">
    ///     Indicates whether the interrupt is active-high (true) or active-low (false).
    ///     Default is active-high.
    /// </param>
    public void SetInterruptTemperatures(Units.Temperature interruptOnTemperature, Units.Temperature interruptOffTemperature, bool interruptActiveHigh = true)
    {
        if (interruptOffTemperature > interruptOnTemperature) throw new ArgumentException("Off temperature must be below On temeprature");

        // require 6 readings to exceed the interrupt temp to trigger
        byte cfg = 0b00011000;
        if (interruptActiveHigh) cfg |= 1 << 2; // interrupt polarity bit
        _comms.WriteRegister((byte)Registers.Conf, cfg);

        // on temp
        var tos = (ushort)(((short)interruptOnTemperature.Celsius / 2) << 7);
        _comms.WriteRegister((byte)Registers.Tos, tos);

        // off temp
        var thyst = (ushort)(((short)interruptOnTemperature.Celsius / 2) << 7);
        _comms.WriteRegister((byte)Registers.Thyst, thyst);
    }
}