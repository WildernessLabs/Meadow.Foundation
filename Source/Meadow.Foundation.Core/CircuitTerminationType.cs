namespace Meadow.Foundation
{
    /// <summary>
    /// Whether the circuit is terminated into the common/ground or a high (3.3V) voltage
    /// source. Used to determine whether to pull the resistor wired to the switch
    /// sensor high or low to close the circuit when the switch is closed.
    /// 
    /// To disable the resistor, set to Floating.
    /// </summary>
    public enum CircuitTerminationType
    {
        /// <summary>
        /// Common ground termination
        /// </summary>
        CommonGround,
        /// <summary>
        /// High or VCC termination
        /// </summary>
        High,
        /// <summary>
        /// Floating / not terminated
        /// </summary>
        Floating
    }
}