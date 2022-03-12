/// <summary>
/// Demos for Neopixels on Adafruit Seesaw
/// Author: Frederick M Meyer
/// Date: /// 2022-03-03
///
/// Copyright: 2022 (c) Frederick M Meyer for Wilderness Labs
/// License: MIT
/// 
/// This demo will change the neopixels' color to indicate the outside temperature (from blue to red as the temperature rises).
/// The data is acquired from openweathermap.org every 10 minutes; that's as often as they update their current weather conditions.
/// 
/// To run the demo:
/// 1) Get an API key from openweathermap.org. It's free.
/// 2) Update the code below with your API key, your latitude/longitude, and your WiFi SSID and password.
/// </summary>
/// <remarks>
/// For hardware, this works with either Seesaw device:
/// Adafruit ATSAMD09 Breakout with seesaw <see href="https://www.adafruit.com/product/3657"</see>
/// or
/// Adafruit ATtiny8x7 Breakout with seesaw - STEMMA QT / Qwiic <see href="https://www.adafruit.com/product/5233"</see>
/// </remarks>

#define pixelsHaveWhite  // <-- Comment out this statement if using RGB/GRB rather than RGBW/GRBW

using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders.Seesaw;
using Meadow.Gateway.WiFi;
using Meadow.Hardware;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Neopixel_Sample2
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        const string apiKey = "--APIKEY--";
        const string latitude = "--LAT--";
        const string longitude = "--LON--";
        const string mySSID = "--SSID--";
        const string myPassword = "--PWD--";

        Seesaw seesaw;
        Neopixel neopixel;
        Neopixel.PixelArray px;
        const int np = 8;

        readonly Regex rx = new Regex("{\"temp\":([0-9]+)");
        int temperature;

        const int minTemp = 40;
        const int maxTemp = 100;
        const int midTemp = 70;  // (minTemp + maxTemp) / 2

        public MeadowApp()
        {
            Console.WriteLine("Neopixel Sample 2");

            Initialize().Wait();

            while (true)
            {
                string apiResponse = GetWebPageViaHttpClient($"http://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&units=imperial&appid={apiKey}").Result;

                if (apiResponse == null)
                {
                    Thread.Sleep(60 * 1000);
                }
                else
                {
                    temperature = Convert.ToInt32(rx.Match(apiResponse).Groups[1].Value);

                    // Scale red proportionally to temp, from mid to max
                    int red = (int)((255.0 / (maxTemp - midTemp)) * (temperature - midTemp));
                    if (red < 0) red = 0;
                    if (red > 255) red = 255;

                    // Scale green proportionally to temp from min to mid, inversely from mid to max
                    int green = (int)(255 - ((255.0 / (midTemp - minTemp)) * Math.Abs(temperature - midTemp)));
                    if (green < 0) green = 0;
                    if (green > 255) green = 255;

                    // Scale blue inversely to temp, from min to mid
                    int blue = (int)(255 - (255.0 / (midTemp - minTemp)) * (temperature - minTemp));
                    if (blue < 0) blue = 0;
                    if (blue > 255) blue = 255;
#if pixelsHaveWhite
                    px.Fill((red, green, blue, 0));
#else
                    px.Fill((red, green, blue));
#endif
                    neopixel.MoveToDisplay();
                    neopixel.Show();

                    Thread.Sleep(10 * 60 * 1000);
                }
            }
        }

        async Task Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            // connected event test.
            Device.WiFiAdapter.WiFiConnected += WiFiAdapter_ConnectionCompleted;

            // connnect to the wifi network.
            Console.WriteLine($"Connecting to WiFi network \"{mySSID}\"");
            var connectionResult = await Device.WiFiAdapter.Connect(mySSID, myPassword);
            if (connectionResult.ConnectionStatus != ConnectionStatus.Success)
            {
                throw new Exception($"Cannot connect to network: {connectionResult.ConnectionStatus}");
            }

            seesaw = new Seesaw(Device.CreateI2cBus(I2cBusSpeed.FastPlus));  // Define I2C bus

            Console.WriteLine($"Seesaw Address: 0x{seesaw.SeesawBoardAddr:X}, Board Id: 0x{seesaw.ChipId:X}, Type: {Enum.GetName(typeof(HwidCodes), seesaw.ChipId)}");

#if pixelsHaveWhite
            neopixel = new Neopixel(seesaw, 9, np, brightness: 0.3);  // Initialize seesaw-attached neopixel environment
#else
            neopixel = new Neopixel(seesaw, 9, np, brightness: 0.3, pixelOrder: Neopixel.GRB);  // Initialize seesaw-attached neopixel environment
#endif
            px = neopixel.PixelArrayInstance;  // Get address of the pixel array created by Neopixel() constructor
        }

        private void WiFiAdapter_ConnectionCompleted(object sender, EventArgs e)
        {
            Console.WriteLine("Connection request completed.");
        }

        public async Task<string> GetWebPageViaHttpClient(string uri)
        {
            Console.WriteLine($"Requesting {uri}");

            using (HttpClient client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 5, 0);

                HttpResponseMessage response = await client.GetAsync(uri);

                try
                {
                    response.EnsureSuccessStatusCode();
                    return response.Content.ReadAsStringAsync().Result;
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Request time out.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Request went sideways: {e.Message}");
                }
            }
            return string.Empty;
        }
    }
}