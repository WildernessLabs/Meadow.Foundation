using Meadow.Hardware;
using Meadow.Modbus;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Environmental;
using Meadow.Units;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental;

/// <summary>
/// Y4000 hardware interface
/// </summary>
public interface IY4000 :
    IWaterQualityConcentrationsSensor,
    IElectricalConductivitySensor,
    IPotentialHydrogenSensor,
    ITurbiditySensor,
    ITemperatureSensor,
    IRedoxPotentialSensor
{
    /// <summary>
    /// Get the Y4000 serial number
    /// </summary>
    /// <returns></returns>
    Task<string> GetSerialNumber();
}

/// <summary>
/// Represents a Yosemitech Y4000 Multiparameter Sonde water quality sensor 
/// for dissolved oxygen, conductivity, turbidity, pH, chlorophyll, 
/// blue green algae, chlorophyll, and temperature
/// </summary>
public partial class Y4000 :
    IDisposable,
    IY4000
{
    private Timer updateTimer;
    private TimeSpan? updateInterval = null;
    private Measurements? lastMeasurements = null;
    /// <summary>
    /// Did we create the port(s) used by the peripheral
    /// </summary>
    private readonly bool createdPort = false;
    private readonly IModbusBusClient modbusClient;

    /// <summary>
    /// 9600 baud 8-N-1
    /// </summary>
    private readonly ISerialPort? serialPort;

    /// <summary>
    /// The default baud rate for communicating with the device
    /// </summary>
    public const int DefaultBaudRate = 9600;

    private EventHandler<IChangeResult<Units.Temperature>>? tempEvents;
    private EventHandler<IChangeResult<Turbidity>>? turbidityEvents;
    private EventHandler<IChangeResult<PotentialHydrogen>>? phEvents;
    private EventHandler<IChangeResult<Conductivity>>? conductivityEvents;
    private EventHandler<IChangeResult<Voltage>>? redoxEvents;
    private EventHandler<IChangeResult<WaterQualityConcentrations>>? concentrationEvents;

    Units.Temperature? ITemperatureSensor.Temperature => lastMeasurements?.Temperature;
    Turbidity? ITurbiditySensor.Turbidity => lastMeasurements?.Turbidity;
    PotentialHydrogen? IPotentialHydrogenSensor.pH => lastMeasurements?.PH;
    Conductivity? IElectricalConductivitySensor.Conductivity => lastMeasurements?.ElectricalConductivity;
    Voltage? IRedoxPotentialSensor.Potential => lastMeasurements?.OxidationReductionPotential;
    WaterQualityConcentrations? IWaterQualityConcentrationsSensor.Concentrations => lastMeasurements?.Concentrations;

    event EventHandler<IChangeResult<Units.Temperature>> ISamplingSensor<Units.Temperature>.Updated
    {
        add => tempEvents += value;
        remove => tempEvents -= value;
    }

    event EventHandler<IChangeResult<Turbidity>> ISamplingSensor<Turbidity>.Updated
    {
        add => turbidityEvents += value;
        remove => turbidityEvents -= value;
    }

    event EventHandler<IChangeResult<PotentialHydrogen>> ISamplingSensor<PotentialHydrogen>.Updated
    {
        add => phEvents += value;
        remove => phEvents -= value;
    }

    event EventHandler<IChangeResult<Conductivity>> ISamplingSensor<Conductivity>.Updated
    {
        add => conductivityEvents += value;
        remove => conductivityEvents -= value;
    }

    event EventHandler<IChangeResult<Voltage>> ISamplingSensor<Voltage>.Updated
    {
        add => redoxEvents += value;
        remove => redoxEvents -= value;
    }

    event EventHandler<IChangeResult<WaterQualityConcentrations>> ISamplingSensor<WaterQualityConcentrations>.Updated
    {
        add => concentrationEvents += value;
        remove => concentrationEvents -= value;
    }

    /// <inheritdoc/>
    public TimeSpan UpdateInterval => updateInterval ?? TimeSpan.FromSeconds(5);

    /// <inheritdoc/>
    public bool IsSampling => updateInterval != null;

    /// <inheritdoc/>
    public void StartUpdating(TimeSpan? updateInterval)
    {
        this.updateInterval = updateInterval;
        updateTimer.Change(TimeSpan.Zero, UpdateInterval);
    }

    /// <inheritdoc/>
    public void StopUpdating()
    {
        this.updateInterval = null;
        updateTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    /// <summary>
    /// Creates a new Y4000 object
    /// </summary>
    public Y4000(
        IModbusBusClient modbusClient,
        byte modbusAddress = 0x01)
    {
        this.modbusClient = modbusClient;
        ModbusAddress = modbusAddress;

        updateTimer = new Timer(UpdateTimerProc);
    }

    /// <summary>
    /// Creates a new Y4000 object
    /// </summary>
    public Y4000(IMeadowDevice device,
        SerialPortName serialPortName,
        byte modbusAddress = 0x01,
        IPin? enablePin = null)
    {
        createdPort = true;

        serialPort = device.CreateSerialPort(serialPortName, 9600, 8, Parity.None, StopBits.One);
        serialPort.WriteTimeout = serialPort.ReadTimeout = TimeSpan.FromSeconds(5);

        if (enablePin != null)
        {
            var enablePort = device.CreateDigitalOutputPort(enablePin, false);
            modbusClient = new ModbusRtuClient(serialPort, enablePort);
        }
        else
        {
            modbusClient = new ModbusRtuClient(serialPort);
        }

        ModbusAddress = modbusAddress;

        updateTimer = new Timer(UpdateTimerProc);
    }

    private async void UpdateTimerProc(object _)
    {
        try
        {
            var currentmeasurements = await ReadSensor();
            if (currentmeasurements != null)
            {
                RaiseEventsAndNotify(currentmeasurements.Value);
            }
        }
        catch (Exception ex)
        {
            Resolver.Log.Debug($"Failed to read: {ex.Message}");
        }
    }

    /// <summary>
    /// Reads all measurements of the sensor
    /// </summary>
    public async Task<Measurements?> ReadSensor()
    {
        var values = await modbusClient.ReadHoldingRegistersFloat(ModbusAddress, Registers.Data.Offset, Registers.Data.Length / 2);

        if (values.Length != 8)
        {
            Resolver.Log.Debug($"Reading sonde at 0x{ModbusAddress} returned {values.Length} bytes");
            return null;
        }
        return new Measurements(values);
    }

    async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
    {
        Measurements? measurements;

        if (!IsSampling || lastMeasurements == null)
        {
            measurements = await ReadSensor();
        }
        else
        {
            measurements = lastMeasurements.Value;
        }
        return measurements.Value.Temperature;
    }

    async Task<Turbidity> ISensor<Turbidity>.Read()
    {
        Measurements? measurements;

        if (!IsSampling || lastMeasurements == null)
        {
            measurements = await ReadSensor();
        }
        else
        {
            measurements = lastMeasurements.Value;
        }
        return measurements.Value.Turbidity;
    }

    async Task<PotentialHydrogen> ISensor<PotentialHydrogen>.Read()
    {
        Measurements? measurements;

        if (!IsSampling || lastMeasurements == null)
        {
            measurements = await ReadSensor();
        }
        else
        {
            measurements = lastMeasurements.Value;
        }
        return measurements.Value.PH;
    }

    async Task<Conductivity> ISensor<Conductivity>.Read()
    {
        Measurements? measurements;

        if (!IsSampling || lastMeasurements == null)
        {
            measurements = await ReadSensor();
        }
        else
        {
            measurements = lastMeasurements.Value;
        }
        return measurements.Value.ElectricalConductivity;
    }

    async Task<Voltage> ISensor<Voltage>.Read()
    {
        Measurements? measurements;

        if (!IsSampling || lastMeasurements == null)
        {
            measurements = await ReadSensor();
        }
        else
        {
            measurements = lastMeasurements.Value;
        }
        return measurements.Value.OxidationReductionPotential;
    }

    async Task<WaterQualityConcentrations> ISensor<WaterQualityConcentrations>.Read()
    {
        Measurements? measurements;

        if (!IsSampling || lastMeasurements == null)
        {
            measurements = await ReadSensor();
        }
        else
        {
            measurements = lastMeasurements.Value;
        }
        return measurements.Value.Concentrations;
    }

    private void RaiseEventsAndNotify(Measurements currentmeasurements)
    {
        tempEvents?.Invoke(this, new ChangeResult<Units.Temperature>(currentmeasurements.Temperature, lastMeasurements?.Temperature));
        turbidityEvents?.Invoke(this, new ChangeResult<Turbidity>(currentmeasurements.Turbidity, lastMeasurements?.Turbidity));
        phEvents?.Invoke(this, new ChangeResult<PotentialHydrogen>(currentmeasurements.PH, lastMeasurements?.PH));
        conductivityEvents?.Invoke(this, new ChangeResult<Conductivity>(currentmeasurements.ElectricalConductivity, lastMeasurements?.ElectricalConductivity));
        redoxEvents?.Invoke(this, new ChangeResult<Voltage>(currentmeasurements.OxidationReductionPotential, lastMeasurements?.OxidationReductionPotential));
        concentrationEvents?.Invoke(this, new ChangeResult<WaterQualityConcentrations>(currentmeasurements.Concentrations, lastMeasurements?.Concentrations));

        lastMeasurements = currentmeasurements;
    }

    /// <summary>
    /// Is the object disposed
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// The current modbus address
    /// </summary>
    public byte ModbusAddress { get; private set; } = 0x01;

    /// <summary>
    /// Initialize sensor
    /// </summary>
    /// <returns></returns>
    public Task Initialize()
    {
        return modbusClient.Connect();
    }

    /// <summary>
    /// Get the device ISDN (address) of the sensor
    /// Note this is a broadcast event so all Y4000 devices on the bus will respond
    /// </summary>
    /// <returns>The address as a byte</returns>
    public async Task<byte> GetISDN()
    {
        var data = await modbusClient.ReadHoldingRegisters(0xFF, Registers.ISDN.Offset, Registers.ISDN.Length);

        return (byte)(data[0] >> 8);
    }

    /// <summary>
    /// Set the ISDN (address) of the sensor
    /// </summary>
    /// <param name="modbusAddress">The address</param>
    /// <returns></returns>
    public async Task SetISDN(byte modbusAddress)
    {
        if (ModbusAddress == modbusAddress) { return; }

        await modbusClient.WriteHoldingRegister(ModbusAddress,
            Registers.ISDN.Offset,
            (ushort)(modbusAddress << 8));

        ModbusAddress = modbusAddress;
    }

    /// <summary>
    /// Get the current supply voltage
    /// </summary>
    /// <returns></returns>
    public async Task<Voltage> GetSupplyVoltage()
    {
        var voltage = await modbusClient.ReadHoldingRegistersFloat(ModbusAddress, Registers.SupplyVoltage.Offset, Registers.SupplyVoltage.Length / 2);
        return new Voltage(voltage[0], Voltage.UnitType.Volts);
    }

    /// <summary>
    /// Get the device serial number 
    /// </summary>
    /// <returns>The serial number</returns>
    public async Task<string> GetSerialNumber()
    {
        var data = await modbusClient.ReadHoldingRegisters(ModbusAddress, Registers.SerialNumber.Offset, Registers.SerialNumber.Length);

        // according to the manual, bytes 0 and 13 are reserved.  bytes 1-12 contain the ascii serial number
        var bytes = data
            .SelectMany(r => new byte[] { (byte)(r >> 8), (byte)(r & 0xff) })
            .ToArray();

        return Encoding.ASCII.GetString(bytes);
    }

    /// <summary>
    /// Get the device version
    /// </summary>
    /// <returns></returns>
    public async Task<ushort[]> GetVersion()
    {
        var data = await modbusClient.ReadHoldingRegisters(ModbusAddress, Registers.Version.Offset, Registers.Version.Length);
        return data;
    }

    /// <summary>
    /// Get the brush or wiper interval
    /// </summary>
    /// <returns></returns>
    public async Task<TimeSpan> GetBrushInterval()
    {
        var value = await modbusClient.ReadHoldingRegisters(ModbusAddress, Registers.BrushInterval.Offset, Registers.BrushInterval.Length);
        return TimeSpan.FromMinutes(value[0]);
    }

    /// <summary>
    /// Set the brush or wiper interval (normalized to minutes)
    /// </summary>
    public Task SetBrushInterval(TimeSpan interval)
    {
        ushort minutes = (ushort)interval.TotalMinutes;
        return modbusClient.WriteHoldingRegister(ModbusAddress, Registers.BrushInterval.Offset, minutes);
    }

    /// <summary>
    /// Start the brush or wiper
    /// </summary>
    /// <returns></returns>
    public Task StartBrush()
    {
        return modbusClient.WriteHoldingRegister(ModbusAddress, Registers.StartBrush.Offset, 0);
    }

    /// <summary>
    /// Read the error flag from the sensor
    /// </summary>
    /// <returns></returns>
    public async Task<ushort> GetErrorFlag()
    {
        var data = await modbusClient.ReadHoldingRegisters(ModbusAddress, Registers.ErrorCode.Offset, 2);
        return data[0];
    }

    /*
     * Get and Set time work but Get returns bad values
     * Leaving code here for future investigation
     */
    /// <summary>
    /// Set the time on the device
    /// Stores: year, month, day, hour, minute and second
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    private Task SetTime(DateTime time)
    {
        byte second = 0x17;// (byte)time.Second;
        byte minute = 0x05;//(byte)time.Minute;
        byte hour = 0x13;//(byte)time.Hour;
        byte day = 0x26;//(byte)time.Day;
        byte month = 0x04;//(byte)time.Month;
        //0
        byte year = 0x16; // (byte)time.Year;
        //0

        var data = new ushort[4];
        data[0] = (ushort)(minute | (second << 8));
        data[1] = (ushort)(day | (hour << 8));
        data[2] = (ushort)(month << 8 | 0x00);
        data[3] = (ushort)(year << 8 | 0x00);

        return modbusClient.WriteHoldingRegisters(ModbusAddress, Registers.Time.Offset, data);
    }

    /// <summary>
    /// Get the time stored on the sensor
    /// </summary>
    /// <returns></returns>
    private async Task<DateTime> GetTime()
    {
        var values = await modbusClient.ReadHoldingRegisters(ModbusAddress, Registers.Time.Offset, 4);

        return DateTime.MinValue;
    }

    ///<inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose of the object
    /// </summary>
    /// <param name="disposing">Is disposing</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing && createdPort)
            {
                if (serialPort is { })
                {
                    if (serialPort.IsOpen)
                    {
                        serialPort.Close();
                    }

                    serialPort.Dispose();
                }
            }

            IsDisposed = true;
        }
    }
}