using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Foundation.ICs.IOExpanders;

namespace BasicHT16K33_Sample
{
    public class HT16K33App : App<F7Micro, HT16K33App>
    {
        HT16K33 ht16k33;

        public HT16K33App()
        {
            var i2cBus = Device.CreateI2cBus();

            ht16k33 = new HT16K33(i2cBus);

            int index = 0;
            bool on = true;

            // write your code here
            while (true)
            {
                ht16k33.ToggleLed((byte)index, on);
                ht16k33.UpdateDisplay();
                index++;

                if(index >= 128)
                {
                    index = 0;
                    on = !on;
                }

                Thread.Sleep(100);
            }




        }
    }

}
