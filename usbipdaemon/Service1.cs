﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Threading;

namespace usbipdaemon
{
    public partial class Service1 : ServiceBase
    {
        bool NotStopped = true;
        Thread _thread;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _thread = new Thread(Start);
            _thread.Start();
        }

        public void Start()
        {
            var ips = ConfigurationManager.AppSettings["IP"];
            while (NotStopped)
            {
                var devices = new List<string>();
                foreach (var ip in ips.Split(' '))
                {
                    var listProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "usbip.exe",
                            Arguments = "list -r " + ip,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };
                    listProcess.Start();
                    while (!listProcess.StandardOutput.EndOfStream)
                    {
                        string line = listProcess.StandardOutput.ReadLine();
                        if (line.StartsWith("      ") && !line.StartsWith("       "))
                        {
                            var dev = line.Substring(6, line.IndexOf(':') - 6);
                            devices.Add(dev);
                        }
                    }
                    foreach (var dev in devices)
                    { 
                        var deviceProcess = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "usbip.exe",
                                Arguments = "attach -r " + ip + " -b " + dev,
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                CreateNoWindow = true
                            }
                        };
                        deviceProcess.Start();
                    }
                }
                Thread.Sleep(5 * 1000);
            }
        }

        protected override void OnStop()
        {
            NotStopped = false;
            Thread.Sleep(10 * 1000);
            if (_thread != null && _thread.IsAlive)
                _thread.Abort();
        }
    }
}
