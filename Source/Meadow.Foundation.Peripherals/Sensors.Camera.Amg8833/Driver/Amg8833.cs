using Meadow.Hardware;
using System.Threading;

namespace Meadow.Foundation.Sensors.Camera;

public partial class Amg8833 : II2cPeripheral
{
    private II2cCommunications _i2cComms;

    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    public Amg8833(II2cBus i2cBus,
        byte address = (byte)Addresses.Default,
        float emissivity = 0.95f)
    {
        _i2cComms = new I2cCommunications(i2cBus, address);

        Initialize();
    }

    private void Initialize()
    {
        SetPowerMode(Modes.Normal);
        WriteRegister(Registers.RST, Commands.RST_InitialReset);
        DisableInterrupts();
        WriteRegister(Registers.FPSC, Commands.FPS_Ten);

        Thread.Sleep(Constants.StartupDelayMs);
    }

    private void SetPowerMode(Modes mode)
    {
        WriteRegister(Registers.PCTL, Modes.Normal);
    }

    private void DisableInterrupts()
    {
        WriteRegister(Registers.INTC, (byte)0x00);
    }

    private void EnableInterrupts(InterruptModes mode)
    {
        WriteRegister(Registers.INTC, (byte)(0x01 | (byte)mode));
    }

    private void WriteRegister(Registers register, byte value)
    {
        _i2cComms.WriteRegister((byte)register, value);
    }

    private void WriteRegister(Registers register, Modes mode)
    {
        WriteRegister(register, (byte)mode);
    }

    private void WriteRegister(Registers register, Commands command)
    {
        WriteRegister(register, (byte)command);
    }
}