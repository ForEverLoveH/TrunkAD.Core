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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TrunkAD.Core.GameSystem.GameWindow
{
    public partial class SelectPunctuationRuleWindow : Form
    {
        public SelectPunctuationRuleWindow()
        {
            InitializeComponent();
        }
        //测试方向
        public int Directionmode = 0;
        public string type = string.Empty;
        private void SelectPunctuationRuleWindow_Load(object sender, EventArgs e)
        {
            uiComboBox2.Items.Clear();
            for (int i = 0; i < 100; i += 10)
            {
                uiComboBox2.Items.Add((i) + "");
            }
            for (int i = 100; i <= 1000; i += 100)
            {

                uiComboBox2.Items.Add(i + "");
            }
            uiComboBox3.Items.Clear();
            for (int i = 5; i <= 100; i += 5)
            {
                uiComboBox3.Items.Add(i + "");
            }
            for (int i = 200; i <= 500; i += 50)
            {
                uiComboBox3.Items.Add(i + "");
            }
            uiComboBox4.SelectedIndex = 0;

            if (!string.IsNullOrEmpty(type))
            {
                switch (type)
                {
                    case "坐位体前屈":
                        uiComboBox2.Text = "-20";
                        uiComboBox3.Text = "30";
                        break;
                    case "立定跳远":
                        break;
                    default:
                        break;
                }
            }

        }

        private void uiComboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (uiComboBox4.SelectedIndex < 0) return;
            Directionmode = uiComboBox4.SelectedIndex;
        }
        public int colum = 0;
        public int initDis = 0;
        public int distance = 0;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                DialogResult = DialogResult.OK;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton2_Click(object sender, EventArgs e)
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
                DialogResult = DialogResult.Yes;
            }
        }
    }
}
