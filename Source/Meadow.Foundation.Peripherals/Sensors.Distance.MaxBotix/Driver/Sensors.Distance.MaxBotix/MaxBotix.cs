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
        SensorType sensorType;

        protected override async Task<Length> ReadSensor()
        {
            return communication switch
            {
                CommunicationType.Analog => await ReadSensorAnalog(),
                CommunicationType.Serial => ReadSensorSerial(),
                _ => throw new NotImplementedException(),
            };
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

        Length.UnitType GetUnitsForSensor(SensorType sensor)
        {
            switch(sensor)
            {
                case SensorType.LV:
                    return Units.Length.UnitType.Inches;
                case SensorType.XL:
                case SensorType.XLLongRange:
                    return Units.Length.UnitType.Centimeters;
                case SensorType.HR5Meter:
                case SensorType.HR10Meter:
                default:
                    return Units.Length.UnitType.Millimeters;
            }
        }
    }
}