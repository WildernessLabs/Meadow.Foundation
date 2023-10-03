using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Pca9685
    {
        /// <summary>
        /// Pca9685 PWM port
        /// </summary>
        public class PwmPort : IPwmPort
        {
            readonly byte address;
            readonly II2cBus i2cBus;
            readonly byte portNumber;
            readonly byte led0OnL;

            float dutyCycle;
            Units.Frequency frequency;

            /// <summary>
            /// Channel info
            /// </summary>
            public IPwmChannelInfo Channel => throw new NotImplementedException();

            /// <summary>
            /// Durration
            /// </summary>
            public float Duration { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            /// <summary>
            /// Period
            /// </summary>
            public float Period { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            /// <summary>
            /// Gets the overall PWM Frequency set for the PCA9685. Can't be changed per port.
            /// </summary>
            public Units.Frequency Frequency { get => frequency; set { } }

            /// <summary>
            /// State
            /// </summary>
            public bool State => throw new NotImplementedException();

            /// <summary>
            /// Pin
            /// </summary>
            public IPin Pin => throw new NotImplementedException();

            /// <summary>
            /// Duty cycle
            /// </summary>
            public float DutyCycle
            {
                get => dutyCycle;
                set
                {
                    dutyCycle = value;
                    Start();
                }
            }

            /// <summary>
            /// Get or set inversion
            /// </summary>
            public bool Inverted { get; set; }

            /// <summary>
            /// Get or set the time scale
            /// </summary>
            public TimeScale TimeScale { get; set; }

            IDigitalChannelInfo IPort<IDigitalChannelInfo>.Channel => throw new NotImplementedException();

            /// <summary>
            /// Create new PwmPort
            /// </summary>
            /// <param name="i2cBus">I2C bus</param>
            /// <param name="address">I2C address</param>
            /// <param name="led0OnL">Led 0 On</param>
            /// <param name="frequency">PWM frequency</param>
            /// <param name="portNumber">Port number</param>
            /// <param name="dutyCycle">Duty cycle</param>
            public PwmPort(II2cBus i2cBus, byte address, byte led0OnL, Units.Frequency frequency, byte portNumber, float dutyCycle)
            {
                this.i2cBus = i2cBus;
                this.address = address;
                this.dutyCycle = dutyCycle;
                this.portNumber = portNumber;
                this.frequency = frequency;
                this.led0OnL = led0OnL;
            }

            /// <summary>
            /// Start PWM ports
            /// </summary>
            public void Start()
            {
                if (Inverted)
                {
                    SetPwm(portNumber, (int)(dutyCycle * 4096), 0);
                }
                else
                {
                    SetPwm(portNumber, 0, (int)(dutyCycle * 4096));
                }
            }

            /// <summary>
            /// Stop PWM ports
            /// </summary>
            public void Stop()
            {
                SetPwm(portNumber, 0, 0);
            }

            private void SetPwm(byte port, int on, int off)
            {
                Write((byte)(led0OnL + (4 * port)), (byte)(on & 0xFF), (byte)(on >> 8), (byte)(off & 0xFF), (byte)(off >> 8));
            }

            private void Write(byte register, byte ledXOnL, byte ledXOnH, byte ledXOffL, byte ledXOffH)
            {
                i2cBus.Write(address, new byte[] { register, ledXOnL, ledXOnH, ledXOffL, ledXOffH });
            }

            /// <summary>
            /// Dispose
            /// </summary>
            /// <exception cref="NotImplementedException"></exception>
            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }
    }
}