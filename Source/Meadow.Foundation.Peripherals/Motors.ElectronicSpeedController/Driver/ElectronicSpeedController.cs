﻿using System;
using Meadow.Hardware;

namespace Meadow.Foundation.Motors
{
    /// <summary>
    /// Driver for Electornic Speed Controllers used, typically, to drive
    /// motors via a PWM signal. To use, you generally have to first _calibrate_
    /// the ESC via the following steps:
    /// 1. Depower the ESC, set power to intended max point (e.g. `1.0` power)
    /// 2. Power the ESC, wait for "happy tones" to indicate good power supply
    /// then (possibly) two beeps to indicate max power limit set.
    /// 3. Set the ESC power to intended minimum power point (e.g. `0.0` power)
    /// and the ESC should provide one beep per every LiPo cell (`3.7V`) of
    /// power supplied, and then a long beep.
    /// 4. Optionally, per some ESCs, arm, by calling the `Arm()` method, which
    /// will drop the power below 0.0;
    /// </summary>
    public class ElectronicSpeedController : IDisposable
    {
        /// <summary>
        /// Gets the PwmPort
        /// </summary>
        protected IPwmPort pwmPort { get; set; }

        /// <summary>
        /// Track if we created the input port in the PushButton instance (true)
        /// or was it passed in via the ctor (false)
        /// </summary>
        protected bool ShouldDisposePorts = false;

        /// <summary>
        /// The pulse duration, in milliseconds, neccessary to "arm" the ESC.
        /// Default value is 0.5ms.
        /// </summary>
        public float ArmingPulseDuration { get; set; } = 0.5f;

        /// <summary>
        /// `0.0` -> `1.0`
        /// </summary>
        public float Power {
            get => power;
            set {
                //if (!armed) {
                //    Resolver.Log.Info("not armed.");
                //    return;
                //}
                if(value < 0) { value = 0; }
                if(value > 1) { value = 1; }
                power = value;
                pwmPort.DutyCycle = CalculateDutyCycle(CalculatePulseDuration(value), Frequency);
            }
        } 
        float power = 0f;

        /// <summary>
        /// Frequency (in Hz) of the PWM signal. Default is 50Hz. Set during
        /// construction, and increase for higher quality ESC's that allow a
        /// higher frequency PWM control signal.
        /// </summary>
        public Units.Frequency Frequency => pwmPort.Frequency;

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Initializes an electronic speed controller on the specified device 
        /// pin, at the specified frequency.
        /// </summary>
        /// <param name="device">IIODevice capable of creating a PWM port.</param>
        /// <param name="pwmPin">PWM capable pin.</param>
        /// <param name="frequency">Frequency of the PWM signal. All ESCs should
        /// support 50Hz, but some support much higher. Increase for finer grained
        /// control of speed in time.</param>
        public ElectronicSpeedController(
            IPwmOutputController device, 
            IPin pwmPin, 
            Units.Frequency frequency) 
            : this (device.CreatePwmPort(pwmPin, frequency)) 
        { 
            ShouldDisposePorts = true;
        }

        /// <summary>
        /// Initializes an electronic speed controller on the specified device 
        /// pin, at the specified frequency.
        /// </summary>
        /// <param name="port">The `IPwmPort` that will drive the ESC.</param>
        public ElectronicSpeedController(IPwmPort port)
        {
            pwmPort = port;
            pwmPort.Start();
        }

        /// <summary>
        /// Sends a 0.5ms pulse to the motor to enable throttle control.
        /// </summary>
        public void Arm()
        {
            pwmPort.DutyCycle = CalculateDutyCycle(ArmingPulseDuration, Frequency);
        }

        /// <summary>
        /// Calculates the appropriate duty cycle of a PWM signal for the given
        /// pulse duration and frequency.
        /// </summary>
        /// <param name="pulseDuration">The duration of the target pulse, in milliseconds.</param>
        /// <param name="frequency">The frequency of the PWM.</param>
        /// <returns>A duty cycle value expressed as a percentage between 0.0 and 1.0.</returns>
        protected float CalculateDutyCycle(float pulseDuration, Units.Frequency frequency)
        {
            // the pulse duration is dependent on the frequency we're driving the servo at
            return pulseDuration / ((1.0f / (float)frequency.Hertz) * 1000f);
        }

        /// <summary>
        /// Returns a pulse duration, in milliseconds, for the given power,
        /// assuming that the allowed power band is between 1ms and 2ms.
        /// </summary>
        /// <param name="power">A value between 0.0 and 1.0 representing the percentage
        /// of power, between 0% and 100%, with 0.0 = 0%, and 1.0 = 100%.</param>
        /// <returns>A pulse duration in milliseconds for the given power.</returns>
        protected float CalculatePulseDuration(float power)
        {
            //Resolver.Log.Info($"CalculatePulseDuration(power:{power:n1}) = {power * 1000} + 1000f");
            // power band is between 1ms -> 2ms pulse durations.
            // so 10% power = 1.1ms, 100% power = 2ms.
            return (power) + 1f;
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        /// <param name="disposing">Is disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && ShouldDisposePorts)
                {
                    pwmPort.Dispose();
                }

                IsDisposed = true;
            }
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}