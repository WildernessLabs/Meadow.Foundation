namespace Meadow.Foundation.Telematics.OBD2;

public partial class Dtc
{
    private string GetReadableBodyErrorCode(int code)
    {
        return code switch
        {
            // TODO
            _ => $"B{code:N4}"
        };
    }
}
