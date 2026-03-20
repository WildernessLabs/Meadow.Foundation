using Meadow.Hardware;
using System;
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

        /// <inheritdoc/>
        public event EventHandler<ICanFrame>? FrameReceived;
        /// <inheritdoc/>
        public event EventHandler<CanErrorInfo>? BusError;

        private Mcp2515 Controller { get; }

        /// <inheritdoc/>
        public CanBitrate BitRate
        {
            get => Controller.bitrate;
            set => Controller.Initialize(value, Controller.oscillator);
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
        /// and dispatches to <see cref="FrameReceived"/> or <see cref="BusError"/> as appropriate
        /// </summary>
        private void OnInterruptPortChanged(object sender, DigitalPortResult e)
        {
            var canstat = (InterruptCode)Controller.ReadRegister(Register.CANSTAT)[0] & InterruptCode.Mask;

            switch (canstat)
            {
                case InterruptCode.RXB0:
                case InterruptCode.RXB1:
                    if (FrameReceived != null)
                    {
                        var frame = ReadFrame();
                        Task.Run(() => FrameReceived.Invoke(this, frame));
                    }
                    break;
                case InterruptCode.Error:
                    if (BusError != null)
                    {
                        var errors = Controller.ReadRegister(Register.EFLG)[0];
                        // read the error counts
                        var tec = Controller.ReadRegister(Register.TEC)[0];
                        var rec = Controller.ReadRegister(Register.REC)[0];
                        BusError.Invoke(this, new CanErrorInfo
                        {
                            ReceiveErrorCount = rec,
                            TransmitErrorCount = tec
                        });
                        // clear the error interrupt
                        Controller.ClearInterrupt(InterruptFlag.ERRIF | InterruptFlag.MERRF);
                    }
                    break;
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
            Controller.WriteFrame(frame, 0);
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
