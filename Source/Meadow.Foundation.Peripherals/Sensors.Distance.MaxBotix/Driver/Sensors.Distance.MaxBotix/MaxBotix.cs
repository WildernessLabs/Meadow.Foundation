using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Distance
{
    /* Supports:
     * HRXL-MaxSonar® - WR Series
     * HRLV-MaxSonar® - EZ Series
     * HRLV-ShortRange® - EZ Series
     * HRLV-MaxSonar® - EZ Series
     * IRXL-MaxSonar® - CS Series
     * XL-MaxSonar® - EZ Series
     * XL-MaxSonar® - WR/WRC Series
     * 
     * Not Supported:
     * ParkSonar® - EZ Sensor Series
     * LV-ProxSonar® - EZ Series (parking sensor)
     * I2CXL-MaxSonar® - EZ Series
     */

    public partial class MaxBotix : SensorBase<Length>
    {
        /// <summary>
        /// Raised when the value of the reading changes.
        /// </summary>
        public event EventHandler<IChangeResult<Length>> LengthUpdated = delegate { };

        public Length? Length { get; protected set; }

        public double VCC { get; set; } = 3.3;

        CommunicationType communication;
        SensorModel sensorModel;

        protected override async Task<Length> ReadSensor()
        {
            return communication switch
            {
                CommunicationType.Analog => await ReadSensorAnalog(),
                CommunicationType.Serial => ReadSensorSerial(),
                _ => throw new NotImplementedException(),
            };
        }

        Length ReadSensorSerial()
        {
            //I think we'll just cache it for serial
            return Length.Value;
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

                //5m 
                case SensorModel.MB1003:
                case SensorModel.MB1013:
                case SensorModel.MB1023:
                case SensorModel.MB1033:
                case SensorModel.MB1043:
                //Intentional fall-through
                //5m 
                case SensorModel.MB1004:
                case SensorModel.MB1014:
                case SensorModel.MB1024:
                case SensorModel.MB1034:
                case SensorModel.MB1044:
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

        double ReadSensorPWM()
        {
            throw new NotImplementedException();
        }

        protected override void RaiseEventsAndNotify(IChangeResult<Length> changeResult)
        {
            LengthUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }

        public void StartUpdating(TimeSpan? updateInterval)
        {
            // thread safety
            lock (samplingLock)
            {
                if (IsSampling) return;
                IsSampling = true;

                if (communication == CommunicationType.Analog)
                {
                    analogInputPort.StartUpdating(updateInterval);
                }
                else if(communication == CommunicationType.Serial)
                {
                    serialMessagePort.Open();
                }
            }
        }

        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public void StopUpdating()
        {
            lock (samplingLock)
            {
                if (!IsSampling) return;
                base.IsSampling = false;

                if (communication == CommunicationType.Analog)
                {
                    analogInputPort.StopUpdating();
                }
                else if(communication != CommunicationType.Serial)
                {
                    serialMessagePort.Close();
                }
            }
        }
    }
}