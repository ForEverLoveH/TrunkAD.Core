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
    public partial class DetermineGradesWindow : HZH_Controls.Forms.FrmWithOKCancel1
    {
        public DetermineGradesWindow()
        {
            InitializeComponent();
        }
        public double score = -1;
        public double score0 = 0;
        public double checkScore = 0;
        public string dangwei = "米";
        public string ReservedDigitsTxt = "0.000";
        public bool isFoul = false;
        private void DetermineGradesWindow_Load(object sender, EventArgs e)
        {
            this.Title = "写入成绩";
            label2.Text = $"{score.ToString(ReservedDigitsTxt)}" + dangwei;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string stl = textBox1.Text.Replace("厘米", "");
            double.TryParse(stl, out checkScore);
        }
        public void setScore(double tScore)
        {
            score = tScore;
            string v = tScore.ToString(ReservedDigitsTxt);
            double.TryParse(v, out score0);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            isFoul = true;
            base.DialogResult = DialogResult.OK;
        }
    }
}
