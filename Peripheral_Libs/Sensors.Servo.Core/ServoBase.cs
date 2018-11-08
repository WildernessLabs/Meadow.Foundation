using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Servos
{
    public abstract class ServoBase : IServo
    {
        protected PWM _pwm = null;

        /// <summary>
        /// Gets the ServoConfig that describes this servo.
        /// </summary>
        public ServoConfig Config
        {
            get { return _config; }
        }
        protected ServoConfig _config = null;

        /// <summary>
        /// Returns the current angle. Returns -1 if the angle is unknown.
        /// </summary>
        public int Angle {
            get { return _angle; }
        } protected int _angle = -1;

        /// <summary>
        /// Instantiates a new Servo on the specified PWM Pin with the specified config.
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="config"></param>
        public ServoBase(Cpu.PWMChannel pin, ServoConfig config)
        {
            _config = config;
            _pwm = new PWM(pin, config.Frequency, 0, false);
        }

        /// <summary>
        /// Rotates the servo to a given angle.
        /// </summary>
        /// <param name="angle">The angle to rotate to.</param>
        public void RotateTo(int angle)
        {
            // angle check
            if (angle < _config.MinimumAngle || angle > _config.MaximumAngle) {
                throw new ArgumentOutOfRangeException("Angle must be within servo configuration tolerance.");
            }

            // calculate the appropriate pulse duration for the angle
            double pulseDuration = CalculatePulseDuration(angle);
            //Debug.Print("Pulse Duration: " + pulseDuration.ToString());
            
            // send our pulse to the servo to make it move
            SendCommandPulse(pulseDuration);

            // update the state
            _angle = angle;
        }

        //public void RotateTo(int angle, double speed)
        //{
        //}

        /// <summary>
        /// Stops the signal that controls the servo angle. For many servos, this will 
        /// return the servo to its nuetral position (usually 0º).
        /// </summary>
        public void Stop()
        {
            _pwm.Stop();
            _angle = -1;
        }

        protected double CalculatePulseDuration(int angle)
        {
            // offset + (angle percent * duration length)
            return _config.MinimumPulseDuration + (((double)angle / _config.MaximumAngle) * (_config.MaximumPulseDuration - _config.MinimumPulseDuration));
            // sample calcs:
            // 0 degrees time = 1000 + ( (0 / 180) * 1000 ) = 1,000 microseconds
            // 90 degrees time = 1000 + ( (90 / 180) * 1000 ) = 1,500 microseconds
            // 180 degrees time = 1000 + ( (180 / 180) * 1000 ) = 2,000 microseconds
        }

        protected double CalculateDutyCycle(double pulseDuration)
        {
            // the pulse duration is dependent on the frequency we're driving the servo at
            return pulseDuration / ((1.0f / (float)_config.Frequency) * 1000000f);
        }

        protected void SendCommandPulse(double pulseDuration)
        {
            //Debug.Print("Sending Command Pulse");
            _pwm.DutyCycle = CalculateDutyCycle(pulseDuration);
            _pwm.Start(); // servo expects to run continuously
        }
    }
}
