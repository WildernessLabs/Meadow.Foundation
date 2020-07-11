using Meadow.Foundation.ICs.IOExpanders;
using System;
using System.Threading;

namespace Meadow.Foundation.FeatherWings
{
    public enum Style
    {
        SINGLE = 1,
        DOUBLE = 2,
        INTERLEAVE = 3,
        MICROSTEP = 4
    }

    public enum Direction
    {
        FORWARD,
        BACKWARD
    }

    public class StepperMotor : Motor
    {
        int _currentstep;
        double _rpmDelay;
        readonly int _motorSteps;
        readonly byte _pwmA;
        readonly byte _AIN2;
        readonly byte _AIN1;
        readonly byte _pwmB;
        readonly byte _BIN2;
        readonly byte _BIN1;

        const short MICROSTEPS = 8;
        readonly byte[] _microStepCurve = { 0, 50, 98, 142, 180, 212, 236, 250, 255 };
        //private readonly int[] microstepcurve = {0,   25,  50,  74,  98,  120, 141, 162, 180,197, 212, 225, 236, 244, 250, 253, 255}//MICROSTEPS == 16

        /// <summary>
        /// Creates a Stepper motor objet un-initialized 
        /// </summary>
        /// <param name="steps">The number of steps per revolution</param>
        /// <param name="num">The Stepper motor port</param>
        /// <param name="pca9685">The PCS968 diver object</param>
        public StepperMotor(int steps, int num, Pca9685 pca9685) : base(pca9685)
        {

            if (num == 0) 
            {
                _pwmA = 8;
                _AIN2 = 9;
                _AIN1 = 10;
                _pwmB = 13;
                _BIN2 = 12;
                _BIN1 = 11;
            }
            else if (num == 1)
            {
                _pwmA = 2;
                _AIN2 = 3;
                _AIN1 = 4;
                _pwmB = 7;
                _BIN2 = 6;
                _BIN1 = 5;
            }
            else 
            {
                throw new ArgumentException("Stepper num must be 0 or 1");
            }

            _motorSteps = steps;
            SetSpeed(15);
            _currentstep = 0;
        }

        /// <summary>
        /// Set the delay for the Stepper Motor speed in RPM
        /// </summary>
        /// <param name="rpm">The desired RPM</param>
        public override void SetSpeed(short rpm)
        {
            _rpmDelay = 60000.0 / (_motorSteps * rpm);
        }

        /// <summary>
        /// Move the stepper with the given RPM
        /// </summary>
        /// <param name="steps">The number of steps to move. Negative number moves the stepper backwards</param>
        /// <param name="style">How to perform the step</param>
        public virtual void Step(int steps = 1, Style style = Style.SINGLE)
        {
            if(steps > 0)
            {
                Step(steps, Direction.FORWARD, style);
            }
            else
            {
                Step(Math.Abs(steps), Direction.BACKWARD, style);
            }
        }

        /// <summary>
        /// Move the stepper with the given RPM
        /// </summary>
        /// <param name="steps">The number of steps to move</param>
        /// <param name="direction">The direction to go</param>
        /// <param name="style">How to perform the step</param>
        protected virtual void Step(int steps, Direction direction, Style style)
        {
            int delay = (int)_rpmDelay;
            if (style == Style.INTERLEAVE)
            {
                delay /= 2;
            }
            else if (style == Style.MICROSTEP)
            {
                delay /= MICROSTEPS;
                steps *= MICROSTEPS;
            }

            while(steps >= 0)
            {
                Step(direction, style);
                Thread.Sleep(delay);
                steps--;
            }
        }

        /// <summary>
        /// Move the stepper one step only
        /// </summary>
        /// <param name="direction">The direction to go</param>
        /// <param name="style">How to perform the step</param>
        /// <returns>The current location</returns>
        protected virtual int Step(Direction direction, Style style)
        {
            int ocrb, ocra;
            ocra = ocrb = 255;

            if (style == Style.SINGLE)
            {
                if ((_currentstep / (MICROSTEPS / 2)) % 2 != 0) // we're at an odd step, weird
                {
                    if (direction == Direction.FORWARD)
                    {
                        _currentstep += MICROSTEPS / 2;
                    }
                    else
                    {
                        _currentstep -= MICROSTEPS / 2;
                    }
                }
                else
                { // go to the next even step
                    if (direction == Direction.FORWARD)
                    {
                        _currentstep += MICROSTEPS;
                    }
                    else
                    {
                        _currentstep -= MICROSTEPS;
                    }
                }
            }
            else if (style == Style.DOUBLE)
            {
                if (((_currentstep / (MICROSTEPS / 2) % 2)) != 0)
                { // we're at an even step, weird
                    if (direction == Direction.FORWARD)
                    {
                        _currentstep += MICROSTEPS / 2;
                    }
                    else
                    {
                        _currentstep -= MICROSTEPS / 2;
                    }
                }
                else
                { // go to the next odd step
                    if (direction == Direction.FORWARD)
                    {
                        _currentstep += MICROSTEPS;
                    }
                    else
                    {
                        _currentstep -= MICROSTEPS;
                    }
                }
            }
            else if (style == Style.INTERLEAVE)
            {
                if (direction == Direction.FORWARD)
                {
                    _currentstep += MICROSTEPS / 2;
                }
                else
                {
                    _currentstep -= MICROSTEPS / 2;
                }
            }
            else if (style == Style.MICROSTEP)
            {
                if (direction == Direction.FORWARD)
                {
                    _currentstep++;
                }
                else
                {
                    // BACKWARDS
                    _currentstep--;
                }


                _currentstep += MICROSTEPS * 4;
                _currentstep %= MICROSTEPS * 4;
                ocra = ocrb = 0;

                if ((_currentstep >= 0) && (_currentstep < MICROSTEPS))
                {
                    ocra = _microStepCurve[MICROSTEPS - _currentstep];
                    ocrb = _microStepCurve[_currentstep];
                }
                else if ((_currentstep >= MICROSTEPS) && (_currentstep < MICROSTEPS * 2))
                {
                    ocra = _microStepCurve[_currentstep - MICROSTEPS];
                    ocrb = _microStepCurve[MICROSTEPS * 2 - _currentstep];
                }
                else if ((_currentstep >= MICROSTEPS * 2) &&
                         (_currentstep < MICROSTEPS * 3))
                {
                    ocra = _microStepCurve[MICROSTEPS * 3 - _currentstep];
                    ocrb = _microStepCurve[_currentstep - MICROSTEPS * 2];
                }
                else if ((_currentstep >= MICROSTEPS * 3) &&
                         (_currentstep < MICROSTEPS * 4))
                {
                    ocra = _microStepCurve[_currentstep - MICROSTEPS * 3];
                    ocrb = _microStepCurve[MICROSTEPS * 4 - _currentstep];
                }
            }

            _currentstep += MICROSTEPS * 4;
            _currentstep %= MICROSTEPS * 4;

            _pca9685.SetPwm(_pwmA, 0, ocra * 16);
            _pca9685.SetPwm(_pwmB, 0, ocrb * 16);

            // release all
            int latch_state = 0; // all motor pins to 0

            // Serial.println(step, DEC);
            if (style == Style.MICROSTEP)
            {
                if ((_currentstep >= 0) && (_currentstep < MICROSTEPS))
                    latch_state |= 0x03;
                if ((_currentstep >= MICROSTEPS) && (_currentstep < MICROSTEPS * 2))
                    latch_state |= 0x06;
                if ((_currentstep >= MICROSTEPS * 2) && (_currentstep < MICROSTEPS * 3))
                    latch_state |= 0x0C;
                if ((_currentstep >= MICROSTEPS * 3) && (_currentstep < MICROSTEPS * 4))
                    latch_state |= 0x09;
            }
            else
            {
                switch (_currentstep / (MICROSTEPS / 2))
                {
                    case 0:
                        latch_state |= 0x1; // energize coil 1 only
                        break;
                    case 1:
                        latch_state |= 0x3; // energize coil 1+2
                        break;
                    case 2:
                        latch_state |= 0x2; // energize coil 2 only
                        break;
                    case 3:
                        latch_state |= 0x6; // energize coil 2+3
                        break;
                    case 4:
                        latch_state |= 0x4; // energize coil 3 only
                        break;
                    case 5:
                        latch_state |= 0xC; // energize coil 3+4
                        break;
                    case 6:
                        latch_state |= 0x8; // energize coil 4 only
                        break;
                    case 7:
                        latch_state |= 0x9; // energize coil 1+4
                        break;
                }
            }

            if ((latch_state & 0x1) == 0x1)
            {
                _pca9685.SetPin(_AIN2, true);
            }
            else
            {
                _pca9685.SetPin(_AIN2, false);
            }

            if ((latch_state & 0x2)== 0x2)
            {
                _pca9685.SetPin(_BIN1, true);
            }
            else
            {
                _pca9685.SetPin(_BIN1, false);
            }

            if ((latch_state & 0x4)== 0x4)
            {
                _pca9685.SetPin(_AIN1, true);
            }
            else
            {
                _pca9685.SetPin(_AIN1, false);
            }

            if ((latch_state & 0x8)== 0x8)
            {
                _pca9685.SetPin(_BIN2, true);
            }
            else
            {
                _pca9685.SetPin(_BIN2, false);
            }

            return _currentstep;

        }
    }
}
