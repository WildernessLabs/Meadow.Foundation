using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation
{
    public abstract class I2cSensorBase<UNIT> :
        SensorBase<UNIT>, IDisposable where UNIT : struct
    {
        public event EventHandler<IChangeResult<UNIT>> Updated = delegate { };

        private object _lock = new object();
        private CancellationTokenSource? SamplingTokenSource { get; set; }

        /// <summary>
        /// The peripheral's address on the I2C Bus
        /// </summary>
        public byte Address { get => I2cPeripheral.Address; }

        protected II2cPeripheral I2cPeripheral { get; private set; }

        protected abstract Task<UNIT> ReadSensor();
            
        protected virtual void RaiseChangedAndNotify(IChangeResult<UNIT> changeResult)
        {
            Updated.Invoke(this, changeResult);
            NotifyObservers(changeResult);
        }

        /// <summary>
        /// The last read conditions.
        /// </summary>
        public UNIT Conditions { get; protected set; }

        protected I2cSensorBase(II2cBus i2cBus, byte address, int rxBufferSize = 8, int txBufferSize = 8)
        {
            I2cPeripheral = new I2cPeripheral(i2cBus, address, rxBufferSize, txBufferSize);
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

        /// <summary>
        /// Gets a value indicating whether the sensor is currently in a sampling
        /// loop. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

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

        public virtual void StartUpdating()
        {
            StartUpdating(TimeSpan.FromSeconds(1));
        }

        public virtual void StartUpdating(TimeSpan period)
        {
            // thread safety
            lock (_lock)
            {
                if (IsSampling) return;

                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                UNIT oldConditions;
                ChangeResult<UNIT> result;

                Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        // cleanup
                        if (ct.IsCancellationRequested)
                        {
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
                        await Task.Delay(period);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public void StopUpdating()
        {
            lock (_lock)
            {
                if (!IsSampling) return;

                SamplingTokenSource?.Cancel();

                // state muh-cheen
                IsSampling = false;
            }
        }
    }
}