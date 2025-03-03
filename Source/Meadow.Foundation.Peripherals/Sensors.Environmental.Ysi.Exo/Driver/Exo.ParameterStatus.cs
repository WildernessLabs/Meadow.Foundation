namespace Meadow.Foundation.Sensors.Environmental.Ysi;

public partial class Exo
{
    /// <summary>
    /// Represents the status of a parameter on the EXO device.
    /// </summary>
    public enum ParameterStatus
    {
        /// <summary>
        /// The parameter is available on the device.
        /// </summary>
        Available = 0,

        /// <summary>
        /// The parameter is not set on the device.
        /// </summary>
        NotSet = 1,

        /// <summary>
        /// The parameter is not available on the device.
        /// </summary>
        NotAvailable = 2
    }
}
