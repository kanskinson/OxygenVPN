﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using OxygenVPN.Controllers;
using OxygenVPN.Forms;
using OxygenVPN.Models;

namespace OxygenVPN {
    public static class Global {
        /// <summary>
        ///     换行
        /// </summary>
        public const string EOF = "\r\n";

        public static readonly string OxygenVPNDir = Application.StartupPath;

        /// <summary>
        ///     主窗体的静态实例
        /// </summary>
        public static MainForm MainForm;

        public static class Flags {
            static Flags() {
                Task.Run(() => {
                    SupportFakeDns = new TUNTAPController().TestFakeDNS();
                    IsWindows10Upper = Environment.OSVersion.Version.Major >= 10;
                });
            }

            public static bool SupportFakeDns;
            public static bool IsWindows10Upper;
        }

        /// <summary>
        ///		出口适配器
        /// </summary>
        public static class Outbound {
            /// <summary>
            ///		索引
            /// </summary>
            public static int Index = -1;

            /// <summary>
            ///		地址
            /// </summary>
            public static IPAddress Address => Adapter.GetIPProperties().UnicastAddresses.First(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork).Address;

            /// <summary>
            ///		网关
            /// </summary>
            public static IPAddress Gateway;

            public static NetworkInterface Adapter;
        }

        /// <summary>
        ///		TUN/TAP 适配器
        /// </summary>
        public static class TUNTAP {
            /// <summary>
            ///		适配器
            /// </summary>
            public static NetworkInterface Adapter;

            /// <summary>
            ///		索引
            /// </summary>
            public static int Index = -1;

            /// <summary>
            ///		组件 ID
            /// </summary>
            public static string ComponentID = string.Empty;
        }

        /// <summary>
        ///     用于读取和写入的配置
        /// </summary>
        public static Setting Settings = new Setting();

        /// <summary>
        ///     用于存储模式
        /// </summary>
        public static readonly List<Mode> Modes = new List<Mode>();
    }
}