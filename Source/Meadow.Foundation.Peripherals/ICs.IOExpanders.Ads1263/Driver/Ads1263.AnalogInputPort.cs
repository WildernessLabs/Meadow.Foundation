using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Ads1263
    {
        /// <summary>
        /// Represents an Ads1263 analog input port
        /// </summary>
        public class AnalogInputPort : AnalogInputPortBase, IAnalogInputPort
        {
            /// <summary>
            /// Is the port sampling
            /// </summary>
            public bool IsSampling { get; protected set; } = false;

            private readonly Ads1263 controller;

            private Voltage previousVoltageReading;

            private CancellationTokenSource? SamplingTokenSource;

            private readonly object _lock = new();

            /// <summary>
            /// Create a new AnalogInputPort object with a non-default reference voltage
            /// </summary>
            /// <param name="controller">The parent Ads1263 controller.</param>
            /// <param name="pin">The pin associated with the port.</param>
            /// <param name="channel">The channel information for the port.</param>
            /// <param name="sampleCount">The number of samples to be taken during each reading.</param>
            /// <param name="sampleInterval">The time interval between samples during each reading.</param>
            /// <exception cref="ArgumentNullException">Thrown if <paramref name="pin"/> or <paramref name="channel"/> is <c>null</c>.</exception>
            public AnalogInputPort(Ads1263 controller,
                IPin pin,
                IAnalogChannelInfo channel,
                int sampleCount, TimeSpan sampleInterval)
                : base(pin, channel, sampleCount, sampleInterval, controller.GetADCReferenceVoltage(pin))
            {
                // TODO: Validate pin is valid before using it?
                this.controller = controller;

                SampleCount = sampleCount;
            }

            /// <summary>
            /// Start the ADC conversions
            /// </summary>
            public void StartConversions()
            {
                controller.ADCStart(Pin);
            }

            /// <summary>
            /// Stop the ADC conversions
            /// </summary>
            public void StopConversions()
            {
                controller.ADCStop(Pin);
            }

            /// <inheritdoc />
            /// <remarks>Assumes conversions have been started</remarks>
            public override async Task<Voltage> Read()
            {
                var result = controller.ReadAnalog(Pin);
                await Task.Delay(SampleInterval);
                return result;
            }

            /// <summary>
            /// Start updating
            /// </summary>
            public override void StartUpdating(TimeSpan? updateInterval = null)
            {
                // thread safety
                lock (_lock)
                {
                    if (IsSampling)
                        return;

                    StartConversions();
                    IsSampling = true;

                    // if an update interval was passed in, override the default value
                    if (updateInterval is { } ui)
                    { UpdateInterval = ui; }

                    SamplingTokenSource = new CancellationTokenSource();
                    var ct = SamplingTokenSource.Token;

                    Task.Factory.StartNew(async () =>
                    {
                        while (true)
                        {
                            // cleanup
                            if (ct.IsCancellationRequested)
                            {
                                // do task clean up here
                                Observers.ForEach(x => x.OnCompleted());
                                break;
                            }

                            var newVoltage = await Read();
                            var result = new ChangeResult<Voltage>(newVoltage, previousVoltageReading);
                            previousVoltageReading = newVoltage;

                            // raise our events and notify our subs
                            RaiseChangedAndNotify(result);

                            await Task.Delay(UpdateInterval, ct);
                        }

                        IsSampling = false;
                    }, SamplingTokenSource.Token);
                }
            }

            /// <summary>
            /// Stop updating the port
            /// </summary>
            public override void StopUpdating()
            {
                lock (_lock)
                {
                    if (!IsSampling)
                        return;

                    SamplingTokenSource?.Cancel();

                    IsSampling = false;

                    StopConversions();
                }
            }
        }
    }
}