using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Power;

public partial class CurrentTransducer : SamplingSensorBase<Current>
{
    private IAnalogInputPort? _inputPort = null;
    private ISensor<Voltage>? _inputSensor = null;

    private Current _ctMaxCurrent;

    public Voltage ReferenceVoltage { get; private set; }

    public CurrentTransducer(IAnalogInputPort input, Voltage referenceVoltage, Current sensorMaximum)
    {
        _inputPort = input;
        ReferenceVoltage = referenceVoltage;
        _ctMaxCurrent = sensorMaximum;
    }

    public CurrentTransducer(ISensor<Voltage> input, Voltage referenceVoltage, Current sensorMaximum)
    {
        _inputSensor = input;
        ReferenceVoltage = referenceVoltage;
        _ctMaxCurrent = sensorMaximum;
    }

    protected override async Task<Current> ReadSensor()
    {
        Voltage rawReading;

        if (_inputPort != null)
        {
            rawReading = await _inputPort.Read();
        }
        else
        {
            rawReading = await _inputSensor.Read();
        }

        var ratio = rawReading.Volts / ReferenceVoltage.Volts;
        var reading = new Current(_ctMaxCurrent.Amps * ratio, Current.UnitType.Amps);
        return reading;
    }

    public override void StartUpdating(TimeSpan? updateInterval = null)
    {
        throw new NotImplementedException();
    }

    public override void StopUpdating()
    {
        throw new NotImplementedException();
    }
}