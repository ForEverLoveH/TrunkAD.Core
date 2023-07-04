using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TrunkAD.Core.GameSystem.GameWindow
{
    public partial class PasswordWindow : Form
    {
        public PasswordWindow()
        {
            InitializeComponent();
        }
        public string acc = "";
        public string pass = "";

        private void uiButton1_Click(object sender, EventArgs e)
        {
             acc = uiTextBox1.Text.Trim();
             pass = uiTextBox2.Text.Trim();
            string  repass = uiTextBox3.Text.Trim();
            if(!string.IsNullOrEmpty(acc) && !string.IsNullOrEmpty(pass)&& !string.IsNullOrEmpty(repass)&&repass==pass ) 
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                UIMessageBox.ShowWarning("请先输入账号信息！！");
                return;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
