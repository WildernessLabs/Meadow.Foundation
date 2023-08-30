using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents a Pcx857x I2C IO Expander
    /// </summary>
    public abstract partial class Pcx857x : IDigitalOutputController, IDigitalInputController, II2cPeripheral, IDisposable
    {
        /// <summary>
        /// The number of IO pins avaliable on the device
        /// </summary>
        public abstract int NumberOfPins { get; }

        /// <inheritdoc/>
        public byte DefaultI2cAddress => 0x20;

        private readonly List<IPin> pinsInUse = new();
        private bool isDisposed;
        private IDigitalInterruptPort? interruptPort;
        private readonly bool createdPort = false;

        /// <summary>
        /// The I2C Communications object
        /// </summary>
        protected readonly II2cCommunications i2CCommunications;
        
        /// <summary>
        /// Creates a new Pcx857x instance
        /// </summary>
        /// <param name="i2cBus">The I2C buss the peripheral is connected to</param>
        /// <param name="address">The bus address of the peripheral</param>
        /// <param name="interruptPin">The interrupt pin</param>
        public Pcx857x(II2cBus i2cBus, byte address, IPin? interruptPin = default)
            : this(i2cBus, address, interruptPin?.CreateDigitalInterruptPort(InterruptMode.EdgeBoth))
        {
            createdPort = true;
        }

        /// <summary>
        /// Creates a new Pcx857x instance
        /// </summary>
        /// <param name="i2cBus">The I2C bus the peripheral is connected to</param>
        /// <param name="address">The I2C bus address of the peripheral</param>
        /// <param name="interruptPort">The interrupt port</param>
        public Pcx857x(II2cBus i2cBus, byte address, IDigitalInterruptPort? interruptPort = default)
        {
            i2CCommunications = new I2cCommunications(i2cBus, address);

            this.interruptPort = interruptPort;

            AllOff();
        }

        /// <inheritdoc/>
        public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType initialOutputType = OutputType.PushPull)
        {
            lock (pinsInUse)
            {
                if (pinsInUse.Contains(pin))
                {
                    throw new PortInUseException($"{GetType().Name} pin {pin.Name} is already in use.");
                }
                var port = new DigitalOutputPort(this, pin, initialState);

                pinsInUse.Add(pin);

                SetPinDirection(false, (byte)pin.Key);

                port.Disposed += (s, e) =>
                {
                    lock (pinsInUse)
                    {
                        pinsInUse.Add(pin);
                    }
                };

                return port;
            }
        }

        /// <inheritdoc/>
        public IDigitalInputPort CreateDigitalInputPort(IPin pin, ResistorMode resistorMode)
        {
            switch (resistorMode)
            {
                case ResistorMode.InternalPullUp:
                case ResistorMode.InternalPullDown:
                    throw new ArgumentException("Internal resistors are not supported");
            }

            lock (pinsInUse)
            {
                if (pinsInUse.Contains(pin))
                {
                    throw new PortInUseException($"{GetType().Name} pin {pin.Name} is already in use.");
                }
                var port = new DigitalInputPort(this, pin);
                pinsInUse.Add(pin);

                SetPinDirection(true, (byte)pin.Key);

                port.Disposed += (s, e) =>
                {
                    lock (pinsInUse)
                    {
                        pinsInUse.Add(pin);
                    }
                };

                return port;
            }
        }

        /// <summary>
        /// Set the pin direction
        /// </summary>
        /// <param name="input">true for input, false for output</param>
        /// <param name="pinKey">The pin key value</param>
        protected abstract void SetPinDirection(bool input, byte pinKey);

        /// <inheritdoc/>
        public abstract IPin GetPin(string pinName);

        /// <summary>
        /// Checks if a pin exists on the Pcx857x
        /// </summary>
        protected abstract bool IsValidPin(IPin pin);

        /// <summary>
        /// Convenience method to turn all outputs off
        /// </summary>
        public void AllOff()
        {
            if(NumberOfPins == 8)
            {
                i2CCommunications.Write(0x00);
            }
            else // 16
            {
                WriteUint16(0x0000);
            }
        }

        /// <summary>
        /// Convenience method to turn all outputs on
        /// </summary>
        public void AllOn()
        {
            if (NumberOfPins == 8)
            {
                i2CCommunications.Write(0xFF);
            }
            else // 16
            {
                WriteUint16(0xFFFF);
            }
        }

        /// <summary>
        /// Retrieves the state of a pin
        /// </summary>
        /// <param name="pin">The pin to query</param>
        protected abstract bool GetState(IPin pin);

        /// <summary>
        /// Sets the state of a pin
        /// </summary>
        /// <param name="pin">The pin to affect</param>
        /// <param name="state"><b>True</b> to set the pin state high, <b>False</b> to set it low</param>
        protected abstract void SetState(IPin pin, bool state);

        void WriteUint16(ushort value)
        {
            Span<byte> buffer = stackalloc byte[2];
            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            i2CCommunications.Write(buffer);
        }

        /// <summary>
        /// Disposes the instances resources
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (createdPort && interruptPort != null) 
                    {
                        interruptPort.Dispose();
                        interruptPort = null;
                    }
                }

                isDisposed = true;
            }
        }

        /// <summary>
        /// Disposes the instances resources
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}