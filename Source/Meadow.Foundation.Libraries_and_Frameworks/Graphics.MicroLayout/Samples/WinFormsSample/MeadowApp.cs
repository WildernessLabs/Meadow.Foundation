using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Hid;
using System.Diagnostics;

public class MeadowApp : App<Windows>
{
    private MicroGraphics _graphics = default!;
    private WinFormsDisplay _display = default!;
    private Keyboard _keyBoard = default!;

    public override async Task Run()
    {
        _ = Task.Run(() =>
        {
            Thread.Sleep(1000);
            DisplayLoop();
        });

        Application.Run(_display);
    }

    public override Task Initialize()
    {
        _display = new WinFormsDisplay();
        _graphics = new MicroGraphics(_display);
        _keyBoard = new Keyboard();

        var rightButton = new PushButton(_keyBoard.CreateDigitalInputPort(_keyBoard.Pins.Right));
        var leftButton = new PushButton(_keyBoard.CreateDigitalInputPort(_keyBoard.Pins.Left));

        rightButton.PressStarted += OnRightButtonPressStarted;
        rightButton.PressEnded += OnRightButtonPressEnded;

        return base.Initialize();
    }

    private void OnRightButtonPressStarted(object? sender, EventArgs e)
    {
        Debug.WriteLine("down");
    }

    private void OnRightButtonPressEnded(object? sender, EventArgs e)
    {
        Debug.WriteLine("up");
    }

    void DisplayLoop()
    {
        while (true)
        {

            _display.Invoke(() =>
            {
                // Do your drawing stuff here

                _graphics.Show();
                Thread.Sleep(10);
            });
        }
    }
}
