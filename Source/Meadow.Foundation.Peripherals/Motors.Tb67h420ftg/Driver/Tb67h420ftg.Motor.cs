using System;
using Meadow.Hardware;

#nullable enable

namespace Meadow.Foundation.Motors
{
    public partial class Tb67h420ftg
    {
        /// <summary>
        /// Represents a Tb67h420ftg controlled motor
        /// </summary>
        public class Motor : HBridgeMotor
        {
            /// <summary>
            /// Raised when the motors is over current
            /// </summary>
            public event EventHandler<MotorOvercurrentEventArgs> MotorOvercurrentFault = delegate { };

            Tb67h420ftg driver;
            IDigitalInputPort? fault;

            internal Motor(
                Tb67h420ftg driver, IPwmPort in1, IPwmPort in2,
                IDigitalOutputPort enable, IDigitalInputPort? fault)
                : base(in1, in2, enable)
            {
                this.driver = driver;
                this.fault = fault;

                Initialize();
            }

            void Initialize() 
            {   // wire up the fault event, if the LOx port is configured.
                if (fault != null) 
                {
                    fault.Changed += (object sender, DigitalPortResult e) => 
                        RaiseMotorOvercurrentFault();
                }
            }

            /// <summary>
            /// Raises the MotorOvercurrentFault event
            /// </summary>
            protected void RaiseMotorOvercurrentFault()
            {
                MotorOvercurrentEventArgs args = new MotorOvercurrentEventArgs();
                this.MotorOvercurrentFault(this, args);
            }
        }
    }
}