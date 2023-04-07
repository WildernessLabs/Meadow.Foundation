namespace Meadow.Foundation.Sensors.Environmental
{
    partial class MiniPID2
    {
        /// <summary>
        /// The MiniPID 2 Volatile Organic Compounds (VOC) sensor variant
        /// </summary>
        public enum MiniPID2Type : byte
        {
            /// <summary>
            /// MiniPID 2 PartsPerMillion VOC Sensor
            /// </summary>
            PPM,
            /// <summary>
            /// MiniPID 2 PartsPerMillion Wide Range VOC Sensor
            /// </summary>
            PPM_WR,
            /// <summary>
            /// MiniPID 2 PartsPerBillion VOC Sensor
            /// </summary>
            PPB,
            /// <summary>
            /// MiniPID 2 PartsPerBillion Wide Range VOC Sensor
            /// </summary>
            PPB_WR,
            /// <summary>
            /// MiniPID 2 High Sensitivity VOC Sensor
            /// </summary>
            HS,
            /// <summary>
            /// MiniPID 2 10.0 eV VOC Sensor
            /// </summary>
            _10ev,
            /// <summary>
            /// MiniPID 2 11.7 eV VOC Sensor
            /// </summary>
            _11_7eV,
            /// <summary>
            /// The number of supported sensors
            /// </summary>
            count
        }
    }
}