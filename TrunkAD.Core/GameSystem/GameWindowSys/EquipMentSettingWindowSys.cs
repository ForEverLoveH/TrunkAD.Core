using HZH_Controls.Forms;
using Newtonsoft.Json;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrunkAD.Core.GameSystem.GameHelper;
using TrunkAD.Core.GameSystem.GameModel;
using TrunkAD.Core.GameSystem.GameWindow;

namespace TrunkAD.Core.GameSystem.GameWindowSys
{
    public class EquipMentSettingWindowSys
    {
        public static EquipMentSettingWindowSys Instance { get; private set; }  
        public void Awake()
        {
            Instance = this;
        }
        PlatFormSettingWindow platFormSettingWindow = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sQLiteHelper"></param>
        /// <returns></returns>
        public bool ShowPlatFormSettingWindow(SQLiteHelper sQLiteHelper)
        {
            if (platFormSettingWindow == null)
            {
                platFormSettingWindow = new PlatFormSettingWindow();
                platFormSettingWindow.helper= sQLiteHelper;
                if (platFormSettingWindow.ShowDialog()==System.Windows.Forms.DialogResult.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="localInfos"></param>
        /// <param name="localValues"></param>
        /// <param name="helper"></param>
        /// <param name="uiComboBox2"></param>
        /// <param name="uiCombox3"></param>
        /// <param name="uiComboBox1"></param>
        public void LoadingInitData(ref List<Dictionary<string, string>> localInfos, ref Dictionary<string, string> localValues, SQLiteHelper helper, UIComboBox comboBox1, UIComboBox comboBox2, UIComboBox combox3,UIComboBox combox4)
        {
            localInfos = helper.ExecuteReaderList("SELECT * FROM localInfos");
            if (localInfos.Count > 0)
            {
                string MachineCode = String.Empty;
                string ExamId = String.Empty;
                string Platform = String.Empty;
                string Platforms = String.Empty;
                int UploadUnit = 0;
                foreach (var item in localInfos)
                {
                    localValues.Add(item["key"], item["value"]);
                    switch (item["key"])
                    {
                        case "MachineCode":
                            MachineCode = item["value"];
                            break;
                        case "ExamId":
                            ExamId = item["value"];
                            break;
                        case "Platform":
                            Platform = item["value"];
                            break;
                        case "Platforms":
                            Platforms = item["value"];
                            break;
                        case "UploadUnit":
                            int.TryParse(item["value"], out UploadUnit);
                            break;
                    }
                }

                if (string.IsNullOrEmpty(MachineCode))
                {
                  UIMessageBox.ShowWarning( "设备码为空");
                }
                else
                {
                    comboBox1.Text = MachineCode;
                }
                if (string.IsNullOrEmpty(ExamId))
                {
                    UIMessageBox.ShowWarning("考试id为空");
                }
                else
                {
                   combox3.Text = ExamId;
                }
                if (string.IsNullOrEmpty(Platforms))
                {
                    UIMessageBox.ShowWarning("平台码为空");
                }
                else
                {
                    string[] Platformss = Platforms.Split(';');
                    comboBox2.Items.Clear();
                    foreach (var item in Platformss)
                    {
                        comboBox2.Items.Add(item);
                    }

                }
                if (string.IsNullOrEmpty(Platform))
                {
                    UIMessageBox.ShowWarning("平台码为空");
                }
                else
                {
                    comboBox2.Text = Platform;
                }
                combox4.SelectedIndex = UploadUnit;

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="combox3"></param>
        /// <param name="comboBox2"></param>
        /// <param name="localValues"></param>
        /// <returns></returns>
        public bool GetExamNumber(UIComboBox combox3, UIComboBox comboBox2, Dictionary<string, string> localValues)
        {
            try
            {
                combox3.Items.Clear();
                string url = comboBox2.Text;
                if (string.IsNullOrEmpty(url))
                {
                    UIMessageBox.ShowError("网址为空!");
                    return false;
                }
                url += RequestUrl.GetExamListUrl;
                RequestParameter RequestParameter = new RequestParameter();
                RequestParameter.AdminUserName = localValues["AdminUserName"];
                RequestParameter.TestManUserName = localValues["TestManUserName"];
                RequestParameter.TestManPassword = localValues["TestManPassword"];

                //序列化
                string JsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(RequestParameter);

                // string url = localValues["Platform"] + RequestUrl.GetExamListUrl;

                var formDatas = new List<FormItemModel>();
                //添加其他字段
                formDatas.Add(new FormItemModel()
                {
                    Key = "data",
                    Value = JsonStr
                });
                var httpUpload = new HttpUpload();
                string result = HttpUpload.PostForm(url, formDatas);
                GetExamList upload_Result = JsonConvert.DeserializeObject<GetExamList>(result);

                if (upload_Result.Results.Count == 0)
                {
                    UIMessageBox.ShowError($"提交错误,错误码:[{upload_Result.Error}]");
                    return false;
                }

                foreach (var item in upload_Result.Results)
                {
                    string str = $"{item.title}_{item.exam_id}";
                    combox3.Items.Add(str);
                }
                UIMessageBox.ShowSuccess("获取成功！！");

                combox3.SelectedIndex = 0;
                return true;
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
        /// <param name="comboBox1"></param>
        /// <param name="combox2"></param>
        /// <param name="combox3"></param>
        /// <param name="localValues"></param>
        /// <returns></returns>
        public bool LoadingEquipMentCodeData(UIComboBox comboBox1, UIComboBox combox2, UIComboBox combox3, Dictionary<string, string> localValues)
        {
            try
            {
                comboBox1.Items.Clear();

                string examId = combox3.Text;
                if (string.IsNullOrEmpty(examId))
                {
                    UIMessageBox.ShowError( "考试id为空!");
                    return false;
                }
                if (examId.IndexOf('_') != -1)
                {
                    examId = examId.Substring(examId.IndexOf('_') + 1);
                }
                string url = combox2.Text;
                if (string.IsNullOrEmpty(url))
                {
                    UIMessageBox.ShowError("网址为空!");
                    return false;
                }
                url += RequestUrl.GetMachineCodeListUrl;

                RequestParameter RequestParameter = new RequestParameter();
                RequestParameter.AdminUserName = localValues["AdminUserName"];
                RequestParameter.TestManUserName = localValues["TestManUserName"];
                RequestParameter.TestManPassword = localValues["TestManPassword"];
                RequestParameter.ExamId = examId;
                //序列化
                string JsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(RequestParameter);

                var formDatas = new List<FormItemModel>();
                //添加其他字段
                formDatas.Add(new FormItemModel()
                {
                    Key = "data",
                    Value = JsonStr
                });
                var httpUpload = new HttpUpload();
                string result = HttpUpload.PostForm(url, formDatas);
                GetMachineCodeList upload_Result = JsonConvert.DeserializeObject<GetMachineCodeList>(result);
                if (upload_Result.Results.Count == 0)
                {
                    UIMessageBox.ShowError( $"提交错误,错误码:[{upload_Result.Error}]");
                    return false;
                }

                foreach (var item in upload_Result.Results)
                {
                    string str = $"{item.title}_{item.MachineCode}";
                    comboBox1.Items.Add(str);

                }
                UIMessageBox.ShowSuccess("获取成功！！");
                comboBox1.SelectedIndex = 0;
                return true;
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return false;
            }
        }
    }
}
