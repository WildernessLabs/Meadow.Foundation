using T3ConfigConsole.UI;
using Terminal.Gui;

Application.Init();

bool running = true;
while (running)
{
    var connWin = new ConnectionWindow();
    Application.Run(connWin);

    if (connWin.ConnectedClient != null && connWin.DeviceInfo != null)
    {
        var mainWin = new MainWindow(connWin.ConnectedClient, connWin.DeviceInfo, connWin.ModbusAddress);
        Application.Run(mainWin);

        // After MainWindow returns, the client is usually disconnected. 
        // We loop back to the ConnectionWindow.
    }
    else
    {
        // If the connection window was closed without a connection, exit the app.
        running = false;
    }
}

Application.Shutdown();
