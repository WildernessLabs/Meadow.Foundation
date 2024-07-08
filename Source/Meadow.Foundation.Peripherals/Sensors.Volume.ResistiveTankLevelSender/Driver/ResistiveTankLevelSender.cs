using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Volume;

/*

Wiring:
  R1    == high-side reistor.  For a 33-240 ohm sender, use a 47-ohm
  R2    == low-side resistor. For a 33-240 ohm sender, use a 120-ohm
  Vref  == Assumes 3.3-5V and is safe for a 3.3V ADC (AN_IN)
  AN_IN == Analog input port
  Sender nets are the 2 wires from the sender.  Non-polarized.

VRef---[R1]-----<Sender A]

GND----[R2]--+--<Sender B]
             |
             +--<AN_IN]

*/

/// <summary>
/// Represents a generic resistive tank level sender sensor.
/// </summary>
public abstract class ResistiveTankLevelSender : SamplingSensorBase<int>, IDisposable
{
    private int _fillLevel = int.MinValue;
    private bool _portCreated = false;

    /// <summary>
    /// Occurs when the fill level changes.
    /// </summary>
    public event EventHandler<int>? FillLevelChanged;

    private IAnalogInputPort AnalogInput { get; }
    private Voltage VRef { get; }
    private Resistance Resistor1 { get; }
    private Resistance Resistor2 { get; }

    /// <summary>
    /// Gets a value indicating whether the object is disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Gets the mapping of resistance (in ohms) to fill level (in 0-100 percent).
    /// </summary>

    // DEV NOTE: Testing has shown the sender is both quantized and non-linear, so the best route is just a lookup table.  Math will *not* work for these senders.
    //           Each length of sender, and resistance range of senders will have a different lookup table
    protected abstract Dictionary<int, int> ResistanceToFillLevelMap { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResistiveTankLevelSender"/> class with the specified input pin and reference voltage.
    /// </summary>
    /// <param name="inputPin">The input pin.</param>
    /// <param name="vRef">The reference voltage.</param>
    protected ResistiveTankLevelSender(
        IPin inputPin,
        Voltage vRef)
        : this(inputPin, vRef, 120.Ohms(), 47.Ohms())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResistiveTankLevelSender"/> class with the specified input pin and reference voltage.
    /// </summary>
    /// <param name="inputPort">The input port.</param>
    /// <param name="vRef">The reference voltage.</param>
    protected ResistiveTankLevelSender(
        IAnalogInputPort inputPort,
        Voltage vRef)
        : this(inputPort, vRef, 120.Ohms(), 47.Ohms())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResistiveTankLevelSender"/> class with the specified input pin, reference voltage, low-side resistor, and high-side resistor.
    /// </summary>
    /// <param name="inputPin">The input pin.</param>
    /// <param name="vRef">The reference voltage.</param>
    /// <param name="lowSideResistor">The resistance of the low-side resistor.</param>
    /// <param name="highSideResistor">The resistance of the high-side resistor.</param>
    protected ResistiveTankLevelSender(
        IPin inputPin,
        Voltage vRef,
        Resistance lowSideResistor,
        Resistance highSideResistor
        )
        : this(inputPin.CreateAnalogInputPort(), vRef, lowSideResistor, highSideResistor)
    {
        _portCreated = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResistiveTankLevelSender"/> class with the specified input pin, reference voltage, low-side resistor, and high-side resistor.
    /// </summary>
    /// <param name="inputPort">The input port.</param>
    /// <param name="vRef">The reference voltage.</param>
    /// <param name="lowSideResistor">The resistance of the low-side resistor.</param>
    /// <param name="highSideResistor">The resistance of the high-side resistor.</param>
    protected ResistiveTankLevelSender(
        IAnalogInputPort inputPort,
        Voltage vRef,
        Resistance lowSideResistor,
        Resistance highSideResistor
        )
    {
        VRef = vRef;

        AnalogInput = inputPort;
        AnalogInput.Updated += OnInputUpdated;

        Resistor2 = lowSideResistor;
        Resistor1 = highSideResistor;
    }

    /// <summary>
    /// Gets the measured fill level percentage.
    /// </summary>
    public int FillLevelPercent
    {
        get => _fillLevel;
        private set
        {
            if (value == FillLevelPercent) { return; }
            _fillLevel = value;
            FillLevelChanged?.Invoke(this, FillLevelPercent);
        }
    }

    private int GetFillLevelForResistance(double resistance)
    {
        return ResistanceToFillLevelMap.OrderBy(item => Math.Abs(resistance - item.Key)).First().Value;
    }

    private void OnInputUpdated(object? sender, IChangeResult<Voltage> e)
    {
        var sensedVoltage = e.New;
        var gaugeResistance = ((VRef.Volts * Resistor2.Ohms) / sensedVoltage.Volts) - Resistor1.Ohms - Resistor2.Ohms;
        FillLevelPercent = GetFillLevelForResistance(gaugeResistance);
    }

    /// <inheritdoc/>
    protected override Task<int> ReadSensor()
    {
        return Task.FromResult(_fillLevel);
    }

    /// <inheritdoc/>
    public override void StartUpdating(TimeSpan? updateInterval = null)
    {
        AnalogInput.StartUpdating(updateInterval);
    }

    /// <inheritdoc/>
    public override void StopUpdating()
    {
        AnalogInput?.StopUpdating();
    }

    /// <inheritdoc/>
    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                if (_portCreated) { AnalogInput.Dispose(); }
            }

            IsDisposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
