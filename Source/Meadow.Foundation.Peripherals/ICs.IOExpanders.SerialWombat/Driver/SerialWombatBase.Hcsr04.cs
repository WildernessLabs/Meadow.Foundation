using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class SerialWombatBase
    {
        /// <summary>
        /// Represents a HCSR04 distance sensor connected to a Serial Wombat
        /// </summary>
        public class Hcsr04 : Sensors.Distance.Hcsr04, IDisposable
        {
            /// <summary>
            /// The default sensor read period
            /// </summary>
            public static TimeSpan DefaultReadPeriod => TimeSpan.FromMilliseconds(250);

            readonly SerialWombatBase controller;
            readonly IPin echoPin;
            bool disposed;

            /// <summary>
            /// Create a new Hcsr04 object
            /// </summary>
            public Hcsr04(SerialWombatBase controller, IPin trigger, IPin echo)
                : this(controller, trigger, echo, DefaultReadPeriod)
            { }

            /// <summary>
            /// Create a new Hcsr04 object
            /// </summary>
            public Hcsr04(SerialWombatBase controller, IPin trigger, IPin echo, TimeSpan readPeriod)
            {
                this.controller = controller;
                echoPin = echo;
                controller.ConfigureUltrasonicSensor(trigger, echo);

                if (readPeriod.TotalMilliseconds > 0)
                {
                    Task.Run(async () =>
                    {
                        while (!disposed)
                        {
                            ReadPulses();
                            await Task.Delay(readPeriod);
                        }
                    });
                }
            }

            /// <summary>
            /// Start a distance measurement
            /// </summary>
            public override void MeasureDistance()
            {
                ReadPulses();
            }

            private void ReadPulses()
            {
                var oldDistance = Distance;

                var d = controller.ReadPublicData(echoPin);

                controller?.Logger?.Debug($"d: {d}");

                var newDistance = new Length(d, Length.UnitType.Millimeters);
                base.RaiseEventsAndNotify(new ChangeResult<Length>(newDistance, oldDistance));

                Distance = newDistance;
            }

            /// <summary>
            /// Dispose
            /// </summary>
            /// <param name="disposing">True if disposing</param>
            protected virtual void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        // TODO: dispose managed state (managed objects)
                    }

                    disposed = true;
                }
            }

            /// <summary>
            /// Dispose
            /// </summary>
            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
