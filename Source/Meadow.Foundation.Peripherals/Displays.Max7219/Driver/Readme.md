# Meadow.Foundation.Displays.Max7219

**MAX7219 SPI LED driver**

The **Max7219** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
Max7219 display;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    display = new Max7219(Device.CreateSpiBus(), Device.Pins.D01, 1, Max7219.Max7219Mode.Character);

    return base.Initialize();
}

void TestCharacterMode()
{
    display.SetMode(Max7219.Max7219Mode.Character);
    //show every supported character 
    for (int i = 0; i < (int)Max7219.CharacterType.Count; i++)
    {
        for (int digit = 0; digit < 8; digit++)
        {
            display.SetCharacter((Max7219.CharacterType)i, digit, i % 2 == 0);
        }
        display.Show();
    }
}

void TestDigitalMode()
{
    Resolver.Log.Info("Digital test");

    display.SetMode(Max7219.Max7219Mode.Digital);
    //control individual LEDs - for 8x8 matrix configurations - use the Meadow graphics library
    for (byte i = 0; i < 64; i++)
    {
        for (int d = 0; d < 8; d++)
        {
            display.SetDigit(i, d);
        }
        display.Show();
    }
}

public override Task Run()
{
    while (true)
    {
        TestDigitalMode();
        TestCharacterMode();
    }
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
