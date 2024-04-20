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

        Return Task.CompletedTask
    End Function

    Public Overrides Async Function Run() As Task
        Dim conditions = Await sensor.Read()
        Resolver.Log.Info("Initial Readings:")
        Resolver.Log.Info($"  Temperature: {conditions.Temperature?.Celsius:N2}C")
        Resolver.Log.Info($"  Pressure: {conditions.Pressure?.Bar:N2}hPa")
        Resolver.Log.Info($"  Relative Humidity: {conditions.Humidity?.Percent:N2}%")

        '        Do While True
        '        conditions = Await sensor.Read()
        '        Resolver.Log.Info($"  Temperature: {conditions.Temperature?.Celsius:N2}C")
        '       Resolver.Log.Info($"  Pressure: {conditions.Pressure?.Bar:N2}hPa")
        '      Resolver.Log.Info($"  Relative Humidity: {conditions.Humidity?.Percent:N2}%")
        '     Await Task.Delay(1000)
        '    Loop
    End Function
End Class