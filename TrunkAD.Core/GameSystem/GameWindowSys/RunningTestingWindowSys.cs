using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Controls;
using AForge.Video.DirectShow;
using NPOI.SS.Formula.Functions;
using OpenCvSharp;
using Spire.Pdf.Exporting.XPS.Schema;
using Spire.Xls;
using Sunny.UI;
using static System.Net.WebRequestMethods;
using TrunkAD.Core.GameSystem.GameHelper;
using TrunkAD.Core.GameSystem.GameHelper.TreeViewHelper;
using TrunkAD.Core.GameSystem.GameModel;
using TrunkAD.Core.GameSystem.GameWindow;
using Point = System.Drawing.Point;
using System.Security.Cryptography.X509Certificates;
using System.Drawing;
using System.IO.Ports;
using System.Management;
using System.Threading;
using CustomControl;

namespace TrunkAD.Core.GameSystem.GameWindowSys
{
    public class RunningTestingWindowSys
    {
        public static RunningTestingWindowSys Instance { get; private set; }
        RunningTestingWindow RunTestWindow= null;
        public void Awake()
        {
            Instance = this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sQLiteHelper"></param>
        /// <param name="fsp"></param>
        /// <param name="list"></param>
        public bool ShowRunningTestWindow(SQLiteHelper sQLiteHelper, string[] fsp,List<Dictionary<string,string>> list)
        {
            Dictionary<string, string> dict = list[0];
            int.TryParse(dict["Type"], out int state);
            RunTestWindow = new RunningTestingWindow();
            RunTestWindow._ProjectId = fsp[0];
            RunTestWindow.sQLiteHelper = sQLiteHelper;
            RunTestWindow._ProjectName = fsp[0];
            if (fsp.Length > 1)
                RunTestWindow._GroupName = fsp[1];
            RunTestWindow._ProjectId = dict["Id"];
            RunTestWindow._Type = dict["Type"];
            RunTestWindow._RoundCount = Convert.ToInt32(dict["RoundCount"]);
            RunTestWindow._BestScoreMode = Convert.ToInt32(dict["BestScoreMode"]);
            RunTestWindow._TestMethod = Convert.ToInt32(dict["TestMethod"]);
            RunTestWindow._FloatType = Convert.ToInt32(dict["FloatType"]);
            RunTestWindow.formTitle = string.Format("考试项目:{0}", fsp[0]);
            var   sl=RunTestWindow.ShowDialog();
            if (sl == DialogResult.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 更新组下拉框信息
        /// </summary>
        /// <param name="GroupCbx"></param>
        /// <param name="projectId"></param>
        /// <param name="helper"></param>
        /// <param name="groupName"></param>
        public void UpDataGroupCombox(UIComboBox GroupCbx, string projectId, SQLiteHelper helper,string groupName="")
        {
            try
            {
                GroupCbx.Items.Clear();
                GroupCbx.Text = " ";
                var sl = helper.ExecuteReader($"SELECT Name FROM DbGroupInfos WHERE Name LIKE'%{groupName}%' AND ProjectId='{projectId}'");
                while (sl.Read())
                {
                    GroupCbx.Items.Add(sl.GetString(0));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        /// <summary>
        /// 加载学生姓名账号框
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="projectId"></param>
        /// <param name="groupName"></param>
        /// <param name="nameCbx"></param>
        public void LoadingCurrentTestingStudentName(SQLiteHelper helper, string projectId, string groupName, UIComboBox nameCbx )
        {
            try
            {
                nameCbx.Items.Clear();
                string sql =  "SELECT dpi.GroupName,dpi.Name" + $" FROM DbPersonInfos as dpi WHERE dpi.GroupName='{groupName}' AND dpi.ProjectId='{projectId}'";
                var sl= helper.ExecuteReader(sql);
                while (sl.Read())
                {
                    string name = sl.GetValue(1).ToString();
                    nameCbx.Items.Add(name);
                }
                
            }
            catch (Exception e)
            {
                LoggerHelper.Debug(e);
                return;
            }
        }
        /// <summary>
        /// 加载当前学生信息
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="studentName">学生名字</param>
        /// <param name="projectId">项目id</param>
        /// <param name="groupName">组号</param>
        /// <param name="currentRoundCount">当前伦次</param>
        /// <returns></returns>
        public StudentTestingData LoadingCurrentTestingStudentData(SQLiteHelper helper, string studentName, string projectId, string groupName, int currentRoundCount)
        {
            StudentTestingData studentTestingData = null;
            try
            {  
                string sql =  "SELECT dpi.IDnumber,dpi.state,dpi.finalscore,dpi.id" + $" FROM DbPersonInfos as dpi WHERE dpi.GroupName='{groupName}' AND dpi.ProjectId='{projectId}' and dpi.Name='{studentName}'";
                var ds = helper.ExecuteReader(sql);
                while (ds.Read())
                {
                    string idNumber = ds.GetValue(0).ToString();
                    int state = ds.GetInt32(1);
                    int perid = ds.GetInt32(3); 
                    studentTestingData = new StudentTestingData()
                    {
                        idNumber = idNumber,
                        groupName = groupName,
                        RoundId = currentRoundCount,
                        name = studentName,
                        id=perid.ToString(),
                        
                    };
                    var res = helper.ExecuteReaderList($"SELECT SortId,RoundId,Result,State,CreateTime,uploadState FROM ResultInfos WHERE personid='{perid}' ");
                    if (res.Count > 0)
                    {
                        foreach (var item in res)
                        {
                            var keys = item.Keys;
                            foreach (var key in keys)
                            {
                                if (key == "RoundId")
                                {
                                    item.TryGetValue(key, out string round);
                                    if (int.Parse(round) == currentRoundCount)
                                    {
                                        item.TryGetValue("Result", out string ress);
                                        if (string.IsNullOrEmpty(ress) || ress == 0.ToString())
                                        {
                                            item.TryGetValue("State", out string str);
                                            int st = int.Parse(str);
                                            studentTestingData.state = st;
                                        }
                                        else
                                        {
                                            studentTestingData.score = double.Parse(ress);
                                            studentTestingData.state = 1;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else { continue; }
                            }
                        }
                    }
                    else
                    {
                        studentTestingData.state = 0;
                        studentTestingData.score=0;
                    }
                }
                return studentTestingData;
            }
            catch (Exception e)
            {
                 LoggerHelper.Debug(e);
                 return null;
            }
        }
        /// <summary>
        /// 通过学生考号加载学生信息
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="groupName"></param>
        /// <param name="idnumber"></param>
        public StudentTestingData LoadingStudentDataByStudentIDNumber(SQLiteHelper helper,string projectId, string idnumber,int currentRound)
        { 
            try
            {
                StudentTestingData studentTestingData = null;
                string sql=   $"SELECT Id,Name,IdNumber,Sex,GroupName FROM DbPersonInfos WHERE ProjectId='{projectId}' and IdNumber='{idnumber}'";
                var sl = helper.ExecuteReaderList(sql);
                if(sl.Count > 0)
                {
                    foreach(var dic in sl)
                    {
                        string stuId = dic["Id"];
                        string stuName = dic["Name"];
                        string idNumber = dic["IdNumber"];
                        string groupName = dic["GroupName"];
                         studentTestingData = new StudentTestingData()
                        {
                            id = stuId,
                            name = stuName,
                            idNumber = idNumber,
                            groupName = groupName,
                            RoundId =currentRound,
                        };
                        sql = $"SELECT SortId,RoundId,Result,State,CreateTime,uploadState FROM ResultInfos WHERE personid='{stuId}' ";
                        var res = helper.ExecuteReaderList(sql);
                        if(res.Count > 0)
                        {
                            foreach (var item in res)
                            {
                                var keys = item.Keys;
                                foreach (var key in keys)
                                {
                                    if (key == "RoundId")
                                    {
                                        item.TryGetValue(key, out string round);
                                        if (int.Parse(round) == currentRound)
                                        {
                                            item.TryGetValue("Result", out string ress);
                                            if (string.IsNullOrEmpty(ress) || ress == 0.ToString())
                                            {
                                                item.TryGetValue("State", out string str);
                                                int st = int.Parse(str);
                                                studentTestingData.state = st;
                                            }
                                            else
                                            {
                                                studentTestingData.score = double.Parse(ress);
                                                studentTestingData.state = 1;
                                            }
                                             
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                    else { continue; }
                                }
                            }


                        }

                    }
                    return studentTestingData;
                }
                else
                {
                    UIMessageBox.ShowWarning($"没有找到准考证号码为{idnumber}的考生");
                    return null;
                }
            }
            catch (Exception e)
            {
               LoggerHelper.Debug(e);
               return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="roundID"></param>
        public void SpeekCurrentStudent(string name, string roundID)
        {
            string say = $"请{name}开始进行第{roundID}轮考试！";
            SpeekHelper.Instance.Speaking(say);
        }
        /// <summary>
        /// 加载当前组所有学生信息
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="groupName"></param>
        /// <param name="studentTestingData"></param>
        /// <param name="currentRoundCount"></param>
        /// <returns></returns>
        public List<StudentTestingData> LoadingCurrentGroupAllStudentData(SQLiteHelper helper,string projectId, string groupName ,int current )
        {
            try
            {
                List<StudentTestingData> studentTestingDatas = new List<StudentTestingData>();
                string sql = "SELECT dpi.Name ,dpi.IDnumber,dpi.state,dpi.finalscore,dpi.id" + $" FROM DbPersonInfos as dpi WHERE dpi.GroupName='{groupName}' AND dpi.ProjectId='{projectId}'";
                var sl = helper.ExecuteReader(sql);
                while (sl .Read())
                {
                    string name = sl.GetValue(0).ToString();
                    string idnum = sl.GetValue(1).ToString();
                    int id = sl.GetInt32(4);
                    StudentTestingData    studentTestingData = new StudentTestingData()
                    {
                        idNumber = idnum,
                        groupName = groupName,
                        RoundId = current,
                        name = name,
                        id=id.ToString(),
                    };
                    var res = helper.ExecuteReaderList($"SELECT SortId,RoundId,Result,State,CreateTime,uploadState FROM ResultInfos WHERE personid='{id}' ");
                    if (res.Count > 0)
                    {
                        foreach (var item in res)
                        {
                            var keys = item.Keys;
                            foreach (var key in keys)
                            {
                                if (key == "RoundId")
                                {
                                    item.TryGetValue(key, out string round);
                                    if (int.Parse(round) == current)
                                    {
                                        item.TryGetValue("Result", out string ress);
                                        if (string.IsNullOrEmpty(ress) || ress == 0.ToString())
                                        {
                                            item.TryGetValue("State", out string str);
                                            int st = int.Parse(str);
                                            studentTestingData.state = st;
                                        }
                                        else
                                        {
                                            studentTestingData.score = double.Parse(ress);
                                            studentTestingData.state = 1;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else { continue; }
                            }
                        }
                    }
                    else
                    {
                        studentTestingData.state = 0;
                        studentTestingData.score=0;
                    }
                    studentTestingDatas.Add(studentTestingData);
                }
                return studentTestingDatas;
            }
            catch (Exception exception)
            {
                LoggerHelper.Debug(exception);
                return null;
            }
        }
        /// <summary>
        /// 展示平台设置页面
        /// </summary>
        /// <param name="helper"></param>
        public void ShowPlatformSettingWindow(SQLiteHelper helper)
        {
            EquipMentSettingWindowSys.Instance.ShowPlatFormSettingWindow(helper);
        }
        /// <summary>
        /// 导出当前组成绩
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="groupName"></param>
        /// <param name="projectId"></param>
        /// <param name="path"></param>
        public bool ExportCurrentGroupGrade(SQLiteHelper helper, string groupName, string projectId, string path)
        {
            List<ExportDataModel> exportDataModels = GetExportCurrentGroupGradeData(helper,groupName,projectId);
            if (exportDataModels.Count > 0 && exportDataModels != null)
            { 
                ExcelUtils.MiniExcel_OutPutExcel(path,exportDataModels);
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 获取当前组的学生信息
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="groupName"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        private List<ExportDataModel> GetExportCurrentGroupGradeData(SQLiteHelper helper, string groupName, string projectId)
        {
            return GradeManager.Instance.GetExportCurrentGroupGradeData(helper, groupName, projectId);
        }  
        /// <summary>
        /// 打印当前组成绩
        /// </summary>
        /// <param name="path"></param>
        /// <param name="helper"></param>
        /// <param name="_GroupName"></param>
        /// <param name="_ProjectId"></param>
        public void PrintCurrentGroupData(string path,SQLiteHelper helper,string _GroupName,string _ProjectId)
        {
            if (ExportCurrentGroupGrade(helper, _GroupName, _ProjectId, path))
            {
                GradeManager.Instance.PrintCurrentGroupGrade(path);
            }
            else
            {
                UIMessageBox .ShowWarning("打印失败。请重试！！");
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentChooseStudentTestingData"></param>
        /// <param name="currentRoundCount"></param>
        /// <param name="state"></param>
        public void SettinCurrentStudentData(SQLiteHelper helper,string projectID,StudentTestingData currentChooseStudentTestingData, int currentRoundCount, string state)
        {
            try
            {
                var  sl = LoadingCurrentTestingStudentData(helper, currentChooseStudentTestingData.name, projectID, currentChooseStudentTestingData.groupName, currentRoundCount);
                if (sl != null)
                {
                    if (state == "成绩重测")  ///重新测试，不需要去管当前考生resultinfos 中的成绩
                    {
                        if (MessageBox.Show($"当前学生{currentChooseStudentTestingData.name}在本轮{currentRoundCount}已测试，是否将其成绩重置？",
                                   "提示", MessageBoxButtons.OKCancel) == DialogResult.OK)
                        {
                            if (GradeManager.Instance.ResettingCurrentGrade(helper, projectID, currentChooseStudentTestingData.name, currentChooseStudentTestingData.groupName,
                                    currentChooseStudentTestingData.idNumber, currentRoundCount))
                            {
                                UIMessageBox.ShowSuccess("成绩重置成功！！");
                            }
                            else
                            {
                                UIMessageBox.ShowError("成绩重置失败！！");
                                return;
                            }
                        }
                        else
                        {
                            UIMessageBox.ShowWarning($"当前考生{currentChooseStudentTestingData.name}本轮已测试，无法将其状态设置为{state}");
                            return;
                        }
                    }
                    else
                    {
                         if(GradeManager.Instance.SettingCurrentStudentTestingState(helper,currentChooseStudentTestingData, currentRoundCount,projectID,state))
                         {
                             UIMessageBox.ShowWarning($"当前考生{currentChooseStudentTestingData.name}本轮成绩已设置为{state}");
                             
                         }
                         else
                         {
                             return;
                         }
                    }
                }
            }
            catch (Exception e)
            {
                 LoggerHelper.Debug(e);
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fsp"></param>
        /// <param name="helper"></param>
        /// <param name="currentRoundCount"></param>
        /// <returns></returns>
        public string UpLoadCurrentStudentData(string[] fsp, SQLiteHelper helper, int currentRoundCount)
        {
            return GradeManager.Instance.UpLoadStudentThreadFun(fsp, helper, currentRoundCount);
        }
        
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="projectName"></param>
        /// <param name="groupName"></param>
        /// <param name="studentListview"></param>
        /// <param name="projectID"></param>
        public void LoadingCurrentStudentData(SQLiteHelper helper, string projectName, string groupName, ListView studentListview,ref string projectID)
        {
            TreeViewHelper.Instance.LoadingCurrentChooseViewStudentData(helper, projectName, groupName, studentListview, ref projectID);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logRichTxt"></param>
        /// <param name="strLog"></param>
        /// <param name="nType"></param>
        public void WriteLog(LogRichTextBox  logRichTxt, string strLog, int nType)
        {
            try
            {
                logRichTxt.BeginInvoke(new ThreadStart((MethodInvoker)delegate ()
                {
                    if (logRichTxt.Lines.Length > 100)
                    {
                        logRichTxt.Clear();
                    }
                    if (nType == 0)
                    {
                        logRichTxt.AppendTextEx(strLog, Color.Indigo);
                    }
                    else if (nType == 2)
                    {
                        logRichTxt.AppendTextEx(strLog, Color.Blue);
                    }
                    else if (nType == 1)
                    {
                        logRichTxt.AppendTextEx(strLog, Color.Red);
                    }
                    else if (nType == 3)
                    {
                        logRichTxt.AppendTextEx(strLog, Color.DarkGreen);
                    }

                    logRichTxt.Select(logRichTxt.TextLength, 0);
                    logRichTxt.ScrollToCaret();
                }));
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="portNameSearch"></param>
        /// <param name="portName"></param>
        /// <returns></returns>
        public string[] GetPortDeviceName(ComboBox portNameSearch, string portName = null)
        {
            if (string.IsNullOrEmpty(portName)) portName = portNameSearch.Text;
            List<string> strs = new List<string>();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_PnPEntity where Name like '%(COM%'"))
            {
                var hardInfos = searcher.Get();
                foreach (var hardInfo in hardInfos)
                {
                    if (hardInfo.Properties["Name"].Value != null)
                    {
                        string deviceName = hardInfo.Properties["Name"].Value.ToString();
                        if (deviceName.Contains(portName))
                        {
                            int a = deviceName.IndexOf("(COM") + 1;//a会等于1
                            string str = deviceName.Substring(a, deviceName.Length - a);
                            a = str.IndexOf(")");//a会等于1
                            str = str.Substring(0, a);
                            strs.Add(str);
                        }
                    }
                }
            }
            return strs.ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="vid"></param>
        public List<string> GetScreenCom(string pid, string vid)
        {
             List<string> coms = new List<string>();
             string[] available_spectrometers = SerialPort.GetPortNames();
             ManagementObjectCollection.ManagementObjectEnumerator enumerator = null;
             string commData = string.Empty;
             ManagementObjectSearcher mObjs = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM WIN32_PnPEntity");
             try
             {
                 enumerator = mObjs.Get().GetEnumerator();
                 while (enumerator.MoveNext())
                 {
                     ManagementObject current = (ManagementObject)enumerator.Current;
                     if (current["Caption"] != null)
                     {
                         string str1 = current["Caption"].ToString(); // "(COM173"
                         if (str1.Contains("(COM"))
                         {
                             string strVid = "0";
                             string strPid = "0";

                             string[] str1g = current["DeviceID"].ToString().Split('\\');
                             foreach (string str2 in str1g)
                             {
                                 if ((str2.Contains("VID_")) || (str2.Contains("PID_")))
                                 {
                                     string[] strbG = str2.Split('&');
                                     foreach (string strb in strbG)
                                     {
                                         if (strb.Contains("VID_"))
                                         {
                                             strVid = strb.Replace("VID_", "");
                                             continue;
                                         }

                                         if (strb.Contains("PID_"))
                                         {
                                             strPid = strb.Replace("PID_", "");
                                             continue;
                                         }
                                     }
                                 }
                             }

                             Debug.WriteLine(current["DeviceID"]);
                             if (strPid == pid && strVid == vid)
                             {
                                 commData = current["Caption"] + "";
                                 commData = SerialTool.PortDeviceName2PortName(commData);
                                 coms.Add(commData);
                             }
                         }
                     }
                 }
             }
             catch (Exception exception)
             {
                 LoggerHelper.Debug(exception);
             }
             finally
             {
                 if (enumerator != null)
                 {
                     enumerator.Dispose();
                 }
             }
             return coms;//返回串口号
             
        }

        public void SendScoreToSerial(SerialPortHelper SerialPortHelper, string name, string score, string txt3 = "米")
        {
            if (SerialPortHelper==null||!SerialPortHelper.IsComOpen()) return;
            try
            {
                int c1 = 0;//红
                int c2 = 1;//绿
                int c3 = 2;//蓝
                if (txt3 == "米")
                {
                    txt3 = "M";
                }
                else if (txt3 == "厘米")
                {
                    txt3 = "CM";
                }
                byte[] result = SerialTool.PushText_BL2(name, c1, score, c2, txt3, c3);
                Task.Run(() =>
                {
                    for (int i = 0; i < 3; i++)
                    {
                        SerialPortHelper.SendMessage(result);
                        Thread.Sleep(300);
                    }
                });
            }
            catch(Exception exception)
            {
                LoggerHelper.Debug(exception);
                return;
            }

        }

        
    }

}
