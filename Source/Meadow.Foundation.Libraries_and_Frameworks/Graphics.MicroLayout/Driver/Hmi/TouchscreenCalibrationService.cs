using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;
using Meadow.Foundation.Serialization;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Meadow.Foundation.Hmi;

public class TouchscreenCalibrationService
{
    public event EventHandler<CalibrationPoint[]> CalibrationComplete;

    private readonly DisplayScreen _screen;
    private readonly Crosshair[] _calibrationPoints;
    private readonly ICalibratableTouchscreen _touchscreen;
    private readonly FileInfo _calibrationDataFile;

    private int _currentPoint = 0;
    private CalibrationPoint[] _calPoints;
    private int _lastTouch = Environment.TickCount;
    private Label _instruction;

    public static Color ScreenColor = Color.White;
    public static Color CrosshairColor = Color.Black;
    public static Color TextColor = Color.DarkBlue;

    public TouchscreenCalibrationService(DisplayScreen screen, FileInfo calibrationDataFile)
    {
        if (screen?.TouchScreen is ICalibratableTouchscreen cts)
        {
            _touchscreen = cts;
        }
        else
        {
            throw new ArgumentException("Touchscreen must be an ICalibratableTouchscreen");
        }

        _calibrationDataFile = calibrationDataFile;

        var margin = 30;

        _screen = screen;
        _instruction = new Label(0, 0, screen.Width, screen.Height)
        {
            Font = new Font8x12(),
            Text = "Touch Cal Point",
            TextColor = TextColor,
            HorizontalAlignment = Meadow.Foundation.Graphics.HorizontalAlignment.Center,
            VerticalAlignment = Meadow.Foundation.Graphics.VerticalAlignment.Center
        };

        _calibrationPoints = new Crosshair[]
        {
            new Crosshair(margin, margin) { ForeColor = CrosshairColor },
            new Crosshair(_screen.Width - margin, _screen.Height - margin) { ForeColor = CrosshairColor }
        };
        _calPoints = new CalibrationPoint[_calibrationPoints.Length];
    }

    public IEnumerable<CalibrationPoint>? GetSavedCalibrationData()
    {
        if (!_calibrationDataFile.Exists) { return null; }
        using var file = _calibrationDataFile.OpenText();
        var json = file.ReadToEnd();
        var data = MicroJson.Deserialize<CalibrationPoint[]>(json);
        return data;
    }

    public void EraseCalibrationData()
    {
        if (_calibrationDataFile.Exists) { _calibrationDataFile.Delete(); }
    }

    public void SaveCalibrationData(IEnumerable<CalibrationPoint> data)
    {
        if (_calibrationDataFile.Exists) { _calibrationDataFile.Delete(); }
        var json = MicroJson.Serialize(data);
        using var file = _calibrationDataFile.CreateText();
        file.Write(json);
    }

    public Task Calibrate(bool saveCalibrationData = true)
    {
        _touchscreen.TouchUp += OnTouchUp;

        _screen.Controls.Clear();
        _screen.BackgroundColor = ScreenColor;
        _screen.Controls.Add(_instruction);
        _screen.Controls.Add(_calibrationPoints[_currentPoint]);
        _screen.Invalidate();

        return Task.Run(async () =>
        {
            while (_currentPoint < _calibrationPoints.Length)
            {
                await Task.Delay(500);
            }
            _touchscreen.TouchUp -= OnTouchUp;
            Resolver.Log.Info($"calibration done");
            _touchscreen.SetCalibrationData(_calPoints);
            _screen.Controls.Clear();
            if (saveCalibrationData)
            {
                _instruction.Text = "Saving Calibration Data...";
                SaveCalibrationData(_calPoints);
            }
            CalibrationComplete?.Invoke(this, _calPoints);
        });
    }

    private void OnTouchUp(ITouchScreen sender, TouchPoint point)
    {
        var now = Environment.TickCount;
        if (now - _lastTouch < 1000)
        {
            // ignore multiples (i.e. debounce)
            return;
        }
        _lastTouch = now;

        _calPoints[_currentPoint] = new CalibrationPoint(
            point.RawX,
            _calibrationPoints[_currentPoint].Left,
            point.RawY,
            _calibrationPoints[_currentPoint].Top);

        Resolver.Log.Info($"point {_currentPoint} captured ({point.RawX},{point.RawY})");

        _currentPoint++;

        if (_currentPoint < _calibrationPoints.Length)
        {
            Resolver.Log.Info($"Getting point {_currentPoint}");
            _screen.Controls.Clear();
            _screen.Controls.Add(_instruction);
            _screen.Controls.Add(_calibrationPoints[_currentPoint]);
        }
    }
}
