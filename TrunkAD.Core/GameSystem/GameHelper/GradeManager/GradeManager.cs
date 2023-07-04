using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Newtonsoft.Json;
using Spire.Xls;
using TrunkAD.Core.GameSystem.GameModel;
using TrunkAD.Core.GameSystem.GameWindowSys;

namespace TrunkAD.Core.GameSystem.GameHelper 
{
    public class GradeManager
    {
        public static GradeManager Instance { get; set; }

        public void Awake()
        {
            Instance = this;
        }
        /// <summary>
        /// 多线程上传成绩
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="helper"></param>
        /// <returns></returns>
        public string UpLoadStudentThreadFun(object obj, SQLiteHelper helper)
        {
            return null; 
        }
        /// <summary>
        /// 上传成绩通过学生名字
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="num"></param>
        /// <param name="helper"></param>
        /// <returns></returns>
        public string UpLoadStudentByName(string projectName, int num, SQLiteHelper helper)
        {
            return null;
        }
        /// <summary>
        /// 获取导出当前组的学生信息
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="groupName"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public List<ExportDataModel> GetExportCurrentGroupGradeData(SQLiteHelper helper, string groupName, string projectId)
        {
             List<ExportDataModel> exportDataModels = new List<ExportDataModel>();
            try
            {

                var dis = helper.ExecuteReaderOne(
                    $"SELECT RoundCount,Name,Type FROM SportProjectInfos WHERE Id='{projectId}' ");
                if (dis.Count > 0)
                {
                    string name = dis["Name"];
                    string roundCount = dis["RoundCount"];
                    string sql =
                        $"SELECT BeginTime,schoolname, Id, GroupName, Name, IdNumber,State,Sex FROM DbPersonInfos WHERE ProjectId='{projectId}' and groupNAME='{groupName}'";
                    var sl = helper.ExecuteReader(sql);
                    while (sl.Read())
                    {
                        ExportDataModel exportDataModel = new ExportDataModel();
                        exportDataModel.projectName = name;
                        exportDataModel.schoolName = sl.GetValue(1).ToString();
                        exportDataModel.groupName = groupName;
                        exportDataModel.id = sl.GetInt32(2).ToString();
                        exportDataModel.name = sl.GetValue(4).ToString();
                        exportDataModel.ExamIDNumber = sl.GetValue(5).ToString();
                        exportDataModel.sex = sl.GetInt32(7) == 0 ? "男" : "女";
                        var po = helper.ExecuteReaderList(
                            $"SELECT SortId,RoundId,Result,State,CreateTime,uploadState FROM ResultInfos WHERE personid='{exportDataModel.id}' ");
                        if (po.Count == 0)
                        {
                            exportDataModel.FristRound = "未测试";
                            exportDataModel.TwoRound = "未测试";
                           // exportDataModel.FinnalGrade = "未测试";
                        }
                        else
                        {
                            foreach (var dic in po)
                            {
                                string frist = dic["RoundId"];
                                if (frist == "1")
                                {
                                    string res =  dic["Result"];
                                    
                                    if (res != 0.ToString())
                                        exportDataModel.FristRound = res;
                                    else
                                    {
                                        string state = dic["State"];
                                        string states = ResultState.ResultState2Str(int.Parse(state));
                                        exportDataModel.FristRound = states;
                                    }
                                }

                                if (frist == "2")
                                {
                                    string res = dic["Result"] ;
                                    if (res != 0.ToString())
                                        exportDataModel.TwoRound = res;
                                    else
                                    {
                                        string state = dic["State"];
                                        string states = ResultState.ResultState2Str(int.Parse(state));
                                        exportDataModel.TwoRound = states;
                                    }
                                }
                            }
                        }
                        exportDataModel.Remark = " ";
                        exportDataModels.Add(exportDataModel);
                    }
                }
                else
                {
                    return null;
                }
            }
            catch(Exception e)
            {
                LoggerHelper.Debug(e);
                return null;
            }

            return exportDataModels;
        }
        /// <summary>
        /// 打印当前组的成绩
        /// </summary>
        /// <param name="path"></param>
        public void PrintCurrentGroupGrade(string path)
        {
            try
            {
                Workbook workbook = new Workbook();
                workbook.LoadFromFile(path);
                Worksheet worksheet = workbook.Worksheets[0];
                worksheet =  AutoResizeColumnWidth(worksheet, 10);
                worksheet.PageSetup.PaperSize = PaperSizeType.PaperA4;
                worksheet.PageSetup.Orientation = PageOrientationType.Landscape;
                PrintDialog printDialog = new PrintDialog();
                printDialog.AllowPrintToFile = true;
                printDialog.AllowCurrentPage = true;
                printDialog.AllowSomePages = true;
                printDialog.PrinterSettings.Duplex = Duplex.Simplex;
                printDialog.PrinterSettings.Copies = 1;
                workbook.PrintDialog = printDialog;
                PrintDocument pd = workbook.PrintDocument;
                pd.DefaultPageSettings.Landscape = true;
                PrintController printController=new StandardPrintController();
                pd.PrintController = printController;
                pd.Print();
            }
            catch (Exception e)
            {
                LoggerHelper.Debug(e);
                 return;
            }
            
        }
        
         /// <summary>
        /// Excel的页面自动列宽处理
        /// </summary>
        /// <param name="sheet">目标Sheet</param>
        /// <param name="nPageWidthChars">Sheet页面设置后每行可容纳字符总宽(需自行测试后确认)</param>
        /// <param name="nRowStart">要自动处理的起始行号</param>
        /// <param name="nColIgnore">要忽略的重分配的列集合</param>
        /// <returns></returns>
        private Worksheet AutoResizeColumnWidth(Worksheet sheet, int nPageWidthChars, int nRowStart = 1, int[] nColIgnore = null)
        {
            //单、双字节列宽自适应(不包括隐藏列)处理
            int allWidth = 0; //总列宽
            int nVisibleColumns = 0; //可见列数
            for (int i = 1; i <= sheet.LastColumn; i++)
            {
                if (sheet.Range[nRowStart, i].ColumnWidth == 0) //隐藏列(宽度为0)
                    continue;
                nVisibleColumns++; //可见列计数
                int columnWidth = 0; //初始化列宽
                for (int j = nRowStart; j <= sheet.LastRow; j++)
                {
                    var cell = sheet.Range[j, i];
                    if (!string.IsNullOrWhiteSpace(cell.DisplayedText))
                    {
                        int length = Encoding.Default.GetBytes(cell.DisplayedText).Length; //单双字节
                        if (columnWidth < length)
                            columnWidth = length;
                    }
                }

                sheet.SetColumnWidth(i, columnWidth);
                allWidth += columnWidth;
            }
            //页宽多余宽度重新分配
            int nAddWidth = (nPageWidthChars - allWidth) /
                            (nVisibleColumns - ((nColIgnore != null) ? nColIgnore.Length : 0));
            for (int i = 1; (i <= sheet.LastColumn && nAddWidth >= 1); i++)
            {
                if (sheet.Range[nRowStart, i].ColumnWidth == 0) //若表格中的列为隐藏列(宽度为0)
                    continue;
                if (nColIgnore != null && ((IList)nColIgnore).Contains(i))
                    continue;
                sheet.SetColumnWidth(i, sheet.GetColumnWidth(i) + nAddWidth);
            }

            return sheet;

        }
        /// <summary>
        /// 重置考生成绩
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="projectId"></param>
        /// <param name="name"></param>
        /// <param name="groupName"></param>
        /// <param name="idNumber"></param>
        /// <param name="currentRoundCount"></param>
        /// <exception cref="NotImplementedException"></exception>
         public bool ResettingCurrentGrade(SQLiteHelper helper, string projectId, string name, string groupName, string idNumber, int currentRoundCount)
         {
             try
             {
                 string sql = $"SELECT dpi.id ,dpi.state,dpi.finalscore" + $" FROM DbPersonInfos as dpi WHERE dpi.GroupName='{groupName}' AND dpi.ProjectId='{projectId}' and dpi.name='{name}' and dpi.idnumber='{idNumber}'";
                 var sl = helper.ExecuteReader(sql);
                 while (sl.Read())
                 {
                     int id = sl.GetInt32(0);
                     int state = sl.GetInt32(1);
                     if (state==1)
                     {
                         sql = $"update  DbPersonInfos  set state='{0}' where ID='{id}' AND  ProjectId='{projectId}' and name='{name}' and idnumber='{idNumber}'";
                        int index =   helper.ExecuteNonQuery(sql);
                        if (index > 0)
                        {
                            var res = helper.ExecuteReaderList($"SELECT id, SortId,RoundId,Result,State,CreateTime,uploadState FROM ResultInfos WHERE personid='{id}'");
                            if (res.Count > 0)
                            {
                                foreach (var item in res)
                                {
                                    if (item["RoundId"] == currentRoundCount.ToString())
                                    {
                                        string ids = item["Id"]; 
                                        var pi= helper.ExecuteNonQuery($"delete from ResultInfos   where Id='{ids}' and RoundId='{item["RoundId"]}'");
                                        Console.WriteLine(pi);   
                                    }
                                }
                                return true;
                            }
                        }
                        else
                        {
                            continue;
                        }
                     }
                     else if(state==0)
                     {
                         var res = helper.ExecuteReaderList($"SELECT id, SortId,RoundId,Result,State,CreateTime,uploadState FROM ResultInfos WHERE personid='{id}'");
                         if (res.Count > 0)
                         {
                             foreach (var item in res)
                             {
                                 if (item["RoundId"] == currentRoundCount.ToString())
                                 {
                                     string ids = item["Id"]; 
                                     var pi= helper.ExecuteNonQuery($"delete from ResultInfos   where Id='{ids}' and RoundId='{item["RoundId"]}'");
                                     Console.WriteLine(pi);   
                                 }
                             }
                         }
                     }
                 }
                 return false;

                  
             }
             catch (Exception e)
             {
                LoggerHelper.Debug(e);
                return false;
             }
         }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="currentChooseStudentTestingData"></param>
        /// <param name="currentRoundCount"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public bool SettingCurrentStudentTestingState(SQLiteHelper helper, StudentTestingData currentChooseStudentTestingData, int currentRoundCount, string projectId,string states)
        {
            try
            {
                string sql = $"SELECT dpi.id ,dpi.state,dpi.finalscore" + $" FROM DbPersonInfos as dpi WHERE dpi.GroupName='{currentChooseStudentTestingData.groupName}' AND dpi.ProjectId='{projectId}' and dpi.name='{currentChooseStudentTestingData.name}' and dpi.idnumber='{currentChooseStudentTestingData.idNumber}'";
                var sl = helper.ExecuteReader(sql);
                while (sl.Read())
                {
                    int id = sl.GetInt32(0);
                    int state = sl.GetInt32(1);
                    if (state==1)
                    {
                        return false;
                    }
                    else if (state == 0)
                    {
                        var res = helper.ExecuteReaderList(
                            $"SELECT id, SortId,RoundId,Result,State,CreateTime,uploadState FROM ResultInfos WHERE personid='{id}'");
                        if (res.Count > 0)
                        {
                            foreach (var item in res)
                            {
                                if (item["RoundId"] == currentRoundCount.ToString())
                                {
                                    return false;
                                }
                                else
                                {
                                    int sta = ResultState.ResultState2Int(states);
                                    var sortid = helper.ExecuteScalar($"SELECT MAX(SortId) + 1 FROM ResultInfos");
                                    string sortid0 = "1";
                                    if (sortid != null && sortid.ToString() != "") sortid0 = sortid.ToString();
                                     sql = $"INSERT INTO ResultInfos(CreateTime,SortId,IsRemoved,PersonId,SportItemType,PersonName,PersonIdNumber,RoundId,Result,State) " +
                                                 $"VALUES('{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', {sortid0}, 0, '{id}',0,'{currentChooseStudentTestingData.name}'," +
                                                 $"'{currentChooseStudentTestingData.idNumber}',{currentRoundCount},{0},{sta})";
                                        
                                    var po= helper.ExecuteNonQuery(sql);
                                    Console.WriteLine(po);
                                }
                            }
                        }
                        else
                        {
                            
                            int sta = ResultState.ResultState2Int(states);
                            var sortid = helper.ExecuteScalar($"SELECT MAX(SortId) + 1 FROM ResultInfos");
                            string sortid0 = "1";
                            if (sortid != null && sortid.ToString() != "") sortid0 = sortid.ToString();
                            sql =
                                $"INSERT INTO ResultInfos(CreateTime,SortId,IsRemoved,PersonId,SportItemType,PersonName,PersonIdNumber,RoundId,Result,State) " +
                                $"VALUES('{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', {sortid0}, 0, '{id}',0,'{currentChooseStudentTestingData.name}'," +
                                $"'{currentChooseStudentTestingData.idNumber}',{currentRoundCount},0,{sta})";
                            int pl = helper.ExecuteNonQuery(sql);
                            Console.WriteLine(pl);
                        }

                        return true;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                LoggerHelper.Debug(e);
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fsp"></param>
        /// <param name="helper"></param>
        /// <param name="currentRoundCount"></param>
        /// <returns></returns>
        public string UpLoadStudentThreadFun(object obj, SQLiteHelper helper, int currentRoundCount)
        {
            return null;

        }
    }
}