using Potato.Fastboot;
using Potato.ImageFlasher;
using PotatoNV_next.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PotatoNV_next
{
    class Core
    {
        public delegate void RunWorkerCompletedHandler();
        public event RunWorkerCompletedHandler RunWorkerCompleted;

        private Fastboot fb;
        private Controls.NVForm.FormEventArgs args;

        private void LogResponse(Fastboot.Response response)
        {
            Log.Debug($"response: {Encoding.UTF8.GetString(response.RawData)}");
        }

        private void FlashBootloader(Bootloader bootloader, string port)
        {
            var flasher = new ImageFlasher();

            Log.Info("Verifying images...");

            int asize = 0, dsize = 0;

            foreach (var image in bootloader.Images)
            {   
                if (!image.IsValid)
                {
                    throw new Exception($"Image `{image.Role}` is not valid!");
                }

                asize += image.Size;
            }

            Log.Debug($"Opening {port}...");

            flasher.Open(port);

            Log.Info($"Uploading {bootloader.Name}...");

            foreach (var image in bootloader.Images)
            {
                var size = image.Size;

                Log.Info($"- {image.Role}");

                flasher.Write(image.Path, (int)image.Address, x => {
                    Log.SetProgressBar(dsize + (int)(size / 100f * x), asize);
                });

                dsize += size;
            }

            flasher.Close();

            Log.SetProgressBar(false);
        }

        private void ReadInfo()
        {
            var serial = fb.GetSerialNumber();
            Log.Info($"Serial number: {serial}");

            var bsn = fb.Command("oem read_bsn");
            if (bsn.Status == Fastboot.Status.Okay)
            {
                Log.Info($"Board ID: {bsn.Payload}");
            }

            var model = fb.Command("oem get-product-model");
            Log.Info($"Model: {model.Payload}");

            var build = fb.Command("oem get-build-number");
            Log.Info($"Build number: {build.Payload.Replace(":", "")}");

            var regex = new Regex(@"FB[\w: ]{1,}UNLOCKED");
            var fblock = fb.Command("oem lock-state info");
            var state = regex.IsMatch(fblock.Payload);
            
            Log.Info($"FBLOCK state: {(state ? "unlocked" : "locked")}");
            LogResponse(fblock);

            if (!state)
            {
                Log.Error("FBLOCK is locked!");
                // throw new Exception("FBLOCK is locked!");
            }
        }

        private void SetNVMEProp(string prop, byte[] value)
        {
            Log.Info($"Writing {prop}...");

            var cmd = new List<byte>();

            cmd.AddRange(Encoding.ASCII.GetBytes($"getvar:nve:{prop}@"));
            cmd.AddRange(value);

            var res = fb.Command(cmd.ToArray());

            LogResponse(res);

            if (!res.Payload.Contains("set nv ok"))
            {
                throw new Exception($"Failed to set: {res.Payload}");
            }
        }

        public static byte[] GetSHA256(string str)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.ASCII.GetBytes(str));
            }
        }

        private void SetHWDogCertify(byte state)
        {
            foreach (var command in new[] { "hwdog certify set", "backdoor set" })
            {
                Log.Info($"Trying {command}...");
                var res = fb.Command($"oem {command} {state}");
                LogResponse(res);
                if (res.Status == Fastboot.Status.Okay || res.Payload.Contains("equal"))
                {
                    Log.Success($"{command}: success");
                    return;
                }
            }
            Log.Error("Failed to set FBLOCK state!");
        }

        private void WidevineLock()
        {
            Log.Debug("WV Lock");
            var res = fb.Command("getvar:nve:WVLOCK");
            LogResponse(res);
            if (res.Status != Fastboot.Status.Fail && res.Payload != "UUUUUUUUUUUUUUUU")
            {
                Log.Info($"Read factory key: {res.Payload}");
            }
            else
            {
                Log.Info("Writing code unconditionally...");
            }

            try
            {
                SetNVMEProp("WVLOCK", Encoding.ASCII.GetBytes(args.UnlockCode));
            }
            catch
            {
                Log.Error("Failed to set the WVLOCK.");
            }
        }

        private void WriteNVME()
        {
            var fblockState = (byte)(args.DisableFBLOCK ? 0 : 1);

            try
            {
                SetNVMEProp("FBLOCK", new[] { fblockState });
            }
            catch (Exception ex)
            {
                Log.Error("Failed to set the FBLOCK, using the alternative method...");
                Log.Debug(ex.Message);
                SetHWDogCertify(fblockState);
            }

            try
            {
                SetNVMEProp("USRKEY", GetSHA256(args.UnlockCode));
            }
            catch (Exception ex)
            {
                Log.Error("Failed to set the USRKEY, using the alternative method...");
                Log.Debug(ex.Message);
                WidevineLock();
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            args = e.Argument as Controls.NVForm.FormEventArgs;
            fb = new Fastboot();

            try
            {
                if (args.TargetMode == UsbController.Device.DMode.DownloadVCOM)
                {
                    FlashBootloader(args.Bootloader, args.Target.Split(':')[0]);

                    Log.Info("Waiting for any device...");
                    fb.Wait();
                }

                Log.Info("Connecting...");

                fb.Connect();
                ReadInfo();
                WriteNVME();

                Log.Info("Rebooting...");

                fb.Command("reboot");

                Log.Info($"New bootloader unlock code: {args.UnlockCode}");

                fb.Disconnect();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Debug(ex.StackTrace);
            }
            finally
            {
                fb.Disconnect();
            }
        }

        public void StartProcess(Controls.NVForm.FormEventArgs args)
        {
            var worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += (sender, e) => RunWorkerCompleted();
            worker.RunWorkerAsync(args);
        }
    }
}
