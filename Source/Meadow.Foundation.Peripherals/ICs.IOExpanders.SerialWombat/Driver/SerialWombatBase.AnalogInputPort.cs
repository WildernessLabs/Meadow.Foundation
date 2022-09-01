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
        public class AnalogInputPort : AnalogPortBase, IAnalogInputPort
        {
            public event EventHandler<IChangeResult<Voltage>> Updated;

            private SerialWombatBase controller;
            private int supplyVoltage;
            private object _lock = new object();
            protected List<IObserver<IChangeResult<Voltage>>> observers { get; set; } = new List<IObserver<IChangeResult<Voltage>>>();
            private List<Voltage> buffer = new List<Voltage>();

            public bool IsSampling { get; protected set; } = false;
            private CancellationTokenSource? SamplingTokenSource;
            private Voltage? _previousVoltageReading;

            public Voltage Voltage { get; private set; }
            public Voltage ReferenceVoltage { get; private set; }
            public int SampleCount { get; private set; }
            public TimeSpan UpdateInterval { get; private set; } = TimeSpan.FromSeconds(1);

            public IList<Voltage> VoltageSampleBuffer => buffer;
            public TimeSpan SampleInterval => TimeSpan.Zero;

            public AnalogInputPort(SerialWombatBase controller, IPin pin, IAnalogChannelInfo channel, int sampleCount)
                : base(pin, channel)
            {
                SampleCount = sampleCount;

                this.controller = controller;

                supplyVoltage = (int)(this.controller.GetSupplyVoltage().Millivolts);
                ReferenceVoltage = new Voltage(supplyVoltage, Voltage.UnitType.Millivolts);

                controller.ConfigureAnalogInput((byte)pin.Key, (ushort)sampleCount);
            }

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
                                observers.ForEach(x => x.OnCompleted());
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

            public void StopUpdating()
            {
                lock (_lock)
                {
                    if (!IsSampling) return;

                    if (SamplingTokenSource != null)
                    {
                        SamplingTokenSource.Cancel();
                    }

                    IsSampling = false;
                }
            }

            protected void RaiseChangedAndNotify(IChangeResult<Voltage> changeResult)
            {
                Updated?.Invoke(this, changeResult);
                observers.ForEach(x => x.OnNext(changeResult));
            }

            public IDisposable Subscribe(IObserver<IChangeResult<Voltage>> observer)
            {
                if (!observers.Contains(observer)) observers.Add(observer);
                return new Unsubscriber(observers, observer);
            }

            private class Unsubscriber : IDisposable
            {
                private List<IObserver<IChangeResult<Voltage>>> _observers;
                private IObserver<IChangeResult<Voltage>> _observer;

                public Unsubscriber(List<IObserver<IChangeResult<Voltage>>> observers, IObserver<IChangeResult<Voltage>> observer)
                {
                    _observers = observers;
                    _observer = observer;
                }

                public void Dispose()
                {
                    if (!(_observer == null)) _observers.Remove(_observer);
                }
            }
        }
    }
}