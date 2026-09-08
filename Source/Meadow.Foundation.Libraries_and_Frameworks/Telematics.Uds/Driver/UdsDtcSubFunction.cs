namespace Meadow.Foundation.Telematics.Uds;

/// <summary>
/// Sub-functions for UDS Service $19 (ReadDtcInformation).
/// </summary>
public enum UdsDtcSubFunction : byte
{
    ReportNumberOfDtcByStatusMask          = 0x01,
    ReportDtcByStatusMask                  = 0x02,
    ReportDtcSnapshotIdentification        = 0x03,
    ReportDtcSnapshotRecordByDtcNumber     = 0x04,
    ReportDtcExtendedDataRecordByDtcNumber = 0x06,
    ReportNumberOfDtcBySeverityMaskRecord  = 0x07,
    ReportDtcBySeverityInformationRecord   = 0x08,
    ReportSupportedDtcs                    = 0x0A,
    ReportFirstTestFailedDtc               = 0x0B,
    ReportFirstConfirmedDtc                = 0x0C,
    ReportMostRecentTestFailedDtc          = 0x0D,
    ReportMostRecentConfirmedDtc           = 0x0E,
    ReportDtcFaultDetectionCounter         = 0x14,
    ReportDtcWithPermanentStatus           = 0x15
}
