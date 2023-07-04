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
    public partial class showChooseRoundDialog : Form
    {
        public showChooseRoundDialog()
        {
            InitializeComponent();
        }
        public int round = 3;
        public int selectRound = 0;

        private void showChooseRoundDialog_Load(object sender, EventArgs e)
        {
            for (int i = 1; i <= round; i++)
            {
                comboBox1.Items.Add($"第{i}轮");
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectIndex = comboBox1.SelectedIndex;
            if (selectIndex != -1)
            {
                selectRound = selectIndex + 1;
            }
            else
            {
                selectRound = 0;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (selectRound == 0)
            {
                MessageBox.Show("请选择轮次");
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
        }
    }
}
