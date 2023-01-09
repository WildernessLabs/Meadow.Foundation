using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Camera;
using Meadow.Hardware;

namespace Sensors.Temperature.MLX90640_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Mlx90640 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.Fast);
            sensor = new Mlx90640(i2cBus);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            bool showTempArrayAsAsciiArt = false;

            Resolver.Log.Info("Run sample...");

            float[] frame;

            Resolver.Log.Info($"Serial #:{sensor.SerialNumber}");

            sensor.SetMode(Mlx90640.Mode.Chess);
            Resolver.Log.Info($"Current Mode: {sensor.GetMode()}");

            sensor.SetResolution(Mlx90640.Resolution.EighteenBit);
            Resolver.Log.Info($"Current resolution: {sensor.GetResolution()}");

            sensor.SetRefreshRate(Mlx90640.RefreshRate._2hz);
            Resolver.Log.Info($"Current frame rate: {sensor.GetRefreshRate()}");

            Resolver.Log.Info($"Broken Pixels: {sensor.Config.BrokenPixels.Count}");
            Resolver.Log.Info($"Outlier Pixels: {sensor.Config.OutlierPixels.Count}");
            Resolver.Log.Info($"Broken Pixels has adjacent broken pixel: {sensor.Config.BrokenPixelHasAdjacentBrokenPixel}");
            Resolver.Log.Info($"Broken Pixels has adjacent Outlier pixel: {sensor.Config.BrokenPixelHasAdjacentOutlierPixel}");
            Resolver.Log.Info($"Outlier Pixels has adjacent Outlier pixel: {sensor.Config.OutlierPixelHasAdjacentOutlierPixel}");

            Thread.Sleep(2000);

            while (true)
            {
                Thread.Sleep(1000);

                frame = sensor.ReadRawData();

                Resolver.Log.Info("");

                //Print out each value
                for (byte h = 0; h < 24; h++)
                {
                    StringBuilder logLine = new StringBuilder();
                    for (byte w = 0; w < 32; w++)
                    {
                        float t = frame[h * 32 + w];
                        //View sensor data as ascii art. It is easier to see shapes, like your fingers.
                        if (!showTempArrayAsAsciiArt)
                        {
                            //Write the Temp value
                            logLine.Append($"{t:0},");
                        }
                        else
                        {
                            //Write the ascii art character
                            char c = '&';
                            if (t < 68) c = ' ';
                            else if (t < 73.4) c = '.';
                            else if (t < 77) c = '-';
                            else if (t < 80.6) c = '*';
                            else if (t < 84) c = '+';
                            else if (t < 87) c = 'x';
                            else if (t < 91) c = '%';
                            else if (t < 95) c = '#';
                            else if (t < 98.6) c = '$';
                            logLine.Append(c);
                        }
                    }

                    Resolver.Log.Info(logLine.ToString());
                }
            }
        }

        //<!=SNOP=>
    }
}