using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace PotatoNV_next.Utils
{
    class IntegrityCheck
    {
        protected static string[] Checks = new string[] {
            "libusb-1.0.dll",
            "LibUsbDotNet.LibUsbDotNet.dll",
            "Potato.Fastboot.dll",
            "Potato.ImageFlasher.dll",
            "bootloaders/"
        };

        public static void Run()
        {
            var issues = new List<string>();

            foreach (var f in Checks)
            {
                var isDir = f.EndsWith("/");
                var path = Path.Combine(Environment.CurrentDirectory, f);

                if (isDir ? !Directory.Exists(path) : !File.Exists(path))
                {
                    issues.Add($"{f}: not found");
                    continue;
                }
            }

            if (issues.Count > 0)
            {
                MessageBox.Show(string.Join("\n", issues), "Integrity check failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }
    }
}
