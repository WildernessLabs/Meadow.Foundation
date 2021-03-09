using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors.Temperature;

namespace Meadow.Foundation.Sensors.Temperature
{
    public class Mcp9808 : FilterableChangeObservableBase<AtmosphericConditionChangeResult, AtmosphericConditions>,
        IAtmosphericSensor, ITemperatureSensor
    {
        const ushort MCP_CONFIG_SHUTDOWN = 0x0100;   // shutdown config
        const ushort MCP_CONFIG_CRITLOCKED = 0x0080; // critical trip lock
        const ushort MCP_CONFIG_WINLOCKED = 0x0040; // alarm window lock
        const ushort MCP_CONFIG_INTCLR = 0x0020;     // interrupt clear
        const ushort MCP_CONFIG_ALERTSTAT = 0x0010;  // alert output status
        const ushort MCP_CONFIG_ALERTCTRL = 0x0008;  // alert output control
        const ushort MCP_CONFIG_ALERTSEL = 0x0004;  // alert output select
        const ushort MCP_CONFIG_ALERTPOL = 0x0002;   // alert output polarity
        const ushort MCP_CONFIG_ALERTMODE = 0x0001;  // alert output mode

        const byte MCP_REG_CONFIG = 0x01;        // config
        const byte MCP_UPPER_TEMP = 0x02;     // upper alert boundary
        const byte MCP_LOWER_TEMP = 0x03;     // lower alert boundery
        const byte MCP_CRIT_TEMP = 0x04;     // critical temperature
        const byte MCP_AMBIENT_TEMP = 0x05;   // ambient temperature
        const byte MCP_MANUFACTURER_ID = 0x06; // manufacturer ID
        const byte MCP_DEVICE_ID = 0x07;    // device ID
        const byte MCP_RESOLUTION = 0x08;     // resolution

        II2cPeripheral i2CPeripheral;

        public const byte DefaultAddress = 0x18;

        public event EventHandler<AtmosphericConditionChangeResult> Updated;

        // <summary>
        /// The temperature, in degrees celsius (°C), from the last reading.
        /// </summary>
        public float Temperature => Conditions.Temperature.Value;

        /// <summary>
        /// The AtmosphericConditions from the last reading.
        /// </summary>
        public AtmosphericConditions Conditions { get; protected set; } = new AtmosphericConditions();

        // internal thread lock
        private object _lock = new object();
        private CancellationTokenSource SamplingTokenSource;

        public bool IsSampling { get; protected set; } = false;

        public Mcp9808(II2cBus i2CBus, byte address = DefaultAddress)
        {
            i2CPeripheral = new I2cPeripheral(i2CBus, address);

            Init();
        }

        void Init()
        {
            i2CPeripheral.WriteUShort(MCP_REG_CONFIG, 0x0);
        }

        /// <summary>
		/// Wake the the device if it's in sleep state
		/// </summary>
        public void Wake()
        {
            ushort conf_shutdown;
            ushort config = i2CPeripheral.ReadUShort(MCP_REG_CONFIG, ByteOrder.BigEndian);

            config = (ushort)(config & (~MCP_CONFIG_SHUTDOWN));

            i2CPeripheral.WriteUShort(MCP_REG_CONFIG, config);
        }

        /// <summary>
		/// Set the device into a low power sleep state
		/// </summary>
        public void Sleep()
        {
            ushort conf_shutdown;
            ushort config = i2CPeripheral.ReadUShort(MCP_REG_CONFIG, ByteOrder.BigEndian);

            i2CPeripheral.WriteUShort(MCP_REG_CONFIG, (ushort)(config | MCP_CONFIG_SHUTDOWN));
         }

        /// <summary>
		/// Read the device ID 
		/// </summary>
        public ushort GetDeviceId()
        {
            return i2CPeripheral.ReadUShort(MCP_DEVICE_ID, ByteOrder.BigEndian);
        }

        /// <summary>
		/// Read the manufacture ID 
		/// </summary>
        public ushort GetManufactureId()
        {
            return i2CPeripheral.ReadUShort(MCP_MANUFACTURER_ID, ByteOrder.BigEndian);
        }

        /// <summary>
		/// Read resolution
		/// </summary>
        public byte GetResolution()
        {
            return i2CPeripheral.ReadRegister(MCP_RESOLUTION);
        }

        /// <summary>
		/// Set resolution
		/// </summary>
        public void SetResolution(byte resolution)
        {
            i2CPeripheral.WriteRegister(MCP_RESOLUTION, resolution);
        }

        /// <summary>
        /// Convenience method to get the current sensor readings. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        public async Task<AtmosphericConditions> Read()
        {
            Conditions = await Read();

            return Conditions;
        }

        /// <summary>
		/// Begin reading temperature data
		/// </summary>
        public void StartUpdating(int standbyDuration = 1000)
        {
            Console.WriteLine("Start updating");

            // thread safety
            lock (_lock)
            {
                if (IsSampling) { return; }

                // state muh-cheen
                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                AtmosphericConditions oldConditions;
                AtmosphericConditionChangeResult result;
                Task.Factory.StartNew(async () => {
                    while (true)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            // do task clean up here
                            _observers.ForEach(x => x.OnCompleted());
                            break;
                        }

                        Console.WriteLine("history");

                        // capture history
                        oldConditions = AtmosphericConditions.From(Conditions);

                        // read
                        Update(); //syncrhnous for this driver 

                        // build a new result with the old and new conditions
                        result = new AtmosphericConditionChangeResult(oldConditions, Conditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(standbyDuration);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        protected void RaiseChangedAndNotify(AtmosphericConditionChangeResult changeResult)
        {
            Console.WriteLine("Raise");

            Updated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public void StopUpdating()
        {
            lock (_lock)
            {
                if (!IsSampling) return;

                SamplingTokenSource?.Cancel();

                IsSampling = false;
            }
        }

        /// <summary>
        ///     Update the Temperature property.
        /// </summary>
        public void Update()
        {
            ushort value = i2CPeripheral.ReadUShort(MCP_AMBIENT_TEMP, ByteOrder.BigEndian);

            if (value == 0xFFFF)
            {
                return; //not ready
            }

            double temp = value & 0x0FFF;

            temp /= 16.0;

            if ((value & 0x1000) != 0)
            {
                temp -= 256;
            }

            Conditions.Temperature = (float)temp;
        }
    }
}