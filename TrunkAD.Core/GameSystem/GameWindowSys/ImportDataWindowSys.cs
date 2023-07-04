using HZH_Controls;
using MiniExcelLibs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrunkAD.Core.GameSystem.GameHelper;
using TrunkAD.Core.GameSystem.GameModel;
using TrunkAD.Core.GameSystem.GameWindow;

namespace TrunkAD.Core.GameSystem.GameWindowSys
{
    public class ImportDataWindowSys
    {
        public static ImportDataWindowSys Instance { get; set; }
        ImportDataWindow ImportDataWindow = null;
        public void Awake()
        {
            Instance = this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public bool ShowImportStudentWindow(SQLiteHelper sQLiteHelper,string projectName)
        {
            ImportDataWindow = new ImportDataWindow();
            ImportDataWindow.projectName = projectName;
            ImportDataWindow .sQLiteHelper= sQLiteHelper;
            if (ImportDataWindow.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return true;
            }
            else { return false; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        public bool InitDataBase(SQLiteHelper helper)
        {
            return helper.InitDataBase();
        }

        public bool BackDataBase(SQLiteHelper helper)
        {
            return helper.BackDataBase( );
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string OpenLocalExcelFile()
        {
            string path = string.Empty;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;      //该值确定是否可以选择多个文件
            dialog.Title = "请选择文件";     //弹窗的标题
            dialog.InitialDirectory = Application.StartupPath + "\\";    //默认打开的文件夹的位置
            dialog.Filter = "MicroSoft Excel文件(*.xlsx)|*.xlsx";       //筛选文件
            dialog.ShowHelp = true;     //是否显示“帮助”按钮
            dialog.RestoreDirectory = true;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path = dialog.FileName;
            }
            return path;
        }

        public bool LoadingCurrentImportStudentData(object obj, string projectName, SQLiteHelper helper, ref int proVal, ref int proMax)
        {
            bool IsResult = false;
            try
            {
                string path = obj as string;
                if (!string.IsNullOrEmpty(path))
                {

                    string projectid = helper.ExecuteScalar($"select Id from SportProjectInfos where name='{projectName}'").ToString();
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
                    System.Data.SQLite.SQLiteTransaction sQLiteTransaction = helper.BeginTransaction();
                    for (int i = 0; i < rolesMarketList.Count; i++)
                    {
                        string role = rolesMarketList[i];
                        string[] roles = role.Split('#');
                        string GroupName = roles[0];
                        string examTime = roles[1];
                        string countstr = helper.ExecuteScalar($"SELECT COUNT(*) FROM DbGroupInfos where ProjectId='{projectid}' and Name='{GroupName}'").ToString();
                        int.TryParse(countstr, out int count);
                        if (count == 0)
                        {
                            string groupsortidstr = helper.ExecuteScalar("select MAX( SortId ) + 1 from DbGroupInfos").ToString();
                            int groupsortid = 1;
                            int.TryParse(groupsortidstr, out groupsortid);
                            /* string insertsql = $"INSERT INTO DbGroupInfos(CreateTime,SortId,IsRemoved,ProjectId,Name,IsAllTested) " +
                                 $"VALUES(datetime(CURRENT_TIMESTAMP, 'localtime'),{groupsortid},0,'{projectid}','{GroupName}',0)";*/
                            string insertsql = $"INSERT INTO DbGroupInfos(CreateTime,SortId,IsRemoved,ProjectId,Name,IsAllTested) " +
                              $"VALUES('{examTime}',{groupsortid},0,'{projectid}','{GroupName}',0)";
                            //插入组
                            helper.ExecuteNonQuery(insertsql);
                        }
                    }
                    helper.CommitTransaction(ref sQLiteTransaction);
                    int groupNum = (rows.Count + 5000) / 5000;
                    List<List<InputData>> rowsSpiltList = SpiltList(rows, groupNum);
                    foreach (var inputDatas in rowsSpiltList)
                    {
                        sQLiteTransaction = helper.BeginTransaction();
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
                            string countstr = helper.ExecuteScalar($"SELECT COUNT(*) FROM DbPersonInfos WHERE ProjectId='{projectid}' AND IdNumber='{PersonIdNumber}'").ToString();
                            int.TryParse(countstr, out int count);
                            if (count == 0)
                            {
                                int personsortid = 1;
                                string personsortidstr = helper.ExecuteScalar("select MAX( SortId ) + 1 from DbPersonInfos").ToString();
                                int.TryParse(personsortidstr, out personsortid);
                                string insertsql = $"INSERT INTO DbPersonInfos(CreateTime,SortId,IsRemoved,ProjectId,SchoolName,GradeName,ClassNumber,GroupName,Name,IdNumber,Sex,State,FinalScore,uploadState) " +
                                    $"VALUES('{examTime}',{personsortid},0,'{projectid}','{SchoolName}','{GradeName}','{classNumber}','{GroupName}'," +
                                    $"'{name}','{PersonIdNumber}',{Sex},0,-1,0)";
                                helper.ExecuteNonQuery(insertsql);
                            }
                            proVal++;
                        }
                        helper.CommitTransaction(ref sQLiteTransaction);
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
                return IsResult;

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
        /// <typeparam name="T"></typeparam>
        /// <param name="Lists"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public  List<List<T>> SpiltList<T>(List<T> Lists, int num) //where T:class
        {
            List<List<T>> fz = new List<List<T>>();
            //元素数量大于等于 分组数量
            if (Lists.Count >= num)
            {
                int avg = Lists.Count / num; //每组数量
                int vga = Lists.Count % num; //余数
                for (int i = 0; i < num; i++)
                {
                    List<T> cList = new List<T>();
                    if (i + 1 == num)
                    {
                        cList = Lists.Skip(avg * i).ToList<T>();
                    }
                    else
                    {
                        cList = Lists.Skip(avg * i).Take(avg).ToList<T>();
                    }
                    fz.Add(cList);
                }
            }
            else
            {
                fz.Add(Lists);//元素数量小于分组数量
            }
            return fz;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="localInfos"></param>
        /// <param name="localValues"></param>
        public void LoadingInitData(SQLiteHelper helper, ref List<Dictionary<string, string>> localInfos, ref Dictionary<string, string> localValues)
        {
            localInfos = new List<Dictionary<string, string>>();
            localInfos = helper.ExecuteReaderList("SELECT * FROM localInfos");
            if (localInfos.Count > 0)
            {
                localValues = new Dictionary<string, string>();
                foreach (var item in localInfos)
                {
                    localValues.Add(item["key"], item["value"]);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sQLiteHelper"></param>
        /// <returns></returns>
        public bool ShowEquipMentSettingWindow(SQLiteHelper sQLiteHelper)
        {
             return EquipMentSettingWindowSys.Instance.ShowPlatFormSettingWindow(sQLiteHelper);
        }
    }
}
