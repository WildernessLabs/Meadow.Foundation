using Meadow.Foundation.ICs.IOExpanders;

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
}