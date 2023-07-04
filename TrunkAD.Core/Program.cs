using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrunkAD.Core.GameSystem.GameHelper;
using TrunkAD.Core.GameSystem.GameWindow;

namespace TrunkAD.Core
{
    internal static class Program
    {
        public const int WM_SYSCOMMAND = 0x112;
        public const int SC_RESTORE = 0xF120;
        public static MeasureDLL measureDll;
        public static bool Verify()
        {
            string t1 = Application.StartupPath + $"\\{MeasureDLL.getCode1()}";
            string t2 = Application.StartupPath + $"\\data\\{MeasureDLL.getCode2()}";
            string v1 = File.ReadAllText(t1, Encoding.UTF8);
            string v2 = File.ReadAllText(t2, Encoding.UTF8);
            measureDll = new MeasureDLL(v1, v2);
            return measureDll.IsUse;
        }
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!Verify())
            {
                MessageBox.Show("激活失败");
                return;
            }
            GameRoot GameRoot = new GameRoot();
            Process cur = Process.GetCurrentProcess();
            foreach (Process p in Process.GetProcesses())
            {
                if (p.Id == cur.Id) continue;
                if (p.ProcessName == cur.ProcessName)
                {
                    GameRoot.SetForegroundWindow(p.MainWindowHandle);
                    GameRoot.SendMessage(p.MainWindowHandle, WM_SYSCOMMAND, SC_RESTORE, 0);
                    return;
                }
            }
            GameRoot.StartGame();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());

            
        }
    }
}
