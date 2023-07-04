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
using TrunkAD.Core.GameSystem.GameModel;
using Newtonsoft.Json;
using System.Threading;
using HZH_Controls;
using MiniExcelLibs;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TrunkAD.Core.GameSystem.GameWindow
{
    public partial class ImportDataWindow : Form
    {
        public string projectName { get; set; }
       

        public List<Dictionary<string, string>> localInfos = new List<Dictionary<string, string>>();
        public Dictionary<string, string> localValues = new Dictionary<string, string>();

        public ImportDataWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton1_Click(object sender, EventArgs e)
        {
            if (ImportDataWindowSys.Instance.InitDataBase(sQLiteHelper))
            {
                UIMessageBox.ShowSuccess("数据库初始化成功！！");
            }
            else
            {
                UIMessageBox.ShowError("数据库初始化失败！！");
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
            if (ImportDataWindowSys.Instance.BackDataBase(sQLiteHelper))
            {
                UIMessageBox.ShowSuccess("数据库备份成功！！");

            }
            else
            {
                UIMessageBox.ShowError("数据库备份失败！！");
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
            SaveFileDialog saveImageDialog = new SaveFileDialog();
            saveImageDialog.Filter = "xlsx file(*.xlsx)|*.xlsx|All files(*.*)|*.*";
            saveImageDialog.RestoreDirectory = true;
            saveImageDialog.FileName = $"导出模板{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
            string path = Application.StartupPath + "\\excel\\output.xlsx";

            if (saveImageDialog.ShowDialog() == DialogResult.OK)
            {
                path = saveImageDialog.FileName;
                File.Copy(@"./模板/导入名单模板1.xlsx", path);
                UIMessageBox.ShowSuccess("导出成功");
            }
        }
        int proVal = 0;
        int proMax = 0;
        public SQLiteHelper sQLiteHelper;

        private void uiuiButton23_Click(object sender, EventArgs e)
        {
            string path = ImportDataWindowSys.Instance.OpenLocalExcelFile();
            ImportDataWindowSys.Instance.LoadingCurrentImportStudentData(path, projectName, sQLiteHelper, ref proVal, ref proMax);
            DialogResult = DialogResult.OK;
            this.Close();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton5_Click(object sender, EventArgs e)
        {
            ImportDataWindowSys.Instance.ShowEquipMentSettingWindow(sQLiteHelper);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uiButton6_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(uiTextBox1.Text.Trim()))
                funTest5(uiTextBox1.Text.Trim());
            else
            {
                UIMessageBox.ShowWarning("请先 输入拉取的数组！！");
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupNums"></param>
        public void funTest5(string groupNums)
        {
            RequestParameter RequestParameter = new RequestParameter();
            RequestParameter.AdminUserName = localValues["AdminUserName"];
            RequestParameter.TestManUserName = localValues["TestManUserName"];
            RequestParameter.TestManPassword = localValues["TestManPassword"];
            string ExamId0 = localValues["ExamId"];
            ExamId0 = ExamId0.Substring(ExamId0.IndexOf('_') + 1);
            string MachineCode0 = localValues["MachineCode"];
            MachineCode0 = MachineCode0.Substring(MachineCode0.IndexOf('_') + 1);

            string MachineCode1 = localValues["MachineCode1"];
            MachineCode1 = MachineCode1.Substring(MachineCode1.IndexOf('_') + 1);
            RequestParameter.ExamId = ExamId0;
            RequestParameter.MachineCode = MachineCode0;
            RequestParameter.GroupNums = groupNums + "";
            //序列化
            string JsonStr = string.Empty;
            string url = localValues["Platform"] + RequestUrl.GetGroupStudentUrl;
            GetGroupStudent upload_Result = null;
            if (!string.IsNullOrEmpty(MachineCode0))
            {
                JsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(RequestParameter);
                //? 下载男
                var formDatas = new List<FormItemModel>();
                //添加其他字段
                formDatas.Add(new FormItemModel()
                {
                    Key = "data",
                    Value = JsonStr
                });

                string result = HttpUpload.PostForm(url, formDatas);
                //GetGroupStudent upload_Result = JsonConvert.DeserializeObject<GetGroupStudent>(result);
                string[] strs = GetGroupStudent.CheckJson(result);

                if (strs[0] == "1")
                {
                    upload_Result = JsonConvert.DeserializeObject<GetGroupStudent>(result);
                }
                else
                {
                    upload_Result = new GetGroupStudent();
                    upload_Result.Error = strs[1];
                }
            }
            GetGroupStudent studentList = new GetGroupStudent();
            studentList.Results = new Results();
            studentList.Results.groups = new List<GroupsItem>();
            bool bFlag = false;
            if (upload_Result == null || upload_Result.Results == null || upload_Result.Results.groups.Count == 0)
            {
                string error = string.Empty;
                try
                {
                    error = upload_Result.Error;

                }
                catch (Exception)
                {

                    error = string.Empty;
                }
                FrmTips.ShowTipsError(this, $"男生组提交错误,错误码:[{error}]");
            }
            else
            {
                bFlag = true;
            }

            int step1 = 0;
            int stepMax1 = 0;
            try
            {
                stepMax1 = upload_Result.Results.groups.Count;
            }
            catch (Exception)
            {

                stepMax1 = 0;
            }
            if (!bFlag) stepMax1 = 0;
            int step2 = 0;

            try
            {
                if (upload_Result.Results.groups.Count > 0)
                {
                    studentList.Results.groups.AddRange(upload_Result.Results.groups);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
            proVal = 0;
            proMax = 0;
            if (studentList.Results.groups.Count > 0)
            {
                //downlistOutputExcel(studentList);
                downlistOutputExcel1(studentList);
                timer1.Start();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportDataWindow_Load(object sender, EventArgs e)
        {
            ImportDataWindowSys.Instance.LoadingInitData(sQLiteHelper, ref localInfos, ref localValues);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="upload_Result"></param>
        void downlistOutputExcel1(GetGroupStudent upload_Result)
        {
            List<GroupsItem> Groups = upload_Result.Results.groups;
            List<InputData> doc = new List<InputData>();
            int step = 1;
            proVal = 0;
            proMax = 0;
            //序号	学校	 年级	班级 	姓名	 性别	准考证号	 组别名称
            foreach (var Group in Groups)
            {
                string groupId = Group.GroupId;
                string groupName = Group.GroupName;
                foreach (var StudentInfo in Group.StudentInfos)
                {
                    InputData idata = new InputData();
                    idata.Id = step;
                    idata.School = StudentInfo.SchoolName;
                    idata.GradeName = StudentInfo.GradeName;
                    idata.ClassName = StudentInfo.ClassName;
                    idata.Name = StudentInfo.Name;
                    idata.Sex = StudentInfo.Sex;
                    idata.IdNumber = StudentInfo.IdNumber;
                    idata.GroupName = groupId;
                    idata.examTime = StudentInfo.examTime;
                    doc.Add(idata);
                    step++;
                }
            }
            string saveDir = Application.StartupPath + $"\\模板\\下载名单\\";
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }
            string path = Path.Combine(saveDir, $"downList{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx");
            //string path = Application.StartupPath + $"\\模板\\下载名单\\downList{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
            ExcelUtils.MiniExcel_OutPutExcel(path, doc);
            ParameterizedThreadStart ParStart = new ParameterizedThreadStart(ExcelListInput);
            Thread t = new Thread(ParStart);
            t.IsBackground = true;
            t.Start(path);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void ExcelListInput(object obj)
        {

            bool IsResult = false;
            try
            {
                string path = obj as string;
                if (!string.IsNullOrEmpty(path))
                {

                    string projectid = sQLiteHelper.ExecuteScalar($"select Id from SportProjectInfos where name='{projectName}'").ToString();
                    var rows = MiniExcel.Query<InputData>(path).ToList();
                    proVal = 0;
                    proMax = rows.Count;
                    ///序号
                    ///学校
                    ///年纪
                    ///班级
                    ///姓名
                    ///性别
                    ///准考证号
                    ///组别名称
                    ///
                    HashSet<String> set = new HashSet<String>();
                    for (int i = 0; i < rows.Count; i++)
                    {
                        string[] examTime = rows[i].examTime.Split(' ');

                        set.Add(rows[i].GroupName + "#" + examTime[0]);
                    }
                    List<String> rolesMarketList = new List<string>();
                    rolesMarketList.AddRange(set);

                    System.Data.SQLite.SQLiteTransaction sQLiteTransaction = sQLiteHelper.BeginTransaction();
                    for (int i = 0; i < rolesMarketList.Count; i++)
                    {
                        string role = rolesMarketList[i];
                        string[] roles = role.Split('#');
                        string GroupName = roles[0];
                        string examTime = roles[1];
                        string countstr = sQLiteHelper.ExecuteScalar($"SELECT COUNT(*) FROM DbGroupInfos where ProjectId='{projectid}' and Name='{GroupName}'").ToString();
                        int.TryParse(countstr, out int count);
                        if (count == 0)
                        {
                            string groupsortidstr = sQLiteHelper.ExecuteScalar("select MAX( SortId ) + 1 from DbGroupInfos").ToString();
                            int groupsortid = 1;
                            int.TryParse(groupsortidstr, out groupsortid);
                            /* string insertsql = $"INSERT INTO DbGroupInfos(CreateTime,SortId,IsRemoved,ProjectId,Name,IsAllTested) " +
                                 $"VALUES(datetime(CURRENT_TIMESTAMP, 'localtime'),{groupsortid},0,'{projectid}','{GroupName}',0)";*/
                            string insertsql = $"INSERT INTO DbGroupInfos(CreateTime,SortId,IsRemoved,ProjectId,Name,IsAllTested) " +
                              $"VALUES('{examTime}',{groupsortid},0,'{projectid}','{GroupName}',0)";
                            //插入组
                            sQLiteHelper.ExecuteNonQuery(insertsql);
                        }
                    }
                    sQLiteHelper.CommitTransaction(ref sQLiteTransaction);
                    int groupNum = (rows.Count + 5000) / 5000;
                    List<List<InputData>> rowsSpiltList = ImportDataWindowSys.Instance.SpiltList(rows, groupNum);
                    foreach (var inputDatas in rowsSpiltList)
                    {
                        sQLiteTransaction = sQLiteHelper.BeginTransaction();
                        foreach (var idata in inputDatas)
                        {
                            //InputData idata = rows[i];
                            string PersonIdNumber = idata.IdNumber;
                            string name = idata.Name;
                            int Sex = idata.Sex == "男" ? 0 : 1;
                            string SchoolName = idata.School;
                            string GradeName = idata.GradeName;
                            string classNumber = idata.ClassName;
                            string GroupName = idata.GroupName;
                            string[] examTimes = idata.examTime.Split(' ');
                            string examTime = examTimes[0];
                            string countstr = sQLiteHelper.ExecuteScalar($"SELECT COUNT(*) FROM DbPersonInfos WHERE ProjectId='{projectid}' AND IdNumber='{PersonIdNumber}'").ToString();
                            int.TryParse(countstr, out int count);
                            if (count == 0)
                            {
                                int personsortid = 1;
                                string personsortidstr = sQLiteHelper.ExecuteScalar("select MAX( SortId ) + 1 from DbPersonInfos").ToString();
                                int.TryParse(personsortidstr, out personsortid);
                                string insertsql = $"INSERT INTO DbPersonInfos(CreateTime,SortId,IsRemoved,ProjectId,SchoolName,GradeName,ClassNumber,GroupName,Name,IdNumber,Sex,State,FinalScore,uploadState) " +
                                    $"VALUES('{examTime}',{personsortid},0,'{projectid}','{SchoolName}','{GradeName}','{classNumber}','{GroupName}'," +
                                    $"'{name}','{PersonIdNumber}',{Sex},0,-1,0)";
                                sQLiteHelper.ExecuteNonQuery(insertsql);
                            }
                            proVal++;
                        }
                        sQLiteHelper.CommitTransaction(ref sQLiteTransaction);
                    }

                    if (rows.Count == 0)
                    {
                        IsResult = false;
                    }
                    else
                    {
                        IsResult = true;
                    }
                }

            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);

            }
            if (IsResult)
            {
                ControlHelper.ThreadInvokerControl(this, () =>
                {
                    //FrmTips.ShowTipsSuccess(this, "导入成功");
                    DialogResult = DialogResult.OK;
                });

            }
            else
            {
                ControlHelper.ThreadInvokerControl(this, () =>
                {
                    //FrmTips.ShowTipsSuccess(this, "导入失败");
                    DialogResult = DialogResult.Cancel;
                });

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (proMax != 0)
            {
                progressBar1.Maximum = proMax;
                if (proVal > proMax)
                {
                    proVal = proMax;
                    timer1.Stop();
                }
                progressBar1.Value = proVal;
            }
        }
    }
    
}
