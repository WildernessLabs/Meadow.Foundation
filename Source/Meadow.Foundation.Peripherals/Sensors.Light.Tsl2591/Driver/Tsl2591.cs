using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Light;
using Meadow.Units;
using System;
using System.Threading.Tasks;
using IU = Meadow.Units.Illuminance.UnitType;

namespace Meadow.Foundation.Sensors.Light
{
    // TODO: This sensor has an interrupt

    /// <summary>
    /// Driver for the TSL2591 light-to-digital converter
    /// </summary>
    public partial class Tsl2591 :
        ByteCommsSensorBase<(Illuminance? FullSpectrum, Illuminance? Infrared, Illuminance? VisibleLight, Illuminance? Integrated)>,
        ILightSensor, IDisposable, IPowerControllablePeripheral
    {
        /// <summary>
        /// Raised when Full Spectrum Illuminance value changes
        /// </summary>
        public event EventHandler<IChangeResult<Illuminance>> FullSpectrumUpdated = default!;

        /// <summary>
        /// Raised when Infrared Illuminance value changes
        /// </summary>
        public event EventHandler<IChangeResult<Illuminance>> InfraredUpdated = default!;

        /// <summary>
        /// Raised when Visible Light value changes
        /// </summary>
        public event EventHandler<IChangeResult<Illuminance>> VisibleLightUpdated = default!;

        /// <summary>
        /// Raised when Luminosity value changes
        /// </summary>
        public event EventHandler<IChangeResult<Illuminance>> Updated = default!;

        /// <summary>
        /// Sensor package ID
        /// </summary>
        public int PackageID => BusComms.ReadRegister((byte)(Register.PackageID | Register.Command));

        /// <summary>
        /// Sensor device ID
        /// </summary>
        public int DeviceID => BusComms.ReadRegister((byte)(Register.DeviceID | Register.Command));

        /// <summary>
        /// Gain of the sensor
        /// </summary>
        public GainFactor Gain
        {
            get => gainFactor;
            set
            {
                PowerOff();
                gainFactor = value;
                BusComms.WriteRegister((byte)(Register.Command | Register.Config), (byte)((byte)integrationTime | (byte)gainFactor));
                PowerOn();
            }
        }

        /// <summary>
        /// Integration time for the sensor
        /// </summary>
        public IntegrationTimes IntegrationTime
        {
            get => integrationTime;
            set
            {
                PowerOff();
                integrationTime = value;
                BusComms.WriteRegister((byte)(Register.Command | Register.Config), (byte)((byte)integrationTime | (byte)gainFactor));
                PowerOn();
            }
        }

        private IntegrationTimes integrationTime;
        private GainFactor gainFactor;

        /// <summary>
        /// Full spectrum luminosity (visible and infrared light combined)
        /// </summary>
        public Illuminance? FullSpectrumLuminosity => Conditions.FullSpectrum;

        /// <summary>
        /// Infrared light luminosity
        /// </summary>
        public Illuminance? InfraredLuminosity => Conditions.Infrared;

        /// <summary>
        /// Visible light luminosity
        /// </summary>
        public Illuminance? VisibleLightLuminosity => Conditions.VisibleLight;

        /// <summary>
        /// Visible lux
        /// </summary>
        public Illuminance? Illuminance => Conditions.Integrated;

        /// <summary>
        /// Create a new Tsl2591 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        public Tsl2591(II2cBus i2cBus,
            byte address = (byte)Addresses.Default)
            : base(i2cBus, address)
        {
            Gain = GainFactor.Medium;
            IntegrationTime = IntegrationTimes.Time_100Ms;
            PowerOn();
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<(Illuminance? FullSpectrum, Illuminance? Infrared, Illuminance? VisibleLight, Illuminance? Integrated)> ReadSensor()
        {
            (Illuminance? FullSpectrum, Illuminance? Infrared, Illuminance? VisibleLight, Illuminance? Integrated) conditions;

            // data sheet indicates you should always read all 4 bytes, in order, for valid data
            var channel0 = BusComms.ReadRegisterAsUShort((byte)(Register.CH0DataL | Register.Command));
            var channel1 = BusComms.ReadRegisterAsUShort((byte)(Register.CH1DataL | Register.Command));

            conditions.FullSpectrum = new Illuminance(channel0, IU.Lux);
            conditions.Infrared = new Illuminance(channel1, IU.Lux);
            conditions.VisibleLight = new Illuminance(channel0 - channel1, IU.Lux);

            double countsPerLux;

            if ((channel0 == 0xffff) || (channel1 == 0xffff))
            {
                conditions.Integrated = new Illuminance(-1, IU.Lux);
            }
            else
            {
                countsPerLux = (IntegrationTimeInMilliseconds(IntegrationTime) * GainMultiplier(Gain)) / 408.0;
                conditions.Integrated = new Illuminance((channel0 - channel1) * (1 - (channel1 / channel0)) / countsPerLux, IU.Lux);
            }

            return Task.FromResult(conditions);
        }

        /// <summary>
        /// Raise events for subscribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<(Illuminance? FullSpectrum, Illuminance? Infrared, Illuminance? VisibleLight, Illuminance? Integrated)> changeResult)
        {
            if (changeResult.New.FullSpectrum is { } ill)
            {
                FullSpectrumUpdated?.Invoke(this, new ChangeResult<Illuminance>(ill, changeResult.Old?.FullSpectrum));
            }
            if (changeResult.New.Infrared is { } infra)
            {
                InfraredUpdated?.Invoke(this, new ChangeResult<Illuminance>(infra, changeResult.Old?.Infrared));
            }
            if (changeResult.New.VisibleLight is { } vis)
            {
                VisibleLightUpdated?.Invoke(this, new ChangeResult<Illuminance>(vis, changeResult.Old?.VisibleLight));
            }
            if (changeResult.New.Integrated is { } integrated)
            {
                Updated?.Invoke(this, new ChangeResult<Illuminance>(integrated, changeResult.Old?.Integrated));
            }

            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Power the sensor on
        /// </summary>
        public Task PowerOn()
        {
            BusComms.WriteRegister((byte)(Register.Enable | Register.Command), 3);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Power the sensor off
        /// </summary>
        public Task PowerOff()
        {
            BusComms.WriteRegister((byte)(Register.Enable | Register.Command), 0);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Convert the integration time into milliseconds
        /// </summary>
        /// <param name="integrationTime">Integration time</param>
        /// <returns>Integration time in milliseconds</returns>
        private double IntegrationTimeInMilliseconds(IntegrationTimes integrationTime)
        {
            double it = 100;            // Default value, 100ms.
            switch (IntegrationTime)
            {
                case IntegrationTimes.Time_100Ms:
                    it = 100;
                    break;
                case IntegrationTimes.Time_200Ms:
                    it = 200;
                    break;
                case IntegrationTimes.Time_300Ms:
                    it = 300;
                    break;
                case IntegrationTimes.Time_400Ms:
                    it = 400;
                    break;
                case IntegrationTimes.Time_500Ms:
                    it = 500;
                    break;
                case IntegrationTimes.Time_600Ms:
                    it = 600;
                    break;
            }

            return it;
        }

        /// <summary>
        /// Multiplication factor for the sensor reading
        /// </summary>
        /// <param name="gain">Gain level to be translated.</param>
        /// <returns>Multiplication factor for the specified gain</returns>
        private double GainMultiplier(GainFactor gain)
        {
            double g = 1.0;             // Default gain = 1.
            switch (gain)
            {
                case GainFactor.Low:
                    g = 1;
                    break;
                case GainFactor.Medium:
                    g = 25;
                    break;
                case GainFactor.High:
                    g = 428;
                    break;
                case GainFactor.Maximum:
                    g = 9876;
                    break;

            }
            return g;
        }

        async Task<Illuminance> ISensor<Illuminance>.Read()
            => (await Read()).FullSpectrum!.Value;
    }
}