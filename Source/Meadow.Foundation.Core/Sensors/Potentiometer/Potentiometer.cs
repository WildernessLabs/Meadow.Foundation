using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Sensors;

/// <summary>
/// Represents a potentiometer - a variable resistor that can be measured through an analog input.
/// Implements both IPotentiometer and IDisposable interfaces.
/// </summary>
public class Potentiometer : IPotentiometer, IDisposable
{
    /// <inheritdoc/>
    public event EventHandler<IChangeResult<Resistance>>? Changed;

    private IAnalogInputPort inputPort;
    private Voltage referenceVoltage;
    private bool portCreated = false;
    private Resistance? oldValue;

    /// <summary>
    /// Gets whether this instance has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Gets the maximum resistance value of the potentiometer.
    /// </summary>
    public Resistance MaxResistance { get; private set; }

    /// <summary>
    /// Initializes a new instance of the Potentiometer class with default reference voltage.
    /// </summary>
    /// <param name="inputPort">The input port to read the potentiometer value from.</param>
    /// <param name="maxResistance">The maximum resistance value of the potentiometer.</param>
    public Potentiometer(IAnalogInputPort inputPort, Resistance maxResistance)
    {
        Initialize(inputPort, maxResistance, inputPort.ReferenceVoltage);
        portCreated = false;
    }

    /// <summary>
    /// Initializes a new instance of the Potentiometer class with default reference voltage.
    /// </summary>
    /// <param name="inputPin">The input pin to read the potentiometer value from.</param>
    /// <param name="maxResistance">The maximum resistance value of the potentiometer.</param>
    public Potentiometer(IPin inputPin, Resistance maxResistance)
    {
        inputPort = inputPin.CreateAnalogInputPort(1);
        MaxResistance = maxResistance;
        referenceVoltage = inputPort.ReferenceVoltage;
        portCreated = true;
    }

    /// <summary>
    /// Initializes a new instance of the Potentiometer class with a specified reference voltage.
    /// </summary>
    /// <param name="inputPin">The input pin to read the potentiometer value from.</param>
    /// <param name="maxResistance">The maximum resistance value of the potentiometer.</param>
    /// <param name="referenceVoltage">The reference voltage used for analog-to-digital conversion.</param>
    public Potentiometer(IPin inputPin, Resistance maxResistance, Voltage referenceVoltage)
    {
        inputPort = inputPin.CreateAnalogInputPort(1);
        MaxResistance = maxResistance;
        this.referenceVoltage = referenceVoltage;
        portCreated = true;
    }

    private void Initialize(IAnalogInputPort inputPort, Resistance maxResistance, Voltage refereceVoltage)
    {
        this.inputPort = inputPort;
        MaxResistance = maxResistance;
        referenceVoltage = inputPort.ReferenceVoltage;

        inputPort.Updated += OnInputPortUpdated;
    }

    private void OnInputPortUpdated(object sender, IChangeResult<Voltage> e)
    {
        var newValue = new Resistance(MaxResistance.Ohms * e.New.Volts / referenceVoltage.Volts);
        Changed?.Invoke(this, new ChangeResult<Resistance>(newValue, oldValue));
        oldValue = newValue;
    }

    /// <summary>
    /// Gets the current resistance value of the potentiometer by reading the analog input.
    /// Setting the resistance throws NotSupportedException as potentiometer value can only be changed mechanically.
    /// </summary>
    /// <exception cref="NotSupportedException">Thrown when attempting to set the resistance value.</exception>
    public Resistance Resistance
    {
        get
        {
            var adcVoltage = inputPort.Read().Result;
            return new Resistance(MaxResistance.Ohms * adcVoltage.Volts / referenceVoltage.Volts);
        }
        set
        {
            throw new NotSupportedException("Standard potentiometer resistance is set mechanically");
        }
    }

    /// <summary>
    /// Releases the unmanaged resources used by the Potentiometer and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                if (portCreated) inputPort.Dispose();
            }
            IsDisposed = true;
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}