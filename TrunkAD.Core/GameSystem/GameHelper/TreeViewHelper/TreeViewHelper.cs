using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrunkAD.Core.GameSystem.GameHelper;
using TrunkAD.Core.GameSystem.GameModel;

namespace TrunkAD.Core.GameSystem.GameHelper.TreeViewHelper
{
    public class TreeViewHelper
    {
        public static TreeViewHelper Instance { get; set; }
        public void Awake()
        {
            Instance = this;
        }

        private  void AutoResizeColumnWidth(ListView lv)
        {
            int allWidth = lv.Width;
            int count = lv.Columns.Count;
            int MaxWidth = 0;
            Graphics graphics = lv.CreateGraphics();
            int width;
            lv.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            if (count == 0) return;
            for (int i = 0; i < count; i++)
            {
                string str = lv.Columns[i].Text;
                //MaxWidth = lv.Columns[i].Width;
                MaxWidth = 0;

                foreach (ListViewItem item in lv.Items)
                {
                    try
                    {
                        str = item.SubItems[i].Text;
                        width = (int)graphics.MeasureString(str, lv.Font).Width;
                        if (width > MaxWidth)
                        {
                            MaxWidth = width;
                        }
                    }
                    catch (Exception)
                    {

                        break;
                    }

                }

                lv.Columns[i].Width = MaxWidth;
                allWidth -= MaxWidth;
            }

            if (allWidth > count && count != 0)
            {
                allWidth /= count;
                for (int i = 0; i < count; i++)
                {
                    lv.Columns[i].Width += allWidth;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="groupName"></param>
        /// <param name="listView1"></param>
        public void LoadingCurrentChooseViewStudentData(SQLiteHelper helper,string projectName, string groupName, System.Windows.Forms.ListView listView1, ref string projectID)
        {
            try
            {
                listView1.Items.Clear();
                if (string.IsNullOrEmpty(groupName)) return;
                var ds = helper.ExecuteReader($"SELECT b.Id,b.RoundCount,b.FloatType,b.Type " +
                 $"FROM DbGroupInfos AS a,SportProjectInfos AS b WHERE a.ProjectId=b.Id AND a.Name='{groupName}' AND b.Name='{projectName}'");
                // 轮次
                int roundCount = 0;
                int FloatType = 0;
                int type0 = 0;
                while (ds.Read())
                {
                    projectID = ds.GetValue(0).ToString();
                    roundCount = ds.GetInt16(1); FloatType = ds.GetInt16(2);
                    type0 = ds.GetInt16(3);

                }
                ds = helper.ExecuteReader($"SELECT dpi.GroupName,dpi.Name,dpi.IdNumber,dpi.State,dpi.FinalScore,dpi.Id" +
                 $" FROM DbPersonInfos as dpi WHERE dpi.GroupName='{groupName}' AND dpi.ProjectId='{projectID}'");
                int i = 1;
                listView1.BeginUpdate();
                //初始化标题
                InitListViewHeader(roundCount,listView1);
                listView1.Items.Clear();
                while (ds.Read())
                {
                    string num = ds.GetString(2);
                    int state = ds.GetInt16(3);
                    string personid = ds.GetValue(5).ToString();
                    ListViewItem item = new ListViewItem();
                    item.UseItemStyleForSubItems = false;
                    item.Text = i.ToString();
                    item.SubItems.Add(projectName);
                    item.SubItems.Add(ds.GetString(0));
                    item.SubItems.Add(ds.GetString(1));
                    item.SubItems.Add(num);
                    if (state == 1)
                    {
                        item.SubItems.Add("已测试");
                        item.SubItems[item.SubItems.Count - 1].BackColor = Color.MediumSpringGreen;
                    }
                    else
                    {
                        item.SubItems.Add("未测试");

                    }
                    double maxscore = 1000;
                    var res = helper.ExecuteReaderList($"SELECT SortId,RoundId,Result,State,CreateTime,uploadState FROM ResultInfos WHERE personid='{personid}'");
                    int k = 0;
                    List<double> list = new List<double>();
                    foreach (var dic in res)
                    {
                        int.TryParse(dic["RoundId"], out int RoundId);
                        double.TryParse(dic["Result"], out double Result);
                        list.Add(Result);
                        string restr = ResultState.ResultState2Str(dic["State"]);
                        if (restr == "已测试")
                        {
                            if (maxscore > Result)
                            {
                                maxscore = Result;
                            }
                            restr = decimal.Round(decimal.Parse(Result.ToString("0.0000")), FloatType).ToString();
                            item.SubItems.Add(restr);

                        }
                        else
                        {
                            item.SubItems.Add(restr);
                            item.SubItems[item.SubItems.Count - 1].ForeColor = Color.Red;

                        }
                        item.SubItems[item.SubItems.Count - 1].Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);

                        if (dic["uploadState"] == "0")
                        {
                            item.SubItems.Add("未上传");
                            item.SubItems[item.SubItems.Count - 1].ForeColor = Color.Red;
                        }
                        else
                        {
                            item.SubItems.Add("已上传");
                            item.SubItems[item.SubItems.Count - 1].Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
                            item.SubItems[item.SubItems.Count - 1].ForeColor = Color.Green;
                        }
                        k++;
                    }
                    for (int j = k; j < roundCount; j++)
                    {
                        item.SubItems.Add("无成绩");
                        item.SubItems.Add("未上传");
                    }
                    if (maxscore != 1000)
                    {
                        if (list.Count > 0)
                        {
                            for (int j = 0; j < list.Count - 1; j++)
                            {
                                var s = (int)list[j];
                                var p = (int)list[j + 1];
                                if (s > p)
                                {
                                    maxscore = list[j];
                                }
                                else
                                {
                                    maxscore = list[j + 1];
                                }
                            }
                        }
                        item.SubItems.Add(decimal.Round(decimal.Parse(maxscore.ToString("0.0000")), FloatType).ToString());
                        item.SubItems[item.SubItems.Count - 1].Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
                    }
                    else
                    {
                        item.SubItems.Add("无成绩");
                    }
                    listView1.Items.Insert(listView1.Items.Count, item);
                    i++;
                }
                //自动列宽
                AutoResizeColumnWidth(listView1);
                listView1.EndUpdate();
            }
            catch(Exception ex)
            {
                LoggerHelper.Debug(ex);
                return;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="roundCount"></param>
        /// <param name="listView1"></param>
        private void InitListViewHeader(int roundCount, System.Windows.Forms.ListView listView1)
        {
            listView1.View = View.Details;
            ColumnHeader[] Header = new ColumnHeader[100];
            int sp = 0;
            Header[sp] = new ColumnHeader();
            Header[sp].Text = "序号";
            Header[sp].Width = 40;
            sp++;

            Header[sp] = new ColumnHeader();
            Header[sp].Text = "项目名称";
            Header[sp].Width = 80;
            sp++;

            Header[sp] = new ColumnHeader();
            Header[sp].Text = "组别名称";
            Header[sp].Width = 40;

            sp++;
            Header[sp] = new ColumnHeader();
            Header[sp].Text = "姓名";
            Header[sp].Width = 100;
            sp++;

            Header[sp] = new ColumnHeader();
            Header[sp].Text = "准考证号";
            Header[sp].Width = 100;
            sp++;
            Header[sp] = new ColumnHeader();
            Header[sp].Text = "考试状态";
            Header[sp].Width = 40;
            sp++;
            for (int i = 1; i <= roundCount; i++)
            {
                Header[sp] = new ColumnHeader();
                Header[sp].Text = $"第{i}轮";
                Header[sp].Width = 40;
                sp++;

                Header[sp] = new ColumnHeader();
                Header[sp].Text = $"上传状态";
                Header[sp].Width = 80;
                sp++;
            }

            Header[sp] = new ColumnHeader();
            Header[sp].Text = "最好成绩";
            Header[sp].Width = 60;
            sp++;

            ColumnHeader[] Header1 = new ColumnHeader[sp];
            listView1.Columns.Clear();
            for (int i = 0; i < Header1.Length; i++)
            {
                Header1[i] = Header[i];
            }
            listView1.Columns.AddRange(Header1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uiTreeView1"></param>

        public void UpdataTreeview(UITreeView uiTreeView1, SQLiteHelper helper)
        {
            uiTreeView1.Nodes.Clear();
            List<ProjectModel> projects = new List<ProjectModel>();
            var ds = helper.ExecuteReader("select id,name from  SportProjectInfos");
            while (ds.Read())
            {
                string projectID = ds.GetValue(0).ToString();
                string projectName = ds.GetString(1);
                var sl = helper.ExecuteReader($"SELECT Name,IsAllTested FROM DbGroupInfos WHERE ProjectId='{projectID}'");
                projects.Add(new ProjectModel { projectName = projectName, Groups = new List<GroupModel>() });
                while (sl.Read())
                {
                    string groupName = sl.GetString(0);
                    int allTested = sl.GetInt32(1);
                    ProjectModel project = projects.FirstOrDefault(a => a.projectName == projectName);
                    if (project != null)
                    {
                        project.Groups.Add(new GroupModel
                        {
                            GroupName = groupName,
                            IsAllTested = allTested,
                        });
                    }
                    else
                    {
                        projects.Add(new ProjectModel()
                        {
                            Groups = new List<GroupModel>()
                            {
                                new GroupModel() { GroupName = groupName,IsAllTested=allTested}
                            },
                            projectName = projectName,

                        });
                    }
                }
            }
            for (int i = 0; i < projects.Count; i++)
            {
                System.Windows.Forms.TreeNode treeNode = new System.Windows.Forms.TreeNode(projects[i].projectName);
                List<GroupModel> groups = projects[i].Groups;
                foreach (GroupModel group in groups)
                {
                    treeNode.Nodes.Add(group.GroupName);
                }
                uiTreeView1.Nodes.Add(treeNode);
                for (int j = 0; j < groups.Count; j++)
                {
                    if (groups[j].IsAllTested != 0)
                    {
                        uiTreeView1.Nodes[i].Nodes[j].BackColor = Color.MediumSpringGreen;
                    }
                }
            }

        }

        internal void SettingCurrentStudentDataView(StudentTestingData sl, ListView studentListview)
        {
            throw new NotImplementedException();
        }
    }
}
