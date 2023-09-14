using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders;

internal interface IFtdiImpl : IDisposable
{
    void Initialize();
    II2cBus CreateI2cBus(int busNumber, I2CClockRate clock);
    ISpiBus CreateSpiBus(int busNumber, SpiClockConfiguration config);
    IDigitalInputPort CreateDigitalInputPort(int channel, IPin pin, ResistorMode resistorMode);
    IDigitalOutputPort CreateDigitalOutputPort(int channel, IPin pin, bool initialState = false, OutputType initialOutputType = OutputType.PushPull);
}
