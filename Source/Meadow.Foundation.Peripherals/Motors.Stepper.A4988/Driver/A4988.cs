using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Linq;
using AU = Meadow.Units.Angle.UnitType;

namespace Meadow.Foundation.Motors.Stepper
{
    /// <summary>
    /// This class is for the A4988 Stepper Motor Driver
    /// </summary>
    public class A4988
    {
        /// <summary>
        /// Gets or sets the angle, in degrees, of one step for the connected stepper motor
        /// </summary>
        public Angle StepAngle
        {
            get => stepAngle;
            set
            {
                if (value <= new Angle(0, AU.Degrees)) { throw new ArgumentOutOfRangeException("Step angle must be positive"); }
                if (value == stepAngle) { return; }
                stepAngle = value;
            }
        }

        /// <summary>
        /// Divisor used to adjust rotational speed of the stepper motor
        /// </summary>
        public int RotationSpeedDivisor
        {
            get => rotationSpeedDivisor;
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("Divisor must be >= 1");
                if (value == RotationSpeedDivisor) return;
                rotationSpeedDivisor = value;
            }
        }
        int rotationSpeedDivisor;

        /// <summary>
        /// Sets or gets the direction of rotation used for Step or Rotate methods
        /// </summary>
        public RotationDirection Direction { get; set; }

        /// <summary>
        /// Divisor for micro-stepping a motor  
        /// This requires the three micro-step control lines to be connected to the motor
        /// </summary>
        public StepDivisor StepDivisor
        {
            get => divisor;
            set
            {
                // micro-steps are either all available or not available, so only check one
                // TODO: should we allow partial (i.e. the user uses full or half steps)?
                if ((ms1Port == null) && (value != StepDivisor.Divisor_1))
                {
                    throw new ArgumentException("No Micro Step Pins were provided");
                }

                lock (syncRoot)
                {
                    switch (value)
                    {
                        case StepDivisor.Divisor_2:
                            ms1Port.State = true;
                            ms2Port.State = ms3Port.State = false;
                            break;
                        case StepDivisor.Divisor_4:
                            ms2Port.State = true;
                            ms1Port.State = ms3Port.State = false;
                            break;
                        case StepDivisor.Divisor_8:
                            ms1Port.State = ms2Port.State = true;
                            ms3Port.State = false;
                            break;
                        case StepDivisor.Divisor_16:
                            ms1Port.State = ms2Port.State = ms3Port.State = true;
                            break;
                        default:
                            ms1Port.State = ms2Port.State = ms3Port.State = false;
                            break;
                    }

                    divisor = value;
                }
            }
        }

        /// <summary>
        /// Gets the number of steps/micro-steps in the current configuration required for one 360-degree revolution
        /// </summary>
        public int StepsPerRevolution
        {
            get
            {
                var v = (int)(360 / stepAngle.Degrees) * (int)StepDivisor;
                return v;
            }
        }

        readonly IDigitalOutputPort stepPort;
        readonly IDigitalOutputPort directionPort;
        readonly IDigitalOutputPort? enablePort;
        readonly IDigitalOutputPort? ms1Port;
        readonly IDigitalOutputPort? ms2Port;
        readonly IDigitalOutputPort? ms3Port;
        readonly object syncRoot = new object();

        StepDivisor divisor;
        Angle stepAngle;

        /// <summary>
        /// Creates an instance of the A4988 Stepper Motor Driver
        /// </summary>
        /// <param name="step">The Meadow pin connected to the STEP pin of the A4988</param>
        /// <param name="direction">The Meadow pin connected to the DIR pin of the A4988</param>
        /// <remarks>You must provide either all of the micro-step (MS) lines or none of them</remarks>
        public A4988(IPin step, IPin direction)
            : this(step, direction, null, null, null, null)
        { }

        /// <summary>
        /// Creates an instance of the A4988 Stepper Motor Driver
        /// </summary>
        /// <param name="step">The Meadow pin connected to the STEP pin of the A4988</param>
        /// <param name="direction">The Meadow pin connected to the DIR pin of the A4988</param>
        /// <param name="ms1Pin">The (optional) Meadow pin connected to the MS1 pin of the A4988</param>
        /// <param name="ms2Pin">The (optional) Meadow pin connected to the MS2 pin of the A4988</param>
        /// <param name="ms3Pin">The (optional) Meadow pin connected to the MS3 pin of the A4988</param>
        /// <remarks>You must provide either all of the micro-step (MS) lines or none of them</remarks>
        public A4988(IPin step, IPin direction, IPin ms1Pin, IPin ms2Pin, IPin ms3Pin)
            : this(step, direction, null, ms1Pin, ms2Pin, ms3Pin)
        { }

        /// <summary>
        /// Creates an instance of the A4988 Stepper Motor Driver
        /// </summary>
        /// <param name="step">The Meadow pin connected to the STEP pin of the A4988</param>
        /// <param name="direction">The Meadow pin connected to the DIR pin of the A4988</param>
        /// <param name="enable">The (optional) Meadow pin connected to the ENABLE pin of the A4988</param>
        public A4988(IPin step, IPin direction, IPin enable)
            : this(step, direction, enable, null, null, null)
        { }

        /// <summary>
        /// Creates an instance of the A4988 Stepper Motor Driver
        /// </summary>
        /// <param name="step">The Meadow pin connected to the STEP pin of the A4988</param>
        /// <param name="direction">The Meadow pin connected to the DIR pin of the A4988</param>
        /// <param name="enablePin">The (optional) Meadow pin connected to the ENABLE pin of the A4988</param>
        /// <param name="ms1Pin">The (optional) Meadow pin connected to the MS1 pin of the A4988</param>
        /// <param name="ms2Pin">The (optional) Meadow pin connected to the MS2 pin of the A4988</param>
        /// <param name="ms3Pin">The (optional) Meadow pin connected to the MS3 pin of the A4988</param>
        /// <remarks>You must provide either all of the micro-step (MS) lines or none of them</remarks>
        public A4988(IPin step, IPin direction, IPin? enablePin, IPin? ms1Pin, IPin? ms2Pin, IPin? ms3Pin)
        {
            stepPort = step.CreateDigitalOutputPort();

            directionPort = direction.CreateDigitalOutputPort();

            if (enablePin != null)
            {
                enablePort = enablePin.CreateDigitalOutputPort();
            }

            // micro-step lines (for now) are all-or-nothing TODO: rethink this?
            if (new IPin?[] { ms1Pin, ms2Pin, ms3Pin }.All(p => p != null))
            {
                ms1Port = ms1Pin?.CreateDigitalOutputPort();
                ms2Port = ms2Pin?.CreateDigitalOutputPort();
                ms3Port = ms3Pin?.CreateDigitalOutputPort();
            }
            else if (new IPin?[] { ms1Pin, ms2Pin, ms3Pin }.All(p => p == null))
            {    // nop
            }
            else
            {
                throw new ArgumentException("All micro-step pins must be either null or valid pins");
            }

            StepAngle = new Angle(1.8, AU.Degrees); // common default
            RotationSpeedDivisor = 2;
        }

        /// <summary>
        /// Rotates the stepper motor a specified number of degrees
        /// </summary>
        /// <param name="degrees">Degrees to rotate</param>
        /// <param name="direction">Direction of rotation</param>
        public void Rotate(float degrees, RotationDirection direction)
        {
            Direction = direction;
            Rotate(degrees);
        }

        /// <summary>
        /// Rotates the stepper motor a specified number of degrees
        /// </summary>
        /// <param name="degrees">Degrees to rotate</param>
        public void Rotate(float degrees)
        {
            // how many steps is it?
            var stepsRequired = (int)(StepsPerRevolution / 360f * degrees);
            Step(stepsRequired);
        }

        /// <summary>
        /// Rotates the stepper motor a specified number of steps (or microsteps)
        /// </summary>
        /// <param name="count">Number of steps to rotate</param>
        /// <param name="direction">Direction of rotation</param>
        public void Step(int count, RotationDirection direction)
        {
            Direction = direction;
            Step(count);
        }

        /// <summary>
        /// Rotates the stepper motor a specified number of steps (or microsteps)
        /// </summary>
        /// <param name="count">Number of steps to rotate</param>
        public void Step(int count)
        {
            lock (syncRoot)
            {
                directionPort.State = Direction == RotationDirection.Clockwise;

                // TODO: add acceleration
                for (int i = 0; i < count; i++)
                {
                    // HACK HACK HACK
                    // We know that each call to set state true == ~210us on Beta 3.10
                    // We could use unmanaged code to tune it better, but we need a <1ms sleep to do it
                    for (var s = 0; s < RotationSpeedDivisor; s++)
                    {
                        stepPort.State = true;
                    }

                    stepPort.State = false;
                }
                // TODO: add deceleration
            }
        }
    }
}