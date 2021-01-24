#nullable enable

using System;
using Meadow.Hardware;

namespace Meadow.Foundation.Motors
{
    public partial class Tb67h420ftg
    {
        protected IDigitalOutputPort inA1;
        protected IDigitalOutputPort inA2;
        protected IPwmPort pwmA;
        protected IDigitalOutputPort inB1;
        protected IDigitalOutputPort inB2;
        protected IPwmPort pwmB;
        protected IDigitalOutputPort? hbMode;
        protected IDigitalOutputPort? tblkab;

        protected HBridgeMode hbridgeMode = HBridgeMode.Dual;

        public Tb67h420ftg(IIODevice device,
            IPin inA1, IPin inA2, IPin pwmA,
            IPin inB1, IPin inB2, IPin pwmB,
            IPin? hbMode = null, IPin? tblkab = null) : this(
                inA1: device.CreateDigitalOutputPort(inA1), inA2: device.CreateDigitalOutputPort(inA2),
                pwmA: device.CreatePwmPort(pwmA),
                inB1: device.CreateDigitalOutputPort(inB1), inB2: device.CreateDigitalOutputPort(inB2),
                pwmB: device.CreatePwmPort(pwmB),
                hbMode: (hbMode == null ? null : device.CreateDigitalOutputPort(hbMode)),
                tblkab: (tblkab == null ? null : device.CreateDigitalOutputPort(tblkab))
                )
        {
        }

        public Tb67h420ftg(
            IDigitalOutputPort inA1, IDigitalOutputPort inA2, IPwmPort pwmA,
            IDigitalOutputPort inB1, IDigitalOutputPort inB2, IPwmPort pwmB,
            IDigitalOutputPort? hbMode = null, IDigitalOutputPort? tblkab = null
            )
        {
            this.inA1 = inA1; this.inA2 = inA2; this.pwmA = pwmA;
            this.inB1 = inB1; this.inB2 = inB2; this.pwmB = pwmB;
            this.hbMode = hbMode; this.tblkab = tblkab;

            if(hbMode is null) { hbridgeMode = HBridgeMode.Dual; }

        }
    }
}
