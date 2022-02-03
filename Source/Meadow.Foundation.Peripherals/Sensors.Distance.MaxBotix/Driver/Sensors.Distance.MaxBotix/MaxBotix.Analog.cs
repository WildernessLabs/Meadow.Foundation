using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Distance
{
    public partial class MaxBotix
    {
        IAnalogInputPort analogInputPort;

        public MaxBotix(IAnalogInputPort analogIntputPort, SensorType sensor)
        {
            analogInputPort = analogIntputPort;

            communication = CommunicationType.Analog;
            sensorType = sensor;

            AnalogInitialize();
        }

        public MaxBotix(SensorType sensor, 
            IMeadowDevice device, 
            IPin analogInputPin, 
            int sampleCount = 5, 
            TimeSpan? sampleInterval = null, 
            Voltage? voltage = null) :
            this(device.CreateAnalogInputPort(analogInputPin, sampleCount, sampleInterval ?? TimeSpan.FromMilliseconds(40), voltage ?? new Voltage(3.3)), sensor)
        {
        }

        void AnalogInitialize()
        {
            // wire up our observable
            // have to convert from voltage to length units for our consumers
            // this is where the magic is: this allows us to extend the IObservable
            // pattern through the sensor driver
            analogInputPort.Subscribe
            (
                IAnalogInputPort.CreateObserver(
                    async result => {
                        // create a new change result from the new value
                        ChangeResult<Length> changeResult = new ChangeResult<Length>()
                        {
                            New = await ReadSensorAnalog(),
                            Old = Length,
                        };
                        // save state
                        Length = changeResult.New;
                        // notify
                        RaiseEventsAndNotify(changeResult);
                    }
                )
           );
        }

        
        async Task<Length> ReadSensorAnalog()
        {
            var volts = (await analogInputPort.Read()).Volts;
            Length.UnitType unit = GetUnitsForSensor(sensorType);

            return sensorType switch
            {
                SensorType.LV => new Length(volts * 512.0 / VCC, unit), //inches
                SensorType.XL => new Length(volts * 1024.0 / VCC, unit), //cm
                SensorType.XLLongRange => new Length(volts * 512.0 / VCC, unit), //cm
                SensorType.HR5Meter => new Length(volts * 5120.0 / VCC, unit), //mm
                SensorType.HR10Meter => new Length(volts * 10240.0 / VCC, unit), //mm
                _ => new Length(volts * 5120.0 / VCC, Units.Length.UnitType.Millimeters) // most common for unknown
            };
        }
    }
}