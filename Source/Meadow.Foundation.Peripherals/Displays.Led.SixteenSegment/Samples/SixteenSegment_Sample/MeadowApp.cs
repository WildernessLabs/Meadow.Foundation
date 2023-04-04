using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.Led;
using System.Threading.Tasks;

namespace Displays.Led.SixteenSegment_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        SixteenSegment sixteenSegment;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            sixteenSegment = new SixteenSegment
            (
                portA: Device.CreateDigitalOutputPort(Device.Pins.D00),
                portB: Device.CreateDigitalOutputPort(Device.Pins.D01),
                portC: Device.CreateDigitalOutputPort(Device.Pins.D02),
                portD: Device.CreateDigitalOutputPort(Device.Pins.D03),
                portE: Device.CreateDigitalOutputPort(Device.Pins.D04),
                portF: Device.CreateDigitalOutputPort(Device.Pins.D05),
                portG: Device.CreateDigitalOutputPort(Device.Pins.D06),
                portH: Device.CreateDigitalOutputPort(Device.Pins.D07),
                portK: Device.CreateDigitalOutputPort(Device.Pins.D08),
                portM: Device.CreateDigitalOutputPort(Device.Pins.D09),
                portN: Device.CreateDigitalOutputPort(Device.Pins.D10),
                portP: Device.CreateDigitalOutputPort(Device.Pins.D11),
                portR: Device.CreateDigitalOutputPort(Device.Pins.D12),
                portS: Device.CreateDigitalOutputPort(Device.Pins.D13),
                portT: Device.CreateDigitalOutputPort(Device.Pins.D14),
                portU: Device.CreateDigitalOutputPort(Device.Pins.D15),
                portDecimal: Device.CreateDigitalOutputPort(Device.Pins.A00),
                isCommonCathode: false
            );

            return base.Initialize();
        }

        public override Task Run()
        {
            sixteenSegment.SetDisplay(asciiCharacter: 'Z', showDecimal: true);

            return base.Run();
        }

        //<!=SNOP=>
    }
}