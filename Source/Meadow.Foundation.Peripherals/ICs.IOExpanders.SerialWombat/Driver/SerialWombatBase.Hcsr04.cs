using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class SerialWombatBase
    {
        public class Hcsr04 : Sensors.Distance.Hcsr04, IDisposable
        {
            public static TimeSpan DefaultReadPeriod = TimeSpan.FromMilliseconds(250);

            SerialWombatBase controller;
            bool disposed;
            TimeSpan readPeriod;
            IPin echoPin;

            public Hcsr04(SerialWombatBase controller, IPin trigger, IPin echo)
                : this(controller, trigger, echo, DefaultReadPeriod)
            {
            }

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

            public override void MeasureDistance()
            {
                ReadPulses();
            }

            private void ReadPulses()
            {
                var oldDistance = Distance;

                var d = controller.ReadPublicData(echoPin);

                controller.Logger.Debug($"d: {d}");

                var newDistance = new Length(d, Length.UnitType.Millimeters);
                base.RaiseEventsAndNotify(new ChangeResult<Length>(newDistance, oldDistance));

                Distance = newDistance;
            }

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

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
