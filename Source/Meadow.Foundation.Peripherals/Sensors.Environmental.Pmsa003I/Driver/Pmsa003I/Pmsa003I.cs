using System;
using System.Linq;
using System.Threading.Tasks;

using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Environmental;
using Meadow.Units;

namespace Meadow.Foundation.Sensors.Environmental
{
    public partial class Pmsa003I :
        ByteCommsSensorBase<(Concentration? PM1_0Std,
            Concentration? PM2_5Std,
            Concentration? PM10_0Std,
            Concentration? PM1_0Env,
            Concentration? PM2_5Env,
            Concentration? PM10_0Env,
            Concentration? P03um,
            Concentration? P05um,
            Concentration? P10um,
            Concentration? P25um,
            Concentration? P50um,
            Concentration? P100um)>,
        IConcentrationSensor
    {
        protected override Task<(Concentration? PM1_0Std, Concentration? PM2_5Std, Concentration? PM10_0Std,
            Concentration? PM1_0Env, Concentration? PM2_5Env, Concentration? PM10_0Env, Concentration? P03um,
            Concentration? P05um, Concentration? P10um, Concentration? P25um, Concentration? P50um, Concentration?
            P100um)> ReadSensor()
        {
            var buffer = new byte[32];
            Peripheral.Read(buffer);
            var span = buffer.AsSpan();
            span.Reverse();
            if (buffer[30..32].SequenceEqual(Preamble) == false)
            {
                throw new Exception("Preamble mismatch!");
            }
            var messageLength = BitConverter.ToUInt16(span[28..30]);
            if (messageLength != 28)
            {
                throw new Exception("Message is corrupt or has invalid length");
            }

            var checksum = BitConverter.ToUInt16(buffer[2..4]);
            // this is in big-endian format, so we need to reverse things...
            var pm10Standard = new Concentration(BitConverter.ToUInt16(span[26..28]), Units.Concentration.UnitType.PartsPerMillion);
            var pm25Standard = new Concentration(BitConverter.ToUInt16(span[24..26]), Units.Concentration.UnitType.PartsPerMillion);
            var pm100Standard = new Concentration(BitConverter.ToUInt16(span[22..24]), Units.Concentration.UnitType.PartsPerMillion);
            var pm10Environmental = new Concentration(BitConverter.ToUInt16(span[20..22]), Units.Concentration.UnitType.PartsPerMillion);
            var pm25Environmental = new Concentration(BitConverter.ToUInt16(span[18..20]), Units.Concentration.UnitType.PartsPerMillion);
            var pm100Environmental = new Concentration(BitConverter.ToUInt16(span[16..18]), Units.Concentration.UnitType.PartsPerMillion);
            var p03um = new Concentration(BitConverter.ToUInt16(span[14..16]), Units.Concentration.UnitType.PartsPerMillion);
            var p05um = new Concentration(BitConverter.ToUInt16(span[12..14]), Units.Concentration.UnitType.PartsPerMillion);
            var p10um = new Concentration(BitConverter.ToUInt16(span[10..12]), Units.Concentration.UnitType.PartsPerMillion);
            var p25um = new Concentration(BitConverter.ToUInt16(span[8..10]), Units.Concentration.UnitType.PartsPerMillion);
            var p50um = new Concentration(BitConverter.ToUInt16(span[6..8]), Units.Concentration.UnitType.PartsPerMillion);
            var p100um = new Concentration(BitConverter.ToUInt16(span[4..6]), Units.Concentration.UnitType.PartsPerMillion);

            Conditions = (pm10Standard, pm25Standard, pm100Standard, pm10Environmental, pm25Environmental,
                          pm100Environmental, p03um, p05um, p10um, p25um, p50um, p100um);

            return Task.FromResult(Conditions);
        }


        public Concentration? Concentration { get; }
        public event EventHandler<IChangeResult<Concentration>> ConcentrationUpdated;

        /// <summary>
        /// Raised when the Standard PM1.0 concentration changes
        /// </summary>
        public event EventHandler<IChangeResult<Concentration>> PM1_0StdConcentrationUpdated = delegate { };

        /// <summary>
        /// Raised when the Standard PM2.5 concentration changes
        /// </summary>
        public event EventHandler<IChangeResult<Concentration>> PM2_5StdConcentrationUpdated = delegate { };

        /// <summary>
        /// Raised when the Standard PM10.0 concentration changes
        /// </summary>
        public event EventHandler<IChangeResult<Concentration>> PM10_0StdConcentrationUpdated = delegate { };

        /// <summary>
        /// Raised when the Environment PM1.0 concentration changes
        /// </summary>
        public event EventHandler<IChangeResult<Concentration>> PM1_0EnvConcentrationUpdated = delegate { };

        /// <summary>
        /// Raised when the Environment PM2.5 concentration changes
        /// </summary>
        public event EventHandler<IChangeResult<Concentration>> PM2_5EnvConcentrationUpdated = delegate { };

        /// <summary>
        /// Raised when the Environment PM10.0 concentration changes
        /// </summary>
        public event EventHandler<IChangeResult<Concentration>> PM10_0EnvConcentrationUpdated = delegate { };

        public Concentration? PM1_0Std => Conditions.PM1_0Std;
        public Concentration? PM2_5Std => Conditions.PM2_5Std;
        public Concentration? PM10_0Std => Conditions.PM10_0Std;
        public Concentration? PM1_0Env => Conditions.PM1_0Env;
        public Concentration? PM2_5Env => Conditions.PM2_5Env;
        public Concentration? PM10_0Env => Conditions.PM10_0Env;
        public Concentration? P03um => Conditions.P03um;
        public Concentration? P05um => Conditions.P05um;
        public Concentration? P10um => Conditions.P10um;
        public Concentration? P25um => Conditions.P25um;
        public Concentration? P50um => Conditions.P50um;
        public Concentration? P100um => Conditions.P100um;

        /// <summary>
        /// Create a new PMSA003I sensor object
        /// </summary>
        /// <remarks></remarks>
        /// <param name="i2cBus">The I2C bus</param>
        public Pmsa003I(II2cBus i2cBus)
            : base(i2cBus, 0x12)
        {

        }

        protected async Task Initialize()
        {

        }

        public Task<Concentration> Read() => throw new NotImplementedException();

        public Task<(Concentration? PM1_0Std, Concentration? PM2_5Std, Concentration? PM10_0Std,
            Concentration? PM1_0Env, Concentration? PM2_5Env, Concentration? PM10_0Env, Concentration? P03um,
            Concentration? P05um, Concentration? P10um, Concentration? P25um, Concentration? P50um, Concentration?
            P100um)> ReadSensor2() =>
            ReadSensor();
    }
}