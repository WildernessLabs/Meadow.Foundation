using System;
using System.Collections.Generic;
using Meadow;
using Meadow.Devices;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {

        public MeadowApp()
        {
            Initialize();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");
        }

        public List<string> GetSampleNmeaSentences()
        {
            List<string> sentences = new List<string>() {
                "$GPGGA,000049.799,,,,,0,00,,,M,,M,,*72",
                "$GPGSA,A,1,,,,,,,,,,,,,,,*1E",
                "$GPRMC,000049.799,V,,,,,0.00,0.00,060180,,,N*48",
                "$GPVTG,0.00,T,,M,0.00,N,0.00,K,N*32",
                "$GPRMC012539.800,V,,,,,0.00,0.00,060180,,,N*78",

            };

            return sentences;

        }
    }
}