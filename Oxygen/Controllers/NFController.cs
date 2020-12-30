using System;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading.Tasks;
using OxygenVPN.Models;
using OxygenVPN.Utils;
using nfapinet;

namespace OxygenVPN.Controllers {
    public class NFController : ModeController {
        public override bool TestNatRequired { get; } = true;

        private static readonly ServiceController NFService = new ServiceController("netfilter2");

        private static readonly string BinDriver = string.Empty;
        private static readonly string SystemDriver = $"{Environment.SystemDirectory}\\drivers\\netfilter2.sys";
        private static string _sysDns;

        static NFController() {
            switch ($"{Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.Minor}") {
            case "10.0":
                BinDriver = "Win-10.sys";
                break;
            case "6.3":
            case "6.2":
                BinDriver = "Win-8.sys";
                break;
            case "6.1":
            case "6.0":
                BinDriver = "Win-7.sys";
                break;
            default:
                Logging.Error($"Unsupported OS: {Environment.OSVersion.Version}");
                return;
            }

            BinDriver = "bin\\" + BinDriver;
        }

        public NFController() {
            Name = "Redirector";
        }

        public override bool Start(Server s, Mode mode) {
            Logging.Info("Driver version: " + Utils.Utils.FileVersion(BinDriver));
            if (Utils.Utils.FileVersion(SystemDriver) != Utils.Utils.FileVersion(BinDriver)) {
                if (File.Exists(SystemDriver)) {
                    Logging.Info("Driver version: " + Utils.Utils.FileVersion(SystemDriver));
                    Logging.Info("Update drivers");
                    UninstallDriver();
                }

                if (!InstallDriver())
                    return false;
            }

            aio_dial((int)NameList.TYPE_CLRNAME, "");
            foreach (var rule in mode.Rule) {
                aio_dial((int)NameList.TYPE_ADDNAME, rule);
            }

            aio_dial((int)NameList.TYPE_ADDNAME, "NTT.exe");

            if (s.IsSocks5()) {
                var result = DNS.Lookup(s.Hostname);
                if (result == null) {
                    Logging.Info("The server IP address could not be resolved");
                    return false;
                }

                aio_dial((int)NameList.TYPE_TCPHOST, $"{result}:{s.Port}");
                aio_dial((int)NameList.TYPE_UDPHOST, $"{result}:{s.Port}");
            } else {
                aio_dial((int)NameList.TYPE_TCPHOST, $"127.0.0.1:{Global.Settings.Socks5LocalPort}");
                aio_dial((int)NameList.TYPE_UDPHOST, $"127.0.0.1:{Global.Settings.Socks5LocalPort}");
            }

            if (Global.Settings.ModifySystemDNS) {
                // 备份并替换系统 DNS
                _sysDns = DNS.OutboundDNS;
                if (string.IsNullOrWhiteSpace(Global.Settings.ModifiedDNS)) {
                    Global.Settings.ModifiedDNS = "1.1.1.1,8.8.8.8";
                }
                DNS.OutboundDNS = Global.Settings.ModifiedDNS;
            }

            return aio_init();
        }

        public override void Stop() {
            Task.Run(() => {
                if (Global.Settings.ModifySystemDNS)
                    //恢复系统DNS
                    DNS.OutboundDNS = _sysDns;
            });

            aio_free();
        }

        #region NativeMethods

        [DllImport("Redirector.bin", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool aio_dial(int name, [MarshalAs(UnmanagedType.LPWStr)] string value);

        [DllImport("Redirector.bin", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool aio_init();

        [DllImport("Redirector.bin", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool aio_free();

        [DllImport("Redirector.bin", CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong aio_getUP();

        [DllImport("Redirector.bin", CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong aio_getDL();

        #endregion

        #region Utils

        /// <summary>
        ///     安装 NF 驱动
        /// </summary>
        /// <returns>驱动是否安装成功</returns>
        public static bool InstallDriver() {
            Logging.Info("Install NF driver");
            try {
                File.Copy(BinDriver, SystemDriver);
            } catch (Exception e) {
                Logging.Error("Drive copy failed\n" + e);
                return false;
            }

            Global.MainForm.StatusText(i18N.Translate("Register driver"));
            // 注册驱动文件
            var result = NFAPI.nf_registerDriver("netfilter2");
            if (result == NF_STATUS.NF_STATUS_SUCCESS) {
                Logging.Info("Driver installed successfully");
            } else {
                Logging.Error($"Failed to register driver, return: {result}");
                return false;
            }

            return true;
        }

        /// <summary>
        ///     卸载 NF 驱动
        /// </summary>
        /// <returns>是否成功卸载</returns>
        public static bool UninstallDriver() {
            Global.MainForm.StatusText(i18N.Translate("Uninstalling NF Service"));
            Logging.Info("卸载 NF 驱动");
            try {
                if (NFService.Status == ServiceControllerStatus.Running) {
                    NFService.Stop();
                    NFService.WaitForStatus(ServiceControllerStatus.Stopped);
                }
            } catch (Exception) {
                // ignored
            }

            if (!File.Exists(SystemDriver)) return true;
            NFAPI.nf_unRegisterDriver("netfilter2");
            File.Delete(SystemDriver);

            return true;
        }

        #endregion
    }
}