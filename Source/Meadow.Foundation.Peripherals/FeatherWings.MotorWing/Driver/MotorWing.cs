using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System;

namespace Meadow.Foundation.FeatherWings
{
    /// <summary>
    /// Represents Adafruit's Feather DC & Stepper Motor Wing
    /// </summary>
    public class MotorWing
    {
        Pca9685 pca9685;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i2cBus">i2c bus</param>
        /// <param name="address">The address of the i2c Peripheral</param>
        /// <param name="frequency">The PWM frequency for the PCA9685 IC</param>
        public MotorWing(II2cBus i2cBus, byte address = (byte)Pca9685.Addresses.Default, int frequency = 1600)
        {
            if (i2cBus == null)
            {
                throw new ArgumentNullException("i2cBus");
            }

            pca9685 = new Pca9685(i2cBus, address, frequency);
        }

        /// <summary>
        /// Initialize the PCA9685
        /// </summary>
        public void Initialize()
        {
            pca9685.Initialize();
        }

        /// <summary>
        /// Returns a instance of a Stepper Motor object
        /// </summary>
        /// <param name="steps">The number of steps the motor has</param>
        /// <param name="stepperMotorIndex">The stepper motor port we want to use: only 0 or 1 are valid</param>
        /// <returns>StepperMotor</returns>
        public StepperMotor GetStepper(short stepperMotorIndex, int steps)
        {
            if (stepperMotorIndex != 1 && stepperMotorIndex != 2)
            {
                throw new ArgumentException("Stepper num must be 1 or 2");
            }

            stepperMotorIndex--;

            return new StepperMotor(steps, stepperMotorIndex, pca9685);
        }

        /// <summary>
        /// Returns a instance of a DC Motor object
        /// </summary>
        /// <param name="dcMotorIndex">The motor port we want to use: 1 thru 4 are valid</param>
        /// <returns>DCMotor</returns>
        public DCMotor GetMotor(short dcMotorIndex)
        {
            if ((dcMotorIndex < 1) || (dcMotorIndex > 4))
            {
                throw new ArgumentException("Motor must be between 1 and 4 inclusive");
            }

            dcMotorIndex--;

            return new DCMotor(dcMotorIndex, pca9685);
        }
    }
}