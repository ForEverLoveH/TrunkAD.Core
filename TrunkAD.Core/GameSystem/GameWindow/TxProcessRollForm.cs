using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TrunkAD.Core.GameSystem.GameWindow
{
    public partial class TxProcessRollForm : Form
    {
        public TxProcessRollForm()
        {
            InitializeComponent();
        }
        public bool isLoad = false;
        private void TxProcessRollForm_Load(object sender, EventArgs e)
        {
            isLoad = true;
            timer1.Start();
        }
        private int proval = 0;

        private void timer1_Tick(object sender, EventArgs e)
        {
            proval += 10;
            if (proval >= 500) proval = 0;
            uchScrollbar1.Value = proval;
        }

        private void TxProcessRollForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (timer1.Enabled) timer1.Stop();
        }

        private void TxProcessRollForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (timer1.Enabled) timer1.Stop();
        }
    }
}
