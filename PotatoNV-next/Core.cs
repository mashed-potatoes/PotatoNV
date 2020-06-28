using Potato.Fastboot;
using Potato.ImageFlasher;
using PotatoNV_next.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PotatoNV_next
{
    class Core
    {
        private static void FlashBootloader(Bootloader bootloader, string port)
        {
            var flasher = new ImageFlasher();

            Log.Info("Verifying images...");

            foreach (var image in bootloader.Images)
            {
                Log.Debug($"VrStat of {image.Role}: {image.IsValid}");
                
                if (!image.IsValid)
                {
                    throw new Exception($"Image `{image.Role}` is invalid!");
                }
            }

            Log.Success("Verification passed!");

            Log.Debug($"Opening {port}...");

            flasher.Open(port);

            Log.Info($"Uploading {bootloader.Name} bootloader");

            foreach (var image in bootloader.Images)
            {
                Log.Info($" - {image.Role}");
                flasher.Write(image.Path, (int)image.Address);
            }

            flasher.Close();

            Log.Success("Bootloader uploaded");
        }

        public static void StartProcess(Controls.NVForm.FormEventArgs args)
        {
            var worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerAsync(args);
        }

        private static void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var args = e.Argument as Controls.NVForm.FormEventArgs;
            var fb = new Fastboot();

            if (args.TargetMode == UsbController.Device.DMode.DownloadVCOM)
            {
                Log.Info("--> Flashing bootloader");
                FlashBootloader(args.Bootloader, args.Target.Split(':')[0]);

                Log.Info("Waiting for any device...");
                fb.Wait();
            }

            Log.Info("Connecting to fastboot device...");
            fb.Connect();
            fb.Command("reboot");
            fb.Disconnect();
        }
    }
}
