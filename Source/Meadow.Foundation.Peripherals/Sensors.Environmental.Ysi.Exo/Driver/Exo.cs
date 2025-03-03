using Meadow.Modbus;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental.Ysi;

/// <summary>
/// Represents a YSI EXO multiparameter water quality sonde with Modbus RTU communication.
/// </summary>
public partial class Exo
{
    /// <summary>
    /// The default Modbus address for EXO devices.
    /// </summary>
    public const byte DefaultModbusAddress = 1;

    /// <summary>
    /// The default baud rate for communication with EXO devices.
    /// </summary>
    public const int DefaultBaudRate = 9600;

    private readonly ModbusRtuClient modbusClient;

    /// <summary>
    /// Gets the Modbus address of the EXO device.
    /// </summary>
    public byte Address { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Exo"/> class.
    /// </summary>
    /// <param name="modbusClient">The Modbus RTU client used for communication.</param>
    /// <param name="modbusAddress">The Modbus address of the EXO device. Defaults to <see cref="DefaultModbusAddress"/>.</param>
    public Exo(
        ModbusRtuClient modbusClient,
        byte modbusAddress = DefaultModbusAddress)
    {
        this.modbusClient = modbusClient;
        Address = modbusAddress;
    }

    /// <summary>
    /// Sets the sample period for the EXO device.
    /// </summary>
    /// <param name="period">The time period between automatic samples.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SetSamplePeriod(TimeSpan period)
    {
        await modbusClient.WriteHoldingRegister(
            Address,
            (ushort)HoldingRegisters.SamplePeriod,
            (ushort)period.TotalSeconds);
    }

    /// <summary>
    /// Forces the EXO device to immediately take a sample.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ForceSample()
    {
        await modbusClient.WriteHoldingRegister(
            Address,
            (ushort)HoldingRegisters.ForceSample,
            (ushort)1);
    }

    /// <summary>
    /// Forces the EXO device to perform a wipe operation on its sensors.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ForceWipe()
    {
        await modbusClient.WriteHoldingRegister(
            Address,
            (ushort)HoldingRegisters.ForceWipe,
            (ushort)1);
    }

    /// <summary>
    /// Gets the current sample period from the EXO device.
    /// </summary>
    /// <returns>A task representing the asynchronous operation that returns the sample period as a TimeSpan.</returns>
    public async Task<TimeSpan> GetSamplePeriod()
    {
        var data = await modbusClient.ReadHoldingRegisters(
            Address,
            (ushort)HoldingRegisters.SamplePeriod,
            1);

        return TimeSpan.FromSeconds(data[0]);
    }

    /// <summary>
    /// Gets the parameter codes that are currently configured to be read from the EXO device.
    /// </summary>
    /// <returns>A task representing the asynchronous operation that returns an array of parameter codes.</returns>
    public async Task<ParameterCode[]> GetParametersToRead()
    {
        var registers = await modbusClient.ReadHoldingRegisters(
            Address,
            (ushort)HoldingRegisters.ParameterType,
            32);

        var parms = new ParameterCode[32];

        for (var s = 0; s < registers.Length; s++)
        {
            parms[s] = (ParameterCode)registers[s];
        }

        return parms;
    }

    /// <summary>
    /// Gets the current sensor data from the EXO device for all configured parameters.
    /// </summary>
    /// <returns>A task representing the asynchronous operation that returns an array of tuples containing parameter codes and their corresponding values.</returns>
    public async Task<(ParameterCode ParameterCode, object Value)[]> GetCurrentData()
    {
        var list = new List<(ParameterCode ParameterCodes, object Value)>();

        var codes = await GetParametersToRead();

        var registers = await modbusClient.ReadHoldingRegisters(
            Address,
            (ushort)HoldingRegisters.ParameterData,
            64); // each parameter is in 2 registers

        for (var s = 0; s < 32; s++)
        {
            if (codes[s] != 0)
            {
                var merged = registers[s * 2] | registers[s * 2 + 1] << 16;
                var bytes = BitConverter.GetBytes(merged);
                list.Add(Converter.Convert(codes[s], BitConverter.ToSingle(bytes, 0)));
            }
        }

        return list.ToArray();
    }

    /// <summary>
    /// Sets the parameters to read from the EXO device.
    /// </summary>
    /// <param name="parameters">An array of parameter codes to read. Maximum of 32 parameters.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when more than 32 parameters are specified.</exception>
    public async Task SetParametersToRead(ParameterCode[] parameters)
    {
        if (parameters.Length > 32)
        {
            throw new ArgumentOutOfRangeException("A maximum of 32 parameters can be read");
        }

        // per the data sheet, we must write a zero after the last requested parameter
        var parms = new ushort[parameters.Length + 1];
        for (var p = 0; p < parameters.Length; p++)
        {
            parms[p] = (ushort)parameters[p];
        }

        await modbusClient.WriteHoldingRegisters(
            Address,
            (ushort)HoldingRegisters.ParameterType,
            parms);
    }

    /// <summary>
    /// Gets the status of all parameters from the EXO device.
    /// </summary>
    /// <returns>A task representing the asynchronous operation that returns an array of parameter status values.</returns>
    public async Task<ParameterStatus[]> GetParameterStatus()
    {
        var registers = await modbusClient.ReadHoldingRegisters(
            Address,
            (ushort)HoldingRegisters.ParameterStatus,
            32);

        var status = new ParameterStatus[32];

        for (var s = 0; s < registers.Length; s++)
        {
            status[s] = (ParameterStatus)registers[s];
        }

        return status;
    }
}