using System;
using Meadow.Hardware;
using Meadow.Peripherals.Motors;

namespace Meadow.Foundation.Motors
{
    public partial class Tb67h420ftg
    {
        public class Motor : HBridgeMotor
        {
            /// <summary>
            /// Raised when the motors is over current.
            /// </summary>
            public event EventHandler<MotorOvercurrentEventArgs> MotorOvercurrentFault = delegate { };

            protected Tb67h420ftg driver;
            protected IDigitalInputPort? fault;

            internal Motor(
                Tb67h420ftg driver, IPwmPort in1, IPwmPort in2,
                IDigitalOutputPort enable, IDigitalInputPort? fault)
                : base(in1, in2, enable)
            {
                this.driver = driver;
                this.fault = fault;

                Init();
            }

            protected void Init() {
                // wire up the fault event, if the LOx port is configured.
                if (fault != null) {
                    fault.Changed += (object sender, DigitalInputPortEventArgs e) => {
                        this.RaiseMotorOvercurrentFault();
                    };
                }
            }

            /// <summary>
            /// Raises the MotorOvercurrentFault event.
            /// </summary>
            protected void RaiseMotorOvercurrentFault()
            {
                MotorOvercurrentEventArgs args = new MotorOvercurrentEventArgs();
                this.MotorOvercurrentFault(this, args);
            }
        }
    }
}
