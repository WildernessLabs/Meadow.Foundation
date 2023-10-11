using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Power
{
    /// <summary>
    /// Represents a general Current Transducer (CT) sensor
    /// </summary>
    public partial class CurrentTransducer : SamplingSensorBase<Current>
    {
        /// <summary>
        /// Raised when the value of the reading changes
        /// </summary>
        public event EventHandler<IChangeResult<Current>> CurrentUpdated = delegate { };

        /// <summary>
        /// The analog input port connected to the transducer
        /// </summary>
        protected IAnalogInputPort AnalogPort { get; private set; } = default!;

        /// <summary>
        /// The maximum voltage the CT outputs
        /// </summary>
        protected Voltage MaxVoltage { get; private set; } = default!;

        /// <summary>
        /// The sensed current at the maximum output voltage
        /// </summary>
        protected Current MaxCurrent { get; private set; } = default!;

        /// <summary>
        /// The minimum voltage the CT outputs
        /// </summary>
        protected Voltage MinVoltage { get; private set; } = default!;

        /// <summary>
        /// The sensed current at the minimum output voltage
        /// </summary>
        protected Current MinCurrent { get; private set; } = default!;

        /// <summary>
        /// The minimum output voltage
        /// </summary>
        protected Current MinVoltageDelta { get; private set; } = default!;

        /// <summary>
        /// The last sensed Current
        /// </summary>
        public Current? Current { get; protected set; }

        /// <summary>
        /// Creates a new CurrentTransducer instance
        /// </summary>
        /// <param name="analogPort">The analog input port connected to the transducer</param>
        /// <param name="maxVoltage">The maximum voltage the CT outputs</param>
        /// <param name="maxCurrent">The sensed current at the maximum output voltage</param>
        /// <param name="minVoltage">The minimum voltage the CT outputs</param>
        /// <param name="minCurrent">The sensed current at the minimum output voltage</param>
        public CurrentTransducer(IAnalogInputPort analogPort, Voltage maxVoltage, Current maxCurrent, Voltage? minVoltage = null, Current? minCurrent = null)
        {
            Initialize(analogPort, maxVoltage, maxCurrent, minVoltage, minCurrent);
        }

        /// <summary>
        /// Creates a new CurrentTransducer instance
        /// </summary>
        protected CurrentTransducer()
        {
        }

        /// <summary>
        /// Initializes the CurrentTransducer instance
        /// Use this method when a derived class must do pre-initialization work
        /// </summary>
        /// <param name="analogPort">The analog input port connected to the transducer</param>
        /// <param name="maxVoltage">The maximum voltage the CT outputs</param>
        /// <param name="maxCurrent">The sensed current at the maximum output voltage</param>
        /// <param name="minVoltage">The minimum voltage the CT outputs</param>
        /// <param name="minCurrent">The sensed current at the minimum output voltage</param>
        protected virtual void Initialize(IAnalogInputPort analogPort, Voltage maxVoltage, Current maxCurrent, Voltage? minVoltage = null, Current? minCurrent = null)
        {
            AnalogPort = analogPort;
            MaxVoltage = maxVoltage;
            MaxCurrent = maxCurrent;
            MinVoltage = minVoltage ?? new Voltage(0, Voltage.UnitType.Volts);
            MinCurrent = minCurrent ?? new Current(0, Units.Current.UnitType.Amps);

            AnalogPort.Subscribe
            (
                IAnalogInputPort.CreateObserver(
                    result =>
                    {
                        ChangeResult<Current> changeResult = new()
                        {
                            New = ConvertVoltageToCurrent(result.New),
                            Old = Current
                        };
                        Current = changeResult.New;
                        RaiseEventsAndNotify(changeResult);
                    }
                )
            );
        }

        /// <summary>
        /// Converts an output voltage from the CT to a sensed current using linear interpolation
        /// </summary>
        /// <param name="voltage">The ADC voltage read by the AnalogPort</param>
        /// <returns>The current being sensed by the CT</returns>
        protected virtual Current ConvertVoltageToCurrent(Voltage voltage)
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
            var newCurrent = ConvertVoltageToCurrent(adc);
            Current = newCurrent;
            return newCurrent;
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

        /// <summary>
        /// Method to notify subscribers to CurrentUpdated event handler
        /// </summary>
        /// <param name="changeResult"></param>
        protected override void RaiseEventsAndNotify(IChangeResult<Current> changeResult)
        {
            CurrentUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }
    }
}