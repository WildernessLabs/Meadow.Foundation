using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using System.Threading;
using System.Threading.Tasks;

namespace ICs.IOExpanders.HT16K33_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Ht16k33 ht16k33;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");
            ht16k33 = new Ht16k33(Device.CreateI2cBus());

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            ht16k33.Set14SegmentDisplay('F', 0);
            ht16k33.Set14SegmentDisplay('7', 1);
            ht16k33.Set14SegmentDisplay('v', 2);
            ht16k33.Set14SegmentDisplay('2', 3);

            ht16k33.UpdateDisplay();
            return Task.CompletedTask;
        }

        //<!=SNOP=>

        void LoopCharacters()
        {
            while (true)
            {
                for (int i = 32; i < 127; i++)
                {
                    ht16k33.Set14SegmentDisplay((char)i, 0);
                    ht16k33.Set14SegmentDisplay((char)i, 1);
                    ht16k33.Set14SegmentDisplay((char)i, 2);
                    ht16k33.Set14SegmentDisplay((char)i, 3);

                    ht16k33.UpdateDisplay();

                    Thread.Sleep(250);
                }
            }
        }
    }
}