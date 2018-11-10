using Meadow;
using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Rotary
{
    public class RotaryEncoder : IRotaryEncoder
    {
        public event RotaryTurnedEventHandler Rotated = delegate { };

        public DigitalInputPort APhasePin => _aPhasePin;
        readonly DigitalInputPort _aPhasePin;
        public DigitalInputPort BPhasePin => _bPhasePin;
        readonly DigitalInputPort _bPhasePin;

        // whether or not we're processing the gray code (encoding of rotational information)
        protected bool _processing = false;

        // we need two sets of gray code results to determine direction of rotation
        protected TwoBitGrayCode[] _results = new TwoBitGrayCode[2];

        public RotaryEncoder(Pins aPhasePin, Pins bPhasePin)
        {
            this._aPhasePin = new DigitalInputPort(); //Port: TODO aPhasePin, true, H.Port.ResistorMode.PullUp, H.Port.InterruptMode.InterruptEdgeBoth);
            this._bPhasePin = new DigitalInputPort(); //Port: TODO bPhasePin, true, H.Port.ResistorMode.PullUp, H.Port.InterruptMode.InterruptEdgeBoth);

            // both events go to the same event handler because we need to read both
            // pins to determine current orientation
            this._aPhasePin.Changed += PhasePin_Changed;
            this._bPhasePin.Changed += PhasePin_Changed;
        }

        private void PhasePin_Changed(object sender, PortEventArgs e)
        { 
            //Debug.Print((!_processing ? "1st result: " : "2nd result: ") + "A{" + (this.APhasePin.Read() ? "1" : "0") + "}, " + "B{" + (this.BPhasePin.Read() ? "1" : "0") + "}");

            // the first time through (not processing) store the result in array slot 0.
            // second time through (is processing) store the result in array slot 2.
            _results[_processing ? 1 : 0].APhase = this.APhasePin.Value;
            _results[_processing ? 1 : 0].BPhase = this.BPhasePin.Value;

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