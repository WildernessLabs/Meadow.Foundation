Imports Meadow
Imports Meadow.Devices
Imports Meadow.Foundation.Sensors.Atmospheric

Public Class FeatherV2App
    Inherits MeadowApp(Of F7FeatherV2)

End Class

Public Class CoreComputeApp
    Inherits MeadowApp(Of F7CoreComputeV2)

End Class

Public Class MeadowApp(Of T As F7MicroBase)
    Inherits App(Of T)

    Private sensor As Bme280

    Public Overrides Function Initialize() As Task
        Resolver.Log.Info("Initializing...")

        Dim bus = Device.CreateI2cBus()
        sensor = New Bme280(bus)

        Dim consumer = Bme280.CreateObserver(
            handler:=Sub(result)
                         Resolver.Log.Info($"Observer: Temp changed by threshold; new temp: {result.New.Temperature?.Celsius:N1}C, old: {result.Old?.Temperature?.Celsius:N1}C")
                     End Sub,
            filter:=Function(result)
                        If result.Old?.Temperature IsNot Nothing AndAlso
                           result.Old?.Humidity IsNot Nothing AndAlso
                           result.New.Temperature IsNot Nothing AndAlso
                           result.New.Humidity IsNot Nothing Then

                            Dim oldTemp = result.Old.Value.Temperature.Value.Celsius
                            Dim oldHumidity = result.Old.Value.Humidity.Value.Percent
                            Dim newTemp = result.New.Temperature.Value.Celsius
                            Dim newHumidity = result.New.Humidity.Value.Percent

                            Return Math.Abs(newTemp - oldTemp) > 0.5 AndAlso
                                   (newHumidity - oldHumidity) / oldHumidity > 0.05
                        End If
                        Return False
                    End Function)

        Return Task.CompletedTask
    End Function

    Public Overrides Async Function Run() As Task
        Dim conditions = Await sensor.Read()
        Resolver.Log.Info("Initial Readings:")
        Resolver.Log.Info($"  Temperature: {conditions.Temperature?.Celsius:N2}C")
        Resolver.Log.Info($"  Pressure: {conditions.Pressure?.Bar:N2}hPa")
        Resolver.Log.Info($"  Relative Humidity: {conditions.Humidity?.Percent:N2}%")

        sensor.StartUpdating(TimeSpan.FromSeconds(1))
    End Function
End Class