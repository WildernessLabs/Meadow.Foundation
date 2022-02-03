using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Distance
{
    public partial class MaxBotix : ByteCommsSensorBase<Length>
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
                CommunicationType.I2C => ReadSensorI2c(),
                _ => throw new NotImplementedException(),
            };
        }

        protected override void RaiseEventsAndNotify(IChangeResult<Length> changeResult)
        {
            LengthUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }

        public override void StartUpdating(TimeSpan? updateInterval)
        {
            // thread safety
            lock (samplingLock)
            {
                if (IsSampling) return;
                IsSampling = true;

                switch(communication)
                {
                    case CommunicationType.Analog:
                        analogInputPort.StartUpdating(updateInterval);
                        break;
                    case CommunicationType.Serial:
                        serialMessagePort.Open();
                        break;
                    case CommunicationType.I2C:
                        base.StartUpdating(updateInterval);
                        break;
                }
            }
        }

        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public override void StopUpdating()
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
                else if (communication == CommunicationType.I2C)
                {   //handled in ByteCommsSensorBase
                    base.StopUpdating();
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