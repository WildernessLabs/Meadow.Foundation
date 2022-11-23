#nullable enable
using Meadow.Devices;
using Meadow.Hardware;
using static Meadow.Units.Frequency;

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
        IPwmPort inA1;
        IPwmPort inA2;
        IDigitalOutputPort pwmA;
        IDigitalInputPort? fault1;
        IPwmPort? inB1;
        IPwmPort? inB2;
        IDigitalInputPort? fault2;
        IDigitalOutputPort? pwmB;
        IDigitalOutputPort? hbMode;
        IDigitalOutputPort? tblkab;

        /// <summary>
        /// H-bridge mode
        /// </summary>
        protected HBridgeMode hbridgeMode = HBridgeMode.Dual;

        /// <summary>
        /// Motor 1
        /// </summary>
        public HBridgeMotor Motor1 { get; protected set; }

        /// <summary>
        /// Motor 2
        /// </summary>
        public HBridgeMotor? Motor2 { get; protected set; }

        /// <summary>
        /// Create a new Tb67h420ftg object
        /// </summary>
        public Tb67h420ftg(IMeadowDevice device,
            IPin inA1, IPin inA2, IPin pwmA,
            IPin? inB1, IPin? inB2, IPin? pwmB,
            IPin? fault1, IPin? fault2,
            IPin? hbMode = null, IPin? tblkab = null) : this(
                inA1: device.CreatePwmPort(inA1, new Units.Frequency(100, UnitType.Hertz)), 
                inA2: device.CreatePwmPort(inA2, new Units.Frequency(100, UnitType.Hertz)),
                pwmA: device.CreateDigitalOutputPort(pwmA),
                inB1: inB1 is null ? null : device.CreatePwmPort(inB1, new Units.Frequency(100, UnitType.Hertz)),
                inB2: inB2 is null ? null : device.CreatePwmPort(inB2, new Units.Frequency(100, UnitType.Hertz)),
                pwmB: pwmB is null ? null : device.CreateDigitalOutputPort(pwmB),
                fault1 is null ? null : device.CreateDigitalInputPort(fault1),
                fault2: null,
                hbMode: hbMode == null ? null : device.CreateDigitalOutputPort(hbMode),
                tblkab: tblkab == null ? null : device.CreateDigitalOutputPort(tblkab)
                )
        { }

        /// <summary>
        /// Create a new Tb67h420ftg object
        /// </summary>
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

            Motor1 = new HBridgeMotor(inA1, inA2, pwmA);

            if (inB1 != null && inB2 != null && pwmB != null)
            {
                Motor2 = new HBridgeMotor(inB1, inB2, pwmB);
            }
        }
        void ValidateConfiguration()
        {
        }
    }
}