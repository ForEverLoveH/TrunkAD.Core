using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TrunkAD.Core.GameSystem.GameHelper
{
    public class MemoryTool
    {
        // 获得为该进程(程序)分配的内存. 做一个计时器,就可以时时查看程序占用系统资源
        public static double GetProcessUsedMemory()

        {

            double usedMemory = 0;

            usedMemory = Process.GetCurrentProcess().WorkingSet64 / 1024.0 / 1024.0;

            return usedMemory;

        }

        #region 内存回收
        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        public static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);

        /// <summary>
        /// 释放内存
        /// </summary>
        public static void ClearMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
            }
        }
        #endregion
    }
}
