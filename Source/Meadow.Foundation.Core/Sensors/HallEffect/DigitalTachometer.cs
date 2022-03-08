using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.HallEffect
{
    /// <summary>
    /// Represents a Lineal Hall Effect tachometer.
    /// 
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
        public IDigitalInputPort InputPort { get; private set; }

        /// <summary>
        /// Returns number of magnets of the sensor.
        /// </summary>
        public ushort NumberOfMagnets { get; }

        /// <summary>
        /// Returns number of revolutions per minute.
        /// </summary>
        public int RPMs => (int)rpms; 

        /// <summary>
        /// Revolutions pers minute 
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
        /// <param name="device">IDigitalInputController to create digital input port</param>
        /// <param name="inputPin"></param>
        /// <param name="type"></param>
        /// <param name="numberOfMagnets"></param>
        /// <param name="rpmChangeNotificationThreshold"></param>
        public LinearHallEffectTachometer(IDigitalInputController device, IPin inputPin, CircuitTerminationType type = CircuitTerminationType.CommonGround,
            ushort numberOfMagnets = 2, float rpmChangeNotificationThreshold = 1.0F) :
            this(device.CreateDigitalInputPort(inputPin), type, numberOfMagnets, rpmChangeNotificationThreshold)
        {
           
        }

        /// <summary>
        /// LinearHallEffectTachometer driver
        /// </summary>
        /// <param name="inputPort"></param>
        /// <param name="type"></param>
        /// <param name="numberOfMagnets"></param>
        /// <param name="rpmChangeNotificationThreshold"></param>
        public LinearHallEffectTachometer(IDigitalInputPort inputPort, CircuitTerminationType type = CircuitTerminationType.CommonGround,
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

        void InputPortChanged(object sender, DigitalPortResult e)
        {
            var time = DateTime.Now;

            // if it's the very first read, set the time and bail out
            if (numberOfReads == 0 && revolutionTimeStart == DateTime.MinValue)
            {
                //S.Console.WriteLine("First reading.");
                revolutionTimeStart = time;
                numberOfReads++;
                return;
            }

            // increment our count of magnet detections
            numberOfReads++;

            // if we've made a full revolution
            if (numberOfReads == NumberOfMagnets)
            {
                //S.Console.WriteLine("Viva La Revolucion!");
                // calculate how much time has elapsed since the start of the revolution 
                var revolutionTime = time - revolutionTimeStart;

                //S.Console.WriteLine("RevTime Milliseconds: " + revolutionTime.Milliseconds.ToString());

                if (revolutionTime.Milliseconds < 3) {
                    //S.Console.WriteLine("rev time < 3. Garbage, bailing.");
                    numberOfReads = 0;
                    revolutionTimeStart = time;
                    return;
                }

                // calculate our rpms
                // RPSecond = 1000 / revTime.millis
                // PPMinute = RPSecond * 60
                rpms = ((float)1000 / (float)revolutionTime.Milliseconds) * (float)60;

                //if (revolutionTime.Milliseconds < 5) {
                //    S.Console.WriteLine("revolution time was < 5. garbage results.");
                //} else {
                //    S.Console.WriteLine("RPMs: " + _RPMs);
                //}


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