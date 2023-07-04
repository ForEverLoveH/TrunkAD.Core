using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TrunkAD.Core.GameSystem.GameHelper;
using TrunkAD.Core.GameSystem.GameHelper.TreeViewHelper;
using TrunkAD.Core.GameSystem.GameWindowSys;

namespace TrunkAD.Core
{
    public class GameRoot
    {
        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        public static extern void SetForegroundWindow(IntPtr mainWindowHandle);
        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        static MainWindowSys MainWindowSys = new MainWindowSys();
        static TreeViewHelper TreeViewHelper = new TreeViewHelper();
        static ImportStudentDataWindowSys ImportStudentDataWindowSys = new ImportStudentDataWindowSys();
        static ImportDataWindowSys ImportDataWindowSys = new ImportDataWindowSys();
        static EquipMentSettingWindowSys EquipMentSettingWindowSys = new EquipMentSettingWindowSys ();
        static RunningTestingWindowSys RunningTestingWindowSys = new RunningTestingWindowSys ();
        private static SpeekHelper SpeekHelper = new SpeekHelper();
        private static GradeManager GradeManager = new GradeManager();
        private static PointHelper PointHelper = new PointHelper();
       // private static OpencvHelper OpencvHelper = new OpencvHelper();

        public void StartGame()
        {
            Awake();
        }

        private void Awake()
        {
            MainWindowSys.Awake();
            TreeViewHelper.Awake();
            ImportStudentDataWindowSys.Awake();
            ImportDataWindowSys.Awake();
            EquipMentSettingWindowSys.Awake();
            RunningTestingWindowSys.Awake ();
            SpeekHelper.Awake();    
            GradeManager .Awake();
            PointHelper.Awake();
             
        }
    }
}
