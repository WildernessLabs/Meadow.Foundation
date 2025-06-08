using Meadow.Modbus;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental.GroPoint;

/// <summary>
/// Base class for GroPoint GPLP2625 series soil moisture and temperature sensors.  Use this class directly if you have a customer device with non-standard counts of temperature sensors.
/// </summary>
public class Gplp2625
{
    private const ushort MoistureSegment1DataRegister = 0;
    private const ushort TempSensor1DataRegister = 100;
    private readonly ModbusRtuClient _modbus;

    /// <summary>
    /// The default Modbus address for GroPoint sensors.
    /// </summary>
    public const byte DefaultModbusAddress = 1;

    /// <summary>
    /// The default baud rate for GroPoint sensors.
    /// </summary>
    public const int DefaultBaudRate = 19200;

    /// <summary>
    /// Gets the number of moisture measurement segments in the sensor.
    /// </summary>
    public int MoistureSegmentCount { get; }

    /// <summary>
    /// Gets the number of temperature sensors in the device.
    /// </summary>
    public int TempSensorCount { get; }

    /// <summary>
    /// Gets the Modbus address of the device.
    /// </summary>
    public byte Address { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Gplp2625"/> class.
    /// </summary>
    /// <param name="modbusClient">The Modbus RTU client used for communication.</param>
    /// <param name="moistureSegmentCount">The number of moisture measurement segments in the sensor.</param>
    /// <param name="tempSensorCount">The number of temperature sensors in the device.</param>
    /// <param name="modbusAddress">The Modbus address of the device. Defaults to <see cref="DefaultModbusAddress"/>.</param>
    public Gplp2625(
        ModbusRtuClient modbusClient,
        int moistureSegmentCount,
        int tempSensorCount,
        byte modbusAddress = DefaultModbusAddress)
    {
        _modbus = modbusClient;
        Address = modbusAddress;
        MoistureSegmentCount = moistureSegmentCount;
        TempSensorCount = tempSensorCount;
    }

    /// <summary>
    /// Reads the device identifier from the sensor.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the device identifier as a string.</returns>
    public async Task<string> ReadIdentifier()
    {
        var idBytes = await _modbus.ReadDeviceId(Address);
        return Encoding.ASCII.GetString(idBytes);
    }

    /// <summary>
    /// Reads the current baud rate setting from the sensor.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the baud rate as an integer.</returns>
    public async Task<int> ReadBaudRate()
    {
        var rate = await _modbus.ReadHoldingRegisters(
            Address,
            (byte)HoldingRegisters.BaudRate,
            1);

        return rate[0] switch
        {
            0 => 19200,
            1 => 9600,
            2 => 4800,
            3 => 2400,
            4 => 1200,
            5 => 600,
            _ => 300
        };
    }

    /// <summary>
    /// Reads the soil moisture values from all sensor segments.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains an array of moisture values in percent volumetric water content (VWC).
    /// </returns>
    /// <exception cref="Exception">Thrown when no data can be retrieved after multiple attempts.</exception>
    public async Task<float[]> ReadMoistures()
    {
        var ackCount = 0;

        while (ackCount <= 1)
        {
            try
            {
                var raw = await _modbus.ReadInputRegisters(Address, MoistureSegment1DataRegister, MoistureSegmentCount);
                // dat is scaled to 10x
                var mois = new float[raw.Length];

                for (var i = 0; i < mois.Length; i++)
                {
                    mois[i] = raw[i] / 10f;
                }

                return mois;
            }
            catch (ModbusException e)
            {
                // DEV NOTE: this device requires 1 read to start conversion, which always returns an ACK,
                //           then a second read to get the data
                if (e.ErrorCode == ModbusErrorCode.Ack)
                {
                    ackCount++;
                    // conversions take ~200ms/segment (per the data sheet)
                    await Task.Delay(200 * MoistureSegmentCount);
                }
                else
                {
                    throw;
                }
            }
        }

        throw new Exception("No data");
    }

    /// <summary>
    /// Reads the temperature values from all temperature sensors.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains an array of temperature values.
    /// </returns>
    /// <exception cref="NotSupportedException">Thrown if the device does not have temperature sensors.</exception>
    /// <exception cref="Exception">Thrown when no data can be retrieved after multiple attempts.</exception>
    public async Task<Units.Temperature[]> ReadTemperatures()
    {
        if (TempSensorCount == 0)
        {
            throw new NotSupportedException("This part does not have temperature sensors");
        }

        var ackCount = 0;

        while (ackCount <= 1)
        {
            try
            {
                var raw = await _modbus.ReadInputRegisters(Address, TempSensor1DataRegister, TempSensorCount);
                // dat is scaled to 10x
                var temps = new Units.Temperature[raw.Length];

                for (var i = 0; i < temps.Length; i++)
                {
                    temps[i] = new Units.Temperature(raw[i] / 10f, Units.Temperature.UnitType.Celsius);
                }

                return temps;
            }
            catch (ModbusException e)
            {
                // DEV NOTE: this device always responds with an ACK error and then you request and receive the data.
                // This happens even with the manufacturer's software, so is expected, even if terrible
                if (e.ErrorCode == ModbusErrorCode.Ack)
                {
                    ackCount++;
                    // conversions take ~200ms/sensor (per the data sheet)
                    await Task.Delay(200 * TempSensorCount);
                }
                else
                {
                    throw;
                }
            }
        }

        throw new Exception("No data");
    }

    /// <summary>
    /// Defines the holding register addresses for GroPoint sensors.
    /// </summary>
    internal enum HoldingRegisters
    {
        /// <summary>
        /// The register address for the baud rate setting.
        /// </summary>
        BaudRate = 202
    }
}