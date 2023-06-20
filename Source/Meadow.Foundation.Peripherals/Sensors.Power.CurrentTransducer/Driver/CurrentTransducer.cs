using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Power
{
    /// <summary>
    /// Represents a general Current Transducer (CT) sensor INA260 Precision Digital Current and Power Monitor
    /// </summary>
    public partial class CurrentTransducer : SamplingSensorBase<Current>
    {
        /// <summary>
        /// The analog input port connected to the transducer
        /// </summary>
        protected IAnalogInputPort AnalogPort { get; }
        /// <summary>
        /// The maximum voltage the CT outputs
        /// </summary>
        protected Voltage MaxVoltage { get; }
        /// <summary>
        /// The sensed current at the maximum output voltage
        /// </summary>
        protected Current MaxCurrent { get; }
        /// <summary>
        /// The minimum voltage the CT outputs
        /// </summary>
        protected Voltage MinVoltage { get; }
        /// <summary>
        /// The sensed current at the minimum output voltage
        /// </summary>
        protected Current MinCurrent { get; }

        /// <summary>
        /// Creates a new CurrentTransducer instance
        /// </summary>
        /// <param name="analogPort">The analog input port connected to the transducer</param>
        /// <param name="maxVoltage">The maximum voltage the CT outputs</param>
        /// <param name="maxCurrent">The sensed current at the maximum output voltage</param>
        /// <param name="minVoltage">The minimum voltage the CT outputs</param>
        /// <param name="minCurrent">The sensed current at the minimum output voltage</param>
        public CurrentTransducer(IAnalogInputPort analogPort, Voltage maxVoltage, Current maxCurrent, Voltage? minVoltage, Current? minCurrent)
        {
            AnalogPort = analogPort;
            MaxVoltage = maxVoltage;
            MaxCurrent = maxCurrent;
            MinVoltage = minVoltage.HasValue ? minVoltage.Value : new Voltage(0, Voltage.UnitType.Volts);
            MinCurrent = minCurrent.HasValue ? minCurrent.Value : new Current(0, Current.UnitType.Amps);
        }

        /// <summary>
        /// Converts an output voltage from the CT to a sensed current using linear interpolation
        /// </summary>
        /// <param name="voltage">The ADC voltage read by the AnalogPort</param>
        /// <returns>The current being sensed by the CT</returns>
        public virtual Current ConvertVoltageToCurrent(Voltage voltage)
        {
            // the default implementation just does a simple linear conversion
            return new Current(
                (MaxCurrent.Amps - MinCurrent.Amps) /
                (MaxVoltage.Volts - MinVoltage.Volts)
                * voltage.Volts);
        }

        ///<inheritdoc/>
        protected override async Task<Current> ReadSensor()
        {
            var adc = await AnalogPort.Read();
            return ConvertVoltageToCurrent(adc);
        }

        ///<inheritdoc/>
        public override void StartUpdating(TimeSpan? updateInterval = null)
        {
            lock (samplingLock)
            {
                if (IsSampling) return;
                IsSampling = true;

                base.SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                AnalogPort.StartUpdating(updateInterval);
            }
        }

        ///<inheritdoc/>
        public override void StopUpdating()
        {
            lock (samplingLock)
            {
                if (!IsSampling) return;

                AnalogPort.StopUpdating();

                SamplingTokenSource?.Cancel();

                // state machine
                IsSampling = false;
            }
        }
    }
}