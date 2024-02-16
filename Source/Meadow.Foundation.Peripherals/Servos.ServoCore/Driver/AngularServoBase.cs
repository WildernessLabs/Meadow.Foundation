﻿using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Servos
{
    /// <summary>
    /// Represents an angular servo base class
    /// </summary>
    public abstract class AngularServoBase : ServoBase, IAngularServo
    {
        /// <summary>
        /// The current angle
        /// </summary>
        public Angle? Angle { get; protected set; }

        /// <summary>
        /// Instantiates a new Servo on the specified PWM Pin with the specified config
        /// </summary>
        /// <param name="pwmPort">The PWM port</param>
        /// <param name="config">The servo configuration</param>
        public AngularServoBase(IPwmPort pwmPort, ServoConfig config)
            : base(pwmPort, config)
        { }

        /// <summary>
        /// Rotates the servo to a given angle
        /// </summary>
        /// <param name="angle">The angle to rotate to</param>
        /// <param name="stopAfterMotion">When true the PWM will stop after motion is complete</param>
        public async Task RotateTo(Angle angle, bool stopAfterMotion = false)
        {
            if (!PwmPort.State)
            {
                PwmPort.Start();
            }

            if (angle < Config.MinimumAngle || angle > Config.MaximumAngle)
            {
                throw new ArgumentOutOfRangeException(nameof(angle), "Angle must be within servo configuration tolerance.");
            }

            float pulseDuration = CalculatePulseDuration(angle);

            SendCommandPulse(pulseDuration);

            var rotationRequired = Math.Abs((Angle.HasValue ? Angle.Value.Degrees : 360) - angle.Degrees);
            var delay = (int)(8 * rotationRequired); // estimating 8ms / degree
            await Task.Delay(delay);

            Angle = angle;

            if (stopAfterMotion)
            {
                Stop();
            }
        }

        /// <summary>
        /// Calculate the pulse duration for an angle
        /// </summary>
        /// <param name="angle">The angle</param>
        /// <returns>The pulse duration as as float</returns>
        protected float CalculatePulseDuration(Angle angle)
            => Config.MinimumPulseDuration + (float)(angle.Degrees / Config.MaximumAngle.Degrees * (Config.MaximumPulseDuration - Config.MinimumPulseDuration));
    }
}