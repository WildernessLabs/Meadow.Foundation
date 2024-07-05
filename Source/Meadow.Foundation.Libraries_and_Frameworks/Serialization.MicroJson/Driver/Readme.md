# Meadow.Foundation.Serialization.MicroJson

**Lightweight .NET Json serializer/deserializer**

The **MicroJson** library is included in the **Meadow.Foundation.Serialization.MicroJson** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Serialization.MicroJson`
## Usage

```csharp
public static void Main(string[] args)
{
    var resourceData = LoadResource("menu.json");

    var menuJson = new string(System.Text.Encoding.UTF8.GetChars(resourceData));

    DeserializeTypeSafe(menuJson);
    DeserializeAsHashtable(menuJson);
}

private static void DeserializeTypeSafe(string menuJson)
{
    string testJsonItem = "{\"ScreenX\":290,\"ScreenY\":210,\"RawX\":3341,\"RawY\":3353}";
    var point = MicroJson.Deserialize<CalibrationPoint>(testJsonItem);

    string testJsonArray = "[{\"ScreenX\":30,\"ScreenY\":30,\"RawX\":522,\"RawY\":514},{\"ScreenX\":290,\"ScreenY\":210,\"RawX\":3341,\"RawY\":3353}]";

    var points = MicroJson.Deserialize<CalibrationPoint[]>(testJsonArray);

    var menu = MicroJson.Deserialize<MenuContainer>(menuJson);
}

private static void DeserializeAsHashtable(string menuJson)
{
    var menuData = MicroJson.DeserializeString(menuJson) as Hashtable;

    if (menuData["menu"] == null)
    {
        throw new ArgumentException("JSON root must contain a 'menu' item");
    }

    Console.WriteLine($"Root element is {menuData["menu"]}");

    var items = (ArrayList)menuData["menu"];

    foreach (Hashtable item in items)
    {
        Console.WriteLine($"Found {item["text"]}");
    }
}

private static byte[] LoadResource(string filename)
{
    var assembly = Assembly.GetExecutingAssembly();
    var resourceName = $"MicroJson_Sample.{filename}";

    using Stream stream = assembly.GetManifestResourceStream(resourceName);
    using var ms = new MemoryStream();
    stream.CopyTo(ms);
    return ms.ToArray();
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
## About Meadow

Meadow is a complete, IoT platform with defense-grade security that runs full .NET applications on embeddable microcontrollers and Linux single-board computers including Raspberry Pi and NVIDIA Jetson.

### Build

Use the full .NET platform and tooling such as Visual Studio and plug-and-play hardware drivers to painlessly build IoT solutions.

### Connect

Utilize native support for WiFi, Ethernet, and Cellular connectivity to send sensor data to the Cloud and remotely control your peripherals.

### Deploy

Instantly deploy and manage your fleet in the cloud for OtA, health-monitoring, logs, command + control, and enterprise backend integrations.


