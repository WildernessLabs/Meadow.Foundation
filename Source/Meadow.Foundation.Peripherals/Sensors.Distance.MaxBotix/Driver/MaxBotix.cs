using Meadow.Peripherals.Sensors.Distance;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Distance
{
    /// <summary>
    /// Represents the MaxBotix series of distance sensors
    /// </summary>
    public partial class MaxBotix : ByteCommsSensorBase<Length>, IRangeFinder, IDisposable
    {
        /// <summary>
        /// Distance from sensor to object
        /// </summary>
        public Length? Distance { get; protected set; }

        /// <summary>
        /// voltage common collector (VCC) typically 3.3V
        /// </summary>
        public double VCC { get; set; } = 3.3;

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Did we create the port(s) used by the peripheral
        /// </summary>
        readonly bool createdPorts = false;

        TimeSpan? updateInterval;

        readonly CommunicationType communication;
        readonly SensorType sensorType;

        /// <summary>
        /// Start a distance measurement
        /// </summary>
        public void MeasureDistance()
        {
            _ = ReadSensor();
        }

        /// <summary>
        /// Read the distance from the sensor
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
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

        /// <summary>
        /// Start updating distances
        /// </summary>
        /// <param name="updateInterval"></param>
        public override void StartUpdating(TimeSpan? updateInterval)
        {
            lock (samplingLock)
            {
                if (IsSampling) return;
                IsSampling = true;

                this.updateInterval = updateInterval;

                switch (communication)
                {
                    case CommunicationType.Analog:
                        analogInputPort?.StartUpdating(updateInterval);
                        break;
                    case CommunicationType.Serial:
                        serialMessagePort?.Open();
                        break;
                    case CommunicationType.I2C:
                        base.StartUpdating(updateInterval);
                        break;
                }
            }
        }

        /// <summary>
        /// Stop sampling
        /// </summary>
        public override void StopUpdating()
        {
            lock (samplingLock)
            {
                if (!IsSampling) return;
                IsSampling = false;

                updateInterval = null;

                if (communication == CommunicationType.Analog)
                {
                    analogInputPort?.StopUpdating();
                }
                else if (communication != CommunicationType.Serial)
                {
                    serialMessagePort?.Close();
                }
                else if (communication == CommunicationType.I2C)
                {
                    base.StopUpdating();
                }
            }
        }

        Length.UnitType GetUnitsForSensor(SensorType sensor)
        {
            return sensor switch
            {
                SensorType.LV => Length.UnitType.Inches,
                SensorType.XL or SensorType.XLLongRange => Length.UnitType.Centimeters,
                _ => Length.UnitType.Millimeters,
            };
        }

        ///<inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        /// <param name="disposing">Is disposing</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!IsDisposed)
            {
                if (disposing && createdPorts)
                {
                    analogInputPort?.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}