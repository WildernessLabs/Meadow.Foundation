namespace Meadow.Foundation.Telematics.J1979;

public partial class Dtc
{
    private string GetReadableNetworkErrorCode(int code)
    {
        return code switch
        {
            // TODO
            _ => $"U{code:N4}"
        };
    }
}
