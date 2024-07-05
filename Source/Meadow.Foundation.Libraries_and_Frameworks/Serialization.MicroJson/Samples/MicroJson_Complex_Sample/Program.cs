using Meadow.Foundation.Serialization;
using System;
using System.IO;
using System.Reflection;
using WifiWeather.DTOs;

namespace MicroJson_Complex_Sample;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, MicroJson - Complex Json");

        var jsonData = LoadResource("weather.json");

        var weather = MicroJson.Deserialize<WeatherReadingDTO>(jsonData);

        Console.WriteLine($"Temperature is: {weather.main.temp - 273.15:N1}C");
    }

    static byte[] LoadResource(string filename)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"MicroJson_Complex_Sample.{filename}";

        using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using var ms = new MemoryStream();

        stream?.CopyTo(ms);
        return ms.ToArray();
    }
}