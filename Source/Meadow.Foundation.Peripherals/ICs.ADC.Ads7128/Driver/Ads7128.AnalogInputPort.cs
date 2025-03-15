using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Meadow.Foundation.ICs.ADC;

public partial class Ads7128
{
    internal class AnalogInputPort : IAnalogInputPort
    {
        private readonly double conversionFactor;

        private readonly Ads7128Pin pin;

        public Voltage ReferenceVoltage => Controller.ReferenceVoltage;

        public IAnalogChannelInfo Channel { get; }
        public IPin Pin => pin;
        private Ads7128 Controller { get; }

        public AnalogInputPort(Ads7128Pin pin)
        {
            this.pin = pin;
            Channel = pin.SupportedChannels.OfType<IAnalogChannelInfo>().First();
            Controller = pin.Controller as Ads7128 ?? throw new System.Exception("Invalid controller");

            // configure PIN_CFG
            Controller.ClearRegisterBit(Registers.PinConfig, pin.Index); // 0 == AIN, 1 == DIO

            // set MANUAL_MODE
            Controller.ClearRegisterBits(Registers.OpModeConfig, 0b11 << 5); // clear bits 5 and 6

            conversionFactor = ReferenceVoltage.Volts / (Math.Pow(2, ADCPrecisionBits) - 1);
        }

        public void Dispose()
        {
            Controller.ReleasePin(pin);
        }

        public Task<Voltage> Read()
        {
            Span<byte> buffer = stackalloc byte[2];

            lock (Controller._syncRoot) // prevent other pins from accessing the registers
            {
                var reg = Controller.ReadRegister(Registers.ChannelSel);
                reg &= 0xf0;
                reg |= pin.Index;
                Controller.WriteRegister(Registers.ChannelSel, reg);
                Controller.ReadBytes(buffer);

                int raw = (buffer[0] << 8) | buffer[1];

                if (Controller.CurrrentOversampling == Oversampling.Samples_1)
                {
                    return Task.FromResult(((raw >> 4) * conversionFactor).Volts());
                }

                return Task.FromResult((raw * conversionFactor).Volts());
            }
        }
    }
}