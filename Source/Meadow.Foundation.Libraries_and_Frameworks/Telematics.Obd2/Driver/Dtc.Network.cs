namespace Meadow.Foundation.Telematics.OBD2;

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
