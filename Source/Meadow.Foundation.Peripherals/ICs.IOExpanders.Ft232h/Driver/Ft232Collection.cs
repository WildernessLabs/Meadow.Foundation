using Meadow.Hardware;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders;

public class Ft232h : FtdiExpander
{
    internal Ft232h(FTDI.FT_DEVICE_INFO_NODE info, FTDI native)
        : base(info, native)
    {
    }

    public override II2cBus CreateI2cBus(int busNumber = 1, I2cBusSpeed busSpeed = I2cBusSpeed.Standard)
    {
        throw new NotImplementedException();
    }

    public override II2cBus CreateI2cBus(IPin[] pins, I2cBusSpeed busSpeed)
    {
        throw new NotImplementedException();
    }

    public override II2cBus CreateI2cBus(IPin clock, IPin data, I2cBusSpeed busSpeed)
    {
        throw new NotImplementedException();
    }
}

public abstract class FtdiExpander :
//    IDisposable,
//    IDigitalInputOutputController,
//    IDigitalOutputController,
//    ISpiController,
    II2cController
{
    public abstract II2cBus CreateI2cBus(int busNumber = 1, I2cBusSpeed busSpeed = I2cBusSpeed.Standard);
    public abstract II2cBus CreateI2cBus(IPin[] pins, I2cBusSpeed busSpeed);
    public abstract II2cBus CreateI2cBus(IPin clock, IPin data, I2cBusSpeed busSpeed);

    internal static FtdiExpander? From(FTDI.FT_DEVICE_INFO_NODE info, FTDI native)
    {
        return info.Type switch
        {
            FTDI.FT_DEVICE.FT_DEVICE_232R => null, // I saw this with a USB->RS485 converter
            FTDI.FT_DEVICE.FT_DEVICE_232H => new Ft232h(info, native),
            _ => throw new NotSupportedException(),
        };
    }

    private FTDI.FT_DEVICE_INFO_NODE _infoNode;
    private FTDI _native;

    public string SerialNumber => _infoNode.SerialNumber;

    internal FtdiExpander(FTDI.FT_DEVICE_INFO_NODE info, FTDI native)
    {
        _infoNode = info;
        _native = native;
    }
}

public class Ft232Collection : IEnumerable<FtdiExpander>
{
    private FTDI _native;

    private static Ft232Collection? _instance;

    private List<FtdiExpander> _list = new List<FtdiExpander>();

    /// <summary>
    /// Gets the number of FtdiExpander devices connected to the host machine.
    /// </summary>
    public int Count => _list.Count;

    /// <summary>
    /// Gets the FtdiExpander device at the specified index in the collection.
    /// </summary>
    /// <param name="index">The index of the FtdiExpander device to retrieve.</param>
    public FtdiExpander this[int index] => _list[index];

    private Ft232Collection()
    {
        _native = new FTDI();
    }

    private void CheckStatus(FTDI.FT_STATUS status)
    {
        if (status == FTDI.FT_STATUS.FT_OK) return;

        throw new Exception($"ftd2xx.dll returned '{status}'");
    }

    public void Refresh()
    {
        uint count = 0;

        CheckStatus(_native.GetNumberOfDevices(ref count));

        _list.Clear();

        if (count > 0)
        {
            var infos = new FTDI.FT_DEVICE_INFO_NODE[count];
            CheckStatus(_native.GetDeviceList(infos));

            foreach (var info in infos)
            {
                var expander = FtdiExpander.From(info, _native);
                if (expander != null)
                {
                    _list.Add(expander);
                }
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerator<FtdiExpander> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Gets the singleton instance of Ft232Collection, initializing it if necessary.
    /// </summary>
    public static Ft232Collection Devices
    {
        get
        {
            if (_instance == null)
            {
                _instance = new Ft232Collection();
                _instance.Refresh();
            }
            return _instance;
        }
    }
}
