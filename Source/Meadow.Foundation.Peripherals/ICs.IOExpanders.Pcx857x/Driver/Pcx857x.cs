using Meadow.Hardware;
using System;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents a Pcx857x I2C IO Expander
    /// </summary>
    public abstract partial class Pcx857x : IDigitalOutputController, IDigitalInputController, IDigitalInterruptController,
        II2cPeripheral, IDisposable
    {
        /// <summary>
        /// The number of IO pins available on the device
        /// </summary>
        public abstract int NumberOfPins { get; }

        /// <inheritdoc/>
        public byte DefaultI2cAddress => 0x20;

        private readonly IDictionary<IPin, DigitalInputPort> inputPorts;
        private readonly IDictionary<IPin, DigitalInterruptPort> interruptPorts;
        private readonly List<IPin> pinsInUse = new();
        private IDigitalInterruptPort? interruptPort;

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Did we create the port(s) used by the peripheral
        /// </summary>
        protected bool createdPorts = false;


        /// <summary>
        /// The I2C Communications object
        /// </summary>
        protected readonly II2cCommunications i2cComms;

        /// <summary>
        /// Creates a new Pcx857x instance
        /// </summary>
        /// <param name="i2cBus">The I2C bus the peripheral is connected to</param>
        /// <param name="address">The bus address of the peripheral</param>
        /// <param name="interruptPin">The interrupt pin</param>
        public Pcx857x(II2cBus i2cBus, byte address, IPin? interruptPin)
            : this(i2cBus, address, interruptPin?.CreateDigitalInterruptPort(InterruptMode.EdgeFalling))
        {
            createdPorts = true;
        }

        /// <summary>
        /// Creates a new Pcx857x instance
        /// </summary>
        /// <param name="i2cBus">The I2C bus the peripheral is connected to</param>
        /// <param name="address">The I2C bus address of the peripheral</param>
        /// <param name="interruptPort">The interrupt port</param>
        public Pcx857x(II2cBus i2cBus, byte address, IDigitalInterruptPort? interruptPort = default)
        {
            i2cComms = new I2cCommunications(i2cBus, address);

            this.interruptPort = interruptPort;

            if (interruptPort != null)
            {
                interruptPort.Changed += InterruptPortChanged;
            }

            interruptPorts = new Dictionary<IPin, DigitalInterruptPort>();
            inputPorts = new Dictionary<IPin, DigitalInputPort>();
        }

        /// <inheritdoc/>
        public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType initialOutputType = OutputType.PushPull)
        {
            if (IsValidPin(pin))
            {
                lock (pinsInUse)
                {
                    if (pinsInUse.Contains(pin))
                    {
                        throw new PortInUseException($"{GetType().Name} pin {pin.Name} is already in use");
                    }
                    var port = new DigitalOutputPort(this, pin, initialState);

                    pinsInUse.Add(pin);

                    port.Disposed += (s, e) =>
                    {
                        lock (pinsInUse)
                        {
                            pinsInUse.Remove(pin);
                        }
                    };

                    return port;
                }
            }

            throw new Exception("Pin is out of range");
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

            if (IsValidPin(pin))
            {
                lock (pinsInUse)
                {
                    if (pinsInUse.Contains(pin))
                    {
                        throw new PortInUseException($"{GetType().Name} pin {pin.Name} is already in use");
                    }
                    var port = new DigitalInputPort(this, pin);

                    pinsInUse.Add(pin);
                    inputPorts.Add(pin, port);

                    port.Disposed += (s, e) =>
                    {
                        lock (pinsInUse)
                        {
                            pinsInUse.Remove(pin);
                            inputPorts.Remove(pin);
                        }
                    };

                    return port;
                }
            }
            throw new Exception("Pin is out of range");
        }

        /// <summary>
        /// Creates a new DigitalInputPort using the specified pin
        /// </summary>
        /// <param name="pin">The pin representing the port</param>
        /// <param name="interruptMode">The port interrupt mode</param>
        /// <param name="resistorMode">The port resistor mode</param>
        /// <returns>IDigitalInterruptPort</returns>
        public IDigitalInterruptPort CreateDigitalInterruptPort(
            IPin pin,
            InterruptMode interruptMode = InterruptMode.None,
            ResistorMode resistorMode = ResistorMode.Disabled)
        {
            return CreateDigitalInterruptPort(pin, interruptMode, resistorMode, TimeSpan.Zero, TimeSpan.Zero);
        }

        /// <summary>
        /// Creates a new DigitalInputPort using the specified pin
        /// </summary>
        /// <param name="pin">The pin representing the port</param>
        /// <param name="interruptMode">The port interrupt mode</param>
        /// <param name="resistorMode">The port resistor mode</param>
        /// <param name="debounceDuration">The debounce duration</param>
        /// <returns>IDigitalInterruptPort</returns>
        public IDigitalInterruptPort CreateDigitalInterruptPort(
            IPin pin,
            InterruptMode interruptMode,
            ResistorMode resistorMode,
            TimeSpan debounceDuration)
        {
            return CreateDigitalInterruptPort(pin, interruptMode, resistorMode, debounceDuration, TimeSpan.Zero);
        }

        /// <summary>
        /// Creates a new DigitalInputPort using the specified pin
        /// </summary>
        /// <param name="pin">The pin representing the port</param>
        /// <param name="interruptMode">The port interrupt mode</param>
        /// <param name="resistorMode">The port resistor mode</param>
        /// <param name="debounceDuration">The debounce duration</param>
        /// <param name="glitchDuration">The glitch duration - not configurable on Mcpxxxx</param>
        /// <returns>IDigitalInterruptPort</returns>
        public IDigitalInterruptPort CreateDigitalInterruptPort(
            IPin pin,
            InterruptMode interruptMode,
            ResistorMode resistorMode,
            TimeSpan debounceDuration,
            TimeSpan glitchDuration)
        {
            switch (resistorMode)
            {
                case ResistorMode.InternalPullUp:
                case ResistorMode.InternalPullDown:
                    throw new ArgumentException("Internal resistors are not supported");
            }

            if (IsValidPin(pin))
            {

                lock (pinsInUse)
                {
                    if (pinsInUse.Contains(pin))
                    {
                        throw new PortInUseException($"{GetType().Name} pin {pin.Name} is already in use");
                    }
                    var port = new DigitalInterruptPort(pin, interruptMode, resistorMode)
                    {
                        DebounceDuration = debounceDuration,
                    };
                    pinsInUse.Add(pin);
                    interruptPorts.Add(pin, port);

                    port.Disposed += (s, e) =>
                    {
                        lock (pinsInUse)
                        {
                            pinsInUse.Remove(pin);
                            interruptPorts.Remove(pin);
                        }
                    };

                    return port;
                }
            }

            throw new Exception("Pin is out of range");
        }

        /// <inheritdoc/>
        public abstract IPin GetPin(string pinName);

        /// <summary>
        /// Checks if a pin exists on the Pcx857x
        /// </summary>
        protected abstract bool IsValidPin(IPin pin);

        /// <summary>
        /// Reads the peripheral state register for 8 pin devices
        /// </summary>
        protected abstract ushort ReadState();

        /// <summary>
        /// Writes to the peripheral state register
        /// </summary>
        protected abstract void WriteState(ushort state);

        /// <summary>
        /// Writes to the peripheral state register and saves internal output state
        /// </summary>
        protected abstract void SetState(ushort state);

        /// <summary>
        /// Set the pin direction
        /// </summary>
        /// <param name="input">true for input, false for output</param>
        /// <param name="pinKey">The pin key value</param>
        protected abstract void SetPinDirection(bool input, byte pinKey);

        /// <summary>
        /// Convenience method to turn all outputs off
        /// </summary>
        public void AllOff()
        {
            SetState(0x0000);
        }

        /// <summary>
        /// Convenience method to turn all outputs on
        /// </summary>
        public void AllOn()
        {
            SetState(0xFFFF);
        }

        /// <summary>
        /// Retrieves the state of a pin
        /// </summary>
        /// <param name="pin">The pin to query</param>
        protected abstract bool GetPinState(IPin pin);

        /// <summary>
        /// Sets the state of a pin
        /// </summary>
        /// <param name="pin">The pin to affect</param>
        /// <param name="state"><b>True</b> to set the pin state high, <b>False</b> to set it low</param>
        protected abstract void SetPinState(IPin pin, bool state);

        void WriteUint16(ushort value)
        {
            Span<byte> buffer = stackalloc byte[2];
            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            i2cComms.Write(buffer);
        }

        private void InterruptPortChanged(object sender, DigitalPortResult e)
        {
            if (interruptPorts.Count == 0 && inputPorts.Count == 0)
            {
                return;
            }

            // determine which pin caused the interrupt
            var currentState = ReadState();

            foreach (var port in interruptPorts)
            {
                var pinMask = 1 << ((byte)port.Key.Key);
                var state = (currentState & pinMask) != 0;

                port.Value.Update(state);
            }
        }

        ///<inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (createdPorts)
                    {
                        interruptPort?.Dispose();
                        interruptPort = null;
                    }
                }

                IsDisposed = true;
            }
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}