using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Threading;
using static Meadow.Foundation.Sensors.Hid.Mpr121;

namespace Meadow.Foundation.Sensors.Hid
{
    public partial class Mpr121
    {
        private readonly II2cPeripheral i2cPeripheral;

        private int refreshPeriod;

        private Timer timer;

        private Dictionary<Channels, bool> channelStatus;

        /// <summary>
        /// Notifies about a the channel statuses have been changed.
        /// Refresh period can be changed by setting PeriodRefresh property.
        /// </summary>
        public event EventHandler<ChannelStatusChangedEventArgs> ChannelStatusesChanged;

        /// <summary>
        /// MPR121 Default I2C Address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x5A;

        private static readonly int NumberOfChannels = Enum.GetValues(typeof(Channels)).Length;

        /// <summary>
        /// Gets or sets the period in milliseconds to refresh the channels statuses.
        /// </summary>
        /// <remark>
        /// Set value 0 to stop the automatically refreshing. Setting the value greater than 0 will start/update auto-refresh.
        /// </remark>
        public int RefreshPeriod
        {
            get => refreshPeriod;
  
            set
            {
                refreshPeriod = value;

                if (refreshPeriod > 0)
                {
                    timer.Change(TimeSpan.FromMilliseconds(RefreshPeriod), TimeSpan.FromMilliseconds(RefreshPeriod));
                }
                else
                {
                    // Disable the auto-refresh.
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
        }

        /// <summary>
        ///     Create a new MPR121 keypad object.
        /// </summary>
        public Mpr121(II2cBus i2cBus, byte address = DefaultI2cAddress, int refreshPeriod = -1, Mpr121Configuration configuration = null)
        {
            this.refreshPeriod = refreshPeriod;

            i2cPeripheral = new I2cPeripheral(i2cBus, address);

            channelStatus = new Dictionary<Channels, bool>();

            foreach (Channels channel in Enum.GetValues(typeof(Channels)))
            {
                channelStatus.Add(channel, false);
            }

            configuration = configuration ?? GetDefaultConfiguration();

            InitializeController(configuration);

            if(refreshPeriod < 1)
            {
                refreshPeriod = Timeout.Infinite;
            }
            
            timer = new Timer(RefreshChannelStatus, this, refreshPeriod, refreshPeriod);
        }

        /// <summary>
        /// Reads the channel status of MPR121 controller.
        /// </summary>
        /// <param name="channel">The channel to read status.</param>
        /// <remark>
        /// Please use ReadChannelStatuses() if you need to read statuses of multiple channels.
        /// Using this method several times to read status for several channels can affect the performance.
        /// </remark>
        public bool ReadChannelStatus(Channels channel)
        {
            RefreshChannelStatus();

            return channelStatus[channel];
        }

        /// <summary>
        /// Reads the channel statuses of MPR121 controller.
        /// </summary>
        public IReadOnlyDictionary<Channels, bool> ReadChannelStatuses()
        {
            RefreshChannelStatus();

            return channelStatus;
        }

        private static Mpr121Configuration GetDefaultConfiguration()
        {
            return new Mpr121Configuration()
            {
                MaxHalfDeltaRising = 0x01,
                NoiseHalfDeltaRising = 0x01,
                NoiseCountLimitRising = 0x00,
                FilterDelayCountLimitRising = 0x00,
                MaxHalfDeltaFalling = 0x01,
                NoiseHalfDeltaFalling = 0x01,
                NoiseCountLimitFalling = 0xFF,
                FilterDelayCountLimitFalling = 0x01,
                ElectrodeTouchThreshold = 0x0F,
                ElectrodeReleaseThreshold = 0x0A,
                ChargeDischargeTimeConfiguration = 0x04,
                ElectrodeConfiguration = 0x0C
            };
        }

        private void InitializeController(Mpr121Configuration configuration)
        {
            SetRegister(Registers.MHDR, configuration.MaxHalfDeltaRising);
            SetRegister(Registers.NHDR, configuration.NoiseHalfDeltaRising);
            SetRegister(Registers.NCLR, configuration.NoiseCountLimitRising);
            SetRegister(Registers.FDLR, configuration.FilterDelayCountLimitRising);
            SetRegister(Registers.MHDF, configuration.MaxHalfDeltaFalling);
            SetRegister(Registers.NHDF, configuration.NoiseHalfDeltaFalling);
            SetRegister(Registers.NCLF, configuration.NoiseCountLimitFalling);
            SetRegister(Registers.FDLF, configuration.FilterDelayCountLimitFalling);
            SetRegister(Registers.E0TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E0RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E1TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E1RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E2TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E2RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E3TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E3RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E4TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E4RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E5TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E5RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E6TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E6RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E7TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E7RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E8TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E8RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E9TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E9RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E10TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E10RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.E11TTH, configuration.ElectrodeTouchThreshold);
            SetRegister(Registers.E11RTH, configuration.ElectrodeReleaseThreshold);
            SetRegister(Registers.CDTC, configuration.ChargeDischargeTimeConfiguration);
            SetRegister(Registers.ELECONF, configuration.ElectrodeConfiguration);
        }

        private void SetRegister(Registers register, byte value)
        {
            i2cPeripheral.WriteRegister((byte)register, value);
        }

        private void RefreshChannelStatus(object value)
        {
            RefreshChannelStatus();
        }

        /// <summary>
        /// Refresh the channel statuses.
        /// </summary>
        private void RefreshChannelStatus()
        {
            // Pause the auto-refresh to prevent possible collisions.
            var period = RefreshPeriod;
            RefreshPeriod = 0;

            var rawStatus = i2cPeripheral.ReadRegisterAsUShort(0x00, ByteOrder.LittleEndian);

            bool isStatusChanged = false;

            for (var i = 0; i < NumberOfChannels; i++)
            {
                bool status = ((1 << i) & rawStatus) > 0;

                if (channelStatus[(Channels)i] != status)
                {
                    channelStatus[(Channels)i] = status;
                    isStatusChanged = true;
                }
            }

            if (isStatusChanged)
            {
                ChannelStatusesChanged?.Invoke(this, new ChannelStatusChangedEventArgs(channelStatus));
            }

            // Resume the auto-refresh.
            RefreshPeriod = period;
        }
    }

    /// <summary>
    /// Represents the arguments of event rising when the channel statuses have been changed.
    /// </summary>
    public class ChannelStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The channel statuses.
        /// </summary>
        public IReadOnlyDictionary<Channels, bool> ChannelStatus { get; private set; }

        /// <summary>
        /// Initialize event arguments.
        /// </summary>
        /// <param name="channelStatus">The channel statuses.</param>
        public ChannelStatusChangedEventArgs(IReadOnlyDictionary<Channels, bool> channelStatus)
            : base()
        {
            ChannelStatus = channelStatus;
        }
    }
}