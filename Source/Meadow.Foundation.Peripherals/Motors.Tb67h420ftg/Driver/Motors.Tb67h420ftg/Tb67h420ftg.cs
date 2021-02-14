#nullable enable

using System;
using Meadow.Hardware;

namespace Meadow.Foundation.Motors
{
    /// <summary>
    /// The TB67H420FTG is a brushed DC motor driver that has a dual H-Bridge
    /// motor driver built in, along with over current protection (and notification).
    ///
    /// With the ability to drive up to `50V` at `9A`, it's a very powerful driver.
    /// It can also be put into single H-Bridge mode in which two motors are driven
    /// in synch, or one motor is driven with both outputs, allowing twice the power.
    /// </summary>
    /// <remarks>
    /// Pololu makes a breakout of this chip to get you started: https://www.pololu.com/product/2999
    /// 
    /// If you're looking to integrate into your own circuit, you can find an
    /// open source hardware design here: https://easyeda.com/bryan_6020/motordriver
    ///
    /// The driver can also drive a single stepper motor, though that driver is
    /// still planned.
    /// </remarks>
    public partial class Tb67h420ftg
    {
        protected IPwmPort inA1;
        protected IPwmPort inA2;
        protected IDigitalOutputPort pwmA;
        protected IDigitalInputPort? fault1;
        protected IPwmPort? inB1;
        protected IPwmPort? inB2;
        protected IDigitalInputPort? fault2;
        protected IDigitalOutputPort? pwmB;
        protected IDigitalOutputPort? hbMode;
        protected IDigitalOutputPort? tblkab;

        protected HBridgeMode hbridgeMode = HBridgeMode.Dual;

        //public Motor Motor1 { get; protected set; }
        //public Motor? Motor2 { get; protected set; }
        public HBridgeMotor Motor1 { get; protected set; }
        public HBridgeMotor? Motor2 { get; protected set; }

        public Tb67h420ftg(IIODevice device,
            IPin inA1, IPin inA2, IPin pwmA,
            IPin? inB1, IPin? inB2, IPin? pwmB,
            IPin? fault1, IPin? fault2,
            IPin? hbMode = null, IPin? tblkab = null) : this(
                inA1: device.CreatePwmPort(inA1), inA2: device.CreatePwmPort(inA2),
                pwmA: device.CreateDigitalOutputPort(pwmA),
                inB1: inB1 is null ? null : device.CreatePwmPort(inB1),
                inB2: inB2 is null ? null : device.CreatePwmPort(inB2),
                pwmB: pwmB is null ? null : device.CreateDigitalOutputPort(pwmB),
                //fault1: null,
                fault1 is null ? null : device.CreateDigitalInputPort(fault1),
                fault2: null,
                //fault2 is null ? null : device.CreateDigitalInputPort(fault2),
                hbMode: hbMode == null ? null : device.CreateDigitalOutputPort(hbMode),
                tblkab: tblkab == null ? null : device.CreateDigitalOutputPort(tblkab)
                )
        {
        }

        public Tb67h420ftg(
            IPwmPort inA1, IPwmPort inA2, IDigitalOutputPort pwmA,
            IPwmPort? inB1, IPwmPort? inB2, IDigitalOutputPort? pwmB,
            IDigitalInputPort? fault1, IDigitalInputPort? fault2,
            IDigitalOutputPort? hbMode = null, IDigitalOutputPort? tblkab = null
            )
        {
            this.inA1 = inA1; this.inA2 = inA2; this.pwmA = pwmA;
            this.inB1 = inB1; this.inB2 = inB2; this.pwmB = pwmB;
            this.fault1 = fault1; this.fault2 = fault2;
            this.hbMode = hbMode; this.tblkab = tblkab;

            ValidateConfiguration();

            if(hbMode is null) { hbridgeMode = HBridgeMode.Dual; }

            //Motor1 = new Motor(this, inA1, inA2, pwmA, fault1);
            Motor1 = new HBridgeMotor(inA1, inA2, pwmA);

            if (inB1 != null && inB2 != null && pwmB != null) {
                //Motor2 = new Motor(this, inB1, inB2, pwmB, fault2);
                Motor2 = new HBridgeMotor(inB1, inB2, pwmB);
            }
        }

        protected void ValidateConfiguration()
        {
            // TODO: validate all the pins are setup correctly

            // if 2nd motor pins are null, hbMode has to be present

            //
        }

    }
}
