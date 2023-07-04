using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TrunkAD.Core.GameSystem.GameHelper;

namespace TrunkAD.Core.GameSystem.GameWindow
{
    public partial class PointFormWindow : Form
    {
        public PointFormWindow()
        {
            InitializeComponent();
        }


        //GetActiveWindow返回线程的活动窗口，而不是系统的活动窗口。如果要得到用户正在激活的窗口，应该使用 GetForegroundWindow
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll ")]
        //设置窗体置顶
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        //窗体句柄
        private IntPtr handle = IntPtr.Zero;
        public IntPtr Handle { get => handle; set => handle = value; }

        #region   窗体在最前
        [DllImport("user32")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        #endregion


        public int columSum = 5;
        PictureBox[] pics1;
        PictureBox[] pics2;
        public int Directionmode = 0;
        private void PointFormWindow_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            //initPic();
        }

        public void initPic()
        {
            int width =pf1 .Width / columSum - 10;
            int height = pf1.Height - 10;
            pf1.SuspendLayout();
            pf2.SuspendLayout();
            pf1.Controls.Clear();
            pf2.Controls.Clear();

            if (pics1 == null || pics2 == null || pics1.Length != columSum || pics2.Length != columSum)
            {
                pics1 = new PictureBox[columSum];
                pics2 = new PictureBox[columSum];
                for (int i = 0; i < columSum; i++)
                {
                    pics1[i] = new PictureBox();
                    pics1[i].Size = new System.Drawing.Size(width, height);
                    pics1[i].BackColor = Color.Green;
                    pics2[i] = new PictureBox();
                    pics2[i].Size = new System.Drawing.Size(width, height);
                    pics2[i].BackColor = Color.Green;
                }
            }
            else
            {
                for (int i = 0; i < columSum; i++)
                {
                    pics1[i].BackColor = Color.Green;
                    pics2[i].BackColor = Color.Green;
                }
            }
            pf1.Controls.AddRange(pics1);
            pf2.Controls.AddRange(pics2);
            pf1.ResumeLayout();
            pf2.ResumeLayout();
        }


        public void updateFlp(int index)
        {
            initPic();
            pf1.SuspendLayout();
            pf2.SuspendLayout();
            pf1.Controls.Clear();
            pf2.Controls.Clear();
            if (index % 2 == 0)
            {
                //下标点
                int downIndex = (index - 2) / 2;
                if (Directionmode == 1) downIndex = columSum - 1 - downIndex;
                pics2[downIndex].BackColor = Color.Red;
            }
            else
            {
                //上标点
                int downIndex = (index - 1) / 2;
                if (Directionmode == 1) downIndex = columSum - 1 - downIndex;
                pics1[downIndex].BackColor = Color.Red;
            }
            pf1.Controls.AddRange(pics1);
            pf2.Controls.AddRange(pics2);
            pf1.ResumeLayout();
            pf2.ResumeLayout();
        }
    }
}