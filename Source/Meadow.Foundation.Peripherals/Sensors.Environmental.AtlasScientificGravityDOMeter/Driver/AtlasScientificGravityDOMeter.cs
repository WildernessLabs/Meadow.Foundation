using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental;

/// <summary>
/// Atlas Scientific Analog Gravity Disolved Oxygen Meter
/// </summary>
public partial class AtlasScientificGravityDOMeter : SamplingSensorBase<double>
{
    /// <summary>
    /// Raised when a new sensor percentage saturation reading is ready
    /// </summary>
    public event EventHandler<IChangeResult<double>> SaturationUpdated = delegate { };

    /// <summary>
    /// Returns the analog input port
    /// </summary>
    protected IAnalogInputPort AnalogInputPort { get; }

    /// <summary>
    /// Last percentage saturation value read from the sensor
    /// </summary>
    public double? Saturation { get; protected set; }

    /// <summary>
    /// Creates a new AtlasScientificGravityDOMeter object
    /// </summary>
    /// <param name="analogInputPin">Analog pin the temperature sensor is connected to</param>
    /// <param name="sampleCount">How many samples to take during a given reading</param>
    /// <param name="sampleInterval">The time, to wait in between samples during a reading</param>
    public AtlasScientificGravityDOMeter(IPin analogInputPin, int sampleCount = 5, TimeSpan? sampleInterval = null)
        : this(analogInputPin.CreateAnalogInputPort(sampleCount, sampleInterval ?? TimeSpan.FromMilliseconds(40), new Voltage(3.3)))
    { }

    /// <summary>
    /// Creates a new AtlasScientificGravityDOMeter object
    /// </summary>
    /// <param name="analogInputPort">The port for the analog input pin</param>
    public AtlasScientificGravityDOMeter(IAnalogInputPort analogInputPort)
    {
        AnalogInputPort = analogInputPort;

        AnalogInputPort.Subscribe
        (
            IAnalogInputPort.CreateObserver(
                result =>
                {
                    ChangeResult<double> changeResult = new()
                    {
                        New = VoltageToSaturation(result.New),
                        Old = Saturation
                    };
                    Saturation = changeResult.New;
                    RaiseEventsAndNotify(changeResult);
                }
            )
       );
    }

    /// <summary>
    /// Reads data from the sensor
    /// </summary>
    /// <returns>The latest sensor reading</returns>
    protected override async Task<double> ReadSensor()
    {
        var voltage = await AnalogInputPort.Read();
        var newSaturation = VoltageToSaturation(voltage);
        Saturation = newSaturation;
        return newSaturation;
    }

    /// <summary>
    /// Starts continuously sampling the sensor
    /// </summary>
    public override void StartUpdating(TimeSpan? updateInterval)
    {
        lock (samplingLock)
        {
            if (IsSampling) { return; }
            IsSampling = true;
            AnalogInputPort.StartUpdating(updateInterval);
        }
    }

    /// <summary>
    /// Stops sampling the sensor
    /// </summary>
    public override void StopUpdating()
    {
        lock (samplingLock)
        {
            if (!IsSampling) { return; }
            IsSampling = false;
            AnalogInputPort.StopUpdating();
        }
    }

    /// <summary>
    /// Raise change events for subscribers
    /// </summary>
    /// <param name="changeResult">The change result with the current sensor data</param>
    protected override void RaiseEventsAndNotify(IChangeResult<double> changeResult)
    {
        SaturationUpdated?.Invoke(this, changeResult);
        base.RaiseEventsAndNotify(changeResult);
    }

    double VoltageToSaturation(Voltage voltage) => voltage.Volts * 11;
}