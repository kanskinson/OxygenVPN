using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using OxygenVPN.Models;
using OxygenVPN.Utils;

namespace OxygenVPN.Forms {
    public partial class Dummy {
    }

    partial class MainForm {
        /// init at <see cref="MainForm_Load"/> 
        private int _sizeHeight;

        private int _profileConfigurationHeight;
        private int _configurationGroupBoxHeight;

        private void InitProfile() {
            // Clear
            comboBoxProfiles.Items.Clear();

            var numProfile = Global.Settings.ProfileCount;
            if (numProfile == 0) {
                // Hide Profile GroupBox, Change window size
                configLayoutPanel.RowStyles[2].SizeType = SizeType.Percent;
                configLayoutPanel.RowStyles[2].Height = 0;

                ConfigurationGroupBox.Size = new Size(ConfigurationGroupBox.Size.Width, _configurationGroupBoxHeight - _profileConfigurationHeight);
                Size = new Size(Size.Width, _sizeHeight - (_profileConfigurationHeight));
            } else {

                while (Global.Settings.Profiles.Count < numProfile) {
                    Global.Settings.Profiles.Add(new Profile());
                }

                for (var i = 0; i < numProfile; ++i) {
                    string profileText = $"[{i+1}] " + (!Global.Settings.Profiles[i].IsDummy ? Global.Settings.Profiles[i].ProfileName : i18N.Translate("None"));
                    comboBoxProfiles.Items.Add(profileText);
                }
                if (numProfile > 0) {
                    comboBoxProfiles.SelectedIndex = 0;
                }

                if (Size.Height == _sizeHeight) return;
                configLayoutPanel.RowStyles[2].SizeType = SizeType.AutoSize;
                ConfigurationGroupBox.Size = new Size(ConfigurationGroupBox.Size.Width, _configurationGroupBoxHeight);
                Size = new Size(Size.Width, _sizeHeight);
            }
        }

        private void LoadProfile(int index) {
            var p = Global.Settings.Profiles[index];
            ProfileNameText.Text = p.ProfileName;

            if (p.IsDummy)
                throw new Exception("Profile not found.");

            var server = ServerComboBox.Items.Cast<Server>().FirstOrDefault(s => s.Remark.Equals(p.ServerRemark));
            var mode = ModeComboBox.Items.Cast<Models.Mode>().FirstOrDefault(m => m.Remark.Equals(p.ModeRemark));

            if (server == null) {
                throw new Exception("Server not found.");
            }

            if (mode == null) {
                throw new Exception("Mode not found.");
            }

            ServerComboBox.SelectedItem = server;
            ModeComboBox.SelectedItem = mode;
        }

        private void SaveProfile(int index) {
            var selectedServer = (Server)ServerComboBox.SelectedItem;
            var selectedMode = (Models.Mode)ModeComboBox.SelectedItem;
            var name = ProfileNameText.Text;

            Global.Settings.Profiles[index] = new Profile(selectedServer, selectedMode, name);
        }

        private void RemoveProfile(int index) {
            Global.Settings.Profiles[index] = new Profile();
        }
    }
}