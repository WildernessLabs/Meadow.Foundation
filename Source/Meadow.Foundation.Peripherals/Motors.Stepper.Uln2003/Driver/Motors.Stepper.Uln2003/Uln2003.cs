using System;
using Meadow.Hardware;

namespace Meadow.Foudnation.Motors.Stepper
{
    /// <summary>
    /// This class is for controlling stepper motors that are controlled by a 4 pin controller board.
    /// </summary>
    /// <remarks>It is tested and developed using the 28BYJ-48 stepper motor and the ULN2003 driver board.</remarks>
    public class Uln2003
    {
        #region Properties

        /// <summary>
        /// Sets the motor speed to revolutions per minute.
        /// </summary>
        /// <remarks>Default revolutions per minute for 28BYJ-48 is approximately 15.</remarks>
        public short RPM { get; set; } = 15;

        /// <summary>
        /// Sets the stepper's mode.
        /// </summary>
        public StepperMode Mode
        {
            get => mode;
            set
            {
                mode = value;

                currentSwitchingSequence = mode switch
                {
                    StepperMode.FullStepSinglePhase => fullStepSinglePhaseSequence,
                    StepperMode.FullStepDualPhase => fullStepDualPhaseSequence,
                    _ => halfStepSequence
                };
            }
        }

        #endregion Properties

        #region Enums

        /// <summary>
        /// The 28BYJ-48 motor has 512 full engine rotations to rotate the drive shaft once.
        /// In half-step mode these are 8 x 512 = 4096 steps for a full rotation.
        /// In full-step mode these are 4 x 512 = 2048 steps for a full rotation.
        /// </summary>
        public enum StepperMode
        {
            /// <summary>Half step mode</summary>
            HalfStep,
            /// <summary>Full step mode (single phase)</summary>
            FullStepSinglePhase,
            /// <summary>Full step mode (dual phase)</summary>
            FullStepDualPhase
        }

        #endregion Enums

        #region Member variables / fields

        /// <summary>
        /// Default delay in microseconds.
        /// </summary>
        private const long StepperMotorDefaultDelay = 1000;

        private int steps;
        private int engineStep;
        private int currentStep;

        private StepperMode mode = StepperMode.HalfStep;

        private bool[,] currentSwitchingSequence = halfStepSequence;

        private static readonly bool[,] halfStepSequence = {
                 { true, true, false, false, false, false, false, true },
                 { false, true, true, true, false, false, false, false },
                 { false, false, false, true, true, true, false, false },
                 { false, false, false, false, false, true, true, true }
            };

        private static readonly bool[,] fullStepSinglePhaseSequence = {
                 { true, false, false, false, true, false, false, false },
                 { false, true, false, false, false, true, false, false },
                 { false, false, true, false, false, false, true, false },
                 { false, false, false, true, false, false, false, true }
            };

        private static readonly bool[,] fullStepDualPhaseSequence = {
                 { true, false, false, true, true, false, false, true },
                 { true, true, false, false, true, true, false, false },
                 { false, true, true, false, false, true, true, false },
                 { false, false, true, true, false, false, true, true }
            };

        private readonly IDigitalOutputPort outputPort1;
        private readonly IDigitalOutputPort outputPort2;
        private readonly IDigitalOutputPort outputPort3;
        private readonly IDigitalOutputPort outputPort4;

        DateTime startTime;

        #endregion Member variables / fields

        #region Contructors

        /// <summary>
        /// Initialize a Uln2003 class.
        /// </summary>
        /// <param name="pin1">The GPIO pin number which corresponds pin A on ULN2003 driver board.</param>
        /// <param name="pin2">The GPIO pin number which corresponds pin B on ULN2003 driver board.</param>
        /// <param name="pin3">The GPIO pin number which corresponds pin C on ULN2003 driver board.</param>
        /// <param name="pin4">The GPIO pin number which corresponds pin D on ULN2003 driver board.</param>
        public Uln2003(IIODevice device, IPin pin1, IPin pin2, IPin pin3, IPin pin4)
        {
            outputPort1 = device.CreateDigitalOutputPort(pin1);
            outputPort2 = device.CreateDigitalOutputPort(pin2);
            outputPort3 = device.CreateDigitalOutputPort(pin3);
            outputPort4 = device.CreateDigitalOutputPort(pin4);
        }

        #endregion Constructors

        #region Methods

        
        /// <summary>
        /// Stop the motor.
        /// </summary>
        public void Stop()
        {
            steps = 0;

            outputPort1.State = false;
            outputPort2.State = false;
            outputPort3.State = false;
            outputPort4.State = false;
        }

        /// <summary>
        /// Moves the motor. If the number is negative, the motor moves in the reverse direction.
        /// </summary>
        /// <param name="steps">Number of steps.</param>
        public void Step(int steps)
        {
            double lastStepTime = 0;

            startTime = DateTime.Now;

            var isClockwise = steps >= 0;
            this.steps = Math.Abs(steps);

            var stepMicrosecondsDelay = RPM > 0 ? 60 * 1000 * 1000 / this.steps / RPM : StepperMotorDefaultDelay;

            currentStep = 0;

            while (currentStep < this.steps)
            {
                double elapsedMicroseconds = (DateTime.Now - startTime).TotalMilliseconds * 1000;

                if (elapsedMicroseconds - lastStepTime >= stepMicrosecondsDelay)
                {
                    lastStepTime = elapsedMicroseconds;

                    if (isClockwise)
                    {
                        engineStep = engineStep - 1 < 1 ? 8 : engineStep - 1;
                    }
                    else
                    {
                        engineStep = engineStep + 1 > 8 ? 1 : engineStep + 1;
                    }

                    ApplyEngineStep();
                    currentStep++;
                }
            }
        }

        private void ApplyEngineStep()
        {
            outputPort1.State = currentSwitchingSequence[0, engineStep - 1];
            outputPort2.State = currentSwitchingSequence[1, engineStep - 1];
            outputPort3.State = currentSwitchingSequence[2, engineStep - 1];
            outputPort4.State = currentSwitchingSequence[3, engineStep - 1];
        }

        #endregion Methods
    }
}