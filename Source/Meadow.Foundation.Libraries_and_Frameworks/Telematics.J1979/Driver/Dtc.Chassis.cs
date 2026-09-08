namespace Meadow.Foundation.Telematics.J1979;

public partial class Dtc
{
    private string GetReadableChassisErrorCode(int code)
    {
        return code switch
        {
            // TODO
            _ => $"C{code:N4}"
        };
    }
}
