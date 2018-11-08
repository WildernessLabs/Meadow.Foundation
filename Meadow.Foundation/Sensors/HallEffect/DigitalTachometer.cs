using Meadow;
using Meadow.Hardware;
using System;
using System.Diagnostics;

namespace Netduino.Foundation.Sensors.HallEffect
{
    public class LinearHallEffectTachometer
    {

        /// <summary>
        ///     Event raised when the RPM change is greater than the 
        ///     RPMChangeNotificationThreshold value.
        /// </summary>
        public event SensorFloatEventHandler RPMsChanged = delegate { };

        /// <summary>
        ///     Any changes to the RPMs that are greater than the RPM change
        ///     threshold will cause an event to be raised when the instance is
        ///     set to update automatically.
        /// </summary>
        public float RPMChangeNotificationThreshold { get; set; } = 0.001F;
        public DigitalInputPort DigitalIn { get; private set; }

        public ushort NumberOfMagnets => _numberOfMagnets;
        private ushort _numberOfMagnets;

        /// <summary>
        ///     
        /// </summary>
        public int RPMs => (int)_RPMs; 

        protected float _RPMs = 0.0F;
        protected float _lastNotifiedRPMs = 0.0F;
        protected DateTime _revolutionTimeStart = DateTime.MinValue;
        protected ushort _numberOfReads = 0; //


        public LinearHallEffectTachometer(Pins inputPin, CircuitTerminationType type = CircuitTerminationType.CommonGround,
            ushort numberOfMagnets = 2, float rpmChangeNotificationThreshold = 1.0F)
        {
            _numberOfMagnets = numberOfMagnets;
            RPMChangeNotificationThreshold = rpmChangeNotificationThreshold;

            // if we terminate in ground, we need to pull the port high to test for circuit completion, otherwise down.
            //var resistorMode = (type == CircuitTerminationType.CommonGround) ? H.Port.ResistorMode.PullUp : H.Port.ResistorMode.PullDown;

            // create the interrupt port from the pin and resistor type
            DigitalIn = new DigitalInputPort(); //Port: TODO (inputPin, true, H.Port.ResistorMode.PullDown, H.Port.InterruptMode.InterruptEdgeHigh);

            // wire up the interrupt handler
            DigitalIn.Changed += DigitalIn_Changed;
        }

        private void DigitalIn_Changed(object sender, PortEventArgs e)
       // protected void DigitalIn_OnInterrupt(uint port, uint state, DateTime time)
        {
            var time = DateTime.Now;

            // if it's the very first read, set the time and bail out
            if (_numberOfReads == 0 && _revolutionTimeStart == DateTime.MinValue)
            {
                //S.Debug.Print("First reading.");
                _revolutionTimeStart = time;
                _numberOfReads++;
                return;
            }

            // increment our count of magnet detections
            _numberOfReads++;

            // if we've made a full revolution
            if (_numberOfReads == _numberOfMagnets)
            {
                //S.Debug.Print("Viva La Revolucion!");
                // calculate how much time has elapsed since the start of the revolution 
                var revolutionTime = time - _revolutionTimeStart;

                //S.Debug.Print("RevTime Milliseconds: " + revolutionTime.Milliseconds.ToString());

                if (revolutionTime.Milliseconds < 3) {
                    //S.Debug.Print("rev time < 3. Garbage, bailing.");
                    _numberOfReads = 0;
                    _revolutionTimeStart = time;
                    return;
                }

                // calculate our rpms
                // RPSecond = 1000 / revTime.millis
                // PPMinute = RPSecond * 60
                _RPMs = ((float)1000 / (float)revolutionTime.Milliseconds) * (float)60;
                Debug.Print("RPMs: " + _RPMs);

                //if (revolutionTime.Milliseconds < 5) {
                //    S.Debug.Print("revolution time was < 5. garbage results.");
                //} else {
                //    S.Debug.Print("RPMs: " + _RPMs);
                //}


                // reset our number of reads and store our revolution time start
                _numberOfReads = 0;
                _revolutionTimeStart = time;

                // if the change is enough, raise the event.
                if (Math.Abs(_lastNotifiedRPMs - _RPMs) > RPMChangeNotificationThreshold)
                {
                    OnRaiseRPMChanged();
                }
            }
        }

        protected void OnRaiseRPMChanged()
        {
            RPMsChanged(this, new SensorFloatEventArgs(_lastNotifiedRPMs, _RPMs));
            _lastNotifiedRPMs = _RPMs;
        }
    }
}