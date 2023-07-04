using HZH_Controls.Controls;
using HZH_Controls.Forms;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrunkAD.Core.GameSystem.GameHelper;
using TrunkAD.Core.GameSystem.GameWindowSys;

namespace TrunkAD.Core.GameSystem.GameWindow
{
    public partial class ImportStudentWindow : Form
    {
        public ImportStudentWindow()
        {
            InitializeComponent();
        }
        public  SQLiteHelper helper = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportStudentWindow_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            ImportStudentDataWindowSys.Instance.LoadingProjectView(uiTreeView1,helper);
        }
        string projectName = "";
        string groupName = "";
        string projectID = "";
        private void uiTreeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            projectName = "";
            groupName = "";
            if (uiTreeView1.SelectedNode != null)
            {
                string path = uiTreeView1.SelectedNode.FullPath;
                string[] fullPath = path.Split('\\');
                if(e.Node.Level==0)
                {
                    projectName = fullPath[0];
                    ImportStudentDataWindowSys.Instance.LoadingProjectAttribute(helper,projectName,ref projectID,txt_Type,txt_RoundCount,txt_TestMethod,txt_projectName,txt_BestScoreMode,txt_FloatType);   
                }
                else if(e.Node.Level==1)
                {
                    projectName= fullPath[0];
                    groupName = fullPath[1];
                    uiTextBox1.Text = groupName;
                    ImportStudentDataWindowSys.Instance.LoadingProjectStudentData(helper,projectID, groupName, ucDataGridView1);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiTreeView1_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                TreeNode  tree = uiTreeView1.GetNodeAt(e.X, e.Y);
                if(tree != null)
                {
                    uiTreeView1.SelectedNode = tree;
                }
                
            }
            else if(e.Button == MouseButtons.Right)
            {
                uiContextMenuStrip1.Show(e.Location);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiuiButton23_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show($"该操作将删除{uiTextBox1.Text}组数据，是否继续？？", "提示", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                string txt = uiTextBox1.Text.Trim();
                ImportStudentDataWindowSys.Instance.DeleteAllGroup(helper,txt,  projectName);
                   UIMessageBox.ShowSuccess("删除成功！！");
                ImportStudentDataWindowSys.Instance.LoadingProjectView(uiTreeView1, helper);
                ImportStudentDataWindowSys.Instance.LoadingProjectStudentData(helper, projectID, groupName, ucDataGridView1);
            }
            else
            {
                UIMessageBox.ShowError("删除失败！！");
                return;
            }
        }
        /// <summary>
        /// 删除选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton5_Click(object sender, EventArgs e)
        {
            if (ucDataGridView1.SelectRows.Count > 0)
            {
                ImportStudentDataWindowSys.Instance.DeleteCurrentChooseStudent(helper,ucDataGridView1, txt_projectName.Text);
                UIMessageBox.ShowSuccess("删除成功！！");
                ImportStudentDataWindowSys.Instance.LoadingProjectStudentData(helper, projectID, uiTextBox1.Text, ucDataGridView1);
            }
            else
            {
                UIMessageBox.ShowWarning("请先选择学生数据！！");
                return;
            }
            
        
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton1_Click(object sender, EventArgs e)
        {
            string projectName= txt_projectName.Text.Trim();
            if (!string.IsNullOrEmpty(projectName))
            {
                if (ImportStudentDataWindowSys.Instance.ShowImportStudentWindow(projectName,helper ))
                {
                    ImportStudentDataWindowSys.Instance.LoadingProjectView(uiTreeView1, helper);
                    ImportStudentDataWindowSys.Instance.LoadingProjectStudentData(helper, projectID, groupName, ucDataGridView1);
                }
            }
            else
            {
                UIMessageBox.ShowWarning("请先选择你需要导入的项目名！！");
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
            SaveFileDialog saveImageDialog = new SaveFileDialog();
            saveImageDialog.Filter = "xls files(*.xls)|*.xls|xlsx file(*.xlsx)|*.xlsx|All files(*.*)|*.*";
            saveImageDialog.RestoreDirectory = true;
            saveImageDialog.FileName = $"导出模板{DateTime.Now.ToString("yyyyMMddHHmmss")}.xls";
            string path = Application.StartupPath + "\\excel\\output.xlsx";

            if (saveImageDialog.ShowDialog() == DialogResult.OK)
            {
                path = saveImageDialog.FileName;
                File.Copy(@"./模板/导入名单模板.xls", path);
                FrmTips.ShowTipsSuccess(this, "导出成功");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton3_Click(object sender, EventArgs e)
        {
            string name0 = uiTreeView1.SelectedNode.Text;
            string Name = txt_projectName.Text;
            int Type = txt_Type.SelectedIndex;
            int RoundCount = txt_RoundCount.SelectedIndex;
            int BestScoreMode = txt_BestScoreMode.SelectedIndex;
            int TestMethod = txt_TestMethod.SelectedIndex;
            int FloatType = txt_FloatType.SelectedIndex;
            if( ImportStudentDataWindowSys.Instance.SaveDataIntoDataBase(helper, name0,Name, Type, RoundCount, BestScoreMode, TestMethod, FloatType))
            {
                UIMessageBox.ShowSuccess("保存成功！！");
                ImportStudentDataWindowSys.Instance.LoadingProjectView(uiTreeView1, helper);
            }
            else
            {
                UIMessageBox.ShowError("保存失败！！");
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 新建项目ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewProject();
        }
        /// <summary>
        /// 
        /// </summary>
        private void CreateNewProject()
        {
            FrmCreateNewProject frm = new FrmCreateNewProject();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                String NewProjectName = frm.ProjectName;
                ImportStudentDataWindowSys.Instance.CreateProject(NewProjectName,helper);
                ImportStudentDataWindowSys.Instance.LoadingProjectView(uiTreeView1, helper);

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 删除项目ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (uiTreeView1.SelectedNode.Level != 0)
            {
                FrmTips.ShowTipsInfo(this, "请选择一个项目");
                return;
            }
            else
            {
                if (FrmDialog.ShowDialog(this, $"是否确认删除{uiTreeView1.SelectedNode.Text}项目？", "删除确认", true) == System.Windows.Forms.DialogResult.OK)
                {
                    DeleteProject(uiTreeView1.SelectedNode.Text);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        private void DeleteProject(string text)
        {
            var value =helper.ExecuteScalar($"SELECT Id FROM SportProjectInfos WHERE Name='{projectName}'");
            string projectId = value.ToString();

            int result =helper.ExecuteNonQuery($"DELETE FROM SportProjectInfos WHERE Id = '{projectId}'");
            if (result == 1)
            {
                helper.ExecuteNonQuery($"DELETE FROM DbGroupInfos WHERE ProjectId = '{projectId}'");
                helper.ExecuteNonQuery($"DELETE FROM DbPersonInfos WHERE ProjectId = '{projectId}'");
                ImportStudentDataWindowSys.Instance.LoadingProjectView(uiTreeView1, helper);
            }
            FrmTips.ShowTipsSuccess(this, $"删除{projectName}");
        }
    }
}
