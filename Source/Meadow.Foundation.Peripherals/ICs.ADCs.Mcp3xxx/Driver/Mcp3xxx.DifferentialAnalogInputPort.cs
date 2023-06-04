#nullable enable

using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders
{
    abstract partial class Mcp3xxx
    {
        /// <summary>
        /// Represents an Mcp3xxx analog input port
        /// </summary>
        public class DifferentialAnalogInputPort : IAnalogInputPort
        {
            /// <summary>
            /// Is the port sampling
            /// </summary>
            public bool IsSampling { get; protected set; } = false;

            /// <summary>
            /// Current port voltage
            /// </summary>
            public Voltage Voltage { get; private set; }

            /// <summary>
            /// The sample count
            /// </summary>
            public int SampleCount { get; private set; }

            /// <summary>
            /// The update interval
            /// </summary>
            public TimeSpan UpdateInterval { get; private set; } = TimeSpan.FromSeconds(1);

            /// <summary>
            /// The voltage sampling buffer
            /// </summary>
            public IList<Voltage> VoltageSampleBuffer => buffer;

            /// <summary>
            /// The sampling interval
            /// </summary>
            public TimeSpan SampleInterval => TimeSpan.Zero;

            private readonly List<Voltage> buffer = new();

            private Voltage? _previousVoltageReading;

            private readonly Mcp3xxx controller;

            private CancellationTokenSource? SamplingTokenSource;

            private readonly object _lock = new();

            /// <summary>
            /// Raised when the port voltage value changes
            /// </summary>
            public event EventHandler<IChangeResult<Voltage>> Updated = (s, e) => { };

            /// <summary>
            /// Collection of event observers for the Updated event
            /// </summary>
            protected List<IObserver<IChangeResult<Voltage>>> Observers { get; set; } = new List<IObserver<IChangeResult<Voltage>>>();



            public IDisposable Subscribe(IObserver<IChangeResult<Voltage>> observer)
            {
                throw new NotImplementedException();
            }
        }
    }
}
