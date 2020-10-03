using OxygenVPN.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OxygenVPN.Utils {
    public partial class LoggingForm : Form {
        public LoggingForm() {
            InitializeComponent();
        }
        bool closing = false;

        public bool CanLog { get { return !this.IsDisposed && !closing; } }

        private void LoggingForm_Load(object sender, EventArgs e) {
            Logging.Info("Show log window");
            InitText();
        }


        private void copyToolStripMenuItem_Click(object sender, EventArgs e) {
            Clipboard.SetDataObject(textBoxLogs.SelectedText);
        }

        private void openLogFileToolStripMenuItem_Click(object sender, EventArgs e) {
            Utils.Open(Logging.LogFile);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            this.Close();
            this.Dispose();
        }

        private void fontToolStripMenuItem_Click(object sender, EventArgs e) {
            FontDialog fontDialog = new FontDialog();
            fontDialog.Font = textBoxLogs.Font;
            fontDialog.Color = textBoxLogs.ForeColor;
            fontDialog.ShowColor = true;
            if (fontDialog.ShowDialog() == DialogResult.OK) {
                this.textBoxLogs.Font = fontDialog.Font;
                textBoxLogs.ForeColor = fontDialog.Color;
            }
        }

        private void backColorToolStripMenuItem_Click(object sender, EventArgs e) {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = textBoxLogs.BackColor;
            if (colorDialog.ShowDialog() == DialogResult.OK) {
                textBoxLogs.BackColor = colorDialog.Color;
            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e) {
            SetText(string.Empty);
        }

        private void InitText() {
            i18N.TranslateForm(this);
        }

        public void SetText(string text) {
            textBoxLogs.Text = text;
            FormatText();
        }

        public void AppendText(string text) {
            textBoxLogs.AppendText(text);
            FormatText();
            textBoxLogs.ScrollToCaret();
        }

        private void FormatText() {

        }

    }
}
