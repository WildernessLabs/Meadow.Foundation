using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Generators
{
    /// <summary>
    /// A Pulse Width Modulation Generator that can
    /// generates waveforms in software. The maximum
    /// Frequency is about 100 Hz.
    /// 
    /// Note: This class is not yet implemented.
    /// </summary>
	public class SoftPwmPort : IPwmPort
    {
        protected IDigitalOutputPort Port { get; set; }

        public float Duration { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float Period { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool Inverted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public TimeScale TimeScale { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public float DutyCycle {
            get => dutyCycle;
            set {
                dutyCycle = value;
                onTimeMilliseconds = CalculateOnTimeMillis();
                offTimeMilliseconds = CalculateOffTimeMillis();
                //Console.WriteLine("OnTime: " + _onTimeMilliseconds.ToString() + ", OffTime: " + _offTimeMilliseconds.ToString());
            }
        } protected float dutyCycle;

        public float Frequency {
            get => frequency;
            set {
                if (Frequency <= 0) {
                    throw new Exception("Frequency must be > 0.");
                }
                frequency = value;
                onTimeMilliseconds = CalculateOnTimeMillis();
                offTimeMilliseconds = CalculateOffTimeMillis();
                //Console.WriteLine("OnTime: " + _onTimeMilliseconds.ToString() + ", OffTime: " + _offTimeMilliseconds.ToString());
            }
        }

        public IPwmChannelInfo Channel {get; protected set;}

        public bool State => this.running;

        public IPin Pin => Port.Pin;

        IDigitalChannelInfo IPort<IDigitalChannelInfo>.Channel => throw new NotImplementedException();

        protected float frequency = 1.0f; // in the case it doesn't get set before dutycycle, initialize to 1

        protected Thread th = null;
        protected int onTimeMilliseconds = 0;
        protected int offTimeMilliseconds = 0;
        protected bool running = false;

        ///// <summary>
        ///// Instantiate a SoftPwm object that can perform PWM using digital pins
        ///// </summary>
        ///// <param name="device"></param>
        ///// <param name="outputPin"></param>
        ///// <param name="dutyCycle"></param>
        ///// <param name="frequency"></param>
        //public SoftPwm(IIODevice device, IPin outputPin, float dutyCycle = 0.5f, float frequency = 1.0f) :
        //    this(device.CreateDigitalOutputPort(outputPin, false), dutyCycle, frequency)
        //{
        //}

        /// <summary>
        /// Instantiate a SoftPwm object that can perform PWM using digital pins
        /// </summary>
        /// <param name="outputPort"></param>
        /// <param name="dutyCycle"></param>
        /// <param name="frequency"></param>
        public SoftPwmPort(IDigitalOutputPort outputPort, float dutyCycle = 0.0f, float frequency = 100f)
        {
            Port = outputPort;
            DutyCycle = dutyCycle;
            Frequency = frequency;

            this.Channel = new PwmChannelInfo("SoftPwmChannel", 0, 0, 1000, 1000, false, false);
        }

        /// <summary>
        /// Start the pulse width modulation
        /// </summary>
        public void Start()
        {
            running = true;

            // create a new thread that actually writes the pwm to the output port
            th = new Thread(() => 
            { 
                while (running)
                {
                    Port.State = true;
                    Thread.Sleep(onTimeMilliseconds);
                    Port.State = false;
                    Thread.Sleep(offTimeMilliseconds);
                }
            });
            th.Start();
        }

        /// <summary>
        /// Stop the pulse width modulation
        /// </summary>
        public void Stop()
        {
            // setting this will wrap up the thread
            running = false;

            // need to make sure the port is off, otherwise it can get
            // stuck in an ON state.
            Port.State = false;
        }

        /// <summary>
        /// Calculates the pulse on time in milliseconds
        /// </summary>
        protected int CalculateOnTimeMillis()
        {
            var dc = DutyCycle;
            // clamp
            if (dc < 0) dc = 0;
            if (dc > 1) dc = 1;
            // on time  = 
            return (int)(dc / Frequency * 1000);
        }

        protected int CalculateOffTimeMillis()
        {
            var dc = DutyCycle;
            // clamp
            if (dc < 0) dc = 0;
            if (dc > 1) dc = 1;
            // off time = 
            return (int)(((1 - dc) / Frequency) * 1000);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SoftPwmPort()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}