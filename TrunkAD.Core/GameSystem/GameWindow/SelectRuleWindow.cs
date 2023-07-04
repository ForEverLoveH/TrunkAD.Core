using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sunny.UI;

namespace TrunkAD.Core.GameSystem.GameWindow
{
    public partial class SelectRuleWindow : Form
    {
        public SelectRuleWindow()
        {
            InitializeComponent();
        }

        private void SelectRuleWindow_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = true;
            LoadingInitData();
        }

        public int colum = 0;
        public int initDis = 0;
        public int distance = 0;
        public bool isMirror = false;
        /// <summary>
        /// 
        /// </summary>
        private void LoadingInitData()
        {
            //uiComboBox1.Items.Clear();
            uiComboBox2.Items.Clear();
            uiComboBox3.Items.Clear();
            
            for (int i = 0; i <= 1000; i++) 
            {
                if(i%10==0)
                {
                    uiComboBox2.Items.Add(i.ToString());
                }
                if(i%5==0)
                {
                    uiComboBox3.Items.Add(i.ToString());
                }
            }
            uiComboBox2.SelectedIndex = 0;
            uiComboBox3.SelectedIndex = 0;
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            if (uiComboBox1.Text.Trim() == "" || uiComboBox2.Text.Trim() == "" || uiComboBox3.Text.Trim() == "")
            {
                UIMessageBox.ShowWarning("请检测参数选择");
                return;
            }
            else
            {
                int.TryParse(uiComboBox1.Text, out colum);
                int.TryParse(uiComboBox2.Text, out initDis);
                int.TryParse(uiComboBox3.Text, out distance);
                string sl = uiComboBox4.Text;
                if (sl == "从左往右")
                    isMirror = true;
                else
                {
                    isMirror = false;
                }
                
                DialogResult = DialogResult.OK;
            }
        }

        private void uiButton2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
