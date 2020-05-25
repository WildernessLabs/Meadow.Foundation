using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Displays.Tft;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Temperature;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.I2cSpiAnalogTemperature_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Ili9163 displaySPI;
        GraphicsLibrary graphicsSPI;
        Ssd1306 displayI2C;
        GraphicsLibrary graphicsI2C;
        List<AnalogTemperature> temperatures;

        public MeadowApp()
        {
            Console.WriteLine("Start...");

            Console.Write("Initializing I2C...");
            displayI2C = new Ssd1306(Device.CreateI2cBus(), 60, Ssd1306.DisplayType.OLED128x32);
            graphicsI2C = new GraphicsLibrary(displayI2C);
            graphicsI2C.CurrentFont = new Font8x12();
            graphicsI2C.Clear();
            graphicsI2C.Stroke = 1;
            graphicsI2C.DrawRectangle(0, 0, 128, 32);
            graphicsI2C.DrawText(5, 12, "I2C WORKING");
            graphicsI2C.Show();
            Console.WriteLine("done");

            Console.Write("Initializing SPI...");
            displaySPI = new Ili9163(
                device: Device,
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D14,
                dcPin: Device.Pins.D11,
                resetPin: Device.Pins.D10,
                width: 128, height: 160);
            graphicsSPI = new GraphicsLibrary(displaySPI);
            graphicsSPI.Rotation = GraphicsLibrary.RotationType._90Degrees;
            graphicsSPI.Clear();
            graphicsSPI.Stroke = 1;
            graphicsSPI.DrawRectangle(0, 0, 160, 128);
            graphicsSPI.CurrentFont = new Font8x12();
            graphicsSPI.DrawText(7, 7, "SPI WORKING!", Color.White);
            graphicsSPI.Show();
            Console.WriteLine("done");

            temperatures = new List<AnalogTemperature>
            {
                new AnalogTemperature(Device, Device.Pins.A00, AnalogTemperature.KnownSensorType.LM35),
                new AnalogTemperature(Device, Device.Pins.A01, AnalogTemperature.KnownSensorType.LM35),
                new AnalogTemperature(Device, Device.Pins.A02, AnalogTemperature.KnownSensorType.LM35),
                new AnalogTemperature(Device, Device.Pins.A03, AnalogTemperature.KnownSensorType.LM35),
                //new AnalogTemperature(Device, Device.Pins.A04, AnalogTemperature.KnownSensorType.LM35), borked
                new AnalogTemperature(Device, Device.Pins.A05, AnalogTemperature.KnownSensorType.LM35),
            };

            TestTemperatures();
        }

        async Task TestTemperatures()
        {
            while (true)
            {
                int tempIndex = 0;
                float? temp;

                for (int i = 0; i < 6; i++)
                {
                    temp = null;

                    if (tempIndex < temperatures.Count)
                    {
                        if ($"A0{i}" == temperatures[tempIndex].AnalogInputPort.Pin.ToString())
                        {
                            var conditions = await temperatures[tempIndex].Read();
                            temp = conditions.Temperature;
                        }
                    }

                    if (temp == null)
                    {
                        graphicsSPI.DrawRectangle(7, i * 17 + 25, 130, 12, Color.Black, true);
                        graphicsSPI.DrawText(7, i * 17 + 25, $"A0{i}: INACTIVE", Color.White);
                    }
                    else
                    {
                        graphicsSPI.DrawRectangle(7, i * 17 + 25, 130, 12, Color.Black, true);
                        graphicsSPI.DrawText(7, i * 17 + 25, $"{temperatures[tempIndex].AnalogInputPort.Pin}: {temp}", Color.White);
                        tempIndex++;
                    }

                    graphicsSPI.Show();
                    Thread.Sleep(100);
                }

                Thread.Sleep(3000);
            }
        }
    }
}