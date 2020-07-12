using Microsoft.Win32;
using PotatoNV_next.Utils;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace PotatoNV_next
{
    public partial class MainWindow : Window
    {
        #region Win32 interop handle
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool InsertMenu(IntPtr hMenu, int wPosition, int wFlags, int wIDNewItem, string lpNewItem);

        public const int WM_SYSCOMMAND = 0x112;
        public const int MF_SEPARATOR = 0x800;
        public const int MF_BYPOSITION = 0x400;
        public const int MF_STRING = 0x0;

        public const int TB_SAVE_LOGS = 1000;

        public IntPtr Handle
        {
            get
            {
                return new WindowInteropHelper(this).Handle;
            }
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SYSCOMMAND)
            {
                switch (wParam.ToInt32())
                {
                    case TB_SAVE_LOGS:
                        SaveLogs();
                        handled = true;
                        break;
                }
            }

            return IntPtr.Zero;
        }
        #endregion

        readonly Core core = new Core();

        public MainWindow()
        {
            Icon = MediaConverter.ImageSourceFromBitmap(Properties.Resources.Fire.ToBitmap());
            InitializeComponent();

            nvFrom.OnFormSubmit += NvFrom_OnFormSubmit;
            core.RunWorkerCompleted += Core_RunWorkerCompleted;
            Loaded += MainWindow_Loaded;

            SetupLog();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            IntPtr systemMenuHandle = GetSystemMenu(Handle, false);

            InsertMenu(systemMenuHandle, 5, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty);
            InsertMenu(systemMenuHandle, 6, MF_BYPOSITION, TB_SAVE_LOGS, "Save log to file");

            HwndSource source = HwndSource.FromHwnd(Handle);
            source.AddHook(new HwndSourceHook(WndProc));
        }

        private static void SaveLogs()
        {
            var dialog = new SaveFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                CheckPathExists = true,
                DefaultExt = "log",
                Filter = "Log file (*.log)|*.log"
            };

            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, Log.GetLog());
            }
        }

        private void NvFrom_OnFormSubmit(Controls.NVForm.FormEventArgs formEventArgs)
        {
            SetupLog();
            core.StartProcess(formEventArgs);
        }

        public void SetupLog()
        {
            logBox.Clear();
            Log.Info($"PotatoNV v{Controls.AboutTab.GetVersion()}");
            Log.Info("User manual: https://kutt.it/pnv-"
                + (CultureInfo.InstalledUICulture.TwoLetterISOLanguageName == "ru"
                    ? "ru"
                    : "en"));
        }

        private void Core_RunWorkerCompleted()
        {
            nvFrom.IsEnabled = true;
        }
    }
}
