using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Environmental;
using System;
using System.Threading.Tasks;

namespace MeadowApp
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Scd30 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            var i2cBus = Device.CreateI2cBus(Meadow.Hardware.I2cBusSpeed.Standard);

            sensor = new Scd30(i2cBus);

            var consumer = Scd30.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Observer: Temp changed by threshold; new temp: {result.New.Temperature?.Celsius:N2}C, old: {result.Old?.Temperature?.Celsius:N2}C");
                },
                filter: result =>
                {
                    if (result.Old?.Temperature is { } oldTemp &&
                        result.Old?.Humidity is { } oldHumidity &&
                        result.New.Temperature is { } newTemp &&
                        result.New.Humidity is { } newHumidity)
                    {
                        return ((newTemp - oldTemp).Abs().Celsius > 0.5 &&
                                (newHumidity - oldHumidity).Percent > 0.05);
                    }
                    return false;
                }
            );

            sensor?.Subscribe(consumer);

            if (sensor != null)
            {
                sensor.Updated += (sender, result) =>
                {
                    Resolver.Log.Info($"  Concentration: {result.New.Concentration?.PartsPerMillion:N0}ppm");
                    Resolver.Log.Info($"  Temperature: {result.New.Temperature?.Celsius:N1}C");
                    Resolver.Log.Info($"  Relative Humidity: {result.New.Humidity:N0}%");
                };
            }

            //The value 5 is used purely for demonstration purposes. 2 is the sensor default.
            sensor?.SetMeasurementInterval(5);
            var measurementInterval = sensor?.GetMeasurementInterval();
            Resolver.Log.Info($"Measurement Interval: {measurementInterval}sec");

            var autoSelfCalibration = sensor?.SelfCalibrationEnabled();
            Resolver.Log.Info($"Auto Self Calibration: {autoSelfCalibration}");

            var ambientPressure = sensor?.GetAmbientPressureOffset();
            Resolver.Log.Info($"Ambient Pressure offset: {ambientPressure}mbar");

            var altitudeOffset = sensor?.GetAltitudeOffset();
            Resolver.Log.Info($"Altitude Offset: {altitudeOffset}m");

            var temperatureOffset = sensor?.GetTemperatureOffset();
            Resolver.Log.Info($"Temperature Offset: {temperatureOffset}C");

            var forceCalibrationValue = sensor?.GetForceCalibrationValue();
            Resolver.Log.Info($"Force Calibration Value: {forceCalibrationValue}");

            sensor?.StartUpdating(TimeSpan.FromSeconds(5));

            return base.Initialize();
        }

        //<!=SNOP=>
    }
}