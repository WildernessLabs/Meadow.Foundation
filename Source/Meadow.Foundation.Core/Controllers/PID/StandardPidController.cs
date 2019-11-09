using System;

namespace Meadow.Foundation.Controllers.Pid
{
    public class StandardPidController : PidControllerBase
    {
        /// <summary>
        /// Integral time in minutes
        /// </summary>
        public override float IntegralComponent { get; set; } = 0;

        /// <summary>
        /// Derivative time in minutes
        /// </summary>
        public override float DerivativeComponent { get; set; } = 0;

        public override float CalculateControlOutput()
        {
            // init vars
            float control = 0.0f;
            var now = DateTime.Now;

            // time delta (how long since last calculation)
            var dt = now - _lastUpdateTime;
            // seconds is better than ticks to bring our calculations into perspective
            //var seconds = (float)(dt.Ticks / 10000 / 1000);
            var seconds = ((float)dt.Ticks / 10000f / 1000f);

            // if no time has passed, don't make any changes.
            if (dt.Ticks <= 0.0) return _lastControlOutputValue;

            // copy vars
            var input = ActualInput;
            var target = TargetInput;

            // calculate the error (how far we are from target)
            var error = target - input;
            //Console.WriteLine("Actual: " + ActualInput.ToString("N1") + ", Error: " + error.ToString("N1"));

            // calculate the integral
            _integral += error * seconds; // add to the integral history
            var integral = (1 / (IntegralComponent * 60)) * _integral; // calcuate the integral action

            // calculate the derivative (rate of change, slop of line) term
            var diff = error - _lastError / seconds;
            var derivative = (DerivativeComponent * 60) * diff;

            // add the appropriate corrections
            control = ProportionalComponent * (error + integral + derivative);

            //
            //Console.WriteLine("PID Control (preclamp): " + control.ToString("N4"));

            // clamp
            if (control > OutputMax) control = OutputMax;
            if (control < OutputMin) control = OutputMin;

            //Console.WriteLine("PID Control (postclamp): " + control.ToString("N4"));

            if (OutputTuningInformation)
            {
                Console.WriteLine("SP+PV+PID+O," + target.ToString() + "," + input.ToString() + "," +
                    ProportionalComponent.ToString() + "," + integral.ToString() + "," +
                    derivative.ToString() + "," + control.ToString());
            }

            // persist our state variables
            _lastControlOutputValue = control;
            _lastError = error;
            _lastUpdateTime = now;

            return control;
        }

    }
}
