using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Collections.Concurrent;

namespace Meadow.Foundation.ICs.ADC;

public partial class Ads7128 : IPinController, IAnalogInputController, IAnalogInputArrayController
{
    public const byte ADCPrecisionBits = 12;

    public PinDefinitions Pins { get; }
    public Oversampling CurrrentOversampling { get; }
    public Voltage ReferenceVoltage => vRef;

    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    private II2cBus _i2cBus;
    private Addresses _busAddress;
    private Mode _currentMode = Mode.NotSet;
    internal object _syncRoot = new();
    private Voltage vRef = 3.3.Volts();

    public Ads7128(II2cBus i2cBus,
        Addresses address)
    {
        _i2cBus = i2cBus;
        _busAddress = address;

        Pins = new PinDefinitions(this)
        {
            Controller = this
        };
    }

    private Status ReadStatus()
    {
        return (Status)ReadRegister(Registers.SystemStatus);
    }

    private void SetOversampling(Oversampling oversampling)
    {
        WriteRegister(Registers.OsrConfig, (byte)oversampling);
    }

    private Oversampling GetOversampling()
    {
        return (Oversampling)ReadRegister(Registers.OsrConfig);
    }

    private void WriteRegister(Registers register, byte value)
    {
        Span<byte> buffer = stackalloc byte[3];
        buffer[0] = (byte)Opcodes.RegisterWrite;
        buffer[1] = (byte)register;
        buffer[2] = value;

        _i2cBus.Write((byte)_busAddress, buffer);
    }

    private byte ReadRegister(Registers register)
    {
        Span<byte> buffer = stackalloc byte[2];
        buffer[0] = (byte)Opcodes.RegisterRead;
        buffer[1] = (byte)register;

        _i2cBus.Write((byte)_busAddress, buffer);
        _i2cBus.Read((byte)_busAddress, buffer[..1]);
        return buffer[0];
    }

    private void ReadBytes(Span<byte> buffer)
    {
        _i2cBus.Read((byte)_busAddress, buffer);
    }

    private void ClearRegisterBit(Registers register, int bitOffset)
    {
        if (bitOffset > 7) throw new ArgumentException();

        Span<byte> buffer = stackalloc byte[3];
        buffer[0] = (byte)Opcodes.ClearBit;
        buffer[1] = (byte)register;
        buffer[2] = (byte)(1 << bitOffset);

        _i2cBus.Write((byte)_busAddress, buffer);
    }

    private void ClearRegisterBits(Registers register, byte bitsToClear)
    {
        Span<byte> buffer = stackalloc byte[3];
        buffer[0] = (byte)Opcodes.ClearBit;
        buffer[1] = (byte)register;
        buffer[2] = bitsToClear;

        _i2cBus.Write((byte)_busAddress, buffer);
    }

    private void SetRegisterBit(Registers register, int bitOffset)
    {
        if (bitOffset > 7) throw new ArgumentException();

        Span<byte> buffer = stackalloc byte[3];
        buffer[0] = (byte)Opcodes.SetBit;
        buffer[1] = (byte)register;
        buffer[2] = (byte)(1 << bitOffset);

        _i2cBus.Write((byte)_busAddress, buffer);
    }

    private void SetRegisterBits(Registers register, byte bitsToSet)
    {
        Span<byte> buffer = stackalloc byte[3];
        buffer[0] = (byte)Opcodes.SetBit;
        buffer[1] = (byte)register;
        buffer[2] = bitsToSet;

        _i2cBus.Write((byte)_busAddress, buffer);
    }

    private ConcurrentDictionary<Ads7128Pin, IAnalogInputPort> _inputCache = new();

    internal void ReleasePin(Ads7128Pin pin)
    {
        _inputCache.TryRemove(pin, out _);
    }

    public IAnalogInputPort CreateAnalogInputPort(IPin pin, Voltage? voltageReference = null)
    {
        switch (_currentMode)
        {
            case Mode.AutoSequence:
            case Mode.Autonomous:
                throw new Exception("Device cannot be used for individual InputPorts and InputArrays at the same time.");
        }
        _currentMode = Mode.Manual;

        var adsPin = pin as Ads7128Pin;
        if (adsPin == null) throw new ArgumentException("Pin must be from the ADS7128.Pins");

        if (_inputCache.ContainsKey(adsPin))
        {
            throw new ArgumentException($"Pin {pin.Name} is already in use");
        }

        var port = new Ads7128.AnalogInputPort(adsPin);
        _inputCache.TryAdd(adsPin, port);
        return port;
    }

    public IAnalogInputArray CreateAnalogInputArray(params IPin[] pins)
    {
        switch (_currentMode)
        {
            case Mode.Manual:
            case Mode.Autonomous:
                throw new Exception("Device cannot be used for individual InputPorts and InputArrays at the same time.");
        }
        _currentMode = Mode.AutoSequence;

        throw new System.NotImplementedException();
    }
}