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
    public partial class FrmCreateNewProject : HZH_Controls.Forms.FrmWithOKCancel1
    {
        public FrmCreateNewProject()
        {
            InitializeComponent();
        }
        public string ProjectName = "";
        private void FrmCreateNewProject_Load(object sender, EventArgs e)
        {
            this.Title = "创建项目";
        }

        private void uiTextBox1_TextChanged(object sender, EventArgs e)
        {
            ProjectName = uiTextBox1.Text;
        }
    }
}
