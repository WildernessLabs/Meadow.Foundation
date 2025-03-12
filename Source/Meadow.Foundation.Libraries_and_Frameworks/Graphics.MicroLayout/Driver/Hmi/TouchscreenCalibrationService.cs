using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;
using Meadow.Foundation.Serialization;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Meadow.Foundation.Hmi;

/// <summary>
/// Service for calibrating a touchscreen.
/// </summary>
public class TouchscreenCalibrationService
{
    /// <summary>
    /// Event that occurs when the calibration is complete.
    /// </summary>
    public event EventHandler<CalibrationPoint[]>? CalibrationComplete;

    private readonly DisplayScreen _screen;
    private readonly Crosshair[] _calibrationPoints;
    private readonly ICalibratableTouchscreen _touchscreen;
    private readonly FileInfo _calibrationDataFile;

    private int _currentPoint = 0;
    private readonly CalibrationPoint[] _calPoints;
    private int _lastTouch = Environment.TickCount;
    private readonly Label _instruction;

    /// <summary>
    /// The current screen color
    /// </summary>
    public static Color ScreenColor = Color.White;
    /// <summary>
    /// The current crosshair color
    /// </summary>
    public static Color CrosshairColor = Color.Black;
    /// <summary>
    /// The current text color
    /// </summary>
    public static Color TextColor = Color.DarkBlue;

    /// <summary>
    /// Initializes a new instance of the <see cref="TouchscreenCalibrationService"/> class.
    /// </summary>
    /// <param name="screen">The display screen to be calibrated.</param>
    /// <param name="calibrationDataFile">The file to save calibration data.</param>
    /// <exception cref="ArgumentException">Thrown when the touchscreen is not calibratable or is null.</exception>
    public TouchscreenCalibrationService(DisplayScreen screen, FileInfo calibrationDataFile)
    {
        if (screen.TouchScreen == null)
        {
            throw new ArgumentException("DisplayScreen.TouchScreen must not be null");
        }

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
            new (margin, margin) { ForeColor = CrosshairColor },
            new (_screen.Width - margin, _screen.Height - margin) { ForeColor = CrosshairColor }
        };
        _calPoints = new CalibrationPoint[_calibrationPoints.Length];
    }

    /// <summary>
    /// Gets the saved calibration data.
    /// </summary>
    /// <returns>A collection of <see cref="CalibrationPoint"/> if data exists; otherwise, null.</returns>
    public IEnumerable<CalibrationPoint>? GetSavedCalibrationData()
    {
        if (!_calibrationDataFile.Exists) { return null; }
        using var file = _calibrationDataFile.OpenText();
        var json = file.ReadToEnd();
        var data = MicroJson.Deserialize<CalibrationPoint[]>(json);
        return data;
    }

    /// <summary>
    /// Erases the saved calibration data.
    /// </summary>
    public void EraseCalibrationData()
    {
        if (_calibrationDataFile.Exists) { _calibrationDataFile.Delete(); }
    }

    /// <summary>
    /// Saves the calibration data.
    /// </summary>
    /// <param name="data">The calibration data to save.</param>
    public void SaveCalibrationData(IEnumerable<CalibrationPoint> data)
    {
        if (_calibrationDataFile.Exists) { _calibrationDataFile.Delete(); }
        var json = MicroJson.Serialize(data);
        using var file = _calibrationDataFile.CreateText();
        file.Write(json);
    }

    /// <summary>
    /// Starts the calibration process.
    /// </summary>
    /// <param name="saveCalibrationData">Whether to save the calibration data after calibration.</param>
    /// <returns>A task representing the calibration process.</returns>
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
                Resolver.Log.Info($"Saving Calibration Data...");
                SaveCalibrationData(_calPoints);
                Resolver.Log.Info($"Saved");
            }
            CalibrationComplete?.Invoke(this, _calPoints);
        });
    }

    /// <summary>
    /// Handles the touch up event during calibration.
    /// </summary>
    /// <param name="sender">The touch screen sender.</param>
    /// <param name="point">The touch point data.</param>
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
