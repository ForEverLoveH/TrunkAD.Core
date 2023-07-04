using HZH_Controls.Controls;
using HZH_Controls.Forms;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using TrunkAD.Core.GameSystem.GameHelper;
using TrunkAD.Core.GameSystem.GameHelper.TreeViewHelper;
using TrunkAD.Core.GameSystem.GameModel;
using TrunkAD.Core.GameSystem.GameWindow;

namespace TrunkAD.Core.GameSystem.GameWindowSys
{
    public class ImportStudentDataWindowSys
    {
        public static ImportStudentDataWindowSys Instance { get; private set; }
        public ImportStudentWindow ImportStudentDataWindow = null;
        
        public void Awake()
        {
            Instance = this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool ShowImportStudentDataWindow(SQLiteHelper helper)
        {
            ImportStudentDataWindow = new ImportStudentWindow();
            ImportStudentDataWindow.helper = helper ;
            if (ImportStudentDataWindow.ShowDialog() == System.Windows.Forms.DialogResult.OK) 
            { 
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uiTreeView1"></param>
        public void LoadingProjectView(UITreeView uiTreeView1, SQLiteHelper helper)
        {
            TreeViewHelper.Instance.UpdataTreeview(uiTreeView1,helper);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="projectID"></param>
        /// <param name="txt_Type"></param>
        /// <param name="txt_RoundCount"></param>
        /// <param name="txt_TestMethod"></param>
        /// <param name="txt_projectName"></param>
        /// <param name="txt_BestScoreMode"></param>
        /// <param name="txt_FloatType"></param>
        public void LoadingProjectAttribute(SQLiteHelper helper,string projectName, ref string projectID, UIComboBox txt_Type, UIComboBox txt_RoundCount, UIComboBox txt_TestMethod, UITextBox txt_projectName, UIComboBox txt_BestScoreMode, UIComboBox txt_FloatType)
        {
            var ds = helper.ExecuteReader("SELECT spi.Name,spi.Type,spi.RoundCount,spi.BestScoreMode,spi.TestMethod,spi.FloatType,spi.Id " +
             $"FROM SportProjectInfos AS spi WHERE spi.Name='{projectName}'");

            while (ds.Read())
            {
                string Name = ds.GetString(0);
                int Type = ds.GetInt16(1);
                int RoundCount = ds.GetInt16(2);
                int BestScoreMode = ds.GetInt16(3);
                int TestMethod = ds.GetInt16(4);
                int FloatType = ds.GetInt16(5);
                projectID = ds.GetValue(6).ToString();
                txt_projectName.Text = Name;
                txt_Type.SelectedIndex = 0;
                txt_RoundCount.SelectedIndex = RoundCount;
                txt_BestScoreMode.SelectedIndex = BestScoreMode;
                txt_TestMethod.SelectedIndex = TestMethod;
                txt_FloatType.SelectedIndex = FloatType;

                break;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectID"></param>
        /// <param name="groupName"></param>
        /// <param name="ucDataGridView1"></param>
        public void LoadingProjectStudentData(SQLiteHelper helper,string projectID, string groupName, UCDataGridView ucDataGridView1)
        {
            try
            {
                ucDataGridView1.DataSource = null;
                if (ucDataGridView1.Rows!=null&&ucDataGridView1.Rows.Count > 0)
                {
                    ucDataGridView1.Rows.Clear();
                }
                List<DataGridViewColumnEntity> lstCulumns = new List<DataGridViewColumnEntity>();
                lstCulumns.Add(new DataGridViewColumnEntity() { DataField = "ID", HeadText = "序号", Width = 5, WidthType = SizeType.AutoSize });
                lstCulumns.Add(new DataGridViewColumnEntity() { DataField = "GroupName", HeadText = "组别名称", Width = 20, WidthType = SizeType.AutoSize });
                lstCulumns.Add(new DataGridViewColumnEntity() { DataField = "School", HeadText = "学校", Width = 20, WidthType = SizeType.AutoSize });
                lstCulumns.Add(new DataGridViewColumnEntity() { DataField = "Grade", HeadText = "年级", Width = 5, WidthType = SizeType.AutoSize });
                lstCulumns.Add(new DataGridViewColumnEntity() { DataField = "Class", HeadText = "班级", Width = 5, WidthType = SizeType.AutoSize });
                lstCulumns.Add(new DataGridViewColumnEntity() { DataField = "Name", HeadText = "姓名", Width = 20, WidthType = SizeType.AutoSize });
                lstCulumns.Add(new DataGridViewColumnEntity() { DataField = "Sex", HeadText = "性别", Width = 5, WidthType = SizeType.AutoSize, Format = (a) => { return ((int)a) == 0 ? "男" : "女"; } });
                lstCulumns.Add(new DataGridViewColumnEntity() { DataField = "IdNumber", HeadText = "准考证号", Width = 20, WidthType = SizeType.AutoSize });
                ucDataGridView1.Columns = lstCulumns;
                ucDataGridView1.IsShowCheckBox = true;
                List<object> lstSource = new List<object>();

                var ds = helper.ExecuteReaderList($"SELECT d.GroupName,d.SchoolName,d.GradeName,d.ClassNumber,d.Name,d.Sex,d.IdNumber " +
                     $"FROM DbPersonInfos AS d WHERE d.GroupName='{groupName}' AND d.ProjectId='{projectID}'");
                int i = 1;
                foreach (var item in ds)
                {
                    DataGridViewModel model = new DataGridViewModel()
                    {
                        ID = i.ToString(),
                        GroupName = item["GroupName"],
                        School = item["SchoolName"],
                        Grade = item["GradeName"],
                        Class = item["ClassNumber"],
                        Name = item["Name"],
                        Sex = Convert.ToInt32(item["Sex"]),
                        IdNumber = item["IdNumber"],

                    };
                    lstSource.Add(model);
                    i++;
                }
                ucDataGridView1.DataSource = lstSource;
                ucDataGridView1.ReloadSource();
            }catch(Exception ex)
            {
                LoggerHelper.Debug(ex);
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ucDataGridView1"></param>
        /// <param name="projectName"></param>
        public void DeleteCurrentChooseStudent(SQLiteHelper helper,UCDataGridView ucDataGridView1, string projectName)
        {
            try
            {
                int count = ucDataGridView1.SelectRows.Count;
                if (count > 0)
                {
                    var value = helper.ExecuteScalar($"SELECT Id FROM SportProjectInfos WHERE Name='{projectName}'");
                    string projectId = value.ToString();
                    if (!string.IsNullOrEmpty(projectId))
                    {
                        for (int i = 0; i < count; i++)
                        {
                            DataGridViewModel osoure = (DataGridViewModel)ucDataGridView1.SelectRows[i].DataSource;
                            var vpersonId = helper.ExecuteScalar($"SELECT  Id FROM DbPersonInfos WHERE ProjectId='{projectId}' and Name='{osoure.Name}' and IdNumber='{osoure.IdNumber}'");
                            //删除人
                            helper.ExecuteNonQuery($"DELETE FROM DbPersonInfos WHERE Id='{vpersonId}'");
                            //删除成绩
                            helper.ExecuteNonQuery($"DELETE FROM ResultInfos WHERE PersonId='{vpersonId}'");

                        }

                    }

                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return ;
            }
        }

        public void DeleteAllGroup(SQLiteHelper helper, string txt, string projectName)
        {
            try
            {
                var value = helper.ExecuteScalar($"SELECT Id FROM SportProjectInfos WHERE Name='{projectName}'");
                string projectId = value.ToString();
                if (!string.IsNullOrEmpty(projectId))
                {
                    //删除组
                    helper.ExecuteNonQuery($"DELETE FROM DbGroupInfos WHERE ProjectId='{projectId}' and Name='{txt}'");
                    var ds = helper.ExecuteReader($"SELECT Id FROM DbPersonInfos WHERE ProjectId='{projectId}' AND GroupName='{txt}'");
                    while (ds.Read())
                    {
                        var vpersonId = ds.GetValue(0).ToString(); ;
                        //删除成绩
                       helper.ExecuteNonQuery($"DELETE FROM ResultInfos WHERE PersonId='{vpersonId}'");
                    }
                    //删除人
                    var res =  helper.ExecuteNonQuery($"DELETE FROM DbPersonInfos WHERE ProjectId='{projectId}' AND GroupName='{txt}'");  
                }
               
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name0"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="roundCount"></param>
        /// <param name="bestScoreMode"></param>
        /// <param name="testMethod"></param>
        /// <param name="floatType"></param>
        /// <returns></returns>
        public bool SaveDataIntoDataBase(SQLiteHelper helper,  string name0, string name, int type, int roundCount, int bestScoreMode, int testMethod, int floatType)
        {
            try
            {
                string projectID = helper.ExecuteScalar($"select Id from SportProjectInfos where Name='{name0}'").ToString();
                string sql = $"UPDATE SportProjectInfos SET Name='{name}', Type={type},RoundCount={roundCount},BestScoreMode={bestScoreMode},TestMethod={testMethod},FloatType={floatType} where Id='{projectID}'";
                int result =helper.ExecuteNonQuery(sql);
                if(result==1)
                    return true;
                else
                    return false;
            }
            catch(Exception e)
            {
                LoggerHelper.Debug(e);
                return false;
            }
        }

        public bool ShowImportStudentWindow(string projectName, SQLiteHelper helper)
        {
            return ImportDataWindowSys.Instance.ShowImportStudentWindow(  helper,projectName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="newProjectName"></param>
        /// <param name="helper"></param>
        public void CreateProject(string newProjectName, SQLiteHelper helper)
        {
            string sql = $"select Id from SportProjectInfos where Name='{newProjectName}' LIMIT 1";

            var existProject = helper.ExecuteScalar(sql);


            int si = 1;
            var ds = helper.ExecuteScalar($"SELECT MAX(SortId) + 1 FROM SportProjectInfos").ToString();
            int.TryParse(ds, out si);

            if (existProject != null)
            {
               UIMessageBox.ShowError( $"项目:{newProjectName}已存在");
            }
            else
            {
                sql = $"INSERT INTO SportProjectInfos (CreateTime, SortId, IsRemoved, Name, Type, RoundCount, BestScoreMode, TestMethod, FloatType ) " +
                    $"VALUES(datetime(CURRENT_TIMESTAMP, 'localtime'),{si}," +
                    $"0,'{newProjectName}',0,2,0,0,2)";
                int result = helper.ExecuteNonQuery(sql);
                if (result == 1)
                {
                    UIMessageBox.ShowSuccess($"添加成功");
                }
                else
                {
                    UIMessageBox.ShowError("添加失败！！");
                }
            }
        }
    }
}
