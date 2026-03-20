using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Environmental.Ysi;

/// <summary>
/// A class providing simulated EXO sonde behavior with comprehensive test data for all parameters
/// </summary>
public class SimulatedExo : IExoSonde
{
    private TimeSpan _samplePeriod = TimeSpan.FromSeconds(30);
    private readonly Random _random = new Random();

    private ParameterCode[] _parameters = [
        ParameterCode.TemperatureC, ParameterCode.TemperatureF, ParameterCode.TemperatureK,
        ParameterCode.ConductivityuScm, ParameterCode.ConductivitymScm,
        ParameterCode.SpecificConductanceuScm, ParameterCode.SpecificConductancemScm,
        ParameterCode.nLFConductivityuScm, ParameterCode.nLFConductivitymScm,
        ParameterCode.Salinity, ParameterCode.pH, ParameterCode.pHmV, ParameterCode.ORP,
        ParameterCode.ODOPercentSat, ParameterCode.ODOmgL, ParameterCode.ODOPercentLocal,
        ParameterCode.TDSmgL, ParameterCode.TDSgL, ParameterCode.TDSkgL,
        ParameterCode.TSSmgL, ParameterCode.TSSgL,
        ParameterCode.PressurePsia, ParameterCode.PressurePsig,
        ParameterCode.DepthMeters, ParameterCode.DepthFeet,
        ParameterCode.VerticalPositionm, ParameterCode.VerticalPositionft,
        ParameterCode.TurbidityNTU, ParameterCode.TurbidityFNU,
        ParameterCode.NH3, ParameterCode.NH4, ParameterCode.NO3,
        ParameterCode.Chloride, ParameterCode.PotassiummgL,
        ParameterCode.ChlorophyllugL, ParameterCode.ChlorophyllRFU,
        ParameterCode.BGAPCugL, ParameterCode.BGAPEugL,
        ParameterCode.fDOMrfu, ParameterCode.fDOMqsu, ParameterCode.RhodamineugL,
        ParameterCode.PARChannel1, ParameterCode.PARChannel2,
        ParameterCode.BatteryVoltage, ParameterCode.ExternalPower,
        ParameterCode.WiperPosition, ParameterCode.WiperPeakCurrent
    ];

    private readonly ParameterStatus[] _status;

    /// <summary>
    /// Initializes a new instance of the SimulatedExo class with comprehensive test data
    /// </summary>
    public SimulatedExo()
    {
        _status = new ParameterStatus[_parameters.Length];
        Array.Fill(_status, ParameterStatus.Available);
    }

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
    public Task<(ParameterCode ParameterCode, IUnit Value)[]> GetCurrentData()
    {
        // Base temperature for consistency across temperature scales
        var baseTemp = 20.0 + (_random.NextDouble() - 0.5) * 4.0; // 18-22°C
        var tempF = (baseTemp * 9.0 / 5.0) + 32.0; // Convert to Fahrenheit
        var tempK = baseTemp + 273.15; // Convert to Kelvin

        var data = new List<(ParameterCode ParameterCode, IUnit Value)>
        {
            // Temperature (consistent values across scales)
            (ParameterCode.TemperatureC, new Units.Temperature(baseTemp, Units.Temperature.UnitType.Celsius)),
            (ParameterCode.TemperatureF, new Units.Temperature(tempF, Units.Temperature.UnitType.Fahrenheit)),
            (ParameterCode.TemperatureK, new Units.Temperature(tempK, Units.Temperature.UnitType.Kelvin)),

            // Conductivity (typical freshwater values)
            (ParameterCode.ConductivityuScm, new Conductivity(150 + _random.NextDouble() * 300, Conductivity.UnitType.MicroSiemensPerCentimeter)),
            (ParameterCode.ConductivitymScm, new Conductivity(0.15 + _random.NextDouble() * 0.3, Conductivity.UnitType.MilliSiemensPerCentimeter)),
            (ParameterCode.SpecificConductanceuScm, new Conductivity(160 + _random.NextDouble() * 290, Conductivity.UnitType.MicroSiemensPerCentimeter)),
            (ParameterCode.SpecificConductancemScm, new Conductivity(0.16 + _random.NextDouble() * 0.29, Conductivity.UnitType.MilliSiemensPerCentimeter)),
            (ParameterCode.nLFConductivityuScm, new Conductivity(155 + _random.NextDouble() * 295, Conductivity.UnitType.MicroSiemensPerCentimeter)),
            (ParameterCode.nLFConductivitymScm, new Conductivity(0.155 + _random.NextDouble() * 0.295, Conductivity.UnitType.MilliSiemensPerCentimeter)),

            // Basic water quality
            (ParameterCode.Salinity, new ConcentrationInWater(0.1 + _random.NextDouble() * 0.3, ConcentrationInWater.UnitType.PartsPerThousand)),
            (ParameterCode.pH, new PotentialHydrogen(7.0 + (_random.NextDouble() - 0.5) * 2.0)), // pH 6-8
            (ParameterCode.pHmV, new Voltage(-50 + _random.NextDouble() * 100, Voltage.UnitType.Millivolts)),
            (ParameterCode.ORP, new Voltage(200 + _random.NextDouble() * 200, Voltage.UnitType.Millivolts)),

            // Dissolved oxygen
            (ParameterCode.ODOPercentSat, new Scalar(85 + _random.NextDouble() * 15)), // 85-100% saturation
            (ParameterCode.ODOmgL, new ConcentrationInWater(7.5 + _random.NextDouble() * 2.5, ConcentrationInWater.UnitType.MilligramsPerLiter)), // 7.5-10 mg/L
            (ParameterCode.ODOPercentLocal, new Scalar(80 + _random.NextDouble() * 20)),

            // Total Dissolved Solids
            (ParameterCode.TDSmgL, new ConcentrationInWater(100 + _random.NextDouble() * 200, ConcentrationInWater.UnitType.MilligramsPerLiter)),
            (ParameterCode.TDSgL, new ConcentrationInWater(0.1 + _random.NextDouble() * 0.2, ConcentrationInWater.UnitType.GramsPerLiter)),
            (ParameterCode.TDSkgL, new ConcentrationInWater(0.0001 + _random.NextDouble() * 0.0002, ConcentrationInWater.UnitType.KilogramsPerLiter)),

            // Total Suspended Solids
            (ParameterCode.TSSmgL, new ConcentrationInWater(5 + _random.NextDouble() * 15, ConcentrationInWater.UnitType.MilligramsPerLiter)),
            (ParameterCode.TSSgL, new ConcentrationInWater(0.005 + _random.NextDouble() * 0.015, ConcentrationInWater.UnitType.GramsPerLiter)),

            // Pressure and depth
            (ParameterCode.PressurePsia, new Pressure(14.7 + _random.NextDouble() * 5, Pressure.UnitType.Psi)),
            (ParameterCode.PressurePsig, new Pressure(_random.NextDouble() * 5, Pressure.UnitType.Psi)),
            (ParameterCode.DepthMeters, new Length(1.0 + _random.NextDouble() * 3.0, Length.UnitType.Meters)),
            (ParameterCode.DepthFeet, new Length(3.0 + _random.NextDouble() * 10.0, Length.UnitType.Feet)),
            (ParameterCode.VerticalPositionm, new Length(1.0 + _random.NextDouble() * 3.0, Length.UnitType.Meters)),
            (ParameterCode.VerticalPositionft, new Length(3.0 + _random.NextDouble() * 10.0, Length.UnitType.Feet)),

            // Turbidity
            (ParameterCode.TurbidityNTU, new Turbidity(1.0 + _random.NextDouble() * 9.0, Turbidity.UnitType.NTU)),
            (ParameterCode.TurbidityFNU, new Scalar(1.0 + _random.NextDouble() * 9.0)),

            // Nutrients
            (ParameterCode.NH3, new ConcentrationInWater(0.01 + _random.NextDouble() * 0.09, ConcentrationInWater.UnitType.MilligramsPerLiter)),
            (ParameterCode.NH4, new ConcentrationInWater(0.05 + _random.NextDouble() * 0.15, ConcentrationInWater.UnitType.MilligramsPerLiter)),
            (ParameterCode.NO3, new ConcentrationInWater(0.5 + _random.NextDouble() * 1.5, ConcentrationInWater.UnitType.MilligramsPerLiter)),
            (ParameterCode.Chloride, new ConcentrationInWater(10 + _random.NextDouble() * 20, ConcentrationInWater.UnitType.MilligramsPerLiter)),
            (ParameterCode.PotassiummgL, new ConcentrationInWater(2 + _random.NextDouble() * 3, ConcentrationInWater.UnitType.MilligramsPerLiter)),

            // Chlorophyll and algae
            (ParameterCode.ChlorophyllugL, new ConcentrationInWater(5 + _random.NextDouble() * 15, ConcentrationInWater.UnitType.MicrogramsPerLiter)),
            (ParameterCode.ChlorophyllRFU, new Scalar(50 + _random.NextDouble() * 100)),
            (ParameterCode.BGAPCugL, new ConcentrationInWater(1 + _random.NextDouble() * 4, ConcentrationInWater.UnitType.MicrogramsPerLiter)),
            (ParameterCode.BGAPEugL, new ConcentrationInWater(0.5 + _random.NextDouble() * 2, ConcentrationInWater.UnitType.MicrogramsPerLiter)),

            // Fluorescence and organic matter
            (ParameterCode.fDOMrfu, new Scalar(20 + _random.NextDouble() * 30)),
            (ParameterCode.fDOMqsu, new Scalar(10 + _random.NextDouble() * 15)),
            (ParameterCode.RhodamineugL, new ConcentrationInWater(0.1 + _random.NextDouble() * 0.4, ConcentrationInWater.UnitType.MicrogramsPerLiter)),

            // PAR (Photosynthetically Active Radiation)
            (ParameterCode.PARChannel1, new Scalar(100 + _random.NextDouble() * 200)),
            (ParameterCode.PARChannel2, new Scalar(120 + _random.NextDouble() * 180)),

            // System monitoring
            (ParameterCode.BatteryVoltage, new Voltage(11.5 + _random.NextDouble() * 1.0, Voltage.UnitType.Volts)), // 11.5-12.5V
            (ParameterCode.ExternalPower, new Voltage(12.0 + _random.NextDouble() * 2.0, Voltage.UnitType.Volts)), // 12-14V
            (ParameterCode.WiperPosition, new Voltage(2.5 + _random.NextDouble() * 2.0, Voltage.UnitType.Volts)), // 2.5-4.5V
            (ParameterCode.WiperPeakCurrent, new Current(150 + _random.NextDouble() * 50, Current.UnitType.Milliamps)) // 150-200mA
        };

        return Task.FromResult(data.ToArray());
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
        _parameters = parameters;
        return Task.CompletedTask;
    }

}
