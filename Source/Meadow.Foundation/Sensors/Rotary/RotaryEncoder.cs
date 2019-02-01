using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Rotary;

namespace Meadow.Foundation.Sensors.Rotary
{
    /// <summary>
    /// Digital rotary encoder that uses two-bit Gray Code to encode rotation.
    ///     
    /// Note: This class is not yet implemented.
    /// </summary>
    public class RotaryEncoder : IRotaryEncoder
    {
        /// <summary>
        /// Returns the pin connected to the A-phase output on the rotary encoder.
        /// </summary>
        public DigitalInputPort APhasePin => _aPhasePin;
        readonly DigitalInputPort _aPhasePin;

        /// <summary>
        /// Returns the pin connected to the B-phase output on the rotary encoder.
        /// </summary>
        public DigitalInputPort BPhasePin => _bPhasePin;
        readonly DigitalInputPort _bPhasePin;

        /// <summary>
        /// Raised when the rotary encoder is rotated and returns a RotaryTurnedEventArgs object which describes the direction of rotation.
        /// </summary>
        public event RotaryTurnedEventHandler Rotated = delegate { };

        // whether or not we're processing the gray code (encoding of rotational information)
        protected bool _processing = false;

        // we need two sets of gray code results to determine direction of rotation
        protected TwoBitGrayCode[] _results = new TwoBitGrayCode[2];

        /// <summary>
        /// Instantiates a new RotaryEncoder on the specified pins.
        /// </summary>
        /// <param name="aPhasePin"></param>
        /// <param name="bPhasePin"></param>
        public RotaryEncoder(IDigitalPin aPhasePin, IDigitalPin bPhasePin)
        {
            //ToDo
            _aPhasePin = new DigitalInputPort(aPhasePin, true, ResistorMode.PullUp);
            _bPhasePin = new DigitalInputPort(bPhasePin, true, ResistorMode.PullUp);

            // both events go to the same event handler because we need to read both
            // pins to determine current orientation
            _aPhasePin.Changed += PhasePinChanged;
            _bPhasePin.Changed += PhasePinChanged;
        }

        private void PhasePinChanged(object sender, PortEventArgs e)
        { 
            //Debug.Print((!_processing ? "1st result: " : "2nd result: ") + "A{" + (APhasePin.Read() ? "1" : "0") + "}, " + "B{" + (BPhasePin.Read() ? "1" : "0") + "}");

            // the first time through (not processing) store the result in array slot 0.
            // second time through (is processing) store the result in array slot 2.
            _results[_processing ? 1 : 0].APhase = APhasePin.State;
            _results[_processing ? 1 : 0].BPhase = BPhasePin.State;

            // if this is the second result that we're reading, we should now have 
            // enough information to know which way it's turning, so process the
            // gray code
            if (_processing)
            {
                ProcessRotationResults();
            }

            // toggle our processing flag
            _processing = !_processing;
        }

        protected void ProcessRotationResults()
        {
            // if there hasn't been any change, then it's a garbage reading. so toss it out.
            if (_results[0].APhase == _results[1].APhase && 
                _results[0].BPhase == _results[1].BPhase) 
                //Debug.Print("Garbage");
                return;
            
            // start by reading the a phase pin. if it's High
            if (_results[0].APhase)
            {
                // if b phase went down, then it spun counter-clockwise
                OnRaiseRotationEvent(_results[1].BPhase ? RotationDirection.CounterClockwise : RotationDirection.Clockwise);
            }
            // if a phase is low
            else
            {
                // if b phase went up, then it spun counter-clockwise
                OnRaiseRotationEvent(_results[1].BPhase ? RotationDirection.CounterClockwise : RotationDirection.Clockwise);
            }
        }


        protected void OnRaiseRotationEvent(RotationDirection direction)
        {
            Rotated?.Invoke(this, new RotaryTurnedEventArgs(direction));
        }
    }
}