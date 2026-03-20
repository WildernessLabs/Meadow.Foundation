using Meadow.Foundation.IOExpanders;
using Meadow.Hardware;
using Meadow.Modbus;
using System.Net;
using System.Text.Json;
using T3ConfigConsole.Devices;
using Terminal.Gui;

namespace T3ConfigConsole.UI;

public class ConnectionWindow : Window
{
    private const string SettingsFile = "connection_settings.json";

    private class ConnectionSettings
    {
        public string LastIp { get; set; } = "192.168.1.100";
        public byte LastModbusAddress { get; set; } = 254;
    }

    private ConnectionSettings LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                return JsonSerializer.Deserialize<ConnectionSettings>(json) ?? new ConnectionSettings();
            }
        }
        catch { }
        return new ConnectionSettings();
    }

    private void SaveSettings(string ip, byte address)
    {
        try
        {
            var settings = new ConnectionSettings { LastIp = ip, LastModbusAddress = address };
            var json = JsonSerializer.Serialize(settings);
            File.WriteAllText(SettingsFile, json);
        }
        catch { }
    }

    private readonly RadioGroup _transportSelector;
    private readonly FrameView _tcpFrame;
    private readonly FrameView _serialFrame;
    private readonly TextField _ipField;
    private readonly TextField _portField;
    private readonly ListView _comPortList;
    private readonly TextField _baudRateField;
    private readonly RadioGroup _paritySelector;
    private readonly RadioGroup _stopBitsSelector;
    private readonly TextField _unitIdField;
    private readonly Button _connectButton;
    private readonly Label _statusLabel;
    private readonly string[] _comPorts;

    public ModbusClientBase? ConnectedClient { get; private set; }
    public DeviceInfo? DeviceInfo { get; private set; }
    public byte ModbusAddress { get; private set; } = 254;

    public ConnectionWindow() : base("T3 Config Tool — Connect")
    {
        X = 0; Y = 0;
        Width = Dim.Fill(); Height = Dim.Fill();

        var settings = LoadSettings();

        // ── Transport selector ───────────────────────────────────────────────
        Add(new Label("Transport:") { X = 2, Y = 1 });
        _transportSelector = new RadioGroup(new NStack.ustring[] { "TCP", "Serial" }) { X = 13, Y = 1 };
        _transportSelector.SelectedItemChanged += (args) =>
        {
            _tcpFrame.Visible = args.SelectedItem == 0;
            _serialFrame.Visible = args.SelectedItem == 1;
        };
        Add(_transportSelector);

        // ── TCP frame ────────────────────────────────────────────────────────
        _tcpFrame = new FrameView("TCP Settings") { X = 1, Y = 4, Width = 48, Height = 8 };
        _ipField = new TextField(settings.LastIp) { X = 13, Y = 1, Width = 20 };
        _portField = new TextField("502") { X = 13, Y = 2, Width = 8 };
        var discoverBtn = new Button("Discover...") { X = 13, Y = 4 };
        discoverBtn.Clicked += OnDiscoverClicked;

        _tcpFrame.Add(new Label("IP Address:") { X = 1, Y = 1 }, _ipField,
                      new Label("Port:") { X = 1, Y = 2 }, _portField,
                      discoverBtn);
        Add(_tcpFrame);

        // ── Serial frame ─────────────────────────────────────────────────────
        _comPorts = System.IO.Ports.SerialPort.GetPortNames();
        if (_comPorts.Length == 0) _comPorts = ["(none)"];

        _serialFrame = new FrameView("Serial Settings") { X = 1, Y = 4, Width = 48, Height = 13, Visible = false };
        _comPortList = new ListView(_comPorts) { X = 12, Y = 1, Width = 16, Height = 4 };
        _baudRateField = new TextField("9600") { X = 12, Y = 5, Width = 8 };
        _paritySelector = new RadioGroup(new NStack.ustring[] { "None", "Odd", "Even" }) { X = 12, Y = 6 };
        _stopBitsSelector = new RadioGroup(new NStack.ustring[] { "1", "2" }) { X = 12, Y = 9 };
        _serialFrame.Add(
            new Label("COM Port:") { X = 1, Y = 1 }, _comPortList,
            new Label("Baud Rate:") { X = 1, Y = 5 }, _baudRateField,
            new Label("Parity:") { X = 1, Y = 6 }, _paritySelector,
            new Label("Stop Bits:") { X = 1, Y = 9 }, _stopBitsSelector);
        Add(_serialFrame);

        // ── Modbus Address ──────────────────────────────────────────────────
        _unitIdField = new TextField(settings.LastModbusAddress.ToString()) { X = 18, Y = 19, Width = 5 };
        Add(new Label("Modbus Address:") { X = 2, Y = 19 }, _unitIdField);

        // ── Connect button & status ──────────────────────────────────────────
        _connectButton = new Button("Connect") { X = 2, Y = 21 };
        _connectButton.Clicked += OnConnectClicked;
        _statusLabel = new Label("") { X = 2, Y = 23, Width = Dim.Fill() - 4 };
        Add(_connectButton, _statusLabel);
    }

    private void OnDiscoverClicked()
    {
        _statusLabel.Text = "Scanning for devices...";

        Task.Run(async () =>
        {
            try
            {
                var discovered = await T3xxx.DiscoverDevices();

                Application.MainLoop.Invoke(() =>
                {
                    if (discovered.Count == 0)
                    {
                        _statusLabel.Text = "No devices found.";
                        return;
                    }

                    _statusLabel.Text = $"Found {discovered.Count} device(s).";

                    var d = discovered[0]; // For now, just pick the first one
                    if (discovered.Count > 1)
                    {
                        // TODO: show a picker dialog if there's more than one
                    }

                    // Check if the device is on a reachable subnet
                    if (!IsOnLocalSubnet(d.IpAddress, out var localIp, out var subnetMask, out var gateway))
                    {
                        ShowIpChangeDialog(d, localIp, subnetMask, gateway);
                    }
                    else
                    {
                        _ipField.Text = d.IpAddress.ToString();
                        _portField.Text = d.ModbusPort.ToString();
                        _unitIdField.Text = d.ModbusAddress.ToString();
                    }
                });
            }
            catch (Exception ex)
            {
                Application.MainLoop.Invoke(() =>
                {
                    _statusLabel.Text = $"Discovery Error: {ex.Message}";
                });
            }
        });
    }

    private bool IsOnLocalSubnet(IPAddress remoteIp, out IPAddress localIp, out IPAddress subnetMask, out IPAddress gateway)
    {
        localIp = IPAddress.None;
        subnetMask = IPAddress.None;
        gateway = IPAddress.None;

        IPAddress? fallbackIp = null;
        IPAddress? fallbackMask = null;
        IPAddress? fallbackGateway = null;
        int fallbackScore = -1;

        foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up) continue;
            if (ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Loopback) continue;

            var props = ni.GetIPProperties();
            foreach (var addr in props.UnicastAddresses)
            {
                if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    var ipStr = addr.Address.ToString();
                    if (ipStr.StartsWith("169.254.")) continue; // Skip link-local (APIPA)

                    if (IsInSubnet(remoteIp, addr.Address, addr.IPv4Mask))
                    {
                        localIp = addr.Address;
                        subnetMask = addr.IPv4Mask;
                        if (props.GatewayAddresses.Count > 0) gateway = props.GatewayAddresses[0].Address;
                        return true;
                    }

                    // Score for fallback selection: Physical (Ethernet/WiFi) > Virtual/Other
                    int score = (ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet ||
                                 ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211) ? 2 : 1;

                    if (score > fallbackScore)
                    {
                        fallbackIp = addr.Address;
                        fallbackMask = addr.IPv4Mask;
                        fallbackGateway = props.GatewayAddresses.Count > 0 ? props.GatewayAddresses[0].Address : IPAddress.None;
                        fallbackScore = score;
                    }
                }
            }
        }

        if (fallbackIp != null)
        {
            localIp = fallbackIp;
            subnetMask = fallbackMask!;
            gateway = fallbackGateway!;
        }

        return false;
    }

    private bool IsInSubnet(IPAddress address, IPAddress subnetAddress, IPAddress netmask)
    {
        byte[] addressBytes = address.GetAddressBytes();
        byte[] subnetBytes = subnetAddress.GetAddressBytes();
        byte[] maskBytes = netmask.GetAddressBytes();

        for (int i = 0; i < addressBytes.Length; i++)
        {
            if ((addressBytes[i] & maskBytes[i]) != (subnetBytes[i] & maskBytes[i]))
            {
                return false;
            }
        }
        return true;
    }

    private void ShowIpChangeDialog(T3DeviceInfo device, IPAddress localIp, IPAddress mask, IPAddress gateway)
    {
        var dialog = new Dialog("Subnet Mismatch", 60, 18);

        var msg = new Label("This device is configured for a different subnet.\nTo connect to it you must change its address.")
        {
            X = Pos.Center(),
            Y = 1,
            TextAlignment = TextAlignment.Centered
        };
        dialog.Add(msg);

        // Suggested IP: use the local network prefix and a .201 or similar
        var localBytes = localIp.GetAddressBytes();
        localBytes[3] = 201;
        var suggestedIp = new IPAddress(localBytes);

        var ipField = new TextField(suggestedIp.ToString()) { X = 20, Y = 4, Width = 20 };
        var maskField = new TextField(mask.ToString()) { X = 20, Y = 5, Width = 20 };
        var gwField = new TextField(gateway.ToString()) { X = 20, Y = 6, Width = 20 };

        dialog.Add(
            new Label("New IP:") { X = 4, Y = 4 }, ipField,
            new Label("Subnet Mask:") { X = 4, Y = 5 }, maskField,
            new Label("Gateway:") { X = 4, Y = 6 }, gwField
        );

        var btnChange = new Button("Change IP", is_default: true);
        btnChange.Clicked += () =>
        {
            if (IPAddress.TryParse(ipField.Text?.ToString(), out var newIp) &&
                IPAddress.TryParse(maskField.Text?.ToString(), out var newMask) &&
                IPAddress.TryParse(gwField.Text?.ToString(), out var newGw))
            {
                _statusLabel.Text = "Applying IP change...";
                Task.Run(async () =>
                {
                    try
                    {
                        var success = await T3xxx.ChangeIpAddress(localIp, device.IpAddress, newIp, newMask, newGw, device.SerialNumber);
                        Application.MainLoop.Invoke(() =>
                        {
                            if (success)
                            {
                                MessageBox.Query("Success", "IP address changed. You can now connect.", "OK");
                                _ipField.Text = newIp.ToString();
                                _portField.Text = device.ModbusPort.ToString();
                                _unitIdField.Text = device.ModbusAddress.ToString();
                            }
                            else
                            {
                                MessageBox.ErrorQuery("Error", "Failed to change IP address. Ensure your firewall allows UDP port 1234.", "OK");
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.MainLoop.Invoke(() => MessageBox.ErrorQuery("Error", ex.Message, "OK"));
                    }
                });
                Application.RequestStop();
            }
        };

        var btnCancel = new Button("Cancel");
        btnCancel.Clicked += () => Application.RequestStop();

        dialog.AddButton(btnChange);
        dialog.AddButton(btnCancel);

        Application.Run(dialog);
    }

    private void OnConnectClicked()
    {
        if (!byte.TryParse(_unitIdField.Text?.ToString(), out var modbusAddress))
            modbusAddress = 254;

        _connectButton.Enabled = false;
        _statusLabel.Text = "Connecting...";

        Task.Run(async () =>
        {
            try
            {
                ModbusClientBase client;

                string? ip = null;

                if (_transportSelector.SelectedItem == 0)
                {
                    ip = _ipField.Text?.ToString() ?? "127.0.0.1";
                    var port = short.TryParse(_portField.Text?.ToString(), out var p) ? p : (short)502;
                    client = new ModbusTcpClient(ip, port);
                }
                else
                {
                    var portName = _comPorts[_comPortList.SelectedItem];
                    var baud = int.TryParse(_baudRateField.Text?.ToString(), out var b) ? b : 9600;
                    // RadioGroup index maps directly: None=0, Odd=1, Even=2
                    var parity = (Parity)_paritySelector.SelectedItem;
                    // StopBits: index 0 → One (1), index 1 → Two (2)
                    var stopBits = (StopBits)(_stopBitsSelector.SelectedItem + 1);
                    var shim = new SerialPortShim(portName, baud, parity, 8, stopBits);
                    client = new ModbusRtuClient(shim);
                }

                await client.Connect();

                if (!client.IsConnected)
                    throw new Exception("Connect returned but IsConnected is false.");

                if (ip != null)
                {
                    SaveSettings(ip, modbusAddress);
                }

                var deviceClient = new T38i8o6doConsoleClient(client, modbusAddress);
                var info = await deviceClient.ReadDeviceInfoAsync();

                ConnectedClient = client;
                DeviceInfo = info;
                ModbusAddress = modbusAddress;

                Application.MainLoop.Invoke(() =>
                {
                    _statusLabel.Text = $"Connected — {info.Summary}";
                    Application.RequestStop();
                });
            }
            catch (Exception ex)
            {
                Application.MainLoop.Invoke(() =>
                {
                    _statusLabel.Text = $"Error: {ex.Message}";
                    _connectButton.Enabled = true;
                });
            }
        });
    }
}
