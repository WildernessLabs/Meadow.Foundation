﻿using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents an Analog Input Multiplexer (Mux) base
    /// </summary>
    public abstract class AnalogMuxBase : IAnalogInputMultiplexer
    {
        /// <summary>
        /// Get the sync root
        /// </summary>
        protected object SyncRoot { get; } = new object();

        /// <summary>
        /// The port connected to the Enable pin of the mux (otherwise must be tied low)
        /// </summary>
        public IDigitalOutputPort? EnablePort { get; }

        /// <summary>
        /// The analog input connected to the Mux output pin (Z)
        /// </summary>
        public IAnalogInputPort Signal { get; }

        /// <summary>
        /// Set input channel
        /// </summary>
        /// <param name="channel">he input channel</param>
        public abstract void SetInputChannel(int channel);

        internal AnalogMuxBase(IAnalogInputPort signalPort, IDigitalOutputPort? enablePort)
        {
            Signal = signalPort;
            EnablePort = enablePort;
        }

        /// <summary>
        /// Enables the multiplexer (if an enable port was provided)
        /// </summary>
        public void Enable()
        {
            if (EnablePort != null)
            {
                lock (SyncRoot)
                {
                    EnablePort.State = false; // active low
                }
            }
        }

        /// <summary>
        /// Disables the multiplexer (if an enable port was provided)
        /// </summary>
        public void Disable()
        {
            if (EnablePort != null)
            {
                lock (SyncRoot)
                {
                    EnablePort.State = true;
                }
            }
        }
    }
}