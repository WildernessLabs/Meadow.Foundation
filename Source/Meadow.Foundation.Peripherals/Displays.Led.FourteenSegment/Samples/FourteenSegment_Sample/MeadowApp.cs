using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.Led;
using System.Threading.Tasks;

namespace Displays.Led.FourteenSegment_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        FourteenSegment fourteenSegment;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            fourteenSegment = new FourteenSegment
            (
                portA: Device.CreateDigitalOutputPort(Device.Pins.D00),
                portB: Device.CreateDigitalOutputPort(Device.Pins.D01),
                portC: Device.CreateDigitalOutputPort(Device.Pins.D02),
                portD: Device.CreateDigitalOutputPort(Device.Pins.D03),
                portE: Device.CreateDigitalOutputPort(Device.Pins.D04),
                portF: Device.CreateDigitalOutputPort(Device.Pins.D05),
                portG: Device.CreateDigitalOutputPort(Device.Pins.D06),
                portG2: Device.CreateDigitalOutputPort(Device.Pins.D07),
                portH: Device.CreateDigitalOutputPort(Device.Pins.D08),
                portJ: Device.CreateDigitalOutputPort(Device.Pins.D09),
                portK: Device.CreateDigitalOutputPort(Device.Pins.D10),
                portL: Device.CreateDigitalOutputPort(Device.Pins.D11),
                portM: Device.CreateDigitalOutputPort(Device.Pins.D12),
                portN: Device.CreateDigitalOutputPort(Device.Pins.D13),
                portDecimal: Device.CreateDigitalOutputPort(Device.Pins.D14),
                isCommonCathode: false
            );

            return base.Initialize();
        }

        public override Task Run()
        {
            fourteenSegment.SetDisplay(asciiCharacter: 'A', showDecimal: true);

            return base.Run();
        }

        //<!=SNOP=>
    }
}