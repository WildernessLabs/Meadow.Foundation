using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.VFDs;

/// <summary>
/// Represents a simulated Franklin Electric CerusXDrive Variable Frequency Drive controller.
/// Provides methods to simulate the behavior of an CerusXDrive device without requiring actual hardware.
/// </summary>
public class SimulatedXDrive : IXDrive
{
    private bool _isConnected = false;
    private ushort _errorCode = 0;
    private ushort _operationalStatus = (ushort)OperationalStatus.RunOff_StopOn;
    private double _outputFrequency = 0.0;
    private double _outputCurrent = 0.0;
    private double _dcBusVoltage = 480.0;
    private double _outputVoltage = 0.0;
    private double _igbtTemperature = 25.0;
    private double _ambientTemperature = 22.0;
    private ushort _driveStatus = 0;
    private ushort _controlMode = 0;
    private ushort _digitalOutputStatus = 0;
    private ushort _digitalInputStatus = 0;

    private readonly Random _random = new Random();

    /// <summary>
    /// Initializes a new instance of the SimulatedXDrive class.
    /// </summary>
    public SimulatedXDrive()
    {
    }

    /// <summary>
    /// Simulates establishing a connection to the CerusXDrive device.
    /// </summary>
    /// <returns>A task representing the asynchronous connection operation.</returns>
    public Task Connect()
    {
        _isConnected = true;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Simulates disconnecting from the CerusXDrive device.
    /// </summary>
    public void Disconnect()
    {
        _isConnected = false;
    }

    /// <summary>
    /// Validates that the connection is established before proceeding with operations.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the device is not connected.</exception>
    private void ValidateConnection()
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("Device is not connected. Call Connect() first.");
        }
    }

    /// <summary>
    /// Simulates reading the current error codes from the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the error code.</returns>
    public Task<ushort> ReadErrorCodes()
    {
        ValidateConnection();
        return Task.FromResult(_errorCode);
    }

    /// <summary>
    /// Simulates reading the current operational status of the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the operational status code.</returns>
    public Task<ushort> ReadOperationalStatus()
    {
        ValidateConnection();
        return Task.FromResult(_operationalStatus);
    }

    /// <summary>
    /// Simulates reading the current output frequency of the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the output frequency in Hertz.</returns>
    public Task<Frequency> ReadOutputFrequency()
    {
        ValidateConnection();
        // Add small random variation to simulate real-world fluctuations
        _outputFrequency += (_random.NextDouble() - 0.5) * 0.1;
        return Task.FromResult(new Frequency(_outputFrequency, Frequency.UnitType.Hertz));
    }

    /// <summary>
    /// Simulates reading the current output current of the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the output current in Amps.</returns>
    public Task<Current> ReadOutputCurrent()
    {
        ValidateConnection();
        // Current is proportional to frequency in this simple simulation
        _outputCurrent = _outputFrequency * 0.1 + (_random.NextDouble() - 0.5) * 0.2;
        return Task.FromResult(new Current(_outputCurrent, Current.UnitType.Amps));
    }

    /// <summary>
    /// Simulates reading the current DC bus voltage of the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the DC bus voltage in Volts.</returns>
    public Task<Voltage> ReadDCBusVoltage()
    {
        ValidateConnection();
        // Add small random variation to simulate real-world fluctuations
        _dcBusVoltage += (_random.NextDouble() - 0.5) * 0.5;
        return Task.FromResult(new Voltage(_dcBusVoltage, Voltage.UnitType.Volts));
    }

    /// <summary>
    /// Simulates reading the current output voltage of the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the output voltage in Volts.</returns>
    public Task<Voltage> ReadOutputVoltage()
    {
        ValidateConnection();
        // Output voltage is proportional to frequency in this simple simulation
        _outputVoltage = _outputFrequency * 8.0 + (_random.NextDouble() - 0.5) * 1.0;
        return Task.FromResult(new Voltage(_outputVoltage, Voltage.UnitType.Volts));
    }

    /// <summary>
    /// Simulates reading the current IGBT (Insulated Gate Bipolar Transistor) temperature of the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the IGBT temperature in Celsius.</returns>
    public Task<Temperature> ReadIGBTTemperature()
    {
        ValidateConnection();
        // Temperature rises with current in this simple simulation
        _igbtTemperature = 25.0 + (_outputCurrent * 2.0) + (_random.NextDouble() - 0.5) * 0.2;
        return Task.FromResult(new Temperature(_igbtTemperature, Temperature.UnitType.Celsius));
    }

    /// <summary>
    /// Simulates reading the current ambient temperature around the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the ambient temperature in Celsius.</returns>
    public Task<Temperature> ReadAmbientTemperature()
    {
        ValidateConnection();
        // Add small random variation to simulate real-world fluctuations
        _ambientTemperature += (_random.NextDouble() - 0.5) * 0.1;
        return Task.FromResult(new Temperature(_ambientTemperature, Temperature.UnitType.Celsius));
    }

    /// <summary>
    /// Simulates reading the current drive status of the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the drive status code.</returns>
    public Task<ushort> ReadDriveStatus()
    {
        ValidateConnection();
        return Task.FromResult(_driveStatus);
    }

    /// <summary>
    /// Simulates reading the current control mode of the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the control mode code.</returns>
    public Task<ushort> ReadControlMode()
    {
        ValidateConnection();
        return Task.FromResult(_controlMode);
    }

    /// <summary>
    /// Simulates reading the current digital output status of the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the digital output status code.</returns>
    public Task<ushort> ReadDigitalOutputStatus()
    {
        ValidateConnection();
        return Task.FromResult(_digitalOutputStatus);
    }

    /// <summary>
    /// Simulates reading the current digital input status of the CerusXDrive.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The value is the digital input status code.</returns>
    public Task<ushort> ReadDigitalInputStatus()
    {
        ValidateConnection();
        return Task.FromResult(_digitalInputStatus);
    }

    #region Simulation Control Methods

    /// <summary>
    /// Sets the simulated output frequency value.
    /// </summary>
    /// <param name="frequency">The frequency value in Hertz.</param>
    public void SetOutputFrequency(double frequency)
    {
        _outputFrequency = frequency;

        // Update operational status based on frequency
        if (frequency > 0)
        {
            _operationalStatus = (ushort)OperationalStatus.RunOn_StopOff;
        }
        else
        {
            _operationalStatus = (ushort)OperationalStatus.RunOff_StopOn;
        }
    }

    /// <summary>
    /// Sets the simulated error code.
    /// </summary>
    /// <param name="errorCode">The error code to set.</param>
    public void SetErrorCode(ushort errorCode)
    {
        _errorCode = errorCode;
    }

    /// <summary>
    /// Sets the simulated operational status.
    /// </summary>
    /// <param name="status">The operational status to set.</param>
    public void SetOperationalStatus(OperationalStatus status)
    {
        _operationalStatus = (ushort)status;
    }

    /// <summary>
    /// Sets the simulated DC bus voltage.
    /// </summary>
    /// <param name="voltage">The voltage value in Volts.</param>
    public void SetDCBusVoltage(double voltage)
    {
        _dcBusVoltage = voltage;
    }

    /// <summary>
    /// Sets the simulated ambient temperature.
    /// </summary>
    /// <param name="temperature">The temperature value in Celsius.</param>
    public void SetAmbientTemperature(double temperature)
    {
        _ambientTemperature = temperature;
    }

    /// <summary>
    /// Sets the simulated drive status.
    /// </summary>
    /// <param name="status">The drive status code to set.</param>
    public void SetDriveStatus(ushort status)
    {
        _driveStatus = status;
    }

    /// <summary>
    /// Sets the simulated control mode.
    /// </summary>
    /// <param name="mode">The control mode code to set.</param>
    public void SetControlMode(ushort mode)
    {
        _controlMode = mode;
    }

    /// <summary>
    /// Sets the simulated digital output status.
    /// </summary>
    /// <param name="status">The digital output status code to set.</param>
    public void SetDigitalOutputStatus(ushort status)
    {
        _digitalOutputStatus = status;
    }

    /// <summary>
    /// Sets the simulated digital input status.
    /// </summary>
    /// <param name="status">The digital input status code to set.</param>
    public void SetDigitalInputStatus(ushort status)
    {
        _digitalInputStatus = status;
    }

    /// <summary>
    /// Simulates a phase U fault in the drive.
    /// </summary>
    public void SimulatePhaseUFault()
    {
        _errorCode = (ushort)ErrorCodes.OutputPhaseUMissing;
    }

    /// <summary>
    /// Simulates a phase V fault in the drive.
    /// </summary>
    public void SimulatePhaseVFault()
    {
        _errorCode = (ushort)ErrorCodes.OutputPhaseVMissing;
    }

    /// <summary>
    /// Simulates a phase W fault in the drive.
    /// </summary>
    public void SimulatePhaseWFault()
    {
        _errorCode = (ushort)ErrorCodes.OutputPhaseWMissing;
    }

    /// <summary>
    /// Clears all simulated faults.
    /// </summary>
    public void ClearFaults()
    {
        _errorCode = 0;
    }

    /// <summary>
    /// Simulates starting the drive with a ramp up to the specified frequency.
    /// </summary>
    /// <param name="targetFrequency">The target frequency in Hertz.</param>
    /// <param name="rampTimeSeconds">The time to ramp up in seconds.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SimulateStartWithRamp(double targetFrequency, double rampTimeSeconds)
    {
        ValidateConnection();

        // Set to transitional state
        _operationalStatus = (ushort)OperationalStatus.RunBlink_StopOn;

        double startFreq = _outputFrequency;
        double freqDiff = targetFrequency - startFreq;
        int steps = (int)(rampTimeSeconds * 10); // 10 steps per second

        for (int i = 1; i <= steps; i++)
        {
            _outputFrequency = startFreq + (freqDiff * i / steps);
            await Task.Delay(100); // 100ms per step
        }

        _outputFrequency = targetFrequency;
        _operationalStatus = (ushort)OperationalStatus.RunOn_StopOff;
    }

    /// <summary>
    /// Simulates stopping the drive with a ramp down to zero frequency.
    /// </summary>
    /// <param name="rampTimeSeconds">The time to ramp down in seconds.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SimulateStopWithRamp(double rampTimeSeconds)
    {
        ValidateConnection();

        // Set to transitional state
        _operationalStatus = (ushort)OperationalStatus.RunOn_StopBlink;

        double startFreq = _outputFrequency;
        int steps = (int)(rampTimeSeconds * 10); // 10 steps per second

        for (int i = 1; i <= steps; i++)
        {
            _outputFrequency = startFreq * (steps - i) / steps;
            await Task.Delay(100); // 100ms per step
        }

        _outputFrequency = 0;
        _operationalStatus = (ushort)OperationalStatus.RunOff_StopOn;
    }

    #endregion
}