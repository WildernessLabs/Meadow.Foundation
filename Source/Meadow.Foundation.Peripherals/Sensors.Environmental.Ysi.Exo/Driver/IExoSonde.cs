using System;
using System.Threading.Tasks;
using static Meadow.Foundation.Sensors.Environmental.Ysi.Exo;

namespace Meadow.Foundation.Sensors.Environmental.Ysi;

/// <summary>
/// An interface representing YSI EXO water quality sonde devices
/// </summary>
public interface IExoSonde
{
    /// <summary>
    /// Gets the current sample period from the EXO device.
    /// </summary>
    Task<TimeSpan> GetSamplePeriod();
    /// <summary>
    /// Sets the sample period for the EXO device.
    /// </summary>
    /// <param name="period">The time period between automatic samples.</param>
    Task SetSamplePeriod(TimeSpan period);
    /// <summary>
    /// Forces the EXO device to immediately take a sample.
    /// </summary>
    Task ForceSample();
    /// <summary>
    /// Gets the parameter codes that are currently configured to be read from the EXO device.
    /// </summary>
    Task<ParameterCode[]> GetParametersToRead();
    /// <summary>
    /// Sets the parameters to read from the EXO device.
    /// </summary>
    Task SetParametersToRead(ParameterCode[] parameters);
    /// <summary>
    /// Gets the current sensor data from the EXO device for all configured parameters.
    /// </summary>
    Task<(ParameterCode ParameterCode, object Value)[]> GetCurrentData();
    /// <summary>
    /// Gets the status of all parameters from the EXO device.
    /// </summary>
    Task<ParameterStatus[]> GetParameterStatus();
}
