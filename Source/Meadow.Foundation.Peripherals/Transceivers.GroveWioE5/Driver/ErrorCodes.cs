namespace Meadow.Foundation.Transceivers;

internal enum ErrorCodes
{
    Invalidparameter = -1,
    UnknownCommand = -10,
    InvalidCommandFormat = -11,
    UnavailableInCurrentMode = -12,
    TooManyparameters = -20,
    CommandTooLong = -21,
    EndSymbolTimeout = -22,
    InvalidCharacter = -23,
    CommandFormatError = -24
}

