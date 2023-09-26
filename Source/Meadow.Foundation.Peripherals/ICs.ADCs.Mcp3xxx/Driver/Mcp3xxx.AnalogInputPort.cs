using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class Mcp3xxx
    {
        /// <summary>
        /// Represents an Mcp3xxx analog input port
        /// </summary>
        public class AnalogInputPort : AnalogPortBase, IAnalogInputPort
        {
            /// <summary>
            /// Raised when the port voltage value changes
            /// </summary>
            public event EventHandler<IChangeResult<Voltage>> Updated = (s, e) => { };

            /// <summary>
            /// Collection of event observers for the Updated event
            /// </summary>
            protected List<IObserver<IChangeResult<Voltage>>> Observers { get; set; } = new List<IObserver<IChangeResult<Voltage>>>();

            /// <summary>
            /// Is the port sampling
            /// </summary>
            public bool IsSampling { get; protected set; } = false;

            /// <summary>
            /// Current port voltage
            /// </summary>
            public Voltage Voltage { get; private set; }

            /// <summary>
            /// Port reference voltage
            /// </summary>
            public Voltage ReferenceVoltage { get; private set; }

            /// <summary>
            /// The sample count
            /// </summary>
            public int SampleCount { get; private set; }

            /// <summary>
            /// The update interval
            /// </summary>
            public TimeSpan UpdateInterval { get; private set; } = TimeSpan.FromSeconds(1);

            /// <summary>
            /// The voltage sampling buffer
            /// </summary>
            public Voltage[] VoltageSampleBuffer { get; }

            /// <summary>
            /// The sampling interval
            /// </summary>
            public TimeSpan SampleInterval => TimeSpan.Zero;

            /// <summary>
            /// The channel input type
            /// </summary>
            public InputType ChannelInputType { get; protected set; }

            private Voltage? _previousVoltageReading;

            private readonly Mcp3xxx controller;

            private CancellationTokenSource? SamplingTokenSource;

            private readonly object _lock = new();

            /// <summary>
            /// Create a new AnalogInputPort object
            /// </summary>
            public AnalogInputPort(Mcp3xxx controller,
                IPin pin,
                IAnalogChannelInfo channel,
                int sampleCount,
                Voltage referenceVoltage,
                InputType inputType = InputType.SingleEnded)
                : base(pin, channel)
            {
                this.controller = controller;

                SampleCount = sampleCount;
                ChannelInputType = inputType;

                ReferenceVoltage = referenceVoltage;

                VoltageSampleBuffer = new Voltage[SampleCount];
            }

            /// <summary>
            /// Create a new AnalogInputPort object
            /// </summary>
            public AnalogInputPort(Mcp3xxx controller,
                IPin pin,
                IAnalogChannelInfo channel,
                int sampleCount,
                InputType inputType = InputType.SingleEnded)
                : base(pin, channel)
            {
                this.controller = controller;

                SampleCount = sampleCount;
                ChannelInputType = inputType;

                ReferenceVoltage = new Voltage(3.3, Voltage.UnitType.Volts);

                VoltageSampleBuffer = new Voltage[SampleCount];
            }

            /// <summary>
            /// Take a reading
            /// </summary>
            public Task<Voltage> Read()
            {
                int rawValue = ChannelInputType switch
                {
                    InputType.SingleEnded => controller.ReadSingleEnded((byte)Pin.Key),
                    InputType.Differential => controller.ReadDifferential((byte)Pin.Key, (byte)Pin.Key + 1),
                    InputType.InvertedDifferential => controller.ReadDifferential((byte)Pin.Key, (byte)Pin.Key - 1),
                    _ => 0
                };

                var voltage = rawValue * ReferenceVoltage.Volts / controller.AdcMaxValue;

                return Task.FromResult(new Voltage(voltage, Voltage.UnitType.Volts));
            }

            /// <summary>
            /// Start updating
            /// </summary>
            public void StartUpdating(TimeSpan? updateInterval = null)
            {
                Console.WriteLine("StartUpdating");

                // thread safety
                lock (_lock)
                {
                    if (IsSampling) return;

                    IsSampling = true;

                    // if an update interval was passed in, override the default value
                    if (updateInterval is { } ui) { UpdateInterval = ui; }

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
                            var result = new ChangeResult<Voltage>(newVoltage, _previousVoltageReading);
                            _previousVoltageReading = newVoltage;

                            // raise our events and notify our subs
                            RaiseChangedAndNotify(result);

                            await Task.Delay(UpdateInterval);
                        }

                        IsSampling = false;
                    }, SamplingTokenSource.Token);
                }
            }

            /// <summary>
            /// Stop updating the port
            /// </summary>
            public void StopUpdating()
            {
                lock (_lock)
                {
                    if (!IsSampling) return;

                    SamplingTokenSource?.Cancel();

                    IsSampling = false;
                }
            }

            /// <summary>
            /// Raise change events for subscribers
            /// </summary>
            /// <param name="changeResult">The change result with the current sensor data</param>
            protected void RaiseChangedAndNotify(IChangeResult<Voltage> changeResult)
            {
                Updated?.Invoke(this, changeResult);
                Observers.ForEach(x => x.OnNext(changeResult));
            }

            /// <summary>
            /// Subscribe an obersver for update events
            /// </summary>
            public IDisposable Subscribe(IObserver<IChangeResult<Voltage>> observer)
            {
                if (!Observers.Contains(observer))
                {
                    Observers.Add(observer);
                }

                return new Unsubscriber(Observers, observer);
            }

            private class Unsubscriber : IDisposable
            {
                private readonly List<IObserver<IChangeResult<Voltage>>> observers;
                private readonly IObserver<IChangeResult<Voltage>> observer;

                public Unsubscriber(List<IObserver<IChangeResult<Voltage>>> observers, IObserver<IChangeResult<Voltage>> observer)
                {
                    this.observers = observers;
                    this.observer = observer;
                }

                public void Dispose()
                {
                    if (observer != null)
                    {
                        observers?.Remove(observer);
                    }
                }
            }
        }
    }
}