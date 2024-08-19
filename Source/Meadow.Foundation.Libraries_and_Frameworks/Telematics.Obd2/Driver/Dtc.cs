using System;

namespace Meadow.Foundation.Telematics.OBD2;

public partial class Dtc
{
    private byte[] _data = new byte[2];

    public DtcCategory Category
    {
        get => (DtcCategory)(_data[0] & 0xC0);
        set => _data[0] = (byte)((_data[0] & 0x3f) | (byte)value);
    }

    public int Code
    {
        get
        {
            return ((_data[0] & 0x03) * 100)
                + (((_data[1] & 0xf0) >> 4) * 10)
                + (_data[1] & 0x0f);
        }
        set
        {
            if (value > 0x3ff) throw new ArgumentException("Max code value is 3ff");
            var digit = value / 100;
            _data[0] = (byte)(digit & 0x03);
            value = value % 100;
            digit = value / 10;
            _data[1] = (byte)(digit << 4);
            value = value % 10;
            _data[1] |= (byte)value;
        }
    }

    public Dtc(byte[] data)
    {
        if (data.Length != 2) throw new ArgumentException("DTC codes must be exactly 2 bytes");
        _data = data;
    }

    public override string ToString()
    {
        return $"{Category}{Code:N4}";
    }

    public string ToReadableErrorCode()
    {
        switch (Category)
        {
            case DtcCategory.P:
                return GetReadablePowertrainErrorCode(Code);
            case DtcCategory.C:
                return GetReadableChassisErrorCode(Code);
            case DtcCategory.B:
                return GetReadableBodyErrorCode(Code);
            case DtcCategory.U:
                return GetReadableNetworkErrorCode(Code);
        }

        throw new NotImplementedException();
    }
}
