﻿using Meadow.Peripherals.Controllers.PID;
using System;

namespace Meadow.Foundation.Controllers.Pid
{
    /// <summary>
    /// Represents a PID controller
    /// </summary>
    public abstract class PidControllerBase : IPidController
    {
        // state vars
        /// <summary>
        /// Last update time
        /// </summary>
        protected DateTime _lastUpdateTime;

        /// <summary>
        /// Last error value
        /// </summary>
        protected float _lastError = 0.0f;

        /// <summary>
        /// Integral
        /// </summary>
        protected float _integral = 0.0f;

        /// <summary>
        /// Last control output value
        /// </summary>
        protected float _lastControlOutputValue = 0.0f;

        /// <summary>
        /// Represents the ProcessVariable (PV), or the actual signal
        /// reading of the system in its current state. For example, 
        /// when heating a cup of coffee to 75°, if the temp sensor
        /// says the coffee is currently at 40°, the 40� is the 
        /// actual input value.
        /// </summary>
        public float ActualInput { get; set; }
        /// <summary>
        /// Represents the SetPoint (SP), or the reference target signal
        /// to achieve. For example, when heating a cup of coffee to 
        /// 75°, 75° is the target input value.
        /// </summary>
        public float TargetInput { get; set; }

        /// <summary>
        /// Output minimum value
        /// </summary>
        public float OutputMin { get; set; } = -1;

        /// <summary>
        /// Output maximum value
        /// </summary>
        public float OutputMax { get; set; } = 1;

        /// <summary>
        /// Proportional gain
        /// </summary>
        public virtual float ProportionalComponent { get; set; } = 1;
        /// <summary>
        /// Integral gain
        /// </summary>
        public virtual float IntegralComponent { get; set; } = 0;
        /// <summary>
        /// Derivative gain
        /// </summary>
        public virtual float DerivativeComponent { get; set; } = 0;
        /// <summary>
        /// Whether or not to print the calculation information to the
        /// output console in an comma-delimited form. Useful for 
        /// pasting into a spreadsheet to graph the system control 
        /// performance when tuning the PID controller corrective
        /// action gains.
        /// </summary>
        public bool OutputTuningInformation { get; set; } = false;

        /// <summary>
        /// Create a new PID controller
        /// </summary>
        public PidControllerBase()
        {
            _lastUpdateTime = DateTime.UtcNow;
            _lastError = 0;
            _integral = 0;
        }

        /// <summary>
        /// Reset integral
        /// </summary>
        public void ResetIntegrator()
        {
            _integral = 0;
        }

        /// <summary>
        /// Calculates the control output based on the Target and Actual, using the current PID values
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract float CalculateControlOutput();
    }
}