using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using Meadow.Units;
using static Meadow.Foundation.ICs.IOExpanders.FtdiExpander;

namespace FT232.Unit.Tests;

public class Ftd2xxTests
{
    [Fact]
    public void GetDeviceList()
    {
        // assumes an FT232 is connected
        var devices = FtdiExpanderCollection.Devices;
        devices.Refresh();
        Assert.True(devices.Count > 0);
    }

    [Fact]
    public void OpenI2CBus()
    {
        // assumes an FT232 is connected
        var ftdi = FtdiExpanderCollection.Devices[0];
        var bus = ftdi.CreateI2cBus();
        Assert.NotNull(bus);
    }

    [Fact]
    public void OpenSPIBus()
    {
        // assumes an FT232 is connected
        var ftdi = FtdiExpanderCollection.Devices[0];
        var bus = ftdi.CreateSpiBus();
        Assert.NotNull(bus);
    }

    [Fact]
    public void NoDeviceForSpiCheck()
    {
        // assumes no FT232 is connected
        var ftdi = FtdiExpanderCollection.Devices[0];
        Assert.Throws<DeviceNotFoundException>(() =>
        {
            var bus = ftdi.CreateSpiBus();
        });
    }

    [Fact]
    public void NoDeviceForI2CCheck()
    {
        // assumes no FT232 is connected
        var ftdi = FtdiExpanderCollection.Devices[0];
        Assert.Throws<DeviceNotFoundException>(() =>
        {
            var bus = ftdi.CreateI2cBus();
        });
    }

    [Fact]
    public void NoDriverCheck()
    {
        // assumes no FT232 driver is installed (rename C:\Windows\System32\ftd2xx.dll)
        Assert.Throws<DriverNotInstalledException>(() =>
        {
            var ftdi = FtdiExpanderCollection.Devices[0];
        });
    }

    [Fact]
    public void RunSpiLoopbackTest()
    {
        var ftdi = FtdiExpanderCollection.Devices[0];

        // Connect MOSI to MISO for this test
        byte[] testPattern = new byte[] { 0x55, 0xAA, 0x00, 0xFF, 0x0F, 0xF0 };
        byte[] readBuffer = new byte[testPattern.Length];

        var spiBus = ftdi.CreateSpiBus();

        spiBus.Exchange(null, testPattern, readBuffer);

        // Verify read data matches written data
        for (int i = 0; i < testPattern.Length; i++)
        {
            if (testPattern[i] != readBuffer[i])
            {
                Assert.Fail("Loopback failed");
            }
        }
    }

    [Fact]
    public void SpiSpeedTests()
    {
        var ftdi = FtdiExpanderCollection.Devices[0];

        // Test speeds (in MHz)
        int[] speedsToTest = new[] { 1, 5, 10, 15, 20, 25, 30 };
        byte[] testByte = new[] { (byte)0x55 }; // Simple toggle pattern
        byte[] readByte = new byte[1];

        foreach (int speedMHz in speedsToTest)
        {
            try
            {
                Console.WriteLine($"Testing at {speedMHz}MHz...");

                var config = new SpiClockConfiguration(
                    new Frequency(speedMHz, Frequency.UnitType.Megahertz)
                     );

                var testBus = new Ft232hSpiBus(ftdi, config);
                testBus.Exchange(null, testByte, readByte);

                if (testByte[0] != readByte[0])
                {
                    Console.WriteLine($"Failed at {speedMHz}MHz: Sent 0x{testByte[0]:X2}, Received 0x{readByte[0]:X2}");
                }

                Console.WriteLine($"Passed at {speedMHz}MHz");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed at {speedMHz}MHz with exception: {ex.Message}");
            }
        }
    }
}