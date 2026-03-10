# SX1303 Driver — Development Notes

## Status: In Progress (Initial scaffold complete)

---

## What's Done

### Project Structure
- [x] `ICs.LoRa.Sx1303/Driver/` — driver project targeting `netstandard2.1` via `Meadow.Sdk/1.1.0`
- [x] `ICs.LoRa.Sx1303/Samples/Sx1303_Sample/` — Meadow.Linux / Raspberry Pi sample app
- [x] `Readme.md` with hardware wiring table, firmware instructions, and usage examples

### Driver Source Files
- [x] `Sx1303.Enums.cs` — `SpreadingFactor`, `Bandwidth`, `CodingRate`, `LoRaBand`, `RadioPath`, `TxPower`
- [x] `Sx1303.Registers.cs` — physical SPI register address constants, documented with HAL source reference
- [x] `Sx1303.Packet.cs` — `LoRaRxPacket` and `LoRaTxPacket` data classes
- [x] `Sx1303.ChannelConfig.cs` — `IfChainConfig`, `ChannelPlan`, pre-built `US915_SubBand1` and `EU868` plans
- [x] `Sx1303.cs` — main driver:
  - Two constructors (pins variant + ports variant, matching Meadow.Foundation convention)
  - `Initialize(agcFw, arbFw, channelPlan)` — power on, hardware reset, version check, MCU firmware load, channel plan programming
  - `Start(pollIntervalMs)` / `Stop()` — background RX polling loop
  - `SendPacket(LoRaTxPacket)` — TX buffer build and fire
  - `PacketReceived` event
  - Low-level SPI: `ReadByte`, `WriteByte`, `BurstRead`, `BurstWrite`
  - `Dispose` pattern (owns ports only when created from pins variant)

### SPI Protocol
Implemented exactly per Semtech sx1302_hal (`libloragw/src/loragw_spi.c`):
- Read  (5 bytes): `[ 0x00, addr>>8 & 0x7F, addr & 0xFF, 0x00, 0x00 ]` → data in `rx[4]`
- Write (4 bytes): `[ 0x00, 0x80 | addr>>8 & 0x7F, addr & 0xFF, data ]`
- Burst extends the data phase by N bytes

---

## Known Gaps / Next Steps

### High Priority

- [ ] **Verify register addresses against hardware**
  - All addresses in `Sx1303.Registers.cs` are derived from the public Semtech HAL
    but have not been tested on real hardware yet.
  - Key file to cross-reference: `sx1302_hal/libloragw/inc/loragw_reg.h` → `loregs[]` table.
  - Priority registers to verify first: `Version` (0x0001), `AgcMcuCtrl` (0x0082),
    `ArbMcuCtrl` (0x00A2), `AgcMcuMem` (0x1000), `ArbMcuMem` (0x2000),
    `RxBuffer` (0x4000), `RxBufferSize` (0x4200), `TxBuffer` (0x5200), `TxCtrl` (0x5100).

- [ ] **Verify RX FIFO packet descriptor format**
  - The byte layout in `DrainRxFifo()` is based on the HAL's `lgw_recv()` internal format.
  - Must be validated with a known packet before relying on RSSI/SNR/SF parsing.

- [ ] **Verify TX buffer descriptor format**
  - `BuildTxBuffer()` layout is derived from `lgw_send()` in the HAL.
  - Test with a known payload and confirm the concentrator actually fires RF.

- [ ] **Populate `FrequencyHz` on `LoRaRxPacket`**
  - Currently always 0. The RX descriptor includes a channel index but not the raw frequency.
  - Driver should look up channel index → frequency using the active `ChannelPlan`.

- [ ] **MCU firmware load verification**
  - After loading AGC/ARB firmware, the HAL reads back a status byte to confirm the MCU started.
  - Add a readback check in `LoadMcuFirmware()` and throw if it fails.

- [ ] **Obtain and test with real firmware binaries**
  - Build sx1302_hal on Raspberry Pi and extract `agc_fw.bin` / `arb_fw.bin`.
  - Confirm 8192-byte size assumption is correct for the firmware version in use.

### Medium Priority

- [ ] **Add more pre-built channel plans**
  - `ChannelPlan.AU915_SubBand1`
  - `ChannelPlan.AS923`
  - `ChannelPlan.IN865`

- [ ] **SX1250 radio front-end configuration**
  - `WriteRadioFrequency()` writes the frequency word but does not configure gain,
    PA/LNA settings, or calibration.
  - The Semtech HAL performs a full SX1250 calibration sequence via `lgw_sx1250_calibrate()`.
  - A calibration method should be added and called from `Initialize()`.

- [ ] **GPS / PPS timestamp support**
  - The SX1303 supports IEEE 1588 / GPS-disciplined timestamps via the `GpsEn` register.
  - Expose `EnableGpsPps(IDigitalInterruptPort ppsPort)` API.

- [ ] **Spectral scan / LBT (Listen Before Talk)**
  - Requires optional SX1261 companion chip (present on some WaveShare variants).
  - Add optional `sx1261ResetPin` parameter and `ScanSpectrum()` method.

- [ ] **FSK channel support**
  - `IfFreqFsk` register is defined but FSK chain is not programmed or parsed.

- [ ] **Interrupt-driven RX** (instead of polling)
  - The WaveShare HAT does not break out a dedicated RX-ready interrupt GPIO,
    but if one becomes available it would replace the polling loop.

### Low Priority / Nice to Have

- [ ] **Thread-safe packet queue**
  - Currently `PacketReceived` is raised on the polling thread.
  - Optionally buffer packets into a `ConcurrentQueue<LoRaRxPacket>` and expose a `TryGetPacket()` method.

- [ ] **Configurable TX timing** (schedule a downlink at a precise timestamp)
  - The TX descriptor supports a `targetTimestamp` field (currently hardcoded to 0 = immediate).
  - Expose `SendPacketAt(LoRaTxPacket, uint timestampUs)`.

- [ ] **NuGet packaging and icon**
  - Add `icon.png` (copy from another driver in the repo).
  - Test `GeneratePackageOnBuild` produces a valid `.nupkg`.

- [ ] **Unit tests** for channel plan frequency offset calculations and TX buffer builder.

---

## Hardware Reference

**WaveShare SX1303 915M LoRaWAN Gateway (B)**
- Product page: https://www.waveshare.com/wiki/SX1303_915M_LoRaWAN_Gateway_(B)
- Chip: Semtech SX1303 + 2× SX1250 RF front-ends
- Semtech HAL: https://github.com/Lora-net/sx1302_hal (Apache-2.0 / Semtech SDK license)

**Default Raspberry Pi GPIO (BCM numbering):**

| Signal       | BCM GPIO | Physical Pin |
|--------------|----------|--------------|
| SPI0 SCLK    | 11       | 23           |
| SPI0 MOSI    | 10       | 19           |
| SPI0 MISO    | 9        | 21           |
| SPI0 CE0 (CS)| 8        | 24           |
| SX1303 Reset | 23       | 16           |
| Power Enable | 18       | 12           |

---

## Reference: Semtech HAL Files of Interest

| File | Relevance |
|------|-----------|
| `libloragw/inc/loragw_reg.h` | Register address table (`loregs[]`) — ground truth for all register addresses |
| `libloragw/src/loragw_spi.c` | SPI read/write protocol implementation |
| `libloragw/src/loragw_sx1302.c` | `lgw_start()`, `lgw_recv()`, `lgw_send()` — init, RX, TX logic |
| `libloragw/src/loragw_sx1250.c` | SX1250 RF front-end calibration and frequency programming |
| `libloragw/inc/loragw_agc_params.h` | AGC MCU firmware byte array |
| `libloragw/inc/loragw_arb_params.h` | ARB MCU firmware byte array |
| `tools/reset_lgw.sh` | Reference GPIO reset sequence |

---

_Last updated: 2026-03-09_
