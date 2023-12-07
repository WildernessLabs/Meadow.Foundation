using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Environmental;
using Meadow.Units;
using Meadow.Utilities;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental
{
    /// <summary>
    /// Represents an ENS160 Digital Metal-Oxide Multi-Gas Sensor
    /// </summary>
    public partial class Ens160 :
        ByteCommsSensorBase<(Concentration? CO2Concentration,
                             Concentration? EthanolConcentration,
                             Concentration? TVOCConcentration)>,
        IConcentrationSensor, II2cPeripheral
    {
        private event EventHandler<IChangeResult<Concentration>> _concentrationHandlers;

        event EventHandler<IChangeResult<Concentration>> ISamplingSensor<Concentration>.Updated
        {
            add => _concentrationHandlers += value;
            remove => _concentrationHandlers -= value;
        }

        /// <summary>
        /// Raised when the CO2 concentration changes
        /// </summary>
        public event EventHandler<IChangeResult<Concentration>> CO2ConcentrationUpdated = default!;

        /// <summary>
        /// Raised when the ethanol concentration changes
        /// </summary>
        public event EventHandler<IChangeResult<Concentration>> EthanolConcentrationUpdated = default!;

        /// <summary>
        /// Raised when the Total Volatile Organic Compounds (TVOC) concentration changes
        /// </summary>
        public event EventHandler<IChangeResult<Concentration>> TVOCConcentrationUpdated = default!;

        /// <summary>
        /// The current C02 concentration value
        /// </summary>
        public Concentration? Concentration => Conditions.CO2Concentration;

        /// <summary>
        /// The current C02 concentration value
        /// </summary>
        public Concentration? CO2Concentration => Conditions.CO2Concentration;

        /// <summary>
        /// The current ethanol concentration value
        /// </summary>
        public Concentration? EthanolConcentration => Conditions.EthanolConcentration;

        /// <summary>
        /// The current Total Volatile Organic Compounds (TVOC) concentration value
        /// </summary>
        public Concentration? TVOCConcentration => Conditions.TVOCConcentration;

        /// <summary>
        /// The current device operating mode
        /// </summary>
        public OperatingMode CurrentOperatingMode
        {
            get => (OperatingMode)BusComms.ReadRegister((byte)Registers.OPMODE);
            set => BusComms.WriteRegister((byte)Registers.OPMODE, (byte)value);
        }

        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// Create a new ENS160 object
        /// </summary>
        /// <remarks>
        /// The constructor sends the stop periodic updates method otherwise 
        /// the sensor may not respond to new commands
        /// </remarks>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        public Ens160(II2cBus i2cBus, byte address = (byte)Addresses.Default)
            : base(i2cBus, address, readBufferSize: 9, writeBufferSize: 9)
        {
            Initialize().Wait();

            CurrentOperatingMode = OperatingMode.Standard;
        }

        /// <summary>
        /// Initialize the sensor
        /// </summary>
        protected async Task Initialize()
        {
            BusComms?.WriteRegister((byte)Registers.COMMAND, (byte)Commands.NOP);
            BusComms?.WriteRegister((byte)Registers.COMMAND, (byte)Commands.CLRGPR);

            await Task.Delay(10);
            await Reset();
        }

        /// <summary>
        /// Reset the sensor
        /// </summary>
        /// <returns></returns>
        public Task Reset()
        {
            BusComms?.WriteRegister((byte)Registers.OPMODE, (byte)OperatingMode.Reset);
            return Task.Delay(10);
        }

        /// <summary>
        /// Get the sensor ID from PART_ID register
        /// Default value is 0x0160 (352)
        /// </summary>
        /// <returns>ID as a ushort (2 bytes)</returns>
        public ushort GetDeviceID()
        {
            return BusComms.ReadRegisterAsUShort((byte)Registers.PART_ID);
        }

        /// <summary>
        /// Get the sensor app / firmware version
        /// </summary>
        /// <returns>The major, minor, release values as a tuple of bytes</returns>
        public (byte Major, byte Minor, byte Release) GetFirmwareVersion()
        {
            BusComms.WriteRegister((byte)Registers.COMMAND, (byte)Commands.GET_APPVER);

            var version = new byte[3];

            BusComms.ReadRegister((byte)Registers.GPR_READ_4);

            return (version[0], version[1], version[2]);
        }

        /// <summary>
        /// Clears the 10 GPR registers
        /// </summary>
        private void ClearGPRRegisters()
        {
            BusComms.WriteRegister((byte)Registers.COMMAND, (byte)Commands.CLRGPR);
        }

        /// <summary>
        /// Set ambient temperature
        /// </summary>
        /// <param name="ambientTemperature"></param>
        public void SetTemperature(Units.Temperature ambientTemperature)
        {
            ushort temp = (ushort)(ambientTemperature.Kelvin * 64);

            BusComms.WriteRegister((byte)Registers.TEMP_IN, temp);
        }

        /// <summary>
        /// Set relative humidity
        /// </summary>
        /// <param name="humidity"></param>
        public void SetHumidity(RelativeHumidity humidity)
        {
            ushort hum = (ushort)(humidity.Percent * 64);

            BusComms.WriteRegister((byte)Registers.RH_IN, hum);
        }

        /// <summary>
        /// Get the air quality index (AQI)
        /// </summary>
        public UBAAirQualityIndex GetAirQualityIndex()
        {
            var value = BusComms.ReadRegister((byte)Registers.DATA_AQI);

            var aqi = value >> 5;

            return (UBAAirQualityIndex)aqi;
        }

        private bool IsNewDataAvailable()
        {
            var value = BusComms.ReadRegister((byte)Registers.DATA_STATUS);

            return BitHelpers.GetBitValue(value, 0x02);
        }

        private bool IsNewGPRAvailable()
        {
            var value = BusComms.ReadRegister((byte)Registers.DATA_STATUS);

            return BitHelpers.GetBitValue(value, 0x03);
        }

        private Concentration GetTotalVolotileOrganicCompounds()
        {
            var con = BusComms.ReadRegisterAsUShort((byte)Registers.DATA_TVOC);
            return new Concentration(con, Units.Concentration.UnitType.PartsPerBillion);
        }

        private Concentration GetCO2Concentration()
        {
            var con = BusComms.ReadRegisterAsUShort((byte)Registers.DATA_ECO2);
            return new Concentration(con, Units.Concentration.UnitType.PartsPerMillion);
        }

        private Concentration GetEthanolConcentration()
        {
            var con = BusComms.ReadRegisterAsUShort((byte)Registers.DATA_ETOH);
            return new Concentration(con, Units.Concentration.UnitType.PartsPerBillion);
        }

        /// <summary>
        /// Get the temperature used for calculations - taken from TEMP_IN if supplied
        /// </summary>
        /// <returns>Temperature</returns>
        public Units.Temperature GetTemperature()
        {
            var temp = BusComms.ReadRegisterAsUShort((byte)Registers.DATA_T);
            return new Units.Temperature(temp / 64.0, Units.Temperature.UnitType.Kelvin);
        }

        /// <summary>
        /// Get the relative humidity used in its calculations -b taken from RH_IN if supplied
        /// </summary>
        /// <returns></returns>
        public RelativeHumidity GetHumidity()
        {
            var hum = BusComms.ReadRegisterAsUShort((byte)Registers.DATA_T);
            return new RelativeHumidity(hum / 512.0);
        }

        /// <summary>
        /// Starts updating the sensor on the updateInterval frequency specified
        /// </summary>
        public override void StartUpdating(TimeSpan? updateInterval = null)
        {
            base.StartUpdating(updateInterval);
        }

        /// <summary>
        /// Stop updating the sensor
        /// The sensor will not respond to commands for 500ms 
        /// The call will delay the calling thread for 500ms
        /// </summary>
        public override void StopUpdating()
        {
            base.StopUpdating();
        }

        /// <summary>
        /// Get Scdx40 C02 Gas Concentration and
        /// Update the Concentration property
        /// </summary>
        protected override Task<(Concentration? CO2Concentration,
                                       Concentration? EthanolConcentration,
                                       Concentration? TVOCConcentration)> ReadSensor()
        {
            (Concentration? CO2Concentration, Concentration? EthanolConcentration, Concentration? TVOCConcentration) conditions;

            conditions.CO2Concentration = GetCO2Concentration();
            conditions.EthanolConcentration = GetEthanolConcentration();
            conditions.TVOCConcentration = GetTotalVolotileOrganicCompounds();

            return Task.FromResult(conditions);
        }

        /// <summary>
        /// Raise change events for subscribers
        /// </summary>
        /// <param name="changeResult">The change result with the current sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<(Concentration? CO2Concentration, Concentration? EthanolConcentration, Concentration? TVOCConcentration)> changeResult)
        {
            if (changeResult.New.CO2Concentration is { } concentration)
            {
                _concentrationHandlers?.Invoke(this, new ChangeResult<Concentration>(concentration, changeResult.Old?.CO2Concentration));
                CO2ConcentrationUpdated?.Invoke(this, new ChangeResult<Concentration>(concentration, changeResult.Old?.CO2Concentration));
            }
            if (changeResult.New.EthanolConcentration is { } ethConcentration)
            {
                EthanolConcentrationUpdated?.Invoke(this, new ChangeResult<Concentration>(ethConcentration, changeResult.Old?.CO2Concentration));
            }
            if (changeResult.New.TVOCConcentration is { } tvocConcentration)
            {
                TVOCConcentrationUpdated?.Invoke(this, new ChangeResult<Concentration>(tvocConcentration, changeResult.Old?.CO2Concentration));
            }

            base.RaiseEventsAndNotify(changeResult);
        }

        async Task<Concentration> ISensor<Concentration>.Read()
            => (await Read()).CO2Concentration!.Value;
    }
}