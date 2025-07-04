using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ApplicationControl
{
    public class ProcessMonitor
    {
        private readonly AppConfig config;
        private bool isRunning;
        private readonly Thread monitorThread;

        // Cache to reduce repeated hash computations
        private readonly ConcurrentDictionary<string, string> hashCache = new();
        private readonly HashSet<int> seenProcessIds = new();

        public ProcessMonitor(AppConfig config)
        {
            this.config = config;
            monitorThread = new Thread(MonitorProcesses)
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest
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
                try
                {
                    var processes = Process.GetProcesses();

                    Parallel.ForEach(processes, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, process =>
                    {
                        try
                        {
                            if (!isRunning || seenProcessIds.Contains(process.Id))
                                return;

                            string exeName = process.ProcessName.ToLowerInvariant() + ".exe";
                            string path = process.MainModule?.FileName;

                            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                                return;

                            var info = FileVersionInfo.GetVersionInfo(path);
                            string product = info.ProductName ?? "";
                            string vendor = info.CompanyName ?? "";

                            // Fastest: check ExeName
                            bool match = config.Blacklist.Any(app =>
                                (!string.IsNullOrWhiteSpace(app.ExeName) && app.ExeName.Equals(exeName, StringComparison.OrdinalIgnoreCase)) ||
                                (!string.IsNullOrWhiteSpace(app.ProductName) && app.ProductName.Equals(product, StringComparison.OrdinalIgnoreCase)) ||
                                (!string.IsNullOrWhiteSpace(app.VendorName) && app.VendorName.Equals(vendor, StringComparison.OrdinalIgnoreCase)) ||
                                (!string.IsNullOrWhiteSpace(app.FileHash) && GetCachedHash(path) == app.FileHash.ToLowerInvariant()));

                            if (!match) return;

                            // If not in whitelist
                            bool whitelisted = config.Whitelist.Any(app =>
                                (!string.IsNullOrWhiteSpace(app.ExeName) && app.ExeName.Equals(exeName, StringComparison.OrdinalIgnoreCase)) ||
                                (!string.IsNullOrWhiteSpace(app.ProductName) && app.ProductName.Equals(product, StringComparison.OrdinalIgnoreCase)) ||
                                (!string.IsNullOrWhiteSpace(app.VendorName) && app.VendorName.Equals(vendor, StringComparison.OrdinalIgnoreCase)) ||
                                (!string.IsNullOrWhiteSpace(app.FileHash) && GetCachedHash(path) == app.FileHash.ToLowerInvariant()));

                            if (whitelisted)
                                return;

                            // Kill immediately
                            process.Kill();
                            seenProcessIds.Add(process.Id);

                            // Optional: Show popup asynchronously
                            Application.Current.Dispatcher.BeginInvoke(() =>
                            {
                                new BlockPopup($"{product}").Show();
                            });
                        }
                        catch
                        {
                            // Ignore access denied or disposed processes
                        }
                    });
                }
                catch
                {
                    // Ignore global monitor exceptions
                }

                Thread.Sleep(50); // fast cycle
            }
        }

        private string GetCachedHash(string path)
        {
            return hashCache.GetOrAdd(path, p =>
            {
                try
                {
                    using var sha256 = SHA256.Create();
                    using var stream = File.OpenRead(p);
                    return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
                }
                catch
                {
                    return string.Empty;
                }
            });
        }
    }
}
