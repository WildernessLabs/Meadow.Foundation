using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class SerialWombatBase
    {
        /// <summary>
        /// Represents a serial wombat PwmPort
        /// </summary>
        public class PwmPort : PwmPortBase
        {
            /// <summary>
            /// The PWM default frequency
            /// </summary>
            public static readonly Frequency DefaultFrequency = new Frequency(100f, Units.Frequency.UnitType.Hertz);

            private SerialWombatBase _controller;
            private Frequency _frequency;
            private bool _running;
            private float _dutyCycle;

            /// <summary>
            /// Create a new PwmPort object
            /// </summary>
            public PwmPort(
                SerialWombatBase controller,
                IPin pin,
                IPwmChannelInfo channel)
                : base(pin, channel, DefaultFrequency)
            {
                Console.WriteLine($"+pwmPort: {channel}");
                _controller = controller;
                Inverted = channel.InverseLogic;
                Frequency = DefaultFrequency;
                DutyCycle = 0.5f;
            }

            /// <summary>
            /// Is the port inverted
            /// </summary>
            public override bool Inverted { get; set; }

            /// <summary>
            /// PWM duty cycle
            /// </summary>
            public override float DutyCycle
            {
                get => _dutyCycle;
                set
                {
                    if (_controller != null)
                    {
                        _dutyCycle = value;
                        _controller.ConfigurePwmDutyCycle((byte)Pin.Key, value);
                    }
                }
            }

            /// <summary>
            /// PWM frequency
            /// </summary>
            public override Frequency Frequency
            {
                get => _frequency;
                set
                {
                    if (_controller != null)
                    {
                        _frequency = value;
                        _controller.ConfigurePwm((byte)Pin.Key, value);
                    }
                }
            }

            /// <summary>
            /// The amount of time, in seconds, that the a PWM pulse is high.  This will always be less than or equal to the Period
            /// </summary>
            public override float Duration
            {
                get => DutyCycle * Period;
                set
                {
                    if (value > Period) throw new ArgumentOutOfRangeException("Duration must be less than Period");
                    // clamp
                    if (value < 0) { value = 0; }

                    DutyCycle = value / Period;
                }
            }

            /// <summary>
            /// PWM period
            /// </summary>
            public override float Period
            {
                get => (float)(1 / _frequency.Hertz);
                set => Frequency = new Frequency(1 / value);
            }

            /// <summary>
            /// Get the port state (is it running)
            /// </summary>
            public override bool State => _running;

            /// <summary>
            /// Start the PWM port
            /// </summary>
            public override void Start()
            {
                if (State) return;

                _controller.ConfigurePwm((byte)Pin.Key, DutyCycle, Inverted);
                _running = true;
            }

            /// <summary>
            /// Stop the PWM port
            /// </summary>
            public override void Stop()
            {
                if (!State) return;

                _controller.ConfigurePwm((byte)Pin.Key, 0, Inverted);
                _running = false;
            }
        }
    }
}