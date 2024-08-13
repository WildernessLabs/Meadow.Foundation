﻿using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.ICs.CAN;

public partial class Mcp2515
{
    public class Mcp2515CanBus : ICanBus
    {
        /// <inheritdoc/>
        public event EventHandler<ICanFrame>? FrameReceived;

        private Mcp2515 Controller { get; }

        /// <inheritdoc/>
        public CanAcceptanceFilterCollection AcceptanceFilters { get; } = new(5);

        internal Mcp2515CanBus(Mcp2515 controller)
        {
            Controller = controller;

            if (Controller.InterruptPort != null)
            {
                Controller.InterruptPort.Changed += OnInterruptPortChanged;
            }

            AcceptanceFilters.CollectionChanged += OnAcceptanceFiltersChanged;
        }

        private int _currentMask = 0;

        private void OnAcceptanceFiltersChanged(object sender, (System.ComponentModel.CollectionChangeAction Action, CanAcceptanceFilter Filter) e)
        {
            switch (e.Action)
            {
                case System.ComponentModel.CollectionChangeAction.Add:
                    if (e.Filter is CanStandardExactAcceptanceFilter sef)
                    {
                        var newMask = _currentMask | 0x7ff;

                        Controller.SetMaskAndFilter(false, newMask, sef.AcceptID, AcceptanceFilters.Count - 1);

                        _currentMask = newMask;
                    }
                    else if (e.Filter is CanStandardRangeAcceptanceFilter srf)
                    {
                    }
                    else if (e.Filter is CanExtendedExactAcceptanceFilter erf)
                    {
                        var newMask = _currentMask | erf.AcceptID;

                        Controller.SetMaskAndFilter(true, newMask, erf.AcceptID, AcceptanceFilters.Count - 1);

                        _currentMask = newMask;
                    }

                    break;
            }
        }

        private void OnInterruptPortChanged(object sender, DigitalPortResult e)
        {
            // TODO: check why the interrupt happened (error, frame received, etc)

            if (FrameReceived != null)
            {
                var frame = ReadFrame();
                Task.Run(() => FrameReceived.Invoke(this, frame));
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
    }
}