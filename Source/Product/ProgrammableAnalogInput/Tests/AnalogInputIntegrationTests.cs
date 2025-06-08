using Meadow.Foundation;
using Meadow.Foundation.Serialization;
using Meadow.Units;

namespace ProgrammableAnalogInputTests;

public class AnalogInputIntegrationTests
{
    // Class to match our JSON structure
    public class ChannelConfigurationsJson
    {
        public ChannelConfig[] ChannelConfigurations { get; set; }
    }

    [Fact]
    public void ConfigureAndTestAllChannelsFromJson()
    {
        // Arrange
        // Load the JSON configuration file
        string jsonContent = File.ReadAllText("inputs/three-channel-config.json");
        var configurations = MicroJson.Deserialize<ChannelConfigurationsJson>(jsonContent);

        // Create the module
        var module = new SimulatedProgrammableAnalogInputModule(8);

        // Apply all configurations from the JSON
        foreach (var config in configurations.ChannelConfigurations)
        {
            module.ConfigureChannel(config);
            Console.WriteLine($"Configured channel {config.ChannelNumber} as {config.Name}");
        }

        // Test 1: 0-100°F Temperature Sensor (Channel 0)
        TestTemperatureSensor_0_100F(module);

        // Test 2: -40-140°F Temperature Sensor (Channel 1)
        TestTemperatureSensor_Neg40_140F(module);

        // Test 3: 0-30psi Pressure Sensor (Channel 2)
        TestPressureSensor_0_30psi(module);
    }

    private void TestTemperatureSensor_0_100F(SimulatedProgrammableAnalogInputModule module)
    {
        Console.WriteLine("\nTesting 0-100°F Temperature Sensor (Channel 0):");

        // Test various points in the range
        // 4mA = 0°F
        TestTemperaturePoint(module, 0, 4.0, new Temperature(0, Temperature.UnitType.Fahrenheit));

        // 12mA = 50°F
        TestTemperaturePoint(module, 0, 12.0, new Temperature(50, Temperature.UnitType.Fahrenheit));

        // 20mA = 100°F
        TestTemperaturePoint(module, 0, 20.0, new Temperature(100, Temperature.UnitType.Fahrenheit));

        // Test a random point
        var random = new Random();
        double randomCurrent = 4.0 + (random.NextDouble() * 16.0);

        // Calculate expected temperature in Fahrenheit
        // With scale = 3.4725, offset = -31.67, we get Celsius
        // Converting to Fahrenheit: expectedF = expectedC * 9/5 + 32
        double expectedC = randomCurrent * 3.4725 - 31.67;
        double expectedF = expectedC * 9.0 / 5.0 + 32.0;

        TestTemperaturePoint(module, 0, randomCurrent, new Temperature(expectedF, Temperature.UnitType.Fahrenheit));
    }

    private void TestTemperatureSensor_Neg40_140F(SimulatedProgrammableAnalogInputModule module)
    {
        Console.WriteLine("\nTesting -40-140°F Temperature Sensor (Channel 1):");

        // Test various points in the range
        // 4mA = -40°F
        TestTemperaturePoint(module, 1, 4.0, new Temperature(-40, Temperature.UnitType.Fahrenheit));

        // 12mA = 50°F
        TestTemperaturePoint(module, 1, 12.0, new Temperature(50, Temperature.UnitType.Fahrenheit));

        // 20mA = 140°F
        TestTemperaturePoint(module, 1, 20.0, new Temperature(140, Temperature.UnitType.Fahrenheit));

        // Test a random point
        var random = new Random();
        double randomCurrent = 4.0 + (random.NextDouble() * 16.0);

        // Calculate expected temperature in Fahrenheit
        // With scale = 6.25, offset = -65.0, we get Celsius
        // Converting to Fahrenheit: expectedF = expectedC * 9/5 + 32
        double expectedC = randomCurrent * 6.25 - 65.0;
        double expectedF = expectedC * 9.0 / 5.0 + 32.0;

        TestTemperaturePoint(module, 1, randomCurrent, new Temperature(expectedF, Temperature.UnitType.Fahrenheit));
    }

    private void TestPressureSensor_0_30psi(SimulatedProgrammableAnalogInputModule module)
    {
        Console.WriteLine("\nTesting 0-30psi Pressure Sensor (Channel 2):");

        // For voltage inputs we need to set the raw voltage
        // Test various points in the range

        // 0V = 0 psi
        SetVoltageAndTestPressure(module, 2, 0.0, new Pressure(0.0, Pressure.UnitType.Psi));

        // 5V = 15 psi
        SetVoltageAndTestPressure(module, 2, 5.0, new Pressure(15.0, Pressure.UnitType.Psi));

        // 10V = 30 psi
        SetVoltageAndTestPressure(module, 2, 10.0, new Pressure(30.0, Pressure.UnitType.Psi));

        // Test a random point
        var random = new Random();
        double randomVoltage = random.NextDouble() * 10.0;
        double expectedPsi = randomVoltage * 3.0; // 0-10V maps to 0-30psi

        SetVoltageAndTestPressure(module, 2, randomVoltage, new Pressure(expectedPsi, Pressure.UnitType.Psi));
    }

    private void TestTemperaturePoint(
        SimulatedProgrammableAnalogInputModule module,
        int channelNumber,
        double currentmA,
        Temperature expectedTemperature)
    {
        // For a 4-20mA input mapped to 0-3.3V ADC
        double adcVoltage = ((currentmA - 4.0) / 16.0) * 3.3;

        // Set the raw voltage that the ADC would read
        module.SetChannelRawVoltage(channelNumber, new Voltage(adcVoltage, Voltage.UnitType.Volts));

        // Read the temperature
        var result = module.ReadChannelAsConfiguredUnit(channelNumber);
        Assert.IsType<Temperature>(result);
        var temperature = (Temperature)result;

        Console.WriteLine($"  Current: {currentmA:F2} mA, ADC Voltage: {adcVoltage:F2} V");
        Console.WriteLine($"  Expected: {expectedTemperature.Fahrenheit:F2}°F ({expectedTemperature.Celsius:F2}°C)");
        Console.WriteLine($"  Actual: {temperature.Fahrenheit:F2}°F ({temperature.Celsius:F2}°C)");

        // Assert within reasonable tolerance (1°F)
        Assert.InRange(temperature.Fahrenheit, expectedTemperature.Fahrenheit - 1.0,
                      expectedTemperature.Fahrenheit + 1.0);
    }

    private void SetVoltageAndTestPressure(
        SimulatedProgrammableAnalogInputModule module,
        int channelNumber,
        double sensorVoltage,
        Pressure expectedPressure)
    {
        // For a 0-10V input mapped to 0-3.3V ADC
        double adcVoltage = (sensorVoltage / 10.0) * 3.3;

        // Set the raw voltage that the ADC would read
        module.SetChannelRawVoltage(channelNumber, new Voltage(adcVoltage, Voltage.UnitType.Volts));

        // Read the pressure
        var result = module.ReadChannelAsConfiguredUnit(channelNumber);
        Assert.IsType<Pressure>(result);
        var pressure = (Pressure)result;

        Console.WriteLine($"  Sensor Voltage: {sensorVoltage:F2} V, ADC Voltage: {adcVoltage:F2} V");
        Console.WriteLine($"  Expected: {expectedPressure.Psi:F2} psi ({expectedPressure.Bar:F4} bar)");
        Console.WriteLine($"  Actual: {pressure.Psi:F2} psi ({pressure.Bar:F4} bar)");

        // Assert within reasonable tolerance (0.5 psi)
        Assert.InRange(pressure.Psi, expectedPressure.Psi - 0.5, expectedPressure.Psi + 0.5);
    }
}