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
    public partial class FixGradeWindow : Form
    {
        public FixGradeWindow()
        {
            InitializeComponent();
        }
        public string projectName { set; get; }
        public string groupName { set; get; }
        public string Name { set; get; }
        public string IdNumber { set; get; }
        public string status { set; get; }
        public int rountid { set; get; }
        private void FixGradeWindow_Load(object sender, EventArgs e)
        {
            this.Text = "修改成绩";
            ProjectNameInput.Text = projectName;
            GroupText.Text = groupName;
            NameText.Text = Name;
            uiComboBox1.Items.Clear();
            for (int i = 0; i < rountid; i++)
            {
                uiComboBox1.Items.Add($"第{i + 1} 轮");
            }
            if (uiComboBox2.Items.Contains(status))
            {
                uiComboBox2.SelectedIndex = uiComboBox2.Items.IndexOf(status);
            }
        }
        public int updaterountId = 0;

        private void uiComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (uiComboBox1.SelectedIndex != -1)
            {
                updaterountId = uiComboBox1.SelectedIndex + 1;
            }
        }
        public double updateScore = 0;

        private void uiTextBox4_TextChanged(object sender, EventArgs e)
        {
            double.TryParse(uiTextBox4.Text, out updateScore);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            status = uiComboBox2.Text.ToString();
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void uiButton2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
