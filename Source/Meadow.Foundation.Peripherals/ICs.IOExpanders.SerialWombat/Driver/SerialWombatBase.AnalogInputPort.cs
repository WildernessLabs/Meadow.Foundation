using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class SerialWombatBase
    {
        /// <summary>
        /// Represents a serial wombat analog input port
        /// </summary>
        public class AnalogInputPort : AnalogPortBase, IAnalogInputPort
        {
            /// <summary>
            /// Raised when the port voltage value changes
            /// </summary>
            public event EventHandler<IChangeResult<Voltage>> Updated = (s, e) => { };

            /// <summary>
            /// Collection of event Observers for the Updated event
            /// </summary>
            protected List<IObserver<IChangeResult<Voltage>>> Observers { get; set; } = new List<IObserver<IChangeResult<Voltage>>>();

            private readonly SerialWombatBase controller;
            private readonly int supplyVoltage;
            private readonly object _lock = new object();

            private readonly List<Voltage> buffer = new List<Voltage>();

            /// <summary>
            /// Is the port sampling
            /// </summary>
            public bool IsSampling { get; protected set; } = false;
            private CancellationTokenSource? SamplingTokenSource;
            private Voltage? _previousVoltageReading;

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
            public IList<Voltage> VoltageSampleBuffer => buffer;

            /// <summary>
            /// The sampling interval
            /// </summary>
            public TimeSpan SampleInterval => TimeSpan.Zero;

            /// <summary>
            /// Create a new AnalogInputPort object
            /// </summary>
            public AnalogInputPort(SerialWombatBase controller, IPin pin, IAnalogChannelInfo channel, int sampleCount)
                : base(pin, channel)
            {
                SampleCount = sampleCount;

                this.controller = controller;

                supplyVoltage = (int)(controller.GetSupplyVoltage().Millivolts);
                ReferenceVoltage = new Voltage(supplyVoltage, Voltage.UnitType.Millivolts);

                controller.ConfigureAnalogInput((byte)pin.Key, (ushort)sampleCount);
            }

            /// <summary>
            /// Take a reading
            /// </summary>
            public Task<Voltage> Read()
            {
                var data = controller.ReadPublicData((byte)Pin.Key);
                Voltage = new Voltage((data * supplyVoltage) >> 16, Voltage.UnitType.Millivolts);

                if (buffer.Count == 0)
                {
                    buffer.Add(Voltage);
                }
                else
                {
                    buffer[0] = Voltage;
                }

                return Task.FromResult(Voltage);
            }

            /// <summary>
            /// Start updating
            /// </summary>
            public void StartUpdating(TimeSpan? updateInterval = null)
            {
                // the wombat does sampling internally
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
                if (!Observers.Contains(observer)) Observers.Add(observer);
                return new Unsubscriber(Observers, observer);
            }

            private class Unsubscriber : IDisposable
            {
                private readonly List<IObserver<IChangeResult<Voltage>>> _observers;
                private readonly IObserver<IChangeResult<Voltage>> _observer;

                public Unsubscriber(List<IObserver<IChangeResult<Voltage>>> observers, IObserver<IChangeResult<Voltage>> observer)
                {
                    _observers = observers;
                    _observer = observer;
                }

                public void Dispose()
                {
                    if (_observer != null)
                    {
                        _observers?.Remove(_observer);
                    }
                }
            }
        }
    }
}