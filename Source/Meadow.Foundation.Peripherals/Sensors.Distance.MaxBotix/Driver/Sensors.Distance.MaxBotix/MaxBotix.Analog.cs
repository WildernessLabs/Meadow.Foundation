using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Distance
{
    public partial class MaxBotix
    {
        IAnalogInputPort analogInputPort;

        public MaxBotix(IAnalogInputPort analogIntputPort,
            SensorModel sensor)
        {
            analogInputPort = analogIntputPort;

            communication = CommunicationType.Analog;
            sensorModel = sensor;

            AnalogInitialize();
        }

        public MaxBotix(IMeadowDevice device, IPin analogIntputPin,
            SensorModel sensor) :
            this(device.CreateAnalogInputPort(analogIntputPin), sensor)
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
    }
}