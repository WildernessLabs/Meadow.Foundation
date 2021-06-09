using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation
{
    public abstract class ByteCommsSensorBase<UNIT> :
        SensorBase<UNIT>, IDisposable where UNIT : struct
    {
        //==== events
        public event EventHandler<IChangeResult<UNIT>> Updated = delegate { };

        //==== internals
        private object _lock = new object();
        private CancellationTokenSource? SamplingTokenSource { get; set; }
        protected IByteCommunications Peripheral { get; set; }

        //==== properties
        /// <summary>
        /// The last read conditions.
        /// </summary>
        public UNIT Conditions { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the sensor is currently in a sampling
        /// loop. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        /// <summary>
        /// 
        /// </summary>
        protected Memory<byte> ReadBuffer { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        protected Memory<byte> WriteBuffer { get; private set; }


        //==== ctors
        protected ByteCommsSensorBase(
            II2cBus i2cBus, byte address,
            int readBufferSize = 8, int writeBufferSize = 8)
        {
            Peripheral = new I2cPeripheral(i2cBus, address, readBufferSize, writeBufferSize);
            Init(readBufferSize, writeBufferSize);
        }

        protected ByteCommsSensorBase(
            ISpiBus spiBus, IDigitalOutputPort? chipSelect,
            int readBufferSize = 8, int writeBufferSize = 8,
            ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            Peripheral = new SpiPeripheral(spiBus, chipSelect, readBufferSize, writeBufferSize, csMode);
            Init(readBufferSize, writeBufferSize);
        }

        protected void Init(int readBufferSize = 8, int writeBufferSize = 8)
        {
            this.ReadBuffer = new byte[readBufferSize];
            this.WriteBuffer = new byte[writeBufferSize];
        }

        //==== ISensor Methods //TODO: move these into the ISensorBase after M.Foundation cleanup
        protected abstract Task<UNIT> ReadSensor();

        protected virtual void RaiseChangedAndNotify(IChangeResult<UNIT> changeResult)
        {
            Updated.Invoke(this, changeResult);
            NotifyObservers(changeResult);
        }

        /// <summary>
        /// Convenience method to get the current sensor readings. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        public async Task<UNIT> Read()
        {
            // update confiruation for a one-off read
            this.Conditions = await ReadSensor();
            return Conditions;
        }


        /// <summary>
        /// Starts updating the sensor on an update interval of `5` seconds.
        ///
        /// This method also starts raising `Changed` events and notifying
        /// IObservable subscribers. Use the `updateInterval` parameter
        /// to specify how often events and notifications are raised/sent.
        /// </summary>
        public virtual void StartUpdating()
        {
            StartUpdating(TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Starts updating the sensor on the updateInterval frequency specified.
        ///
        /// This method also starts raising `Changed` events and notifying
        /// IObservable subscribers. Use the `updateInterval` parameter
        /// to specify how often events and notifications are raised/sent.
        /// </summary>
        /// <param name="updateInterval">A `TimeSpan` that specifies how long to
        /// wait between readings. This value influences how often `*Updated`
        /// events are raised and `IObservable` consumers are notified.</param>
        public virtual void StartUpdating(TimeSpan updateInterval)
        {
            // thread safety
            lock (_lock) {
                if (IsSampling) return;

                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                UNIT oldConditions;
                ChangeResult<UNIT> result;

                Task.Factory.StartNew(async () => {
                    while (true) {
                        // cleanup
                        if (ct.IsCancellationRequested) {
                            // do task clean up here
                            observers.ForEach(x => x.OnCompleted());
                            IsSampling = false;
                            break;
                        }
                        // capture history
                        oldConditions = Conditions;

                        // read
                        Conditions = await Read();

                        // build a new result with the old and new conditions
                        result = new ChangeResult<UNIT>(Conditions, oldConditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(updateInterval);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public void StopUpdating()
        {
            lock (_lock) {
                if (!IsSampling) return;

                SamplingTokenSource?.Cancel();

                // state muh-cheen
                IsSampling = false;
            }
        }




        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                StopUpdating();
            }
        }

        /// <summary>
        /// Dispose managed resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}