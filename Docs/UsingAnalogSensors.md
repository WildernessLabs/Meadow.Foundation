# Using Analog Sensors

There are a couple general ways to use analog sensors in Meadow.Foundation, and which method you choose depends on your use case and application needs.

## Read() Once, or Occasionally

The simplest way to read a value from a sensor is to call `Read()`, which is an async call that will automatically oversample (take multiple readings and average) and return the value. It also saves the reading for later access on the appropriate property. For instance, the following code does a one-off read from an analog temperature sensor:

```csharp
AnalogTemperature analogTemperature = new AnalogTemperature
(
    device: Device,
    analogPin: Device.Pins.A00,
    sensorType: AnalogTemperature.KnownSensorType.LM35
);

var temp = analogTemperature.Read().Wait();
```

Later on, that value can be accessed via the `Temperature` property:

```csharp
Console.WriteLine($"Last read temp: {analogTemperature.Temperature}ºC");
```

### Recommended Use

This method is recommended for when you need an occasional reading, or want to manually control when the sensor is read.

## Automatic Polling

Sensors can also be setup to provide continuous readings, and offers multiple ways to consume the reading information from them. To set up continuous readings, you call `StartUpdating()` which spins up a thread to continuously poll (sample) and feed that data to either traditional event subscribers or `IObservable` subscribers.

### With Traditional Events



analogTemperature.StartUpdating();

```csharp
AnalogTemperature analogTemperature = new AnalogTemperature
(
    device: Device,
    analogPin: Device.Pins.A00,
    sensorType: AnalogTemperature.KnownSensorType.LM35
);

analogTemperature.Changed += (s, e) => {
	Console.WriteLine($"Temperature: {e.Temperature)}";
}

analogTemperature.StartUpdating();
```

### With FilterableObservable and IObservable

For a more powerful and composeable approach, you can use the same IObservable/Reactive pattern that the underling ports use. For instance, the following code creates a `FilterableObservable` handler that subscribes to the changes from an analog temperature sensor, but automatically filters so that the application is only notified when the temperature changes by at least `1ºC`:

```csharp
AnalogTemperature analogTemperature = new AnalogTemperature
(
    device: Device,
    analogPin: Device.Pins.A00,
    sensorType: AnalogTemperature.KnownSensorType.LM35
);

analogTemperature.Subscribe(new FilterableObserver<FloatChangeResult, float>(
    h => {
        Console.WriteLine($"Temp changed by a degree; new: {h.New}, old: {h.Old}");
    },
    e => {
        return (Math.Abs(e.Delta) > 1);
    }
    ));

analogTemperature.StartUpdating();
```

In the case above, a _predicate_ that tests for a particular condition is passed in to the `FilterableObservable` constructor, which is used as a filter. Any expression that evaluates to a `boolean` (`true`/`false`), can be used. So for instance, you could get notified when the temperature changes by at least `3%` using the following predicate expression:

```csharp
return (Math.Abs(e.Delta / e.Old) > 0.03);
```

Or you could get notified if the temperature went _down_ by `1ºC`:

```csharp
return (e.Delta <= -1);
```

### Recommended Use

The advantage of this approach is that it will automatically poll the sensor in the background without having to manually manage the lifecycle. And because there is also a `StopUpdating()` method, you can still spin up and spin down the polling thread.

And with the `FilterableObservable`, you can create filters to only get notified when needed, rather than having to manually filter all events. 

