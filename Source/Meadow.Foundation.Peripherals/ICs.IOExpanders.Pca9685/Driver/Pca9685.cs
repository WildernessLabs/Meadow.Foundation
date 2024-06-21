using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents a PCA9685
/// </summary>
/// <remarks>All PWM channels run at the same Frequency</remarks>
public partial class Pca9685 : II2cPeripheral, IDigitalOutputController, IPwmOutputController, IDisposable
{
    private readonly byte address;

    private Dictionary<int, IPwmPort> pwmPortCache = new();
    private Dictionary<int, IDigitalOutputPort> outputPortCache = new();
    private bool isDisposed;

    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    /// <summary>
    /// I2C Communication bus used to communicate with the peripheral
    /// </summary>
    protected readonly II2cCommunications i2cComms;

    /// <summary>
    /// The I2C bus connected to the pca9685
    /// </summary>
    protected II2cBus i2cBus { get; set; }

    /// <summary>
    /// PCA9685 pin definitions
    /// </summary>
    public PinDefinitions Pins { get; }

    /// <summary>
    /// The frequency for the PWM outputs
    /// </summary>
    /// <remarks>
    /// All PWMs on the part share the same frequency
    /// </remarks>
    public Frequency Frequency
    {
        get;
    }

    /// <summary>
    /// Create a new Pca9685 object
    /// </summary>
    /// <param name="i2cBus">The I2C bus connected to the peripheral</param>
    /// <param name="frequency">The frequency</param>
    /// <param name="address">The I2C address</param>
    public Pca9685(II2cBus i2cBus, Frequency frequency, byte address = (byte)Addresses.Default)
    {
        Pins = new PinDefinitions(this)
        {
            Controller = this
        };

        this.Frequency = frequency;
        this.i2cBus = i2cBus;
        this.address = address;
        i2cComms = new I2cCommunications(this.i2cBus, address);

        Initialize();
    }

    /// <summary>
    /// Create a new Pca9685 object with a 50Hz PWM frequency
    /// </summary>
    /// <param name="i2cBus">The I2C bus connected to the peripheral</param>
    /// <param name="address">The I2C address</param>
    public Pca9685(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        : this(i2cBus, new Frequency(50, Frequency.UnitType.Hertz), address)
    {
    }

    /// <summary>
    /// Initializes the PCA9685 IC
    /// </summary>
    private void Initialize()
    {
        i2cBus.Write(address, new byte[] { Registers.Mode1, 0x00 });

        Thread.Sleep(5);

        SetFrequency(Frequency);

        for (byte i = 0; i < 16; i++)
        {
            SetPwm(i, 0, 0);
        }
    }

    /*
    /// <summary>
    /// Turns the specified pin On or Off
    /// </summary>
    /// <param name="pin">The pin to set</param>
    /// <param name="on">true is on, false if off</param>
    public virtual void SetPin(byte pin, bool on)
    {
        if (pin is < 0 or > 15)
        {
            throw new ArgumentException("PWM pin must be between 0 and 15");
        }

        SetPwm(pin, on ? 4096 : 0, 0);
    }
    */

    /// <summary>
    /// Set the values for specified output pin.
    /// </summary>
    /// <param name="pin">The pwm Pin</param>
    /// <param name="on">LED{X}_ON_L and LED{X}_ON_H register value</param>
    /// <param name="off">LED{X}_OFF_L and LED{X}_OFF_H register value</param>
    /// <remarks>On parameter is an inverted pwm signal</remarks>
    private void SetPwm(byte pin, int on, int off)
    {
        if (pin is < 0 or > 15)
        {
            throw new ArgumentException("Value has to be between 0 and 15", nameof(pin));
        }

        if (on is < 0 or > 4096)
        {
            throw new ArgumentException("Value has to be between 0 and 4096", nameof(on));
        }

        if (off is < 0 or > 4096)
        {
            throw new ArgumentException("Value has to be between 0 and 4096", nameof(off));
        }

        Write((byte)(Registers.Led0OnL + (4 * pin)), (byte)(on & 0xFF), (byte)(on >> 8), (byte)(off & 0xFF), (byte)(off >> 8));
    }

    private void Write(byte register, byte ledXOnL, byte ledXOnH, byte ledXOffL, byte ledXOffH)
    {
        i2cComms.Write(new byte[] { register, ledXOnL, ledXOnH, ledXOffL, ledXOffH });
    }

    private void Write(byte register, byte value)
    {
        i2cComms.WriteRegister(register, value);
    }

    private void SetFrequency(Frequency frequency)
    {
        var prescaleval = 25_000_000d;  //  # 25MHz
        prescaleval /= 4096.0;       //# 12-bit
        prescaleval /= frequency.Hertz;
        prescaleval -= 1.0;

        double prescale = Math.Floor(prescaleval + 0.5);
        byte oldmode = i2cComms.ReadRegister(Registers.Mode1);
        byte newmode = (byte)((oldmode & ~Mode1.Restart) | Mode1.Sleep);         //   # sleep

        Write(Registers.Mode1, newmode);//       # go to sleep
        Write(Registers.PreScale, (byte)((int)(Math.Floor(prescale))));
        Write(Registers.Mode1, oldmode);

        Thread.Sleep(5);
        Write(Registers.Mode1, (byte)(oldmode | Mode1.Restart | Mode1.AutoIncrement));
    }

    /// <inheritdoc/>
    public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType initialOutputType = OutputType.PushPull)
    {
        var portNumber = (byte)pin.Key;

        if (portNumber is < 0 or > 15)
        {
            throw new ArgumentException("Value must be between 0 and 15", nameof(portNumber));
        }

        if (pwmPortCache.ContainsKey(portNumber))
        {
            throw new ArgumentException("Pin is already in use", nameof(portNumber));

        }

        if (outputPortCache.ContainsKey(portNumber))
        {
            return outputPortCache[portNumber];
        }

        var outputPort = new DigitalOutputPort(this, pin, initialState);
        outputPortCache.Add(portNumber, outputPort);

        return outputPort;
    }

    /// <inheritdoc/>
    public IPwmPort CreatePwmPort(IPin pin, float dutyCycle = 0.5F, bool invert = false)
    {
        return CreatePwmPort(pin, Frequency, dutyCycle, invert);
    }

    /// <inheritdoc/>
    public IPwmPort CreatePwmPort(IPin pin, Frequency frequency, float dutyCycle = 0.5F, bool invert = false)
    {
        var portNumber = (byte)pin.Key;

        if (portNumber is < 0 or > 15)
        {
            throw new ArgumentException("Value must be between 0 and 15", nameof(portNumber));
        }

        if (pwmPortCache.ContainsKey(portNumber))
        {
            return pwmPortCache[portNumber];
        }

        if (outputPortCache.ContainsKey(portNumber))
        {
            throw new ArgumentException("Pin is already in use", nameof(portNumber));
        }

        var pwmPort = new PwmPort(this, pin, frequency, dutyCycle, invert);

        pwmPortCache.Add(portNumber, pwmPort);

        return pwmPort;
    }

    /// <inheritdoc/>
    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                foreach (var p in pwmPortCache.Values)
                {
                    p.Dispose();
                }
            }

            isDisposed = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}