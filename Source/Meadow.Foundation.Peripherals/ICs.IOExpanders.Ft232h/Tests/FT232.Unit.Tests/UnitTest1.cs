using Meadow.Foundation.ICs.IOExpanders;

namespace FT232.Unit.Tests;

public class Ftd2xxTests
{
    [Fact]
    public void GetDeviceList()
    {
        var devices = new FtdiDeviceCollection();
        devices.Refresh();
        Assert.True(devices.Count > 0);
    }

    [Fact]
    public void OpenI2CBus()
    {
        var ftdi = new Ft232h(false);
        var bus = ftdi.CreateI2cBus();

    }
}