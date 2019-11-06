# HowTo: Creating Analog Sensor Drivers

When creating analog sensor drivers, there are some requirements that should be met to satisfy the patterns for their use. If you haven't read the doc on [using analog sensors](UsingAnalogSensors.md), it's a must-read before implementing them.

# 1. Build against a sensor Interface/contract.

All sensors should implement the appropriate sensor Interface. For instance, an analog moisture sensor (which measures soil moisture) should implement `Meadow.Peripherals.Sensors.Moisture.IMoistureSensor`:

```csharp
public interface IMoistureSensor : ISensor, IObservable<FloatChangeResult>
{
    float Moisture { get; }

    event EventHandler<FloatChangeResult> Changed;

    Task<float> Read(int sampleCount = 10, int sampleInterval = 40);

    void StartUpdating(
        int sampleCount = 10,
        int sampleIntervalDuration = 40,
        int sampleSleepDuration = 0);

    void StopUpdating();
}

```

## Sample Implementation

There are a few important aspects of this contract:

1. `IObservable<FloatChangeResult>` - This means that the sensor should support the `IObservable` pattern for event subscriptions.
2. `float Moisture { get; }` - This specifies that the last `Moisture` level should be made available via a simple property.
3. `event EventHandler<FloatChangeResult> Changed;` - This specifies that a classical .NET eventing model is supported for change notifications.
4. `Read()` - This specifies that the driver should support individual reads.
5. `StartUpdating()` and `StopUpdating` - This specifies that the consumer can manually manage the sensor polling lifecycle.

Let's look at the `Capacitive` moisture sensor driver implementation: 

```csharp
using System;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Moisture;
//using Meadow.Foundation.Sensors;

namespace Meadow.Foundation.Sensors.Moisture
{
    /// <summary>
    /// Capacitive Soil Moisture Sensor
    /// </summary>
    public class Capacitive : FilterableObservableBase<FloatChangeResult, float>, IMoistureSensor
    {
        /// <summary>
        /// Raised when the value of the reading changes.
        /// </summary>
        public event EventHandler<FloatChangeResult> Changed = delegate { };

        /// <summary>
        /// Returns the analog input port
        /// </summary>
        public IAnalogInputPort AnalogInputPort { get; protected set; }

        /// <summary>
        /// Last value read from the moisture sensor.
        /// </summary>
        public float Moisture { get; protected set; }


        /// <summary>
        /// Default constructor is private to prevent it being called.
        /// </summary>
        private Capacitive() { }

        /// <summary>
        /// Creates a Capacitive soil moisture sensor object with the especified analog pin and a IO device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="analogPin"></param>
        public Capacitive(IIODevice device, IPin analogPin)
            : this(device.CreateAnalogInputPort(analogPin)) {
        }

        /// <summary>
        /// Creates a Capacitive soil moisture sensor object with the especified AnalogInputPort.
        /// </summary>
        /// <param name="analogPort"></param>
        public Capacitive(IAnalogInputPort analogPort)
        {
            AnalogInputPort = analogPort;

            // wire up our observable
            // have to convert from voltage to temp units for our consumers
            // this is where the magic is: this allows us to extend the IObservable
            // pattern through the sensor driver
            AnalogInputPort.Subscribe(
                new FilterableObserver<FloatChangeResult, float>(
                    h => {
                        var newMoisture = VoltageToMoisture(h.New);
                        var oldMoisture = VoltageToMoisture(h.Old);
                        this.Moisture = newMoisture; // save state
                        RaiseChangedAndNotify(new FloatChangeResult(
                            newMoisture,
                            oldMoisture));
                    })
                );

        }

        /// <summary>
        /// Convenience method to get the current soil moisture. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        /// <param name="sampleCount">The number of sample readings to take. 
        /// must be greater than 0.</param>
        /// <param name="sampleInterval">The interval, in milliseconds, between
        /// sample readings.</param>
        /// <returns></returns>
        public async Task<float> Read(int sampleCount = 10, int sampleInterval = 40)
        {
            // read the voltage
            float voltage = await this.AnalogInputPort.Read(sampleCount, sampleInterval);
            // convert and save to our temp property for later retreival
            this.Moisture = VoltageToMoisture(voltage);
            // return
            return this.Moisture;
        }

        /// <summary>
        /// Starts continuously sampling the temperature. Also triggers the
        /// events to fire, and IObservable subscribers to get notified.
        /// </summary>
        /// <param name="sampleCount"></param>
        /// <param name="sampleIntervalDuration"></param>
        /// <param name="sampleSleepDuration"></param>
        public void StartUpdating(
            int sampleCount = 10,
            int sampleIntervalDuration = 40,
            int sampleSleepDuration = 0)
        {
            AnalogInputPort.StartSampling(sampleCount, sampleIntervalDuration, sampleSleepDuration);
        }

        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public void StopUpdating()
        {
            AnalogInputPort.StopSampling();
        }

        protected void RaiseChangedAndNotify(FloatChangeResult changeResult)
        {
            Changed?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }


        protected float VoltageToMoisture(float voltage) {
            return Map(voltage, 0f, 1.0f, 0f, 3.3f);
        }

        /// <summary>
        /// Re-maps a value from one range (fromLow - fromHigh) to another (toLow - toHigh).
        /// </summary>
        /// <param name="value"></param>
        /// <param name="fromLow"></param>
        /// <param name="fromHigh"></param>
        /// <param name="toLow"></param>
        /// <param name="toHigh"></param>
        /// <returns></returns>
        protected float Map(float value, float fromLow, float fromHigh, float toLow, float toHigh)
        {
            return (((toHigh - toLow) * (value - fromLow)) / (fromHigh - fromLow)) - toLow;
        }
    }
}
```

# 2. Provide a convenience method to get a single reading, called `Read()`.

In the code above, there are two important pieces to support this.

First, is the `Moisture` value property:

```csharp
public float Moisture { get; protected set; }
```

Next, is the `Read()` method:

```csharp
public async Task<float> Read(int sampleCount = 10, int sampleInterval = 40)
{
    // read the voltage
    float voltage = await this.AnalogInputPort.Read(sampleCount, sampleInterval);
    // convert and save to our temp property for later retrieval
    this.Moisture = VoltageToMoisture(voltage);
    // return
    return this.Moisture;
}
```

This method calls into the underling analog port and gets the raw value, converts it to a meaningful unit, and stores the value in the `Moisture` property for later access.

# 3. Implement the IObservable and eventing pattern.

Meadow.Foundation includes a `FilterableObservableBase<C,T>` class that does a lot of the heavy lifting here, but there are important steps that need to be taken to integrate it. The first is to add it to the sensor class declaration:

```csharp
public class Capacitive : FilterableObservableBase<FloatChangeResult, float>, IMoistureSensor
```

## Subscribe to change notifications in the constructor.

In the sensor constructor, make sure to sure to subscribe to notifications from the underlying analog port, **and convert the voltages to the meaningful unit of the sensor**. In the case of the moisture sensor, it takes the voltage and converts to a value between `0f` and `1.0f`, so that the consumer can write filters that work on a meaningful unit:

```csharp
AnalogInputPort.Subscribe(
    new FilterableObserver<FloatChangeResult, float>(
        h => {
            var newMoisture = VoltageToMoisture(h.New);
            var oldMoisture = VoltageToMoisture(h.Old);
            this.Moisture = newMoisture; // save state
            RaiseChangedAndNotify(new FloatChangeResult(
                newMoisture,
                oldMoisture));
        })
    );
```

Again, you should also save the reading on the appropriate sensor property here, in this case it's `Moisture`.

Note that this also requires a `RaiseChangedAndNotify()` method:

```csharp
protected void RaiseChangedAndNotify(FloatChangeResult changeResult)
{
    Changed?.Invoke(this, changeResult);
    base.NotifyObservers(changeResult);
}
```

The `FilterableObservableBase` class will handle the observer notifications.


## Add StartUpdating() and StopUpdating() methods

Next, add `StartUpdating()` and `StopUpdating` methods to spin up and spin down the analog port conversions:

```csharp
public void StartUpdating(
    int sampleCount = 10,
    int sampleIntervalDuration = 40,
    int sampleSleepDuration = 0)
{
    AnalogInputPort.StartSampling(sampleCount, sampleIntervalDuration, sampleSleepDuration);
}

...

public void StopUpdating()
{
    AnalogInputPort.StopSampling();
}
```



