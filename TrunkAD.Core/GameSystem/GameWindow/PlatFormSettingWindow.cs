using HZH_Controls.Forms;
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
using TrunkAD.Core.GameSystem.GameHelper;
using TrunkAD.Core.GameSystem.GameWindowSys;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TrunkAD.Core.GameSystem.GameWindow
{
    public partial class PlatFormSettingWindow : Form
    {
        public PlatFormSettingWindow()
        {
            InitializeComponent();
        }
        public List<Dictionary<string, string>> localInfos = new List<Dictionary<string, string>>();
        public Dictionary<string, string> localValues = new Dictionary<string, string>();
        public SQLiteHelper helper;
        private void PlatFormSettingWindow_Load(object sender, EventArgs e)
        {
            EquipMentSettingWindowSys.Instance.LoadingInitData(ref localInfos, ref localValues,helper, comboBox1,comboBox2, combox3,comboBox4);
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton1_Click(object sender, EventArgs e)
        {
            bool s = EquipMentSettingWindowSys.Instance.GetExamNumber( combox3,comboBox2,localValues);
            if (s)
            {
                UIMessageBox.ShowSuccess("获取成功！！");
            }
            else
            {
                UIMessageBox.ShowError("获取失败！！");
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

            bool s = EquipMentSettingWindowSys.Instance.LoadingEquipMentCodeData(comboBox1,comboBox2, combox3,   localValues);
            if (s)
            {
                UIMessageBox.ShowSuccess("获取成功！！");
            }
            else
            {
                UIMessageBox.ShowError("获取失败！！");
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton3_Click(object sender, EventArgs e)
        {
            string Platform = comboBox2.Text;
            string ExamId = combox3.Text;
            string MachineCode = comboBox1.Text;
            int UploadUnit = comboBox4.SelectedIndex;
            System.Data.SQLite.SQLiteTransaction sQLiteTransaction = helper.BeginTransaction();
            helper.ExecuteNonQuery($"UPDATE localInfos SET value = '{Platform}' WHERE key = 'Platform'");
            helper.ExecuteNonQuery($"UPDATE localInfos SET value = '{ExamId}' WHERE key = 'ExamId'");
            helper.ExecuteNonQuery($"UPDATE localInfos SET value = '{MachineCode}' WHERE key = 'MachineCode'");
            helper.ExecuteNonQuery($"UPDATE localInfos SET value = '{UploadUnit}' WHERE key = 'UploadUnit'");
            helper.CommitTransaction(ref sQLiteTransaction);
            FrmTips.ShowTipsSuccess(this, "保存成功");
            DialogResult= DialogResult.OK;
            this.Close();
        }

        private void uiuiButton23_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close  ();
        }
    }
}
