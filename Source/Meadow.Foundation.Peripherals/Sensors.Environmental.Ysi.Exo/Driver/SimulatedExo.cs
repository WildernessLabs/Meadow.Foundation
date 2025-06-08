using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental.Ysi;

/// <summary>
/// A class providing simulated EXO sonde behavior
/// </summary>
public class SimulatedExo : IExoSonde
{
    private TimeSpan _samplePeriod = TimeSpan.FromSeconds(30);
    private readonly ParameterCode[] _parameters = [ParameterCode.TemperatureF];
    private readonly ParameterStatus[] _status = [ParameterStatus.Available];

    /// <inheritdoc/>
    public Task<TimeSpan> GetSamplePeriod()
    {
        return Task.FromResult(_samplePeriod);
    }

    /// <inheritdoc/>
    public Task SetSamplePeriod(TimeSpan period)
    {
        _samplePeriod = period;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task ForceSample()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<(ParameterCode ParameterCode, object Value)[]> GetCurrentData()
    {

        var data = new (ParameterCode ParameterCode, object Value)[]
        {
            new (ParameterCode.TemperatureF, 72.1)
        };

        return Task.FromResult(data);
    }

    /// <inheritdoc/>
    public Task<ParameterStatus[]> GetParameterStatus()
    {
        return Task.FromResult(_status);
    }

    /// <inheritdoc/>
    public Task<ParameterCode[]> GetParametersToRead()
    {
        return Task.FromResult(_parameters);
    }

    /// <inheritdoc/>
    public Task SetParametersToRead(ParameterCode[] parameters)
    {
        throw new NotImplementedException();
    }

}
