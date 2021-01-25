#nullable enable

using System;
using Meadow.Hardware;

namespace Meadow.Foundation.Motors
{
    public partial class Tb67h420ftg
    {
        protected IPwmPort inA1;
        protected IPwmPort inA2;
        protected IDigitalOutputPort pwmA;
        protected IPwmPort? inB1;
        protected IPwmPort? inB2;
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
            IPin? hbMode = null, IPin? tblkab = null) : this(
                inA1: device.CreatePwmPort(inA1), inA2: device.CreatePwmPort(inA2),
                pwmA: device.CreateDigitalOutputPort(pwmA),
                inB1: inB1 is null ? null : device.CreatePwmPort(inB1),
                inB2: inB2 is null ? null : device.CreatePwmPort(inB2),
                pwmB: pwmB is null ? null : device.CreateDigitalOutputPort(pwmB),
                hbMode: (hbMode == null ? null : device.CreateDigitalOutputPort(hbMode)),
                tblkab: (tblkab == null ? null : device.CreateDigitalOutputPort(tblkab))
                )
        {
        }

        public Tb67h420ftg(
            IPwmPort inA1, IPwmPort inA2, IDigitalOutputPort pwmA,
            IPwmPort? inB1, IPwmPort? inB2, IDigitalOutputPort? pwmB,
            IDigitalOutputPort? hbMode = null, IDigitalOutputPort? tblkab = null
            )
        {
            this.inA1 = inA1; this.inA2 = inA2; this.pwmA = pwmA;
            this.inB1 = inB1; this.inB2 = inB2; this.pwmB = pwmB;
            this.hbMode = hbMode; this.tblkab = tblkab;

            ValidateConfiguration();

            if(hbMode is null) { hbridgeMode = HBridgeMode.Dual; }

            Motor1 = new HBridgeMotor(inA1, inA2, pwmA);

            if (inB1 != null && inB2 != null && pwmB != null) {
                Motor2 = new HBridgeMotor(inB1, inB2, pwmB);
            }

            //Motor1 = new Motor(this);
            //Motor2 = new Motor(this);
        }

        protected void ValidateConfiguration()
        {
            // TODO: validate all the pins are setup correctly

            // if 2nd motor pins are null, hbMode has to be present

            //
        }

    }
}
