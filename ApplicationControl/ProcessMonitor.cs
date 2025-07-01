using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
            var seenPids = new HashSet<int>();

            while (isRunning)
            {
                try
                {
                    var processes = Process.GetProcesses();

                    foreach (var process in processes)
                    {
                        if (seenPids.Contains(process.Id)) continue;

                        string exeName = process.ProcessName.ToLowerInvariant() + ".exe";

                        // 🔹 FAST BLOCK: check against blacklist exe names
                        if (config.Blacklist.Any(app =>
                            !string.IsNullOrEmpty(app.ExeName) && app.ExeName.Equals(exeName, StringComparison.OrdinalIgnoreCase)))
                        {
                            try
                            {
                                process.Kill();
                                seenPids.Add(process.Id);

                                // Show notification popup asynchronously (optional)
                                Application.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    new BlockPopup(exeName).Show(); // non-blocking
                                });
                            }
                            catch { /* Ignore access denied/system processes */ }

                            continue;
                        }

                        // 🔹 Skip if whitelisted by exe name
                        if (config.Whitelist.Any(app =>
                            !string.IsNullOrEmpty(app.ExeName) && app.ExeName.Equals(exeName, StringComparison.OrdinalIgnoreCase)))
                            continue;

                        // 🔹 Fallback: check product/vendor/hash
                        try
                        {
                            string path = process.MainModule?.FileName;
                            if (string.IsNullOrEmpty(path) || !File.Exists(path)) continue;

                            var info = FileVersionInfo.GetVersionInfo(path);
                            string product = info.ProductName ?? "";
                            string vendor = info.CompanyName ?? "";
                            string hash = ComputeFileHash(path);

                            bool isWhitelisted = config.Whitelist.Any(app =>
                                (!string.IsNullOrEmpty(app.ProductName) && app.ProductName == product) ||
                                (!string.IsNullOrEmpty(app.VendorName) && app.VendorName == vendor) ||
                                (!string.IsNullOrEmpty(app.FileHash) && app.FileHash == hash));

                            if (isWhitelisted) continue;

                            bool isBlacklisted = config.Blacklist.Any(app =>
                                (!string.IsNullOrEmpty(app.ProductName) && app.ProductName == product) ||
                                (!string.IsNullOrEmpty(app.VendorName) && app.VendorName == vendor) ||
                                (!string.IsNullOrEmpty(app.FileHash) && app.FileHash == hash));

                            if (isBlacklisted)
                            {
                                process.Kill();
                                seenPids.Add(process.Id);

                                Application.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    new BlockPopup($"{product} ({exeName})").Show(); // non-blocking
                                });
                            }
                        }
                        catch { /* Access denied or system process — ignore */ }
                    }
                }
                catch { /* general try-catch for monitor loop */ }

                Thread.Sleep(100); // ✅ Lower to 100ms or even 50ms
            }
        }






        private string ComputeFileHash(string path)
        {
            try
            {
                using var sha256 = SHA256.Create();
                using var stream = File.OpenRead(path);
                var hashBytes = sha256.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
