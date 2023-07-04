using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using TrunkAD.Core.GameSystem.GameHelper;
using TrunkAD.Core.GameSystem.GameHelper.TreeViewHelper;
using TrunkAD.Core.GameSystem.GameModel;
using TrunkAD.Core.GameSystem.GameWindow;

namespace TrunkAD.Core.GameSystem.GameWindowSys
{
    public class MainWindowSys
    {
        public static MainWindowSys Instance { get; private set; }
        private SQLiteHelper helper = new SQLiteHelper();
        public void Awake()
        {
            Instance = this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uiTreeView1"></param>


        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="groupName"></param>
        /// <param name="listView1"></param>
        public void LoadingCurrentChooseViewStudentData(string projectName, string groupName, System.Windows.Forms.ListView listView1, ref string projectID)
        {
            TreeViewHelper.Instance.LoadingCurrentChooseViewStudentData(helper, projectName, groupName, listView1, ref projectID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool InitSQLiteDataBase()
        {
            return helper.InitDataBase();
        }

        public bool ExportSQLiteDataBase()
        {
            return helper.BackDataBase();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool ShowImportStudentDataWindow()
        {
            return ImportStudentDataWindowSys.Instance.ShowImportStudentDataWindow(helper);
        }

        public void ShowPlatFormWindow()
        {
            EquipMentSettingWindowSys.Instance.ShowPlatFormSettingWindow(helper);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uiTreeView1"></param>
        public void UpdataTreeview(UITreeView uiTreeView1)
        {
            TreeViewHelper.Instance.UpdataTreeview(uiTreeView1, helper);
        }

        public bool VerficationPassword(string acc, string pass)
        {
            try
            {
                string sql = $"select user password FROM FixGradeTable where User ='{acc}' and password= '{pass}'";
                var res = helper.ExecuteReaderList(sql);
                if (res.Count > 0)
                {
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return false;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="groupName"></param>
        /// <param name="name"></param>
        /// <param name="idNumber"></param>
        /// <param name="status"></param>
        /// <param name="projectID"></param>
        /// <returns></returns>
        public bool ModifyCurrentGrade(string projectName, string groupName, string name, string idNumber, string status, string projectID)
        {
            try
            {
                int rountid = 0;
                 //查询项目数据信息
                Dictionary<string, string> SportProjectDic = helper.ExecuteReaderOne($"SELECT Id,Type,RoundCount,BestScoreMode,TestMethod," +
                         $"FloatType,TurnsNumber0,TurnsNumber1 FROM SportProjectInfos WHERE Name='{projectName}'");
                int FloatType = 0;
                if (SportProjectDic.Count > 0)
                {
                    FloatType = Convert.ToInt32(SportProjectDic["FloatType"]);
                    rountid = Convert.ToInt32(SportProjectDic["RoundCount"]);
                }
                FixGradeWindow frm = new  FixGradeWindow();
                frm.projectName = projectName;
                frm.groupName = groupName;
                frm.Name = name;
                frm.IdNumber = idNumber;
                frm.status = status;
                frm.rountid = rountid;
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    int roundid = frm.updaterountId;
                    double updateScore = frm.updateScore;
                    decimal.Round(decimal.Parse(updateScore.ToString("0.0000")), FloatType).ToString();

                    string updatestatus = frm.status;
                    int Resultinfo_State = ResultState.ResultState2Int(updatestatus);
                    string perid = "";
                    var ds0 = helper.ExecuteReaderOne($"SELECT Id FROM DbPersonInfos WHERE IdNumber='{idNumber}' and ProjectId='{projectID}'");
                    if (ds0 == null || ds0.Count == 0) return false;
                    perid = ds0["Id"];

                    string sql = $"UPDATE ResultInfos SET Result={updateScore},State={Resultinfo_State} WHERE PersonId='{perid}' AND RoundId={roundid}";
                    int result = helper.ExecuteNonQuery(sql);
                    if (result == 0)
                    {
                        if (string.IsNullOrEmpty(perid))
                        {
                            return false;
                        }
                        sql = $"INSERT INTO ResultInfos(CreateTime,SortId,IsRemoved,PersonId,SportItemType,PersonName,PersonIdNumber,RoundId,Result,State) " +
                                    $"VALUES (datetime(CURRENT_TIMESTAMP, 'localtime') ,(SELECT MAX(SortId)+1 FROM ResultInfos),0," +
                                     $"'{perid}',0,'{name}','{idNumber}',{rountid},{updateScore},{Resultinfo_State})";
                        int result0 = helper.ExecuteNonQuery(sql);
                    }
                    else if (result > 1)
                    {
                        return false;
                    }
                    
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return false;
            }
            
           
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectID"></param>
        /// <param name="projectName"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public bool ExportCurrentGrade(string projectID, string projectName, string groupName)
        {
            if (!string.IsNullOrEmpty(projectID) && !string.IsNullOrEmpty(projectName))
            {
                try
                {
                    ExportGradeWindow oesf = new ExportGradeWindow();
                    oesf.helper = helper;
                    oesf._projectId = projectID;
                    oesf._groupName = groupName;
                    oesf._projectName = projectName;
                    oesf.ShowDialog();
                }
                catch (Exception ex)
                {
                    LoggerHelper.Debug(ex);
                    return false;
                }
                return true;
            }
            else
            {
                UIMessageBox.ShowWarning("请先确定项目信息！！");
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="personIdNumber"></param>
        /// <returns></returns>
        public bool ClearCurrentGrade(string name, string personIdNumber)
        {
            try
            {
                string sql = $"DELETE FROM ResultInfos WHERE PersonIdNumber = '{personIdNumber}'";
                int result = helper.ExecuteNonQuery(sql);
                sql = $"update DbPersonInfos SET State=0 where IdNumber='{personIdNumber}'";
                int result1 = helper.ExecuteNonQuery(sql);
                if (result1 == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return false;
            }

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fsp"></param>
        public bool StartTesting(string[] fsp)
        {
            List<Dictionary<string, string>> list = helper.ExecuteReaderList($"SELECT Id,Type,RoundCount,BestScoreMode,TestMethod," +
                         $"FloatType,TurnsNumber0,TurnsNumber1 FROM SportProjectInfos WHERE Name='{fsp[0]}'");
            if (list.Count == 1)
            {
                return RunningTestingWindowSys.Instance.ShowRunningTestWindow(helper, fsp,list);
            }
            else
            {
                return false;
            }
        }
    }
    
}
