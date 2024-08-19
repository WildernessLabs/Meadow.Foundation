using Meadow.Foundation.Telematics.OBD2;

namespace Obd2.Unit.Tests;

public class UnitTest1
{
    [Fact]
    public void DtcCreationTest()
    {
        var data = new byte[] { 0b1100_0001, 0b0101_1000 };

        var dtc = new Dtc(data);

        Assert.Equal(DtcCategory.U, dtc.Category);
        Assert.Equal(158, dtc.Code);
        Assert.Equal("U0158", dtc.ToString());
    }

    [Fact]
    public void DtcSetCodeTest()
    {
        var dtc = new Dtc([0, 0]);

        dtc.Code = 0263;
        dtc.Category = DtcCategory.C;
        Assert.Equal(263, dtc.Code);
        Assert.Equal(DtcCategory.C, dtc.Category);
    }
}