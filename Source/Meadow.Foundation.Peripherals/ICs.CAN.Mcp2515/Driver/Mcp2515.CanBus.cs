using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Meadow.Foundation.ICs.CAN;

public partial class Mcp2515
{
    /// <summary>
    /// Represents a CAN bus using the MCP2515
    /// </summary>
    public class Mcp2515CanBus : ICanBus
    {
        private int _currentMask = 0;
        // Serializes all SPI sequences. The interrupt handler holds this for the entire
        // drain loop so WriteFrame responses cannot interleave mid-sequence and leave
        // interrupt flags uncleaned (which would permanently assert INT low).
        private readonly object _spiLock = new();

        /// <inheritdoc/>
        public event EventHandler<ICanFrame>? FrameReceived;
        /// <inheritdoc/>
        public event EventHandler<CanErrorInfo>? BusError;

        private Mcp2515 Controller { get; }

        /// <inheritdoc/>
        public CanBitrate BitRate
        {
            get => Controller.bitrate;
            set
            {
                lock (_spiLock)
                {
                    Controller.Initialize(value, Controller.oscillator);
                }
            }
        }

        /// <inheritdoc/>
        public CanAcceptanceFilterCollection AcceptanceFilters { get; } = new(5);

        /// <summary>
        /// Creates a new Mcp2515CanBus instance bound to the specified MCP2515 controller
        /// </summary>
        /// <param name="controller">The MCP2515 controller that owns this bus</param>
        internal Mcp2515CanBus(Mcp2515 controller)
        {
            Controller = controller;

            if (Controller.InterruptPort != null)
            {
                Controller.InterruptPort.Changed += OnInterruptPortChanged;
            }

            AcceptanceFilters.CollectionChanged += OnAcceptanceFiltersChanged;
        }

        /// <summary>
        /// Handles changes to the acceptance filter collection by programming the hardware mask and filter registers
        /// </summary>
        private void OnAcceptanceFiltersChanged(object? sender, (System.ComponentModel.CollectionChangeAction Action, CanAcceptanceFilter Filter) e)
        {
            switch (e.Action)
            {
                case System.ComponentModel.CollectionChangeAction.Add:
                    if (e.Filter is CanStandardExactAcceptanceFilter sefa)
                    {
                        // Standard 11-bit frame: all 11 ID bits must match
                        var newMask = 0x7ff;

                        Controller.SetMaskAndFilter(false, newMask, sefa.AcceptID, AcceptanceFilters.Count - 1);

                        _currentMask = newMask;
                    }
                    else if (e.Filter is CanExtendedExactAcceptanceFilter eef)
                    {
                        // Extended 29-bit frame: all 29 ID bits must match
                        var newMask = 0x1FFFFFFF;

                        Controller.SetMaskAndFilter(true, newMask, eef.AcceptID, AcceptanceFilters.Count - 1);

                        _currentMask = newMask;
                    }
                    else if (e.Filter is CanStandardRangeAcceptanceFilter)
                    {
                        throw new NotSupportedException("Range-based acceptance filters are not supported by the MCP2515 hardware");
                    }
                    else if (e.Filter is CanExtendedRangeAcceptanceFilter)
                    {
                        throw new NotSupportedException("Range-based acceptance filters are not supported by the MCP2515 hardware");
                    }

                    break;
                case System.ComponentModel.CollectionChangeAction.Remove:
                    if (e.Filter is CanStandardExactAcceptanceFilter)
                    {
                        _currentMask = 0x00;
                    }
                    else if (e.Filter is CanExtendedExactAcceptanceFilter)
                    {
                    }
                    else if (e.Filter is CanStandardRangeAcceptanceFilter)
                    {
                        throw new NotSupportedException("Range-based acceptance filters are not supported by the MCP2515 hardware");
                    }
                    else if (e.Filter is CanExtendedRangeAcceptanceFilter)
                    {
                        throw new NotSupportedException("Range-based acceptance filters are not supported by the MCP2515 hardware");
                    }

                    break;

            }
        }

        /// <summary>
        /// Handles interrupt pin transitions, reads the interrupt cause from CANSTAT.ICOD,
        /// and dispatches to <see cref="FrameReceived"/> or <see cref="BusError"/> as appropriate.
        /// All SPI operations are performed inside _spiLock. Frames are collected first and
        /// dispatched after the lock is released so that WriteFrame responses cannot race with
        /// the interrupt-clearing SPI sequence and leave INT permanently asserted.
        /// </summary>
        private void OnInterruptPortChanged(object sender, DigitalPortResult e)
        {
            var pendingFrames = new List<ICanFrame>();

            try
            {
                lock (_spiLock)
                {
                    while (true)
                    {
                        var canstat = (InterruptCode)Controller.ReadRegister(Register.CANSTAT)[0] & InterruptCode.Mask;

                        if (canstat == InterruptCode.None)
                        {
                            break;
                        }

                        switch (canstat)
                        {
                            case InterruptCode.RXB0:
                            case InterruptCode.RXB1:
                                // Always read the frame even if no subscriber — ReadDataFrame clears the
                                // RX interrupt flag, which de-asserts INT. If we skip ReadFrame, INT stays
                                // low permanently (edge-triggered: no new falling edge = no more interrupts).
                                try
                                {
                                    var frame = ReadFrame();
                                    if (frame != null) pendingFrames.Add(frame);
                                }
                                catch (Exception ex)
                                {
                                    // ReadDataFrame threw (e.g. DLC > 8 from a corrupted frame) before it
                                    // could clear the interrupt flag — clear it here so INT de-asserts.
                                    Resolver.Log?.Warn($"[MCP2515] Frame read failed: {ex.Message}");
                                    Controller.ClearInterrupt(InterruptFlag.RX0IF | InterruptFlag.RX1IF);
                                }
                                break;
                            case InterruptCode.Error:
                                var eflg = Controller.ReadRegister(Register.EFLG)[0];
                                if (BusError != null)
                                {
                                    var tec = Controller.ReadRegister(Register.TEC)[0];
                                    var rec = Controller.ReadRegister(Register.REC)[0];
                                    BusError.Invoke(this, new CanErrorInfo
                                    {
                                        ReceiveErrorCount = rec,
                                        TransmitErrorCount = tec
                                    });
                                }
                                Controller.ClearInterrupt(InterruptFlag.ERRIF | InterruptFlag.MERRF);

                                // Clear the overflow flags in EFLG if they are set (bits 6 and 7).
                                // Without this, ERRIF may remain set, keeping INT low.
                                if ((eflg & 0xC0) != 0)
                                {
                                    Controller.ModifyRegister(Register.EFLG, 0xC0, 0);
                                }

                                // EFLG bit 5 (TXBO): TEC overflowed, controller entered bus-off and disconnected.
                                // Reinitialize to recover — without this an app restart is required.
                                if ((eflg & 0x20) != 0)
                                {
                                    Controller.Initialize(Controller.bitrate, Controller.oscillator);
                                }
                                break;
                            default:
                                // Unexpected interrupt code (Wake, TXB0-2) — clear all flags so INT
                                // de-asserts. Without this, an unrecognised code leaves INT permanently low.
                                Controller.ClearInterrupt((InterruptFlag)0xff);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Safety net: if any SPI call above throws, we must still de-assert INT.
                // Without this, an unhandled exception would leave INT permanently low.
                Resolver.Log?.Error($"[MCP2515] Interrupt handler failed: {ex.Message}");
                try { Controller.ClearInterrupt((InterruptFlag)0xff); } catch { }
            }

            // Dispatch collected frames only after the SPI lock is released, so that
            // WriteFrame (called by FrameReceived subscribers to send responses) can
            // acquire the lock without deadlocking.
            if (FrameReceived != null && pendingFrames.Count > 0)
            {
                foreach (var f in pendingFrames)
                {
                    var captured = f;
                    Task.Run(() => FrameReceived.Invoke(this, captured));
                }
            }
        }

        /// <inheritdoc/>
        public bool IsFrameAvailable()
        {
            var status = Controller.GetStatus();

            if ((status & Status.RX0IF) == Status.RX0IF)
            {
                return true;
            }
            else if ((status & Status.RX1IF) == Status.RX1IF)
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public void WriteFrame(ICanFrame frame)
        {
            lock (_spiLock)
            {
                Controller.WriteFrame(frame, 0);
            }
        }

        /// <inheritdoc/>
        public ICanFrame? ReadFrame()
        {
            var status = Controller.GetStatus();

            if ((status & Status.RX0IF) == Status.RX0IF)
            { // message in buffer 0
                return Controller.ReadDataFrame(RxBufferNumber.RXB0);
            }
            else if ((status & Status.RX1IF) == Status.RX1IF)
            { // message in buffer 1
                return Controller.ReadDataFrame(RxBufferNumber.RXB1);
            }
            else
            { // no messages available
                return null;
            }
        }

        /// <inheritdoc/>
        public void ClearReceiveBuffers()
        {
            var status = Controller.GetStatus();

            if ((status & Status.RX0IF) == Status.RX0IF)
            { // message in buffer 0
                Controller.ReadDataFrame(RxBufferNumber.RXB0);
            }

            if ((status & Status.RX1IF) == Status.RX1IF)
            { // message in buffer 1
                Controller.ReadDataFrame(RxBufferNumber.RXB1);
            }

            // clear erase rx interrupts
            Controller.ClearInterrupt(InterruptFlag.RX0IF | InterruptFlag.RX1IF);
        }
    }
}
