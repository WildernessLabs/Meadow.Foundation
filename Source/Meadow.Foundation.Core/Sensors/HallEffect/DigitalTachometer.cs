using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.HallEffect
{
    /// <summary>
    /// Represents a Lineal Hall Effect tachometer.
    /// </summary>
    public class LinearHallEffectTachometer : IDisposable
    {
        /// <summary>
        /// Input port for the tachometer
        /// </summary>
        protected IDigitalInputPort InputPort { get; set; }

        /// <summary>
        /// Revolutions pers minute 
        /// </summary>
        protected float Rpms = 0.0f;

        /// <summary>
        /// Last notified RPM value
        /// </summary>
        protected float LastNotifiedRPMs = 0.0f;

        /// <summary>
        /// Revolution start time
        /// </summary>
        protected DateTime RevolutionTimeStart = DateTime.MinValue;

        /// <summary>
        /// Number of reads
        /// </summary>
        protected ushort NumberOfReads = 0;

        /// <summary>
        /// Track if we created the input port in the PushButton instance (true)
        /// or was it passed in via the ctor (false)
        /// </summary>
        protected bool ShouldDisposePort = false;

        /// <summary>
        /// Event raised when the RPM change is greater than the 
        /// RPMChangeNotificationThreshold value.
        /// </summary>
        public event EventHandler<ChangeResult<float>> RPMsChanged = delegate { };

        /// <summary>
        /// Any changes to the RPMs that are greater than the RPM change
        /// threshold will cause an event to be raised when the instance is
        /// set to update automatically.
        /// </summary>
        public float RPMChangeNotificationThreshold { get; set; } = 0.001f;

        /// <summary>
        /// Returns number of magnets of the sensor.
        /// </summary>
        public ushort NumberOfMagnets { get; }

        /// <summary>
        /// Returns number of revolutions per minute.
        /// </summary>
        public int RPMs => (int)Rpms;

        /// <summary>
        /// Is the peripheral disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// LinearHallEffectTachometer driver
        /// </summary>
        /// <param name="device">IDigitalInputController to create digital input port</param>
        /// <param name="inputPin"></param>
        /// <param name="numberOfMagnets"></param>
        /// <param name="rpmChangeNotificationThreshold"></param>
        public LinearHallEffectTachometer(
            IDigitalInputController device, 
            IPin inputPin, 
            ushort numberOfMagnets = 2, 
            float rpmChangeNotificationThreshold = 1.0f)
            : this(
                device.CreateDigitalInputPort(inputPin, InterruptMode.None, ResistorMode.Disabled, TimeSpan.Zero, TimeSpan.Zero), 
                numberOfMagnets, 
                rpmChangeNotificationThreshold)
        { 
            ShouldDisposePort = true;
        }

        /// <summary>
        /// LinearHallEffectTachometer driver
        /// </summary>
        /// <param name="inputPort"></param>
        /// <param name="numberOfMagnets"></param>
        /// <param name="rpmChangeNotificationThreshold"></param>
        public LinearHallEffectTachometer(
            IDigitalInputPort inputPort, 
            ushort numberOfMagnets = 2, 
            float rpmChangeNotificationThreshold = 1.0F)
        {
            NumberOfMagnets = numberOfMagnets;
            RPMChangeNotificationThreshold = rpmChangeNotificationThreshold;

            InputPort = inputPort;

            InputPort.Changed += InputPortChanged;
        }

        void InputPortChanged(object sender, DigitalPortResult e)
        {
            var time = DateTime.Now;

            // if it's the very first read, set the time and bail out
            if (NumberOfReads == 0 && RevolutionTimeStart == DateTime.MinValue)
            {
                //S.Console.WriteLine("First reading.");
                RevolutionTimeStart = time;
                NumberOfReads++;
                return;
            }

            // increment our count of magnet detections
            NumberOfReads++;

            // if we've made a full revolution
            if (NumberOfReads == NumberOfMagnets)
            {
                //S.Console.WriteLine("Viva La Revolucion!");
                // calculate how much time has elapsed since the start of the revolution 
                var revolutionTime = time - RevolutionTimeStart;

                //S.Console.WriteLine("RevTime Milliseconds: " + revolutionTime.Milliseconds.ToString());

                if (revolutionTime.Milliseconds < 3)
                {
                    //S.Console.WriteLine("rev time < 3. Garbage, bailing.");
                    NumberOfReads = 0;
                    RevolutionTimeStart = time;
                    return;
                }

                // calculate our rpms
                // RPSecond = 1000 / revTime.millis
                // PPMinute = RPSecond * 60
                Rpms = ((float)1000 / (float)revolutionTime.Milliseconds) * (float)60;

                //if (revolutionTime.Milliseconds < 5) {
                //    S.Console.WriteLine("revolution time was < 5. garbage results.");
                //} else {
                //    S.Console.WriteLine("RPMs: " + _RPMs);
                //}


                // reset our number of reads and store our revolution time start
                NumberOfReads = 0;
                RevolutionTimeStart = time;

                // if the change is enough, raise the event.
                if (Math.Abs(LastNotifiedRPMs - Rpms) > RPMChangeNotificationThreshold)
                {
                    OnRaiseRPMChanged();
                }
            }
        }

        /// <summary>
        /// Notify when RPMs change
        /// </summary>
        protected void OnRaiseRPMChanged()
        {
            RPMsChanged(this, new ChangeResult<float>(LastNotifiedRPMs, Rpms));
            LastNotifiedRPMs = Rpms;
        }

        /// <summary>
        /// Dispose peripheral
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && ShouldDisposePort)
                {
                    InputPort.Dispose();
                }

                IsDisposed = true;
            }
        }

        /// <summary>
        /// Dispose Peripheral
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}