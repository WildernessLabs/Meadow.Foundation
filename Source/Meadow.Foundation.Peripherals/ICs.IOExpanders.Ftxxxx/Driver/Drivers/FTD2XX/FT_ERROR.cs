namespace FTD2XX;

/// <summary>
/// Error states not supported by FTD2XX DLL.
/// </summary>
public enum FT_ERROR
{
    FT_NO_ERROR = 0,
    FT_INCORRECT_DEVICE,
    FT_INVALID_BITMODE,
    FT_BUFFER_SIZE
};

public static class FT_ERROR_EXTENSIONS
{
    public static bool IsOK(this FT_ERROR error)
    {
        return error == FT_ERROR.FT_NO_ERROR;
    }

    public static void ThrowIfNotOK(this FT_ERROR error)
    {
        switch (error)
        {
            case FT_ERROR.FT_NO_ERROR:
                return;
            case FT_ERROR.FT_INCORRECT_DEVICE:
                throw new FT_EXCEPTION("The current device type does not match the EEPROM structure.");
            case FT_ERROR.FT_INVALID_BITMODE:
                throw new FT_EXCEPTION("The requested bit mode is not valid for the current device.");
            case FT_ERROR.FT_BUFFER_SIZE:
                throw new FT_EXCEPTION("The supplied buffer is not big enough.");
            default:
                throw new FT_EXCEPTION($"Unknown Error: {error}");
        }
    }
}