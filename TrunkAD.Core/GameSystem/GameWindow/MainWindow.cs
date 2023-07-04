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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrunkAD.Core.GameSystem.GameHelper;
using TrunkAD.Core.GameSystem.GameWindowSys;

namespace TrunkAD.Core.GameSystem.GameWindow
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            string code = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.toolStripStatusLabel1.Text = "德育龙体育测试系统" + code;
            MainWindowSys.Instance.UpdataTreeview(uiTreeView1);
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiTreeView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TreeNode treeNode = uiTreeView1.GetNodeAt(e.X, e.Y);
                if (treeNode != null)
                {
                    uiTreeView1.SelectedNode = treeNode;
                }
                else { return; }
            }
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
                if (!string.IsNullOrEmpty(uiTreeView1.SelectedNode.Text.Trim()))
                {
                    string path = uiTreeView1.SelectedNode.FullPath.Trim();
                    string[] fullPath = path.Split('\\');
                    if(e.Node.Level == 0)
                    {
                        projectName = fullPath[0];
                    }
                    if (e.Node.Level == 1)
                    {
                        projectName = fullPath[0];
                        groupName = fullPath[1];
                    }
                    MainWindowSys.Instance.LoadingCurrentChooseViewStudentData(projectName, groupName,listView1,ref projectID);

                }
                else
                {
                    UIMessageBox.ShowWarning("请先选择项目数据！！");
                    return;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ucNavigationMenu1_ClickItemed(object sender, EventArgs e)
        {
            var txt = ucNavigationMenu1.SelectItem.Text.Trim();
            if(!string.IsNullOrEmpty(txt)) 
            {
                switch (txt)
                {
                    case "名单导入":
                        if (MainWindowSys.Instance.ShowImportStudentDataWindow())
                        {
                            MainWindowSys.Instance.UpdataTreeview(uiTreeView1);
                            MainWindowSys.Instance.LoadingCurrentChooseViewStudentData(projectName, groupName, listView1,ref projectID);
                        }
                        break;
                    case "平台设备码":
                        if (SoftWareProperty.singleMode != "0")
                        {
                            MessageBox.Show("单机模式无法设置");
                            return;
                        }
                        MainWindowSys.Instance.ShowPlatFormWindow();
                        break;
                    case "初始化数据库":
                        if (MessageBox.Show("改操作会将数据库清空,是否继续？？", "提示", MessageBoxButtons.OKCancel) == DialogResult.OK) {
                            if (MainWindowSys.Instance.InitSQLiteDataBase())
                            {
                                UIMessageBox.ShowSuccess("数据库初始化成功！！");
                                MainWindowSys.Instance.UpdataTreeview(uiTreeView1);
                            }
                            else
                            {
                                UIMessageBox.ShowError("数据库初始化失败！！");
                                return;
                            }
                        }
                        break;
                    case "数据库备份":
                        if (MessageBox.Show("改操作会将数据库备份,是否继续？？", "提示", MessageBoxButtons.OKCancel) == DialogResult.OK)
                        {
                            if (MainWindowSys.Instance.ExportSQLiteDataBase())
                            {
                                UIMessageBox.ShowSuccess("数据库备份成功！！");
                            }
                            else
                            {
                                UIMessageBox.ShowError("数据库备份失败！！");
                                return;
                            }
                        }
                        break;
                    case "上传成绩":
                        if (SoftWareProperty.singleMode != "0")
                        {
                            MessageBox.Show("单机模式无法设置");
                            return;
                        }
                        MessageBox.Show("请从测试页面上传!");
                        return;
                        break;
                    case "修改成绩":
                        FixUpGradeData();
                        break;
                    case "导出成绩":
                        ExportGrade();

                        break;
                    case "清除成绩":
                        ClearCurrentGrade();
                        break;
                    case "启动测试":
                        StartTesting();
                        break;
                    case "导入名单模板":
                        string path = Application.StartupPath + "\\模板\\导入名单模板.xls";
                        if (File.Exists(path))
                        {
                            System.Diagnostics.Process.Start(path);
                        }
                        break;
                    case "导入成绩模板":
                         string paths = Application.StartupPath + "\\模板\\导入成绩模板.xls";
                        if (File.Exists(paths))
                        {
                            System.Diagnostics.Process.Start(paths);
                        }
                        break;
                    case "退出":
                        this.Close();
                        Application.Exit();
                        break;
                }
            }
        }

        private void StartTesting()
        {
            try
            {
                var ls = uiTreeView1.SelectedNode;
                if (ls != null)
                {
                    string path = uiTreeView1.SelectedNode.FullPath;
                    string[] fsp = path.Split('\\');
                    if (fsp.Length > 0)
                    {
                        this.Hide();
                        if (MainWindowSys.Instance.StartTesting(fsp))
                        {
                            MainWindowSys.Instance.LoadingCurrentChooseViewStudentData(projectName, groupName, listView1,ref projectID);
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return;
            }
            finally
            {
                this.Show();
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        private void ClearCurrentGrade()
        {
            if(listView1.SelectedItems.Count > 0)
            {
                string Name = listView1.SelectedItems[0].SubItems[3].Text;
                string PersonIdNumber = listView1.SelectedItems[0].SubItems[4].Text;
                if (MessageBox.Show( $"是否清空学生:{Name}的成绩？", "提示",MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                {
                    if(MainWindowSys.Instance.ClearCurrentGrade(Name, PersonIdNumber))
                    {
                        UIMessageBox.ShowSuccess("清除成功！！");

                    }
                    else
                    {
                        UIMessageBox.ShowError("清除失败！！");
                        return;
                    }
                }
               
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private  bool  ExportGrade()
        {
             return MainWindowSys.Instance.ExportCurrentGrade(projectID,projectName,groupName); 
        }

        /// <summary>
        /// 修改成绩
        /// </summary>
        private void FixUpGradeData()
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string projectName = listView1.SelectedItems[0].SubItems[1].Text;
                string groupName = listView1.SelectedItems[0].SubItems[2].Text;
                string name = listView1.SelectedItems[0].SubItems[3].Text;
                string idNumber = listView1.SelectedItems[0].SubItems[4].Text;
                string status = listView1.SelectedItems[0].SubItems[5].Text; 
                PasswordWindow password = new PasswordWindow();
                if (password.ShowDialog() == DialogResult.OK)
                {
                    string acc = password.acc;
                    string pass = password.pass;
                    if( MainWindowSys.Instance.VerficationPassword(acc, pass))
                    {
                        if(MainWindowSys.Instance.ModifyCurrentGrade(projectName, groupName, name, idNumber, status, projectID))
                        {
                            UIMessageBox.ShowSuccess("修改成绩成功！！");
                            MainWindowSys.Instance.LoadingCurrentChooseViewStudentData(projectName, groupName, listView1, ref projectID);
                        }
                        else
                        {
                            UIMessageBox.ShowError("修改成绩失败！！");
                            return;
                        }
                    }
                    else
                    {
                        UIMessageBox.ShowWarning("请输入账号信息验证！！");
                        return;
                    }

                }
                else
                {
                    UIMessageBox.ShowError("修改成绩失败！！");
                    return;
                }
            }
            else
            {
                UIMessageBox.ShowError("请先选择学生数据！！");
                return;
            }
        }
    }
}
