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
    public partial class InputPassword : Form
    {
        public InputPassword()
        {
            InitializeComponent();
        }
        public string passWord = "";
        private void InputPassword_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            passWord = textBox1.Text;
            this.DialogResult = DialogResult.OK;
        }
    }
}
