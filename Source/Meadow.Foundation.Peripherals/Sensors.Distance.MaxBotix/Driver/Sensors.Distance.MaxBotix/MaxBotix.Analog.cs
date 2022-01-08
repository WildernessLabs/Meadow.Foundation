using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Units;
using System.Threading.Tasks;

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

        async Task<Length> ReadSensorAnalog()
        {
            var volts = (await analogInputPort.Read()).Volts;
            Length length;

            switch (sensorModel)
            {
                case SensorModel.MB1000:
                case SensorModel.MB1010:
                case SensorModel.MB1020:
                case SensorModel.MB1030:
                case SensorModel.MB1040:
                    //(Vcc/512) per inch
                    length = new Length(volts * 512.0 / VCC, Meadow.Units.Length.UnitType.Inches);
                    break;
                //10m
                case SensorModel.MB1260:
                case SensorModel.MB1261:
                case SensorModel.MB1360:
                case SensorModel.MB1361:
                //16.5m
                case SensorModel.MB2530:
                case SensorModel.MB2532:
                    //(Vcc / 1024) per 2 - cm
                    length = new Length(volts * 2048.0 / VCC, Meadow.Units.Length.UnitType.Centimeters);
                    break;

                case SensorModel.MB1603:
                case SensorModel.MB1604:
                case SensorModel.MB1613:
                case SensorModel.MB1614:
                case SensorModel.MB1623:
                case SensorModel.MB1624:
                case SensorModel.MB1633:
                case SensorModel.MB1634:
                case SensorModel.MB1643:
                case SensorModel.MB1644:
                    //= [Vobserved / ((Vcc/1024) * 6)] - 300 (in mm)
                    length = new Length(volts * 1024.0 / VCC * 6.0 - 300.0, Meadow.Units.Length.UnitType.Millimeters);
                    break;

                //5m 
                case SensorModel.MB1003:
                case SensorModel.MB1013:
                case SensorModel.MB1023:
                case SensorModel.MB1033:
                case SensorModel.MB1043:
                //Intentional fall-through
                //5m HRXL
                case SensorModel.MB7360:
                case SensorModel.MB7367:
                case SensorModel.MB7369:
                case SensorModel.MB7380:
                case SensorModel.MB7387:
                case SensorModel.MB7389:
                    //(Vcc/5120) per 1-mm
                    length = new Length(volts * 5120.0 / VCC, Meadow.Units.Length.UnitType.Millimeters);
                    break;
                //10m HRXL
                case SensorModel.MB7363:
                case SensorModel.MB7366:
                case SensorModel.MB7368:
                case SensorModel.MB7383:
                case SensorModel.MB7386:
                case SensorModel.MB7388:
                //Intentional fall-through
                //7.6m
                case SensorModel.MB1200:
                case SensorModel.MB1210:
                case SensorModel.MB1220:
                case SensorModel.MB1230:
                case SensorModel.MB1240:
                case SensorModel.MB1300:
                case SensorModel.MB1310:
                case SensorModel.MB1320:
                case SensorModel.MB1330:
                case SensorModel.MB1340:
                //(Vcc / 1024) per 1 - cm
                //1.5m HRXL
                case SensorModel.MB7375:
                case SensorModel.MB7395:
                    //(Vcc / 1024) per 1 - cm
                    length = new Length(volts * 1024.0 / VCC, Meadow.Units.Length.UnitType.Centimeters);
                    break;
                default:
                    length = new Length(0);
                    break;
            }

            return length;
        }
    }
}