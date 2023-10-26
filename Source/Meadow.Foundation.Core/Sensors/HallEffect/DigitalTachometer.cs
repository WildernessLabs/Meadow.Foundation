using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.HallEffect
{
    /// <summary>
    /// Represents a Lineal Hall Effect tachometer.
    /// </summary>
    public class LinearHallEffectTachometer
    {
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
        public float RPMChangeNotificationThreshold { get; set; } = 0.001F;

        /// <summary>
        /// Input port for the tachometer
        /// </summary>
        protected IDigitalInterruptPort InputPort { get; set; }

        /// <summary>
        /// Returns number of magnets of the sensor.
        /// </summary>
        public ushort NumberOfMagnets { get; }

        /// <summary>
        /// Returns number of revolutions per minute.
        /// </summary>
        public int RPMs => (int)rpms;

        /// <summary>
        /// Revolutions per minute 
        /// </summary>
        protected float rpms = 0.0F;
        /// <summary>
        /// Last notified RPM value
        /// </summary>
        protected float lastNotifiedRPMs = 0.0F;
        /// <summary>
        /// Revolution start time
        /// </summary>
        protected DateTime revolutionTimeStart = DateTime.MinValue;
        /// <summary>
        /// Number of reads
        /// </summary>
        protected ushort numberOfReads = 0;

        /// <summary>
        /// LinearHallEffectTachometer driver
        /// </summary>
        /// <param name="inputPin"></param>
        /// <param name="type"></param>
        /// <param name="numberOfMagnets"></param>
        /// <param name="rpmChangeNotificationThreshold"></param>
        public LinearHallEffectTachometer(IPin inputPin, CircuitTerminationType type = CircuitTerminationType.CommonGround,
            ushort numberOfMagnets = 2, float rpmChangeNotificationThreshold = 1.0F) :
            this(inputPin.CreateDigitalInterruptPort(InterruptMode.None, ResistorMode.Disabled, TimeSpan.Zero, TimeSpan.Zero), type, numberOfMagnets, rpmChangeNotificationThreshold)
        { }

        /// <summary>
        /// LinearHallEffectTachometer driver
        /// </summary>
        /// <param name="inputPort"></param>
        /// <param name="type"></param>
        /// <param name="numberOfMagnets"></param>
        /// <param name="rpmChangeNotificationThreshold"></param>
        public LinearHallEffectTachometer(IDigitalInterruptPort inputPort, CircuitTerminationType type = CircuitTerminationType.CommonGround,
            ushort numberOfMagnets = 2, float rpmChangeNotificationThreshold = 1.0F)
        {
            NumberOfMagnets = numberOfMagnets;
            RPMChangeNotificationThreshold = rpmChangeNotificationThreshold;

            // if we terminate in ground, we need to pull the port high to test for circuit completion, otherwise down.
            //var resistorMode = (type == CircuitTerminationType.CommonGround) ? H.Port.ResistorMode.InternalPullUp : H.Port.ResistorMode.InternalPullDown;

            // create the interrupt port from the pin and resistor type
            InputPort = inputPort;

            InputPort.Changed += InputPortChanged;
        }

        private void InputPortChanged(object sender, DigitalPortResult e)
        {
            var time = DateTime.UtcNow;

            // if it's the very first read, set the time and bail out
            if (numberOfReads == 0 && revolutionTimeStart == DateTime.MinValue)
            {
                revolutionTimeStart = time;
                numberOfReads++;
                return;
            }

            // increment our count of magnet detections
            numberOfReads++;

            // if we've made a full revolution
            if (numberOfReads == NumberOfMagnets)
            {
                // calculate how much time has elapsed since the start of the revolution 
                var revolutionTime = time - revolutionTimeStart;

                if (revolutionTime.Milliseconds < 3)
                {
                    numberOfReads = 0;
                    revolutionTimeStart = time;
                    return;
                }

                // calculate our rpms
                rpms = 1000 / (float)revolutionTime.Milliseconds * 60;

                // reset our number of reads and store our revolution time start
                numberOfReads = 0;
                revolutionTimeStart = time;

                // if the change is enough, raise the event.
                if (Math.Abs(lastNotifiedRPMs - rpms) > RPMChangeNotificationThreshold)
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
            RPMsChanged(this, new ChangeResult<float>(lastNotifiedRPMs, rpms));
            lastNotifiedRPMs = rpms;
        }
    }
}