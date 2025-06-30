using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;

namespace ApplicationControl
{
    public class ProcessMonitor
    {
        private readonly AppConfig config;
        private readonly Thread monitorThread;
        private bool isRunning;

        public ProcessMonitor(AppConfig config)
        {
            this.config = config;
            monitorThread = new Thread(MonitorProcesses)
            {
                IsBackground = true
            };
        }

        public void Start()
        {
            isRunning = true;
            monitorThread.Start();
        }

        public void Stop()
        {
            isRunning = false;
        }

        private void MonitorProcesses()
        {
            while (isRunning)
            {
                var processes = Process.GetProcesses();

                foreach (var process in processes)
                {
                    try
                    {
                        string processName = process.ProcessName.ToLower() + ".exe";

                        if (config.Blacklist.Contains(processName) && !config.Whitelist.Contains(processName))
                        {
                            process.Kill();

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                var popup = new BlockPopup(processName);
                                popup.ShowDialog();
                            });
                        }
                    }
                    catch
                    {
                        // Handle any process access exceptions silently
                    }
                }

                Thread.Sleep(2000); // check every 2 seconds
            }
        }
    }
}
