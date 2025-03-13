using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.ICs.ADC;

public partial class Ads7128 : IPinController, IAnalogInputController, IAnalogInputArrayController
{
    public static byte ADCPrecisionBits => 12;

    public PinDefinitions Pins { get; }

    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    /// <summary>
    /// I2C Communication bus used to communicate with the peripheral
    /// </summary>
    private readonly II2cCommunications i2cComms;

    public Ads7128(II2cBus i2cBus,
        Addresses address)
    {
        i2cComms = new I2cCommunications(i2cBus, (byte)address, 3);
    }

    private void Initialize()
    {
    }

    public IAnalogInputPort CreateAnalogInputPort(IPin pin, Voltage? voltageReference = null)
    {
        throw new System.NotImplementedException();
    }

    public IAnalogInputArray CreateAnalogInputArray(params IPin[] pins)
    {
        throw new System.NotImplementedException();
    }
}