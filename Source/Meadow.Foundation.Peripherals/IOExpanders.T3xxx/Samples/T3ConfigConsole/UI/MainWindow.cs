using Meadow.Modbus;
using Meadow.Foundation.IOExpanders;
using System.Data;
using System.Net;
using T3ConfigConsole.Devices;
using Terminal.Gui;

namespace T3ConfigConsole.UI;

public class MainWindow : Window
{
    private readonly ModbusClientBase _client;
    private readonly DeviceInfo _info;
    private readonly T38i8o6doConsoleClient _deviceClient;
    private readonly DataTable _table;
    private readonly TableView _tableView;
    private readonly Button _refreshButton;
    private readonly Label _statusLabel;
    private List<InputPoint> _inputPoints = new();
    private List<OutputPoint> _outputPoints = new();

    public MainWindow(ModbusClientBase client, DeviceInfo info, byte modbusAddress)
        : base($"T3 Config — {info.Summary}")
    {
        _client = client;
        _info = info;
        _deviceClient = new T38i8o6doConsoleClient(client, modbusAddress);

        X = 0; Y = 0;
        Width = Dim.Fill(); Height = Dim.Fill();

        // ── Toolbar ──────────────────────────────────────────────────────────
        _refreshButton = new Button("[ Refresh ]") { X = 1, Y = 0 };
        _refreshButton.Clicked += OnRefreshClicked;

        var disconnectBtn = new Button("[ Disconnect ]") { X = 16, Y = 0 };
        disconnectBtn.Clicked += () => { _client.Disconnect(); Application.RequestStop(); };

        var configBtn = new Button("[ Config ]") { X = 34, Y = 0 };
        configBtn.Clicked += OnConfigClicked;

        Add(_refreshButton, disconnectBtn, configBtn);

        // ── IO Table ─────────────────────────────────────────────────────────
        _table = new DataTable();
        _table.Columns.Add("#", typeof(string));
        _table.Columns.Add("Type", typeof(string));
        _table.Columns.Add("Mode", typeof(string));
        _table.Columns.Add("Range", typeof(string));
        _table.Columns.Add("Value", typeof(string));
        _table.Columns.Add("Status", typeof(string));

        _tableView = new TableView
        {
            X = 0,
            Y = 2,
            Width = Dim.Fill(),
            Height = Dim.Fill() - 2
        };
        _tableView.Table = _table;
        _tableView.CellActivated += OnCellActivated;
        Add(_tableView);

        // ── Status bar ───────────────────────────────────────────────────────
        _statusLabel = new Label("Press Refresh to load data  |  Enter on a row for details")
        {
            X = 1,
            Y = Pos.AnchorEnd(1)
        };
        Add(_statusLabel);
    }

    private void OnConfigClicked()
    {
        var dialog = new Dialog("Device Configuration", 60, 15);

        var ipLabel = new Label("IP Address:") { X = 2, Y = 1 };
        var ipField = new TextField(_info.IpAddress.ToString()) { X = 20, Y = 1, Width = 20 };

        var baudLabel = new Label("Baud Rate:") { X = 2, Y = 2 };
        var baudOptions = new string[] { "9600", "19200", "38400", "57600", "115200" };
        var baudList = new ComboBox() { X = 20, Y = 2, Width = 20, Height = 6, Source = new ListWrapper(baudOptions) };
        
        var addrLabel = new Label("Modbus Addr:") { X = 2, Y = 3 };
        var addrField = new TextField(_info.ModbusAddress.ToString()) { X = 20, Y = 3, Width = 5 };

        dialog.Add(ipLabel, ipField, baudLabel, baudList, addrLabel, addrField);

        // Fetch current baud rate to set initial selection
        Task.Run(async () =>
        {
            try
            {
                var currentBaud = await _deviceClient.ReadBaudRateAsync();
                Application.MainLoop.Invoke(() =>
                {
                    baudList.Text = currentBaud.ToString();
                });
            }
            catch { }
        });

        var saveBtn = new Button("Apply Changes", is_default: true);
        saveBtn.Clicked += () =>
        {
            if (!IPAddress.TryParse(ipField.Text?.ToString(), out var newIp))
            {
                MessageBox.ErrorQuery("Error", "Invalid IP Address", "OK");
                return;
            }

            if (!byte.TryParse(addrField.Text?.ToString(), out var newAddr))
            {
                MessageBox.ErrorQuery("Error", "Invalid Modbus Address", "OK");
                return;
            }

            if (!int.TryParse(baudList.Text?.ToString(), out int newBaud))
            {
                newBaud = 9600;
            }

            _statusLabel.Text = "Applying configuration...";
            Task.Run(async () =>
            {
                try
                {
                    // 1. IP Address (requires broadcast)
                    IPAddress localIp = IPAddress.Any;
                    IPAddress mask = IPAddress.Parse("255.255.255.0");
                    IPAddress gateway = IPAddress.Parse("0.0.0.0");

                    foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                    {
                        if (ni.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up) continue;
                        var props = ni.GetIPProperties();
                        foreach (var addr in props.UnicastAddresses)
                        {
                            if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !addr.Address.ToString().StartsWith("169.254."))
                            {
                                localIp = addr.Address;
                                mask = addr.IPv4Mask;
                                if (props.GatewayAddresses.Count > 0) gateway = props.GatewayAddresses[0].Address;
                                goto found;
                            }
                        }
                    }
                    found:

                    // Apply IP change via broadcast
                    await T3xxx.ChangeIpAddress(localIp, _info.IpAddress, newIp, mask, gateway, _info.SerialNumber);

                    // 2. Baud Rate (Modbus write)
                    await _deviceClient.WriteBaudRateAsync(newBaud);

                    // 3. Modbus Address (Modbus write)
                    await _deviceClient.WriteModbusAddressAsync(newAddr);

                    Application.MainLoop.Invoke(() =>
                    {
                        MessageBox.Query("Success", "Configuration applied. Note: IP change may take a few seconds to take effect.", "OK");
                        Application.RequestStop(); 
                    });
                }
                catch (Exception ex)
                {
                    Application.MainLoop.Invoke(() => MessageBox.ErrorQuery("Error", ex.Message, "OK"));
                }
            });
        };

        var cancelBtn = new Button("Cancel");
        cancelBtn.Clicked += () => Application.RequestStop();

        dialog.AddButton(saveBtn);
        dialog.AddButton(cancelBtn);

        Application.Run(dialog);
    }

    private void OnRefreshClicked()
    {
        _refreshButton.Enabled = false;
        _statusLabel.Text = "Reading device...";

        Task.Run(async () =>
        {
            try
            {
                var inputs = await _deviceClient.ReadAllInputsAsync(_info.ProductModel);
                var outputs = await _deviceClient.ReadAllOutputsAsync();

                Application.MainLoop.Invoke(() =>
                {
                    _inputPoints = inputs;
                    _outputPoints = outputs;
                    RebuildTable();
                    _statusLabel.Text = $"Updated {DateTime.Now:HH:mm:ss}  |  Enter on a row for details";
                    _refreshButton.Enabled = true;
                });
            }
            catch (Exception ex)
            {
                Application.MainLoop.Invoke(() =>
                {
                    _statusLabel.Text = $"Read error: {ex.Message}";
                    _refreshButton.Enabled = true;
                });
            }
        });
    }

    private void RebuildTable()
    {
        _table.Rows.Clear();
        foreach (var p in _inputPoints)
        {
            _table.Rows.Add(
                $"AI{p.Index + 1}",
                p.TypeLabel,
                p.ModeLabel,
                p.RangeLabel,
                p.ValueLabel,
                $"0x{p.RawStatus:X4}");
        }
        foreach (var p in _outputPoints)
        {
            string label = p.Index >= 100 ? $"DO{p.Index - 100 + 1}" : $"AO{p.Index + 1}";
            _table.Rows.Add(
                label,
                p.TypeLabel,
                p.ModeLabel,
                p.RangeLabel,
                p.ValueLabel,
                $"0x{p.RawStatus:X4}");
        }
        _tableView.SetNeedsDisplay();
    }

    private void OnCellActivated(TableView.CellActivatedEventArgs args)
    {
        if (args.Row < 0) return;

        if (args.Row < _inputPoints.Count)
        {
            var p = _inputPoints[args.Row];
            if (args.Col == 0) // Name column
            {
                MessageBox.Query("Input Registers",
                    $"AI{p.Index + 1} Modbus Registers:\n\n" +
                    $"Value:  {p.ValueRegister}\n" +
                    $"Type:   {p.TypeRegister}\n" +
                    $"Mode:   {p.ModeRegister}\n" +
                    $"Range:  {p.RangeRegister}\n" +
                    $"Status: {p.StatusRegister}",
                    "Close");
            }
            else if (args.Col == 1) // Type column
            {
                int selection = MessageBox.Query("Change Input Type",
                    $"Change AI{p.Index + 1} type to:",
                    "Digital", "Analog", "Cancel");

                if (selection == 0 || selection == 1)
                {
                    bool setAnalog = selection == 1;
                    _statusLabel.Text = $"Setting AI{p.Index + 1} to {(setAnalog ? "Analog" : "Digital")}...";
                    Task.Run(async () =>
                    {
                        try
                        {
                            await _deviceClient.WriteInputTypeAsync(p.Index, setAnalog);
                            Application.MainLoop.Invoke(() => { OnRefreshClicked(); });
                        }
                        catch (Exception ex)
                        {
                            Application.MainLoop.Invoke(() => { MessageBox.ErrorQuery("Error", ex.Message, "OK"); });
                        }
                    });
                }
            }
            else if (args.Col == 2) // Mode column
            {
                int selection = MessageBox.Query("Change Input Mode",
                    $"Change AI{p.Index + 1} mode to:",
                    "Auto", "Manual", "Cancel");

                if (selection == 0 || selection == 1)
                {
                    bool setManual = selection == 1;
                    _statusLabel.Text = $"Setting AI{p.Index + 1} to {(setManual ? "Manual" : "Auto")}...";
                    Task.Run(async () =>
                    {
                        try
                        {
                            await _deviceClient.WriteInputModeAsync(p.Index, setManual);
                            Application.MainLoop.Invoke(() => { OnRefreshClicked(); });
                        }
                        catch (Exception ex)
                        {
                            Application.MainLoop.Invoke(() => { MessageBox.ErrorQuery("Error", ex.Message, "OK"); });
                        }
                    });
                }
            }
            else if (args.Col == 3 && p.IsAnalog) // Range column
            {
                var dialog = new Dialog("Select Analog Range", 40, 20);
                var list = new ListView(InputPoint.RangeOptions)
                {
                    X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() - 1,
                    SelectedItem = p.RangeCode < InputPoint.RangeOptions.Length ? (int)p.RangeCode : 0
                };

                dialog.Add(list);
                var ok = new Button("OK", is_default: true);
                ok.Clicked += () => { dialog.Data = list.SelectedItem; Application.RequestStop(); };
                var cancel = new Button("Cancel");
                cancel.Clicked += () => { dialog.Data = -1; Application.RequestStop(); };
                dialog.AddButton(ok);
                dialog.AddButton(cancel);

                Application.Run(dialog);

                if (dialog.Data is int selection && selection >= 0)
                {
                    _statusLabel.Text = $"Setting AI{p.Index + 1} range to {InputPoint.RangeOptions[selection]}...";
                    Task.Run(async () =>
                    {
                        try
                        {
                            await _deviceClient.WriteInputRangeAsync(p.Index, (ushort)selection);
                            // Re-assert analog type after range change
                            await _deviceClient.WriteInputTypeAsync(p.Index, true);
                            
                            Application.MainLoop.Invoke(() => { OnRefreshClicked(); });
                        }
                        catch (Exception ex)
                        {
                            Application.MainLoop.Invoke(() => { MessageBox.ErrorQuery("Error", ex.Message, "OK"); });
                        }
                    });
                }
            }
            else if (args.Col == 4) // Value column
            {
                _statusLabel.Text = $"Reading AI{p.Index + 1} value...";
                Task.Run(async () =>
                {
                    try
                    {
                        var newVal = await _deviceClient.ReadInputValueAsync(p.Index);
                        Application.MainLoop.Invoke(() =>
                        {
                            p.RawValue = newVal;
                            RebuildTable();
                            _statusLabel.Text = $"Updated AI{p.Index + 1} at {DateTime.Now:HH:mm:ss}";
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.MainLoop.Invoke(() => { MessageBox.ErrorQuery("Error", ex.Message, "OK"); });
                    }
                });
            }
            else
            {
                _statusLabel.Text = $"Input {p.Index + 1}: {p.ValueLabel}";
            }
        }
        else
        {
            var outputIdx = args.Row - _inputPoints.Count;
            if (outputIdx < _outputPoints.Count)
            {
                var p = _outputPoints[outputIdx];
                if (args.Col == 4 && !p.IsAnalog) // Value column and Digital
                {
                    ushort newValue = (ushort)(p.RawValue > 0 ? 0 : 1);
                    string label = p.Index >= 100 ? $"DO{p.Index - 100 + 1}" : $"AO{p.Index + 1}";
                    _statusLabel.Text = $"Toggling {label} to {(newValue > 0 ? "ON" : "OFF")}...";
                    
                    Task.Run(async () =>
                    {
                        try
                        {
                            await _deviceClient.WriteOutputValueAsync(p.Index, newValue);
                            Application.MainLoop.Invoke(() => { OnRefreshClicked(); });
                        }
                        catch (Exception ex)
                        {
                            Application.MainLoop.Invoke(() => { MessageBox.ErrorQuery("Error", ex.Message, "OK"); });
                        }
                    });
                }
                else
                {
                    Application.Run(new IoDetailDialog(p, _deviceClient));
                }
            }
        }
    }
}
