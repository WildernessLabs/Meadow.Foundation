using Meadow.Foundation.Telematics.J1979;
using System;
using System.Collections.Generic;

namespace Meadow.Foundation.Telematics.Uds;

/// <summary>
/// Protocol parsing and encoding helpers for ISO 14229-1 (UDS).
/// </summary>
/// <remarks>
/// Pure byte work. Every human-readable string comes from an <see cref="IUdsDescriptionProvider"/>
/// the caller supplies; pass none and the parsers still decode, they just describe nothing.
/// </remarks>
public static class UdsProtocol
{
    /// <summary>
    /// Decodes a 2-byte SAE/ISO DTC code into standard letter+4-digit notation (e.g., P0100, U0100).
    /// </summary>
    public static string DecodeBaseDtc(byte hi, byte lo)
    {
        char prefix = (hi >> 6) switch
        {
            0 => 'P',
            1 => 'C',
            2 => 'B',
            3 => 'U',
            _ => 'P'
        };

        int d1 = (hi >> 4) & 0x03;
        int d2 = hi & 0x0F;
        int d3 = (lo >> 4) & 0x0F;
        int d4 = lo & 0x0F;

        return $"{prefix}{d1:X1}{d2:X1}{d3:X1}{d4:X1}";
    }

    /// <summary>
    /// Parses a UDS Service $19 SubFunction $02 response (0x59 0x02 [AvailabilityMask] [DTC records...])
    /// into a list of <see cref="UdsDtc"/>.
    /// </summary>
    public static IReadOnlyList<UdsDtc> ParseDtcResponse(byte[]? responseData, IUdsDescriptionProvider? descriptions = null)
    {
        if (responseData == null || responseData.Length < 3)
            return [];

        // Check for Service $59 and subfunction $02 (or $0A/$15)
        if (responseData[0] != ((byte)UdsService.ReadDtcInformation + Obd2Addresses.ResponseOffset))
            return [];

        descriptions ??= NullUdsDescriptions.Instance;

        var list = new List<UdsDtc>();

        // Byte 0: 0x59, Byte 1: SubFunction (0x02), Byte 2: DTCStatusAvailabilityMask
        // Remaining bytes are 4-byte records: [DTC_HI, DTC_MID, DTC_LO (FTB), Status]
        int index = 3;
        while (index + 4 <= responseData.Length)
        {
            byte hi = responseData[index];
            byte mid = responseData[index + 1];
            byte ftb = responseData[index + 2];
            byte statusByte = responseData[index + 3];

            // Ignore blank records (0x00 0x00 0x00)
            if (hi != 0 || mid != 0 || ftb != 0)
            {
                string baseCode = DecodeBaseDtc(hi, mid);
                string fullCode = $"{baseCode}-{ftb:X2}";
                string ftbDesc = descriptions.GetFaultTypeDescription(ftb);
                string description = descriptions.GetDtcDescription(baseCode);
                var status = (UdsDtcStatusMask)statusByte;

                list.Add(new UdsDtc(baseCode, ftb, ftbDesc, fullCode, description, status));
            }

            index += 4;
        }

        return list;
    }

    /// <summary>
    /// Parses a UDS Service $22 ReadDataByIdentifier response (0x62 [DID_HI] [DID_LO] [Data...]).
    /// </summary>
    public static UdsDidValue? ParseDidResponse(ushort did, byte[]? responseData, IUdsDescriptionProvider? descriptions = null)
    {
        if (responseData == null || responseData.Length < 3)
            return null;

        if (responseData[0] != ((byte)UdsService.ReadDataByIdentifier + Obd2Addresses.ResponseOffset))
            return null;

        ushort respDid = (ushort)((responseData[1] << 8) | responseData[2]);
        if (respDid != did)
            return null;

        descriptions ??= NullUdsDescriptions.Instance;

        int dataLen = responseData.Length - 3;
        var dataBytes = new byte[dataLen];
        Array.Copy(responseData, 3, dataBytes, 0, dataLen);

        return new UdsDidValue(did, descriptions.GetDidName(did), dataBytes, descriptions.FormatDidValue(did, dataBytes));
    }

    /// <summary>
    /// Checks if a payload is a UDS Negative Response (0x7F [ServiceId] [NRC])
    /// and returns the decoded NRC description if true.
    /// </summary>
    public static bool TryParseNegativeResponse(byte[]? data, out byte requestService, out UdsNrc nrc, out string errorDescription,
        IUdsDescriptionProvider? descriptions = null)
    {
        requestService = 0;
        nrc = 0;
        errorDescription = "";

        if (data == null || data.Length < 3 || data[0] != (byte)UdsService.NegativeResponse)
            return false;

        descriptions ??= NullUdsDescriptions.Instance;

        requestService = data[1];
        nrc = (UdsNrc)data[2];
        errorDescription = $"Negative Response (0x7F): Service 0x{requestService:X2} rejected with NRC 0x{(byte)nrc:X2} ({descriptions.GetNrcDescription(data[2])})";
        return true;
    }

    /// <summary>
    /// Builds a negative response payload: 0x7F [ServiceId] [NRC].
    /// </summary>
    public static byte[] BuildNegativeResponse(byte requestService, UdsNrc nrc)
        => [(byte)UdsService.NegativeResponse, requestService, (byte)nrc];
}
