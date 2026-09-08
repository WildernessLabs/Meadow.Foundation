namespace Meadow.Foundation.Telematics.Uds;

/// <summary>
/// Standard UDS Negative Response Codes (NRC) defined in ISO 14229-1.
/// </summary>
public enum UdsNrc : byte
{
    GeneralReject                               = 0x10,
    ServiceNotSupported                         = 0x11,
    SubFunctionNotSupported                     = 0x12,
    IncorrectMessageLengthOrInvalidFormat       = 0x13,
    ResponseTooLong                             = 0x14,
    BusyRepeatRequest                           = 0x21,
    ConditionsNotCorrect                        = 0x22,
    RequestSequenceError                        = 0x24,
    NoResponseFromSubnetComponent               = 0x25,
    FailurePreventsExecutionOfRequestedAction   = 0x26,
    RequestOutOfRange                           = 0x31,
    SecurityAccessDenied                        = 0x33,
    InvalidKey                                  = 0x35,
    ExceededNumberOfAttempts                    = 0x36,
    RequiredTimeDelayNotExpired                 = 0x37,
    UploadDownloadNotAccepted                   = 0x70,
    TransferDataSuspended                       = 0x71,
    GeneralProgrammingFailure                   = 0x72,
    WrongBlockSequenceCounter                   = 0x73,
    RequestCorrectlyReceivedResponsePending     = 0x78,
    SubFunctionNotSupportedInActiveSession      = 0x7E,
    ServiceNotSupportedInActiveSession          = 0x7F
}
