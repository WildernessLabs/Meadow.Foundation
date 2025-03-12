using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Environmental;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;
using static Meadow.Foundation.Sensors.Environmental.Y4000;

namespace Meadow.Foundation.Sensors.Environmental;

/// <summary>
/// Represents a simulated Yosemitech Y4000 Multiparameter Sonde
/// </summary>
public class SimulatedY4000 :
    IY4000
{
    private Timer updateTimer;
    private TimeSpan? updateInterval = null;
    private Measurements lastMeasurements;
    private readonly Random random = new Random();

    private EventHandler<IChangeResult<Units.Temperature>>? tempEvents;
    private EventHandler<IChangeResult<Turbidity>>? turbidityEvents;
    private EventHandler<IChangeResult<PotentialHydrogen>>? phEvents;
    private EventHandler<IChangeResult<Conductivity>>? conductivityEvents;
    private EventHandler<IChangeResult<Voltage>>? redoxEvents;
    private EventHandler<IChangeResult<WaterQualityConcentrations>>? concentrationEvents;

    Units.Temperature? ITemperatureSensor.Temperature => lastMeasurements.Temperature;
    Turbidity? ITurbiditySensor.Turbidity => lastMeasurements.Turbidity;
    PotentialHydrogen? IPotentialHydrogenSensor.pH => lastMeasurements.PH;
    Conductivity? IElectricalConductivitySensor.Conductivity => lastMeasurements.ElectricalConductivity;
    Voltage? IRedoxPotentialSensor.Potential => lastMeasurements.OxidationReductionPotential;
    WaterQualityConcentrations? IWaterQualityConcentrationsSensor.Concentrations => lastMeasurements.Concentrations;

    event EventHandler<IChangeResult<Units.Temperature>> ISamplingSensor<Units.Temperature>.Updated
    {
        add => tempEvents += value;
        remove => tempEvents -= value;
    }

    event EventHandler<IChangeResult<Turbidity>> ISamplingSensor<Turbidity>.Updated
    {
        add => turbidityEvents += value;
        remove => turbidityEvents -= value;
    }

    event EventHandler<IChangeResult<PotentialHydrogen>> ISamplingSensor<PotentialHydrogen>.Updated
    {
        add => phEvents += value;
        remove => phEvents -= value;
    }

    event EventHandler<IChangeResult<Conductivity>> ISamplingSensor<Conductivity>.Updated
    {
        add => conductivityEvents += value;
        remove => conductivityEvents -= value;
    }

    event EventHandler<IChangeResult<Voltage>> ISamplingSensor<Voltage>.Updated
    {
        add => redoxEvents += value;
        remove => redoxEvents -= value;
    }

    event EventHandler<IChangeResult<WaterQualityConcentrations>> ISamplingSensor<WaterQualityConcentrations>.Updated
    {
        add => concentrationEvents += value;
        remove => concentrationEvents -= value;
    }

    /// <inheritdoc/>
    public TimeSpan UpdateInterval => updateInterval ?? TimeSpan.FromSeconds(5);

    /// <inheritdoc/>
    public bool IsSampling => updateInterval != null;

    /// <summary>
    /// Creates a SimulatedY4000 instance
    /// </summary>
    public SimulatedY4000()
    {
        updateTimer = new Timer(UpdateTimerProc);

        lastMeasurements = new Measurements
        {
            Temperature = 65.Fahrenheit(),
            PH = new PotentialHydrogen(7.0),
            ElectricalConductivity = new Conductivity(0.5),
            OxidationReductionPotential = 1.2.Volts(),
            Turbidity = new Turbidity(2.3),
            Concentrations = new WaterQualityConcentrations
            {
                DissolvedOxygen = new ConcentrationInWater(0.2),
                Chlorophyl = new ConcentrationInWater(0.3),
                BlueGreenAlgae = new ConcentrationInWater(0.4)
            }
        };
    }

    /// <inheritdoc/>
    public Task<string> GetSerialNumber()
    {
        return Task.FromResult("Simulated");
    }

    /// <inheritdoc/>
    public void StartUpdating(TimeSpan? updateInterval)
    {
        this.updateInterval = updateInterval ?? TimeSpan.FromSeconds(5);
        updateTimer.Change(TimeSpan.Zero, this.updateInterval.Value);
    }

    /// <inheritdoc/>
    public void StopUpdating()
    {
        this.updateInterval = null;
        updateTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    private async void UpdateTimerProc(object _)
    {
        var currentmeasurements = await ReadSensor();
        RaiseEventsAndNotify(currentmeasurements);
    }

    /// <summary>
    /// Reads all measurements of the sensor
    /// </summary>
    public Task<Measurements> ReadSensor()
    {
        var newTemp = lastMeasurements.Temperature.Fahrenheit + random.Next(0, 10) / 10d - 0.5;
        var newpH = lastMeasurements.PH.pH + random.Next(0, 100) / 100d - 0.05;
        var m = new Measurements
        {
            Temperature = newTemp.Fahrenheit(),
            PH = new PotentialHydrogen(newpH),
            ElectricalConductivity = new Conductivity(0.5),
            OxidationReductionPotential = 1.2.Volts(),
            Turbidity = new Turbidity(2.3),
            Concentrations = new WaterQualityConcentrations
            {
                DissolvedOxygen = new ConcentrationInWater(0.2),
                Chlorophyl = new ConcentrationInWater(0.3),
                BlueGreenAlgae = new ConcentrationInWater(0.4)
            }
        };

        return Task.FromResult(m);
    }

    async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
    {
        Measurements measurements;

        if (!IsSampling)
        {
            measurements = await ReadSensor();
        }
        else
        {
            measurements = lastMeasurements;
        }
        return measurements.Temperature;
    }

    async Task<Turbidity> ISensor<Turbidity>.Read()
    {
        Measurements measurements;

        if (!IsSampling)
        {
            measurements = await ReadSensor();
        }
        else
        {
            measurements = lastMeasurements;
        }
        return measurements.Turbidity;
    }

    async Task<PotentialHydrogen> ISensor<PotentialHydrogen>.Read()
    {
        Measurements measurements;

        if (!IsSampling)
        {
            measurements = await ReadSensor();
        }
        else
        {
            measurements = lastMeasurements;
        }
        return measurements.PH;
    }

    async Task<Conductivity> ISensor<Conductivity>.Read()
    {
        Measurements measurements;

        if (!IsSampling)
        {
            measurements = await ReadSensor();
        }
        else
        {
            measurements = lastMeasurements;
        }
        return measurements.ElectricalConductivity;
    }

    async Task<Voltage> ISensor<Voltage>.Read()
    {
        Measurements measurements;

        if (!IsSampling)
        {
            measurements = await ReadSensor();
        }
        else
        {
            measurements = lastMeasurements;
        }
        return measurements.OxidationReductionPotential;
    }

    async Task<WaterQualityConcentrations> ISensor<WaterQualityConcentrations>.Read()
    {
        Measurements measurements;

        if (!IsSampling)
        {
            measurements = await ReadSensor();
        }
        else
        {
            measurements = lastMeasurements;
        }
        return measurements.Concentrations;
    }

    private void RaiseEventsAndNotify(Measurements currentmeasurements)
    {
        tempEvents?.Invoke(this, new ChangeResult<Units.Temperature>(currentmeasurements.Temperature, lastMeasurements.Temperature));
        turbidityEvents?.Invoke(this, new ChangeResult<Turbidity>(currentmeasurements.Turbidity, lastMeasurements.Turbidity));
        phEvents?.Invoke(this, new ChangeResult<PotentialHydrogen>(currentmeasurements.PH, lastMeasurements.PH));
        conductivityEvents?.Invoke(this, new ChangeResult<Conductivity>(currentmeasurements.ElectricalConductivity, lastMeasurements.ElectricalConductivity));
        redoxEvents?.Invoke(this, new ChangeResult<Voltage>(currentmeasurements.OxidationReductionPotential, lastMeasurements.OxidationReductionPotential));
        concentrationEvents?.Invoke(this, new ChangeResult<WaterQualityConcentrations>(currentmeasurements.Concentrations, lastMeasurements.Concentrations));

        lastMeasurements = currentmeasurements;
    }
}
