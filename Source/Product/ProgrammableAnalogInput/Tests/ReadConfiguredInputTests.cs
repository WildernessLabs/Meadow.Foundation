using Meadow.Foundation;
using Meadow.Units;

namespace ProgrammableAnalogInputTests;

public class ReadConfiguredInputTests
{
    [Fact]
    public void ReadChannelAsConfiguredUnit_WithCurrent_AppliesOffset()
    {
        // Arrange

        // Create the module
        var module = new SimulatedProgrammableAnalogInputModule(8);

        module.SetChannelRawVoltage(2, 1.65.Volts());

        // Configure channel 2 as 4-20mA with temperature unit
        // Scale of 100 means 4mA = 0°C, 20mA = 100°C
        // Offset of -20 means the final range is -20°C to 80°C
        module.ConfigureChannel(new ChannelConfig
        {
            ChannelNumber = 2,
            ChannelType = ConfigurableAnalogInputChannelType.Current_4_20,
            UnitType = "Temperature",
            Scale = 6.25,
            Offset = -25
        });

        // Act
        var result = module.ReadChannelAsConfiguredUnit(2);

        // Assert
        Assert.IsType<Temperature>(result);
        var temperature = (Temperature)result;

        // For a 1.65V reading on a 3.3V reference, we'd have 1.65/3.3 = 0.5 or 50% of the range
        // For 4-20mA, 50% of the range corresponds to 12mA
        // With scaling of 100, that's 50°C
        Assert.InRange(temperature.Celsius, 49.9, 50.1);
    }

    [Fact]
    public void ReadChannelAsConfiguredUnit_WithCurrent_TestMultipleRandomPoints()
    {
        // Arrange
        // Create the module
        var module = new SimulatedProgrammableAnalogInputModule(8);

        // Create a random number generator
        var random = new Random();

        var channel = random.Next(0, 8);

        // Configure channel as 4-20mA with temperature unit
        // For a sensor that measures -40°F to 140°F (-40°C to 60°C)
        module.ConfigureChannel(new ChannelConfig
        {
            ChannelNumber = channel,
            ChannelType = ConfigurableAnalogInputChannelType.Current_4_20,
            UnitType = "Temperature",
            Scale = 6.25,    // 100°C span ÷ 16mA = 6.25°C/mA
            Offset = -65.0   // To map 4mA to -40°C
        });

        // Test three random points within the valid range
        for (int i = 0; i < 3; i++)
        {
            // Generate a random current value in the 4-20mA range
            double current = 4.0 + (random.NextDouble() * 16.0);

            // Calculate the corresponding voltage (4-20mA maps to 0-3.3V)
            double voltage = ((current - 4.0) / 16.0) * 3.3;

            // Calculate the expected temperature in Celsius
            double expectedTemp = current * 6.25 - 65.0;

            // Set the channel voltage and read the result
            module.SetChannelRawVoltage(channel, new Voltage(voltage, Voltage.UnitType.Volts));
            var result = module.ReadChannelAsConfiguredUnit(channel);

            // Assert the result is a Temperature object
            Assert.IsType<Temperature>(result);
            var temperature = (Temperature)result;

            // Verify the temperature is within 0.5°C of expected value
            Assert.InRange(temperature.Celsius, expectedTemp - 0.1, expectedTemp + 0.1);

            // Output test point details for debugging
            System.Diagnostics.Debug.WriteLine($"Test Point {i + 1}: {current:F2} mA, {voltage:F2} V, {expectedTemp:F2}°C");
        }
    }

    [Fact]
    public void ReadChannelAsConfiguredUnit_WithVoltage_TestRandomPressurePoints()
    {
        // Arrange
        // Create the module
        var module = new SimulatedProgrammableAnalogInputModule(8);

        var random = new Random();

        var channel = random.Next(0, 8);

        // Configure channel 3 as 0-10V with pressure unit
        // For a sensor that measures 0-30 psi across 0-10V
        module.ConfigureChannel(new ChannelConfig
        {
            ChannelNumber = channel,
            ChannelType = ConfigurableAnalogInputChannelType.Voltage_0_10,
            UnitType = "Pressure",
            Scale = 0.2068, // 2.07 bar span ÷ 10V = 0.2068 bar/V
            Offset = 0.0    // No offset needed, 0V = 0 bar
        });

        // Test three random points within the valid range
        for (int i = 0; i < 3; i++)
        {
            // Generate a random pressure value in the 0-30 psi range
            double expectedPressurePsi = random.NextDouble() * 30.0;

            // Calculate what voltage the sensor would output (0-10V range)
            double sensorVoltage = expectedPressurePsi / 3.0; // 3.0 psi per volt

            // Calculate what the ADC would read (assuming 3.3V reference)
            // The ADC reads a proportion of the sensor voltage on a 0-3.3V scale
            double adcVoltage = sensorVoltage * 3.3 / 10.0;

            // Set the channel raw voltage to the ADC reading value
            module.SetChannelRawVoltage(channel, new Voltage(adcVoltage, Voltage.UnitType.Volts));

            // Act - Read the pressure from the module
            var result = module.ReadChannelAsConfiguredUnit(channel);

            // Assert the result is a Pressure object
            Assert.IsType<Pressure>(result);
            var pressure = (Pressure)result;

            // Convert the bar value to psi for comparison (1 bar = 14.5038 psi)
            double actualPressurePsi = pressure.Bar * 14.5038;

            // Verify the pressure is within 0.5 psi of expected value
            Assert.InRange(actualPressurePsi, expectedPressurePsi - 0.5, expectedPressurePsi + 0.5);

            // Output test point details for debugging
            System.Diagnostics.Debug.WriteLine($"Test Point {i + 1}:");
            System.Diagnostics.Debug.WriteLine($"  Expected Pressure: {expectedPressurePsi:F2} psi");
            System.Diagnostics.Debug.WriteLine($"  Sensor Voltage: {sensorVoltage:F2} V");
            System.Diagnostics.Debug.WriteLine($"  ADC Voltage: {adcVoltage:F2} V");
            System.Diagnostics.Debug.WriteLine($"  Actual Pressure: {actualPressurePsi:F2} psi");
        }
    }
}