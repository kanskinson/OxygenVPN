using System;
using System.Drawing;
using System.Text;
using OxygenVPN.Models;
using OxygenVPN.Utils;

namespace OxygenVPN.Forms
{
    public partial class Dummy
    {
    }

    partial class MainForm
    {
        private bool IsWaiting => State == State.Waiting || State == State.Stopped;

        private State _state = State.Waiting;

        /// <summary>
        ///     当前状态
        /// </summary>
        public State State {
            get => _state;
            private set {
                void StartDisableItems(bool enabled) {
                    ServerComboBox.Enabled =
                        ModeComboBox.Enabled =
                            EditModePictureBox.Enabled =
                                EditServerPictureBox.Enabled =
                                    DeleteModePictureBox.Enabled =
                                        DeleteServerPictureBox.Enabled = enabled;

                    // 启动需要禁用的控件
                    UninstallServiceToolStripMenuItem.Enabled =
                        updateACLWithProxyToolStripMenuItem.Enabled =
                            UpdateServersFromSubscribeLinksToolStripMenuItem.Enabled =
                                reinstallTapDriverToolStripMenuItem.Enabled =
                                    ReloadModesToolStripMenuItem.Enabled = enabled;
                }

                _state = value;

                StatusText(i18N.Translate(StateExtension.GetStatusString(value)));
                switch (value) {
                case State.Waiting:
                    ControlButton.Enabled = true;
                    ControlButton.Text = i18N.Translate("Start");

                    break;
                case State.Starting:
                    ControlButton.Enabled = false;
                    ControlButton.Text = "...";

                    comboBoxProfiles.Enabled = false;
                    tableLayoutPanelProfileOptions.Enabled = false;
                    StartDisableItems(false);
                    break;
                case State.Started:
                    ControlButton.Enabled = true;
                    ControlButton.Text = i18N.Translate("Stop");

                    StatusTextAppend(StatusPortInfoText.Value);

                    comboBoxProfiles.Enabled = false;
                    tableLayoutPanelProfileOptions.Enabled = false;

                    break;
                case State.Stopping:
                    ControlButton.Enabled = false;
                    ControlButton.Text = "...";

                    comboBoxProfiles.Enabled = false;
                    tableLayoutPanelProfileOptions.Enabled = false;
                    labelUsed.Text = "--";
                    labelSpeed.Text = "--";

                    NatTypeStatusText();
                    break;
                case State.Stopped:
                    ControlButton.Enabled = true;
                    ControlButton.Text = i18N.Translate("Start");

                    LastUploadBandwidth = 0;
                    LastDownloadBandwidth = 0;
                    Bandwidth.Stop();

                    comboBoxProfiles.Enabled = true;
                    tableLayoutPanelProfileOptions.Enabled = true;
                    labelUsed.Text = "--";
                    labelSpeed.Text = "--";
                    StartDisableItems(true);
                    break;
                case State.Terminating:
                    Dispose();
                    Environment.Exit(Environment.ExitCode);
                    return;
                }
            }
        }

        public void NatTypeStatusText(string text = "", string country = "")
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string, string>(NatTypeStatusText), text, country);
                return;
            }

            if (State != State.Started)
            {
                labelNat.Text = "--";
                labelNatLight.Visible = false;
                return;
            }

            if (!string.IsNullOrEmpty(text))
            {
                labelNat.Text = $"{text} {(country != string.Empty ? $"[{country}]" : "")}"; ;
                
                UpdateNatTypeLight(int.TryParse(text, out var natType) ? natType : -1);
            }
            else
            {
                labelNat.Text= $@"NAT{i18N.Translate(": ", "Test failed")}";
            }

        }

        /// <summary>
        ///     更新 NAT指示灯颜色
        /// </summary>
        /// <param name="natType"></param>
        private void UpdateNatTypeLight(int natType = -1)
        {
            if (natType > 0 && natType < 5)
            {
                labelNatLight.Visible = Global.Flags.IsWindows10Upper;
                Color c;
                switch (natType)
                {
                    case 1:
                        c = Color.LimeGreen;
                        break;
                    case 2:
                        c = Color.Yellow;
                        break;
                    case 3:
                        c = Color.Red;
                        break;
                    case 4:
                        c = Color.Black;
                        break;
                    default:
                        c = Color.Black;
                        break;
                }

                labelNatLight.ForeColor = c;
            }
            else
            {
                labelNatLight.Visible = false;
            }
        }

        /// <summary>
        ///     更新状态栏文本
        /// </summary>
        /// <param name="text"></param>
        public void StatusText(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(StatusText), text);
                return;
            }

            StatusLabel.Text = i18N.Translate("Status", ": ") + text;
        }

        public void StatusTextAppend(string text)
        {
            StatusLabel.Text += text;
        }

        public static class StatusPortInfoText
        {
            public static int Socks5Port = 0;
            public static int HttpPort = 0;
            public static bool ShareLan = false;

            public static string Value
            {
                get
                {
                    if (Socks5Port == 0 && HttpPort == 0)
                        return string.Empty;

                    var text = new StringBuilder();
                    if (ShareLan)
                        text.Append(i18N.Translate("Allow other Devices to connect") + " ");

                    if (Socks5Port != 0)
                        text.Append($"Socks5: {Socks5Port}");

                    if (HttpPort != 0)
                    {
                        if (Socks5Port != 0)
                            text.Append(" | ");
                        text.Append($"HTTP: {HttpPort}");
                    }

                    return $" ({text})";
                }
            }
        }
    }
}