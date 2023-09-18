namespace Meadow.Foundation.Sensors.Atmospheric
{
    public partial class HC2
    {
        /// <summary>
        /// Communication used to get data from sensor
        /// </summary>
        private enum CommunicationType
        {
            /// <summary>
            /// Analog: 0-1 Volt
            /// </summary>
            Analog,
            /// <summary>
            /// Serial: 19200,8,N,1
            /// </summary>
            Serial,
        }

        /// <summary>
        /// Enumeration of the different fields supplied as a response from the HC2 probe.
        /// </summary>
        private enum ResponseFields
        {
            UnitID,
            RHValue,
            RHUnits,
            RHAlarm,
            RHTrend,
            TempValue,
            TempUnits,
            TempAlarm,
            TempTrend,
            CalcType,
            CalcValue,
            CalcUnits,
            CalcAlarm,
            CalcTrend,
            DeviceType,
            FirmwareVersion,
            SerialNumber,
            DeviceName,
            AlarmByte
        }
    }
}