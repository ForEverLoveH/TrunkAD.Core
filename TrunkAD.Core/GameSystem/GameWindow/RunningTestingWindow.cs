 using AForge.Video;
using AForge.Video.DirectShow;
using HZH_Controls;
using HZH_Controls.Forms;
using MiniExcelLibs;
using OpenCvSharp;
using SpeechLib;
using Spire.Xls;
using Sunny.UI;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrunkAD.Core.GameSystem.GameHelper;
using TrunkAD.Core.GameSystem.GameModel;
using TrunkAD.Core.GameSystem.GameWindowSys;

namespace TrunkAD.Core.GameSystem.GameWindow
{
    public partial class RunningTestingWindow : Form
    {
        public RunningTestingWindow()
        {
            InitializeComponent();
        }
        public static string strMainModule = System.AppDomain.CurrentDomain.BaseDirectory + "data\\";

        //项目编号
        public string _ProjectId = "";

        public string formTitle = "";

        //测试模式 0:自动下一位 1:自动下一轮
        public int _TestMethod = 0;

        //组名
        public string _GroupName = "";

        //项目名
        public string _ProjectName = "";

        //测试最大次数
        public int _RoundCount = 1;

        //当前测试轮次
        public int RoundCount0 = 1;

        //项目模式
        public string _Type = "";

        //最好成绩模式 0:取大值 1:取小值
        /// <summary>
        /// 0:末位删除 1:非零进一 2:四舍五入
        /// </summary>
        public int _BestScoreMode = 0;

        //保留位数
        public int _FloatType = 0;

        //数据库
        public SQLiteHelper sQLiteHelper = null;

        //考试组分配信息
        private RaceStudentData nowRaceStudentData = new RaceStudentData();

        private string dangwei = "米";
        /// <summary>
        /// 存储考生数据
        /// </summary>
        private class RaceStudentData
        {
            public RaceStudentData()
            {
                id = String.Empty;
                idNumber = String.Empty;
                name = String.Empty;
                groupName = String.Empty;
                score = 0;
                RoundId = 1;
                state = 0;
            }

            public string id;//编号
            public string idNumber;//考号
            public string name;//姓名
            public double score;//成绩
            public int RoundId;//轮次

            //状态 0:未测试 1:已测试 2:中退 3:缺考 4:犯规 5:弃权
            public int state;//状态

            public string groupName;
        }
        public string typeName = "立定跳远";
        public double MeasureLen = 0;//测量长度
        public double MeasureLenX = 0;//测量水平长度
        public double MeasureLenY = 0;//测量垂直长度
        //远距离摄像头
        public double m_MeasureLen = 0;//测量长度
        public double m_MeasureLenX = 0;//测量水平长度
        public double m_MeasureLenY = 0;//测量垂直长度
        private Boolean Measure = false;//测量长度状态
        private string markPointFile = "markPoint.dat";
        private string markPointFile1 = "markPoint1.dat";
        private string markDirectionFile = "markDirection.dat";
        private string markDirectionFile1 = "markDirection1.dat";
        private string MemoStrFile = "MemoStrFile.dat";
        private string nowFileName = "";//当前文件名
        private string nowTestDir = String.Empty;//当前文件目录
        private string ScoreDir = string.Empty;
        private int recTimeR0 = 0;//计时时间
        private int frameSum = 0;
        private int frameSum1 = 0;
        private Thread threadVideo2;
        public double boy_MaxScore = 3.6;
        public double girl_MaxScore = 3.6;
        public NFC_Helper USBWatcher = new NFC_Helper();
        Dictionary<string, string> _LocalInfos = new Dictionary<string, string>();

        private string _MemoStr = "";//备注信息


        private void RunTest_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            string code = "程序集版本：" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string code1 = "文件版本：" + Application.ProductVersion.ToString();
            bool flag = true;
            string admincode = "0";
            if (File.Exists(@"data/admincode.txt"))
            {
                admincode = File.ReadAllText(@"data/admincode.txt");
            }

            if (admincode != "1")
            {
                LoginWindowForm lf = new LoginWindowForm();
                if (lf.ShowDialog() != DialogResult.OK)
                {
                    flag = false;
                }
            }

            if (!flag)
            {
                this.Close();
            }
            else
            {
                LoadLocalInfos();
                VersionLabel.Text = code;
                AsyncMethodCaller caller = new AsyncMethodCaller(TestMethodAsync);
                var workTask = Task.Run(() => caller.Invoke());
                RunTestLoadInit();
                UpdateListView(_ProjectId, _GroupName, 1);
                loadJumpInit();
                ParameterizedThreadStart method = new ParameterizedThreadStart(ImagePredictLabelQueues2ThreadFun);
                threadVideo2 = new Thread(method);
                threadVideo2.IsBackground = true;
                threadVideo2.Start();
                serialInit();
                CameraInit();
                USBWatcher.AddUSBEventWatcher(USBEventHandler, USBEventHandler, new TimeSpan(0, 0, 1));
                /* vHandle = GetWindowDC(pictureBox1.Handle);
                 vGraphics = Graphics.FromHdc(vHandle);*/
                comboBox3.SelectedIndex = 0;
                comboBox4.SelectedIndex = 0;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        public void LoadLocalInfos()
        {
            {
                _LocalInfos.Clear();
                List<Dictionary<string, string>> list0 = sQLiteHelper.ExecuteReaderList("SELECT * FROM localInfos");
                foreach (var item in list0)
                {
                    _LocalInfos.Add(item["key"], item["value"]);
                }
                //最大值成绩
                double.TryParse(_LocalInfos["boy_MaxScore"], out boy_MaxScore);
                textBox2.Text = _LocalInfos["boy_MaxScore"];
                //boy_MaxScore = boy_MaxScore > 0 ? boy_MaxScore : 3.6;
                double.TryParse(_LocalInfos["girl_MaxScore"], out girl_MaxScore);
                textBox3.Text = _LocalInfos["girl_MaxScore"];
                if (_LocalInfos.Keys.Contains("bUnusualScoreMin")) bUnusualScoreMinTb.Text = _LocalInfos["bUnusualScoreMin"];
                if (_LocalInfos.Keys.Contains("bUnusualScoreMax")) bUnusualScoreMaxTb.Text = _LocalInfos["bUnusualScoreMax"];
                if (_LocalInfos.Keys.Contains("gUnusualScoreMin")) gUnusualScoreMinTb.Text = _LocalInfos["gUnusualScoreMin"];
                if (_LocalInfos.Keys.Contains("gUnusualScoreMax")) gUnusualScoreMaxTb.Text = _LocalInfos["gUnusualScoreMax"];
                //girl_MaxScore = girl_MaxScore > 0 ? girl_MaxScore : 3.6;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void SaveInfos()
        {
            //男生满分
            string sql = $"update localInfos set value='{boy_MaxScore.ToString()}' where key='boy_MaxScore'";
            int v = sQLiteHelper.ExecuteNonQuery(sql);
            if (v == 0)
            {
                sql = $"insert into localInfos(value,key) values ('{boy_MaxScore.ToString()}','boy_MaxScore')";
                sQLiteHelper.ExecuteNonQuery(sql);
            }
            //女生满分
            sql = $"update localInfos set value='{girl_MaxScore.ToString()}' where key='girl_MaxScore'";
            v = sQLiteHelper.ExecuteNonQuery(sql);
            if (v == 0)
            {
                sql = $"insert into localInfos(value,key) values ('{girl_MaxScore.ToString()}','girl_MaxScore')";
                sQLiteHelper.ExecuteNonQuery(sql);
            }

            /* double bUnusualScoreMin = 0;
             double bUnusualScoreMax = 3;*/
            //男生异常成绩
            sql = $"update localInfos set value='{bUnusualScoreMin.ToString()}' where key='bUnusualScoreMin'";
            v = sQLiteHelper.ExecuteNonQuery(sql);
            if (v == 0)
            {
                sql = $"insert into localInfos(value,key) values ('{bUnusualScoreMin.ToString()}','bUnusualScoreMin')";
                sQLiteHelper.ExecuteNonQuery(sql);
            }
            sql = $"update localInfos set value='{bUnusualScoreMax.ToString()}' where key='bUnusualScoreMax'";
            v = sQLiteHelper.ExecuteNonQuery(sql);
            if (v == 0)
            {
                sql = $"insert into localInfos(value,key) values ('{bUnusualScoreMax.ToString()}','bUnusualScoreMax')";
                sQLiteHelper.ExecuteNonQuery(sql);
            }
            /*double gUnusualScoreMin = 0;
            double gUnusualScoreMax = 3;*/
            //女生异常成绩
            sql = $"update localInfos set value='{gUnusualScoreMin.ToString()}' where key='gUnusualScoreMin'";
            v = sQLiteHelper.ExecuteNonQuery(sql);
            if (v == 0)
            {
                sql = $"insert into localInfos(value,key) values ('{gUnusualScoreMin.ToString()}','gUnusualScoreMin')";
                sQLiteHelper.ExecuteNonQuery(sql);
            }
            sql = $"update localInfos set value='{gUnusualScoreMax.ToString()}' where key='gUnusualScoreMax'";
            v = sQLiteHelper.ExecuteNonQuery(sql);
            if (v == 0)
            {
                sql = $"insert into localInfos(value,key) values ('{gUnusualScoreMax.ToString()}','gUnusualScoreMax')";
                sQLiteHelper.ExecuteNonQuery(sql);
            }
            try
            {
                //保存备注信息
                string MemoStr = richTextBox1.Text;
                string MemoStrPath = strMainModule + MemoStrFile;
                TextFileHelper.Instance.Write(MemoStrPath, MemoStr);

            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
        }
        /// <summary>
        /// usb设备拔插监视
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void USBEventHandler(Object sender, EventArrivedEventArgs e)
        {
            //暂未实现
            var watcher = sender as ManagementEventWatcher;
            watcher.Stop();

            if (e.NewEvent.ClassPath.ClassName == "__InstanceCreationEvent")
            {

            }
            else if (e.NewEvent.ClassPath.ClassName == "__InstanceDeletionEvent")
            {
                //检测断开,断开提示
                //MessageBox.Show("设备断开请检查");
                if (ScreenState.Text != "单人显示屏未连接")
                {
                    if ((sReader == null) || (sReader != null && !sReader.IsComOpen()))
                    {
                        ControlHelper.ThreadInvokerControl(this, () =>
                        {
                            ScreenState.Text = "单人显示屏未连接";
                            ScreenState.ForeColor = Color.Red;
                            openSerialPortBtn.Text = "连接显示屏";

                        });
                    }
                }
            }
            watcher.Start();
        }
        /// <summary>
        /// 
        /// </summary>
        private void stuViewInit()
        {
            try
            {

                List<string> Header = new List<string> { "序号", "考号", "姓名" };
                List<string> names = new List<string> { "colum0", "colum1", "colum11" };
                for (int i = 1; i <= _RoundCount; i++)
                {
                    Header.Add($"第{i}轮");
                    Header.Add($"考试状态");
                    Header.Add($"上传状态");
                    names.Add($"Score{i}");
                    names.Add($"Score1{i}");
                    names.Add($"Score2{i}");
                }
                Header.Add("唯一编号");
                names.Add("colum5");

                stuView.Columns.Clear();
                for (int i = 0; i < Header.Count; i++)
                {
                    DataGridViewColumn data_colu = new DataGridViewTextBoxColumn();
                    data_colu.Name = names[i];//设置列名称
                    data_colu.HeaderText = Header[i];//设置列标题文本
                    data_colu.ReadOnly = true;//是否允许编辑
                    data_colu.DataPropertyName = names[i];//设置绑定数据时列名称

                    data_colu.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    stuView.Columns.Add(data_colu);//添加到表格列
                }

            }
            catch (Exception ex)
            {

                LoggerHelper.Debug(ex);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RunTest_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveInfos();
            CloseCamera();
            CloseCamera1();
            if (sReader != null && sReader.IsComOpen())
            {
                sReader.CloseCom();
            }
        }
        /// <summary>
        /// 委托必须和要调用的异步方法有相同的签名
        /// </summary>
        /// <param name="callDuration">sleep时间</param>
        /// <param name="threadId">当前线程id</param>
        /// <returns></returns>
        public delegate string AsyncMethodCaller();
        /// 与委托对应的方法
        /// </summary>
        /// <param name="callDuration"></param>
        /// <param name="threadId"></param>
        /// <returns></returns>
        private string TestMethodAsync()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //startMeasure();
            sw.Stop();
            string useTimeStr = string.Format("耗时{0}ms.", sw.ElapsedMilliseconds.ToString());
            return useTimeStr;
        }

        #region 初始化函数
        /// <summary>
        /// 
        /// </summary>
        private void loadJumpInit()
        {
            ScoreDir = Path.Combine(strMainModule, "Score");
            if (!Directory.Exists(ScoreDir)) Directory.CreateDirectory(ScoreDir);
            string[] strg =TextFileHelper.Instance.Read(strMainModule + markPointFile);
            if (strg != null)
            {
                try
                {
                    if (strg.Length > 0)
                    {
                        foreach (var s in strg)
                        {
                            string[] ss1 = s.Split(';');
                            if (ss1.Length == 4)
                            {
                                System.Drawing.Point p =   PointHelper.Instance. XYString2Point(ss1[1]);
                                bool bl = true;
                                if (ss1[3] == "0")
                                {
                                    bl = false;
                                }
                                targetPoints.Add(new TargetPoint()
                                {
                                    x = p.X,
                                    y = p.Y,
                                    name = ss1[0],
                                    dis = PointHelper.Instance.Str2int(ss1[2]),//cm
                                    status = bl
                                });
                            }
                        }
                        updateTargetListView(true, 0);
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Debug(ex);
                }
            }
            strg = TextFileHelper.Instance.Read(strMainModule + markPointFile1);
            if (strg != null)
            {
                try
                {
                    if (strg.Length > 0)
                    {
                        foreach (var s in strg)
                        {
                            string[] ss1 = s.Split(';');
                            if (ss1.Length == 4)
                            {
                                System.Drawing.Point p =  PointHelper.Instance. XYString2Point(ss1[1]);
                                bool bl = true;
                                if (ss1[3] == "0")
                                {
                                    bl = false;
                                }
                                targetPoints1.Add(new TargetPoint()
                                {
                                    x = p.X,
                                    y = p.Y,
                                    name = ss1[0],
                                    dis =  PointHelper.Instance.Str2int(ss1[2]),//cm
                                    status = bl
                                });
                            }
                        }
                        updateTargetListView(true, 1);
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Debug(ex);
                }
            }
            if (_FloatType == 0)
            {
                ReservedDigitsTxt = "0";
            }
            else
            {
                ReservedDigitsTxt = "0.";
                for (int i = 0; i < _FloatType; i++)
                {
                    ReservedDigitsTxt += "0";
                }
            }
            strg = TextFileHelper.Instance.Read(strMainModule + markDirectionFile);
            if (strg.Length > 0)
            {
                int.TryParse(strg[0], out Directionmode);
            }
            strg = TextFileHelper.Instance.Read(strMainModule + markDirectionFile1);
            if (strg.Length > 0)
            {
                int.TryParse(strg[0], out Directionmode1);
            }
            strg = TextFileHelper.Instance.Read(Path.Combine(strMainModule, "tolerance.dat"));
            if (strg.Length > 0)
            {
                int.TryParse(strg[0], out tolerance);
                toleranceTxt.Text = strg[0];
            }
            strg = TextFileHelper.Instance.Read(Path.Combine(strMainModule, "pidtxt.dat"));
            if (strg.Length > 0)
            {
                pidtxt.Text = strg[0];
            }
            strg = TextFileHelper.Instance.Read(Path.Combine(strMainModule, "vidtxt.dat"));
            if (strg.Length > 0)
            {
                vidtxt.Text = strg[0];
            }
            try
            {
                string MemoStrPath = strMainModule + MemoStrFile;
                string strg1 = TextFileHelper.Instance.loadString(MemoStrPath);
                richTextBox1.Text = strg1;
                _MemoStr = strg1;
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void RunTestLoadInit()
        {
            try
            {
                stuViewInit();
                updateGroupCombox();
                updateRoundCountCombox();
                RoundCountCbx.SelectedIndex = 0;
                typeName = ProjectState.ProjectState2Str(_Type);
                if (typeName.Trim() == "立定跳远")
                {
                    isBallCheckBoxb = false;
                    isShowImgList = true;
                    isShotPut = false;
                    isSitting = false;
                    checkBox2.Visible = false;
                    checkBox2.Visible = false;
                    groupBox1.Visible = false;
                    button4.Visible = false;
                }
                else if (typeName.Trim() == "投掷实心球")
                {
                    isBallCheckBoxb = true;
                    isShowImgList = false;
                    isShotPut = false;
                    isSitting = false;
                    checkBox2.Visible = false;
                    groupBox1.Visible = false;
                    button4.Visible = false;
                }
                else if (typeName.Trim() == "坐位体前屈")
                {
                    isBallCheckBoxb = false;
                    isShowImgList = false;
                    isShotPut = false;
                    isSitting = true;
                    checkBox2.Visible = true;
                    groupBox1.Visible = true;
                    button4.Visible = true;
                    dangwei = "厘米";
                }
                else if (typeName.Trim() == "投掷铅球")
                {
                    isBallCheckBoxb = true;
                    isShowImgList = false;
                    isShotPut = true;
                    isSitting = false;
                    checkBox2.Visible = false;
                    groupBox1.Visible = false;
                    button4.Visible = false;
                }
                groupBox2.Text = $"满分值设置({dangwei})";
                this.Text = $"{typeName}影像测距";
                comboBox1.SelectedIndex = _TestMethod;
                if (_TestMethod == 0)
                {
                    //0:自动下一位 1:自动下一轮
                    checkBox3.Text = "自动下一位";
                }
                else if (_TestMethod == 1)
                {
                    checkBox3.Text = "自动下一轮";
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
        }
        #endregion 初始化函数

        

        #region 定时器模块
        /// <summary>
        /// 页面刷新率计算
        /// </summary>
        int flashCount = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (BeginRecTimeOut > 0) BeginRecTimeOut--;
            clearMemory();
            frameSpeed_txt.Text = "fps:" + frameRecSum;
            frameRecSum = 0;
            FrameFlashLb0.Text = $"刷新帧率({FrameFlash0}):";
            FrameFlashLb1.Text = $"刷新帧率({FrameFlash1}):";
            flashCount = 0;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        private void clearMemory(int value = 50)
        {
            double v = MemoryTool.GetProcessUsedMemory();
            if (v > value)
            {
                MemoryTool.ClearMemory();
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer2_Tick(object sender, EventArgs e)
        {
            if (RecordEnd == 1)
            {
                if (remainderImgSum > 0)
                {
                    imgProgressBar.Maximum = remainderImgSum;
                    imgProgressBar.Value = remainderImgCount;
                }
            }
            else if (RecordEnd == 2)
            {
                if (remainderImgSum > 0)
                {
                    imgProgressBar.Maximum = remainderImgSum;
                    imgProgressBar.Value = remainderImgSum;
                }

                RecordEnd = 0;
            }
            if (recTimeR0 > 0)
            {
                recTimeR0--;
                if (recTimeR0 == 0)
                {
                    try
                    {
                        rgbVideoSourceStop();
                        rgbVideoSourceStop1();
                        nowRaceStudentData.state = 1;
                        ControlHelper.ThreadInvokerControl(this, () =>
                        {
                            button12.Text = "开始录像(空格)";
                            button12.BackColor = Color.White;
                           // pictureBox2.Image = peilinInfraredEquipment.Properties.Resources.开始录制_line;
                        });
                        recTime.Text = "文件数:" + frameSum + "";
                        videoSourceRuningR0 = 0;//播放中的心跳包
                                                //录像结束进行操作
                                                //打开图片集
                        RecordEnd = 1;
                        /* ReleaseVideoOutPut();
                         ReleaseVideoOutPut1();*/
                        setHScrollBarValue(0);
                        setHScrollBarValue1(0);
                        if (isShowImgList)
                        {
                            openImgList();
                        }
                        else
                        {
                            if (isSitting)
                            {
                                int nlen = imgMs.Count;
                                int maxW = 0;
                                int maxH = 0;
                                int index = 0;
                                int minWidth = conPoints0[0].X;
                                int maxWidth = conPoints0[3].X;
                                int minHeigh = conPoints0[0].Y;
                                int maxHeigh = conPoints0[3].Y;
                                int anaylistLen = 10;
                                int[] anaylist = new int[anaylistLen];
                                OpenCvSharp.Point[] anaPoint = new OpenCvSharp.Point[nlen];
                                if (Directionmode == 1)
                                {
                                    maxW = maxWidth;
                                    maxH = maxHeigh;
                                }
                                for (int k = 0; k < nlen; k++)
                                {
                                    bool[][] isHand = imgMs[k].isHand;
                                    //maxW = 0;
                                    //maxH = 0;
                                    int maxwT = 0;
                                    int maxhT = 0;
                                    if (isHand == null) continue;
                                    for (int i = minWidth; i < maxWidth; i++)
                                    {
                                        for (int j = minHeigh; j < maxHeigh; j++)
                                        {
                                            if (isHand[i][j])
                                            {
                                                int nl = i - 5;
                                                if (nl < 0) nl = 0;
                                                bool flag0 = true;
                                                for (int i1 = nl; i1 < i; i1++)
                                                {
                                                    if (!isHand[i1][j])
                                                    {
                                                        flag0 = false; break;
                                                    }
                                                }
                                                if (!flag0) continue;
                                                //当前图片最大值
                                                maxwT = i;
                                                maxhT = j;
                                                //全局最大值
                                                if (Directionmode == 1)
                                                {
                                                    if (i < maxW)
                                                    {
                                                        maxW = i;
                                                        maxH = j;
                                                        index = k;
                                                    }
                                                }
                                                else if (Directionmode == 0)
                                                {
                                                    if (i > maxW)
                                                    {
                                                        maxW = i;
                                                        maxH = j;
                                                        index = k;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    anaPoint[k] = new OpenCvSharp.Point(maxwT, maxhT);
                                }

                                try
                                {
                                    string Log = $"最大值:index:{index},X:{maxW}  y:{maxH}";
                                    WriteLog(lrtxtLog, Log, 0);
                                    if (imgMs[index] != null && imgMs[index].img != null)
                                    {
                                        nowFileName = index + "";
                                        ControlHelper.ThreadInvokerControl(this, () =>
                                        {
                                            pictureBox1.Image = ImageHelper.MemoryStream2Bitmap(imgMs[index].img);
                                            recImgIndex.Text = $"图片索引:{nowFileName}";

                                        });
                                        setHScrollBarValue(index);
                                        startMeasure();
                                        dispJumpLength1(maxW, maxH);
                                        stopMeasure();
                                        RefreshPictureBox();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LoggerHelper.Debug(ex);
                                }


                            }
                            else
                            {
                                //setHScrollBarValue(imgMs.Count - 1);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Debug(ex);
                    }
                }
                else
                {
                    recTime.Text = "REC " + (recTimeR0 + 10) / 10;
                }
            }
        }
        #endregion 定时器模块

        private bool UpdateListViewRun = false;

        public List<StuViewPojo> stuViewPojos = new List<StuViewPojo>();
        /// <summary>
        /// 更新名单视图
        /// </summary>
        /// <param name="ProjectId"></param>
        /// <param name="GroupName"></param>
        /// <param name="roundId"></param>
        private void UpdateListView(string ProjectId, string GroupName, int roundId)
        {
            if (UpdateListViewRun) return;
            UpdateListViewRun = true;
            try
            {
                stuViewPojos.Clear();
                List<string> StuNames = new List<string>();
                stuView.Rows.Clear();
                var ds = sQLiteHelper.ExecuteReaderList($"SELECT Id,Name,IdNumber,Sex FROM DbPersonInfos WHERE ProjectId='{ProjectId}' and GroupName='{GroupName}'");
                int listviewCount = 1;
                foreach (var dic in ds)
                {
                    int k = stuView.Rows.Add(new DataGridViewRow());
                    string stuId = dic["Id"];
                    string stuName = dic["Name"];
                    StuNames.Add(stuName);
                    string idNumber = dic["IdNumber"];
                    string Sex = dic["Sex"];
                    var ds1 = sQLiteHelper.ExecuteReaderList($"SELECT PersonName,Result,State,uploadState,RoundId FROM ResultInfos " +
                        $"WHERE PersonId='{stuId}' GROUP BY RoundId order by RoundId asc");
                    int flag = 0;
                    stuView.Rows[k].Cells[0].Value = listviewCount.ToString();
                    stuView.Rows[k].Cells[1].Value = idNumber;
                    stuView.Rows[k].Cells[2].Value = stuName;

                    StuViewPojo svp = new StuViewPojo();
                    svp.Number = listviewCount.ToString();
                    svp.Name = stuName;
                    svp.IdNumber = idNumber;
                    svp.Id = stuId;
                    bool flag_maxScore = false;
                    double Maxscore = Sex == "1" ? girl_MaxScore : boy_MaxScore;
                    //序号 考号 姓名 成绩 考试状态 上传状态 唯一编号
                    foreach (var item in ds1)
                    {
                        ScoreViewPojo scorevp = new ScoreViewPojo();
                        int flag1 = flag * 3;
                        string PersonName0 = item["PersonName"];
                        double.TryParse(item["Result"], out double Result0);
                        int.TryParse(item["State"], out int State0);
                        int.TryParse(item["uploadState"], out int uploadState0);
                        int.TryParse(item["RoundId"], out int RoundId);
                        string sstate = ResultState.ResultState2Str(State0);
                        scorevp.roundCound = RoundId;
                        if (State0 > 1)
                        {
                            //犯规异常操作
                            stuView.Rows[k].Cells[3 + flag1].Value = 0;
                            stuView.Rows[k].Cells[4 + flag1].Value = sstate;
                            stuView.Rows[k].Cells[4 + flag1].Style.ForeColor = Color.Red;
                            scorevp.Score = "0";
                        }
                        else if (State0 != 0)
                        {
                            if (Result0 >= Maxscore) flag_maxScore = true;
                            double UnusualScoreMin = 0;
                            double UnusualScoreMax = 0;
                            if (Sex == "0")
                            {
                                //男
                                UnusualScoreMin = bUnusualScoreMin;
                                UnusualScoreMax = bUnusualScoreMax;
                            }
                            else if (Sex == "1")
                            {
                                //女
                                UnusualScoreMin = gUnusualScoreMin;
                                UnusualScoreMax = gUnusualScoreMax;
                            }
                            //异常成绩
                            bool UnusualFlag = false;
                            if (Result0 >= UnusualScoreMax || Result0 <= UnusualScoreMin)
                            {
                                UnusualFlag = true;
                            }
                            stuView.Rows[k].Cells[3 + flag1].Value = Result0.ToString();
                            if (UnusualFlag)
                            {
                                stuView.Rows[k].Cells[4 + flag1].Value = "异常";
                                stuView.Rows[k].Cells[4 + flag1].Style.BackColor = Color.Red;
                                //stuView.Rows[k].DefaultCellStyle.BackColor = Color.MediumSpringGreen;
                            }
                            else
                            {
                                stuView.Rows[k].Cells[4 + flag1].Value = sstate;
                                stuView.Rows[k].DefaultCellStyle.BackColor = Color.MediumSpringGreen;
                            }

                            scorevp.Score = Result0.ToString();
                        }
                        if (uploadState0 > 0)
                        {
                            stuView.Rows[k].Cells[5 + flag1].Value = "已上传";
                            scorevp.IsUpload = true;
                        }
                        else
                        {
                            stuView.Rows[k].Cells[5 + flag1].Value = "未上传";
                            stuView.Rows[k].Cells[5 + flag1].Style.ForeColor = Color.Red;
                        }

                        if (scorevp.roundCound != -1)
                        {
                            svp.ScoreViewPojos.Add(scorevp);
                        }
                        flag++;
                    }
                    flag--;
                    stuView.Rows[k].Cells[6 + ((_RoundCount - 1) * 3)].Value = stuId;
                    int ds1Count = ds1.Count;
                    for (int i = ds1Count; i < _RoundCount; i++)
                    {
                        int i1 = i * 3;
                        stuView.Rows[k].Cells[3 + i1].Value = "未测试";
                        stuView.Rows[k].Cells[4 + i1].Value = "未测试";
                        stuView.Rows[k].Cells[5 + i1].Value = "未上传";
                        stuView.Rows[k].Cells[5 + i1].Style.ForeColor = Color.Red;
                    }
                    stuView.Rows[k].Height = 35;
                    svp.isMaxScore = flag_maxScore;
                    stuViewPojos.Add(svp);
                    listviewCount++;
                }
                comboBox2.Items.Clear();
                comboBox2.Items.AddRange(StuNames.ToArray());
                try
                {
                    int nlen = stuView.Rows.Count;
                    for (int i = 0; i < nlen; i++)
                    {
                        this.stuView.Rows[i].Selected = false;
                    }
                }
                catch (Exception)
                {
                    nowRaceStudentData = new RaceStudentData();
                }
                if (!string.IsNullOrEmpty(AdminUserIdNumber))
                {
                    int ncount = stuView.Rows.Count;

                    for (int i = 0; i < ncount; i++)
                    {
                        try
                        {
                            string idnumber0 = stuView.Rows[i].Cells[1].Value.ToString();
                            if (idnumber0 == AdminUserIdNumber)
                            {
                                this.stuView.Rows[i].Selected = true;
                                this.stuView.CurrentCell = this.stuView.Rows[i].Cells[1];
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerHelper.Debug(ex);
                        }
                    }
                }
                else
                {
                    int ncount = stuView.Rows.Count;
                    if (ncount > 0)
                    {
                        AdminUserIdNumber = stuView.Rows[0].Cells[1].Value.ToString();
                        this.stuView.Rows[0].Selected = true;
                        this.stuView.CurrentCell = this.stuView.Rows[0].Cells[1];
                        // RoundCountCbx.SelectedIndex = 0;
                    }
                }

            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
            finally
            {
                //stuView.ClearSelection();
                UpdateListViewRun = false;
            }
        }
        /// <summary>
        /// 更新本项目轮次次数
        /// </summary>
        private void updateRoundCountCombox()
        {
            try
            {
                RoundCountCbx.Items.Clear();
                for (int i = 1; i <= _RoundCount; i++)
                {
                    RoundCountCbx.Items.Add(i.ToString());
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
        }
        /// <summary>
        /// 更新组信息
        /// </summary>
        /// <param name="groupname"></param>
        private void updateGroupCombox(string groupname = "")
        {
            try
            {
                groupCbx.Items.Clear();

                groupCbx.Text = "";

                var ds = sQLiteHelper.ExecuteReader($"SELECT Name FROM DbGroupInfos WHERE Name LIKE'%{groupname}%' AND ProjectId='{_ProjectId}'");
                AutoCompleteStringCollection lstsourece = new AutoCompleteStringCollection();

                while (ds.Read())
                {
                    groupCbx.Items.Add(ds.GetString(0));

                    lstsourece.Add(ds.GetString(0));
                }
                groupCbx.AutoCompleteCustomSource = lstsourece;

                int index = -1;
                groupCbx.SelectedIndex = index;
                groupCbx.Text = "";
                if (string.IsNullOrEmpty(_GroupName) && groupCbx.Items.Count > 0)
                {
                    _GroupName = groupCbx.Items[0].ToString();
                    groupCbx.SelectedIndex = 0;
                }
                else
                {
                    if ((index = groupCbx.Items.IndexOf(_GroupName)) >= 0)
                    {
                        groupCbx.SelectedIndex = index;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
        }
        /// <summary>
        /// 选择轮次触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RoundCountCbx_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (RoundCountCbx.SelectedIndex != -1)
            {
                RoundCount0 = RoundCountCbx.SelectedIndex + 1;
                //UpdateListView(_ProjectId, _GroupName, RoundCount0);
                label7.Text = RoundCountCbx.Text;
                //nowRaceStudentData = new RaceStudentData();
            }
        }
        private bool isGroupCbxChange = false;
        /// <summary>
        /// 选择组触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void groupCbx_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                isGroupCbxChange = true;
                AdminUserIdNumber = string.Empty;
                ControlHelper.ThreadInvokerControl(this, () =>
                {
                    _GroupName = groupCbx.Text;
                    label13.Text = "当前测试组:" + _GroupName;
                    if (RoundCount0 > 0)
                    {
                        UpdateListView(_ProjectId, _GroupName, RoundCount0);
                        //nowRaceStudentData = new RaceStudentData();
                    }
                });
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
            finally
            {
                isGroupCbxChange = false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        private void setHScrollBarValue(int value)
        {
            ControlHelper.ThreadInvokerControl(this, () =>
            {
                hScrollBar1.Value = 0;
                hScrollBar1.Maximum = imgMs.Count - 1;
                if (value > imgMs.Count - 1)
                {
                    value = imgMs.Count - 1;
                }
                if (value < 0)
                {
                    value = 0;
                }

                hScrollBar1.Value = value;
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        private void setHScrollBarValue1(int value)
        {
            ControlHelper.ThreadInvokerControl(this, () =>
            {
                hScrollBar2.Value = 0;
                hScrollBar2.Maximum = imgMs1.Count - 1;
                if (value > imgMs1.Count - 1)
                {
                    value = imgMs1.Count - 1;
                }
                if (value < 0)
                {
                    value = 0;
                }

                hScrollBar2.Value = value;
            });
        }
        #region 测距模块
        /// <summary>
        /// 开始测距
        /// </summary>
        private void startMeasure()
        {
            Measure = true;//测量长度
            ControlHelper.ThreadInvokerControl(this, () =>
            {
                button13.Text = "测量中(S)";
                button13.BackColor = Color.Red;
            });
        }
        /// <summary>
        /// 
        /// </summary>
        private void stopMeasure()
        {
            Measure = false;//测量长度
            ControlHelper.ThreadInvokerControl(this, () =>
            {
                button13.Text = "测量(S)";
                button13.BackColor = Color.White;
            });
        }
        /// <summary>
        /// 
        /// </summary>
        private void MeasureFun()
        {
            if (Measure)
            {
                stopMeasure();
            }
            else
            {
                startMeasure();
            }
        }
        private int falseR0 = 0;
        private System.Drawing.Point mousePoint;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        private void dispJumpLength1(int x3, int y3)
        {
            if (Measure == false) return;//不测量
            if (recTimeR0 > 0) return;
            mousePoint.X = x3;
            mousePoint.Y = y3;
            Program.measureDll.dispJumpLength1(x3, y3,
                gfencePnts,
               gfencePntsDisValue,
                distanceMode,
                Directionmode,
                mousePoint,
               gfencePnts1,
                gfencePntsDisValue1,
                Directionmode1,
               ref markerTopJump,
               ref markerBottomJump,
               ref MeasureLenX,
               ref m_markerTopJump,
               ref m_markerBottomJump,
               ref m_MeasureLenX,
               ref markerTopJumpY,
               ref markerBottomJumpY,
               ref MeasureLenY,
               ref m_markerTopJumpY,
               ref m_markerBottomJumpY,
               ref m_MeasureLenY);


            try
            {
                if (distanceMode == 0)
                {
                    MeasureLen = MeasureLenX;
                    //是否铅球
                    if (isShotPut)
                    {
                        double distancey = 0;
                        double distancex = MeasureLenX;
                        int personY = cenMarkPoint.Y;
                        if (mousePoint.Y <= personY)
                        {
                            //distancey = (str2double("0.4") / 2) * 1000 - MeasureLenY;
                            distancey = 2000 - MeasureLenY;
                        }
                        else
                        {
                            //distancey = MeasureLenY - (str2double("0.4") / 2) * 1000;
                            distancey = MeasureLenY - 2000;
                        }
                        MeasureLen = Math.Sqrt((distancey * distancey) + (distancex * distancex));
                    }
                }
                else if (distanceMode == 1)
                {
                    m_MeasureLen = m_MeasureLenX;
                    //是否铅球
                    if (isShotPut)
                    {
                        double distancey = 0;
                        double distancex = m_MeasureLenX;
                        int personY = cenMarkPoint1.Y;
                        if (mousePoint.Y <= personY)
                        {
                            //distancey = (str2double("0.4") / 2) * 1000 - MeasureLenY;
                            distancey = 2000 - m_MeasureLenY;
                        }
                        else
                        {
                            //distancey = MeasureLenY - (str2double("0.4") / 2) * 1000;
                            distancey = m_MeasureLenY - 2000;
                        }
                        m_MeasureLen = Math.Sqrt((distancey * distancey) + (distancex * distancex));
                    }
                }

            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }

            return;
        }


        #endregion 测距模块

        #region 成绩上传

        /// <summary>
        /// 本次成绩上传
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (SoftWareProperty.singleMode != "0")
            {
                MessageBox.Show("单机模式无法操作");
                return;
            }
            ParameterizedThreadStart method = new ParameterizedThreadStart(uploadScoreForNowGroup);
            Thread threadRead = new Thread(method);
            threadRead.IsBackground = true;
            threadRead.Start();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void uploadScoreForNowGroup(object obj)
        {
            ControlHelper.ThreadInvokerControl(this, () =>
            {
                button2.Text = "上传中";
                button2.BackColor = Color.Red;
            });
            string[] fusp = new string[2];
            fusp[0] = _ProjectName;
            fusp[1] = _GroupName;
            string outmessage = uploadStudentThreadFun(fusp, sQLiteHelper, RoundCount0);
            //string outmessage=uploadStudentThreadFun1(fusp, sQLiteHelper);
            if (string.IsNullOrEmpty(outmessage))
            {
                FrmTips.ShowTipsInfo(this, "上传结束");
            }
            else
            {
                MessageBox.Show(outmessage);
            }

            ControlHelper.ThreadInvokerControl(this, () =>
            {
                button2.Text = "成绩上传";
                button2.BackColor = System.Drawing.SystemColors.Control;
                if (RoundCount0 > 0)
                {
                    UpdateListView(_ProjectId, _GroupName, RoundCount0);
                }
            });
        }

        /// <summary>
        /// 上传学生的多线程方法 多人
        /// 先不上传视频
        /// </summary>
        /// <param name="obj"></param>
        public string uploadStudentThreadFun(Object obj, SQLiteHelper sQLiteHelper, int nowRound)
        {
            try
            {
                List<Dictionary<string, string>> successList = new List<Dictionary<string, string>>();
                List<Dictionary<string, string>> errorList = new List<Dictionary<string, string>>();
                Dictionary<string, string> localInfos = new Dictionary<string, string>();
                List<Dictionary<string, string>> list0 = sQLiteHelper.ExecuteReaderList("SELECT * FROM localInfos");
                foreach (var item in list0)
                {
                    localInfos.Add(item["key"], item["value"]);
                }
                int.TryParse(localInfos["UploadUnit"], out int UploadUnit);

                string[] fusp = obj as string[];
                ///项目名称
                string projectName = string.Empty;
                //组
                string groupName = string.Empty;
                if (fusp.Length > 0)
                    projectName = fusp[0];
                if (fusp.Length > 1)
                    groupName = fusp[1];
                Dictionary<string, string> SportProjectDic = sQLiteHelper.ExecuteReaderOne($"SELECT Id,Type,RoundCount,BestScoreMode,TestMethod," +
                         $"FloatType,TurnsNumber0,TurnsNumber1 FROM SportProjectInfos WHERE Name='{projectName}'");
                string sql0 = $"SELECT Id,ProjectId,Name FROM DbGroupInfos WHERE ProjectId='{SportProjectDic["Id"]}'";
                int.TryParse(SportProjectDic["Type"], out int SportProjectDicType);

                ///查询本项目已考组
                if (!string.IsNullOrEmpty(groupName))
                {
                    sql0 += $" AND Name = '{groupName}'";
                }
                List<Dictionary<string, string>> sqlGroupsResults = sQLiteHelper.ExecuteReaderList(sql0);
                UploadResultsRequestParameter urrp = new UploadResultsRequestParameter();
                urrp.AdminUserName = localInfos["AdminUserName"];
                urrp.TestManUserName = localInfos["TestManUserName"];
                urrp.TestManPassword = localInfos["TestManPassword"];
                string MachineCode = localInfos["MachineCode"];
                string ExamId = localInfos["ExamId"];
                if (ExamId.IndexOf('_') != -1)
                {
                    ExamId = ExamId.Substring(ExamId.IndexOf('_') + 1);
                }
                urrp.ExamId = ExamId;

                if (MachineCode.IndexOf('_') != -1)
                {
                    MachineCode = MachineCode.Substring(MachineCode.IndexOf('_') + 1);
                }
                StringBuilder messageSb = new StringBuilder();
                StringBuilder logWirte = new StringBuilder();
                ///按组上传
                foreach (var sqlGroupsResult in sqlGroupsResults)
                {
                    string sql = $"SELECT Id,GroupName,Name,IdNumber,SchoolName,GradeName,ClassNumber,State,Sex,BeginTime,FinalScore,uploadState FROM DbPersonInfos" +
                        $" WHERE ProjectId='{SportProjectDic["Id"]}' AND GroupName = '{sqlGroupsResult["Name"]}'";

                    List<Dictionary<string, string>> list = sQLiteHelper.ExecuteReaderList(sql);
                    //轮次
                    urrp.MachineCode = MachineCode;
                    if (list.Count == 0)
                    {
                        continue;
                    }

                    StringBuilder resultSb = new StringBuilder();
                    List<SudentsItem> sudentsItems = new List<SudentsItem>();
                    //IdNumber 对应Id
                    Dictionary<string, string> map = new Dictionary<string, string>();
                    for (int i = 1; i <= _RoundCount; i++)
                    {
                        foreach (var stu in list)
                        {
                            List<RoundsItem> roundsItems = new List<RoundsItem>();
                            ///成绩
                            List<Dictionary<string, string>> resultScoreList1 = sQLiteHelper.ExecuteReaderList(
                                $"SELECT Id,CreateTime,RoundId,State,uploadState,Result FROM ResultInfos WHERE PersonId='{stu["Id"]}' And IsRemoved=0 And RoundId={i} LIMIT 1");
                            #region 查询文件

                            //成绩根目录
                            Dictionary<string, string> dic_images = new Dictionary<string, string>();
                            Dictionary<string, string> dic_viedos = new Dictionary<string, string>();
                            Dictionary<string, string> dic_texts = new Dictionary<string, string>();
                            //string scoreRoot = Application.StartupPath + $"\\Scores\\{projectName}\\{stu["GroupName"]}\\";

                            #endregion 查询文件

                            foreach (var item in resultScoreList1)
                            {
                                //if (item["uploadState"] != "0") continue;
                                DateTime.TryParse(item["CreateTime"], out DateTime dtBeginTime);
                                string dateStr = dtBeginTime.ToString("yyyyMMdd");
                                string GroupNo = $"{dateStr}_{stu["GroupName"]}_{stu["IdNumber"]}_{item["RoundId"]}";
                                //轮次成绩
                                RoundsItem rdi = new RoundsItem();
                                rdi.RoundId = Convert.ToInt32(item["RoundId"]);
                                rdi.State = ResultState.ResultState2Str(item["State"]);
                                rdi.Time = item["CreateTime"];
                                double.TryParse(item["Result"], out double score);
                                if (UploadUnit == 1 && SportProjectDicType != 2)
                                {
                                    score *= 100;
                                }
                                if (item["State"] != "1") score = 0;
                                rdi.Result = score;
                                //string.Format("{0:D2}:{1:D2}", ts.Minutes, ts.Seconds);
                                rdi.GroupNo = GroupNo;
                                rdi.Text = dic_texts;
                                rdi.Images = dic_images;
                                rdi.Videos = dic_viedos;
                                rdi.Memo = "";
                                roundsItems.Add(rdi);
                            }
                            if (roundsItems.Count > 0)
                            {
                                SudentsItem ssi = new SudentsItem();
                                ssi.SchoolName = stu["SchoolName"];
                                ssi.GradeName = stu["GradeName"];
                                ssi.ClassNumber = stu["ClassNumber"];
                                ssi.Name = stu["Name"];
                                ssi.IdNumber = stu["IdNumber"];
                                ssi.Rounds = roundsItems;
                                sudentsItems.Add(ssi);
                                if (!map.Keys.Contains(stu["IdNumber"]))
                                {
                                    map.Add(stu["IdNumber"], stu["Id"]);
                                }
                            }
                        }
                    }

                    #region 上传数据包装

                    if (sudentsItems.Count == 0) continue;
                    urrp.Sudents = sudentsItems;
                    //序列化json
                    string JsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(urrp);
                    string url = localInfos["Platform"] + RequestUrl.UploadResults;

                    var httpUpload = new HttpUpload();
                    var formDatas = new List<FormItemModel>();
                    //添加其他字段
                    formDatas.Add(new FormItemModel()
                    {
                        Key = "data",
                        Value = JsonStr
                    });

                    logWirte.AppendLine();
                    logWirte.AppendLine();
                    logWirte.AppendLine(JsonStr);
                    //上传学生成绩
                    string result = HttpUpload.PostForm(url, formDatas);
                    UpLoadResult upload_Result = Newtonsoft.Json.JsonConvert.DeserializeObject<UpLoadResult>(result);
                    string errorStr = "null";
                    List<Dictionary<string, int>> result1 = upload_Result.Results;
                    foreach (var item in sudentsItems)
                    {
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        //map
                        dic.Add("Id", map[item.IdNumber]);
                        dic.Add("IdNumber", item.IdNumber);
                        dic.Add("Name", item.Name);
                        dic.Add("uploadGroup", item.Rounds[0].GroupNo);
                        dic.Add("RoundId", item.Rounds[0].RoundId.ToString());
                        int roundId = item.Rounds[0].RoundId;
                        roundId--;
                        var value = 0;
                        // result1.FindAll(a => a.TryGetValue(item.IdNumber, out value));
                        List<Dictionary<string, int>> list1 = result1.FindAll(a => a.Keys.Contains(item.IdNumber));
                        if (roundId < list1.Count)
                        {
                            list1[roundId].TryGetValue(item.IdNumber, out value);
                            if (value == 1 || value == -4)
                            {
                                successList.Add(dic);
                            }
                            else if (value != 0)
                            {
                                errorStr = UpLoadResults.Match(value);
                                dic.Add("error", errorStr);
                                errorList.Add(dic);
                                messageSb.AppendLine($"{sqlGroupsResult["Name"]}组 考号:{item.IdNumber} 姓名:{item.Name}上传失败,错误内容:{errorStr}");
                            }
                        }

                    }

                    #endregion 上传数据包装
                }
                LoggerHelper.Monitor(logWirte.ToString());

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"成功:{successList.Count},失败:{errorList.Count}");
                sb.AppendLine("****************error***********************");

                foreach (var item in errorList)
                {
                    sb.AppendLine($"考号:{item["IdNumber"]} 姓名:{item["Name"]} 错误:{item["error"]}");
                }
                sb.AppendLine("*****************error**********************");
                System.Data.SQLite.SQLiteTransaction sQLiteTransaction = sQLiteHelper.BeginTransaction();

                sb.AppendLine("******************success*********************");
                foreach (var item in successList)
                {
                    //更新成绩上传状态
                    string sql1 = $"UPDATE ResultInfos SET uploadState=1 WHERE PersonId={item["Id"]} and RoundId={item["RoundId"]}";
                    sQLiteHelper.ExecuteNonQuery(sql1);
                    sb.AppendLine($"考号:{item["IdNumber"]} 姓名:{item["Name"]}");
                }
                sQLiteHelper.CommitTransaction( ref sQLiteTransaction);
                sb.AppendLine("*******************success********************");

                try
                {
                    string txtpath = Application.StartupPath + $"\\Log\\upload\\";
                    if (!Directory.Exists(txtpath))
                    {
                        Directory.CreateDirectory(txtpath);
                    }
                    if (successList.Count != 0 || errorList.Count != 0)
                    {
                        txtpath = Path.Combine(txtpath, $"upload_{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt");
                        File.WriteAllText(txtpath, sb.ToString());
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Debug(ex);
                }

                string outpitMessage = messageSb.ToString();
                return outpitMessage;
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return ex.Message;
            }
        }


        #endregion 成绩上传

        #region 图像显示

        private OpenCvSharp.VideoWriter VideoOutPut;
        private OpenCvSharp.VideoWriter VideoOutPut1;
        private object bmpObj = new object();
        private Bitmap bmpSoure = new Bitmap(1, 1);
        private int _width;
        private int _height;
        private string jpName = "";
        private string jpDis = "";
        private bool jpStatus = true;//true是上定点,false是下定点
        //记录定标数据
        private List<TargetPoint> targetPoints = new List<TargetPoint>();
        private List<TargetPoint> targetPoints1 = new List<TargetPoint>();
        private PointFormWindow pf1 = new PointFormWindow();//? 画点界面
        private List<TargetPoint> TopListSort = new List<TargetPoint>();//? 顶部坐标列表
        private List<TargetPoint> BottomListSort = new List<TargetPoint>();//? 底部坐标列表
        private List<TargetPoint> TopListSort1 = new List<TargetPoint>();//? 顶部坐标列表
        private List<TargetPoint> BottomListSort1 = new List<TargetPoint>();//? 底部坐标列表
        private System.Drawing.Point cenMarkPoint;//? 铅球中心点  
        private System.Drawing.Point markerTopJumpY, markerBottomJumpY, markerTopJump, markerBottomJump, markerTmp, mouseMovePoint;
        private List<System.Drawing.Point[]> gfencePnts = new List<System.Drawing.Point[]>();
        private List<int[]> gfencePntsDisValue = new List<int[]>();
        private System.Drawing.Point cenMarkPoint1;//? 铅球中心点
        private System.Drawing.Point m_markerTopJumpY, m_markerBottomJumpY, m_markerTopJump, m_markerBottomJump, m_markerTmp, m_mouseMovePoint;
        private List<System.Drawing.Point[]> gfencePnts1 = new List<System.Drawing.Point[]>();
        private List<int[]> gfencePntsDisValue1 = new List<int[]>();

        private int GCCounta = 0;
        private int videoSourceRuningR0 = 0;
        private int frameRecSum = 0;//计算帧数


        /// <summary>
        /// 定标参数
        /// </summary>
        public int colum = 0;
        public int initDis = 0;
        public int distance = 0;
        public int nowColum = 0;
        private int formShowStatus = 0;

        //测试方向
        public int Directionmode = 0;
        public int Directionmode1 = 0;
        private object ImageQueuesLock = new object();
        private ConcurrentQueue<ImageAndIndex> ImageQueues = new ConcurrentQueue<ImageAndIndex>();
        private Bitmap backBp = null;
        private Bitmap resultBp = null;
        //? 是否实心球模式
        private bool isBallCheckBoxb = false;

        //? 是否展示选择
        private bool isShowImgList = true;

        //? 是不是铅球
        private bool isShotPut = false;
        //? 是不是坐位体前屈
        private bool isSitting = false;
        public List<ImgMsS> imgMs = new List<ImgMsS>();
        public List<ImgMsS> imgMs1 = new List<ImgMsS>();
        private string ReservedDigitsTxt = "0.000";


        /// <summary>
        /// 视频输入设备信息
        /// </summary>
        private FilterInfoCollection filterInfoCollection;
        /// <summary>
        /// RGB摄像头设备
        /// </summary>
        private VideoCaptureDevice rgbDeviceVideo;

        private FuseImage FuseImage = null;
        private int skipFrameDispR0 = 0;

        private int cameraSkip = 0;
        private int cameraSkip1 = 0;




        private OpenCvSharp.Point[] conPoints0 = new OpenCvSharp.Point[4];
        private OpenCvSharp.Point[] conPoints1 = new OpenCvSharp.Point[4];
        /// <summary>
        /// 是否在多边形内
        /// </summary>
        public bool[][] isInTrangle;

        /// <summary>
        /// 更新标点显示
        /// </summary>
        private void updateTargetListView(bool flag = false, int flag1 = -1)
        {
            if (flag1 == -1) flag1 = distanceMode;
            if (flag1 == 0)
            {
                //gfencePnts
                gfencePnts.Clear();
                gfencePntsDisValue.Clear();
                List<TargetPoint> TopList = targetPoints.FindAll(a => a.status);
                List<TargetPoint> BottomList = targetPoints.FindAll(a => !a.status);
                TopListSort.Clear();
                BottomListSort.Clear();
                TopListSort = TopList.OrderBy(a => a.x).ToList();
                BottomListSort = BottomList.OrderBy(a => a.x).ToList();

                int min = TopListSort.Count > BottomListSort.Count ? BottomListSort.Count : TopListSort.Count;
                int max = TopListSort.Count > BottomListSort.Count ? TopListSort.Count : BottomListSort.Count;
                for (int i = 0; i < min; i++)
                {
                    if (i <= min - 2)
                    {
                        System.Drawing.Point[] fpnts = new System.Drawing.Point[4];
                        int[] DisValue = new int[2];
                        System.Drawing.Point p = new System.Drawing.Point(TopListSort[i].x, TopListSort[i].y);
                        System.Drawing.Point p1 = new System.Drawing.Point(TopListSort[i + 1].x, TopListSort[i + 1].y);
                        System.Drawing.Point p2 = new System.Drawing.Point(BottomListSort[i].x, BottomListSort[i].y);
                        System.Drawing.Point p3 = new System.Drawing.Point(BottomListSort[i + 1].x, BottomListSort[i + 1].y);
                        int maxv = 0;
                        int minv = 0;
                        minv = Math.Min(BottomListSort[i].dis, BottomListSort[i + 1].dis);
                        maxv = Math.Max(BottomListSort[i].dis, BottomListSort[i + 1].dis);
                        DisValue[0] = minv;
                        DisValue[1] = maxv;
                        fpnts[0] = p;
                        fpnts[1] = p1;
                        fpnts[2] = p2;
                        fpnts[3] = p3;
                        gfencePnts.Add(fpnts);
                        gfencePntsDisValue.Add(DisValue);
                    }
                }
                if (gfencePnts.Count > 0 && gfencePnts[0].Length > 2)
                {
                    System.Drawing.Point bottomStartP = gfencePnts[0][0];
                    System.Drawing.Point bottomEndP = gfencePnts[0][2];
                    int x = (bottomStartP.X + bottomEndP.X) / 2;
                    int y = (bottomStartP.Y + bottomEndP.Y) / 2;
                    cenMarkPoint = new System.Drawing.Point(x, y);
                    Console.WriteLine();
                }
                if (flag)
                {
                    //gfencePnts
                    int igfInde = gfencePnts.Count - 1;
                    OpenCvSharp.Point[] conPointsTemp = new OpenCvSharp.Point[4];
                    conPointsTemp[0] = new OpenCvSharp.Point(gfencePnts[0][0].X, gfencePnts[0][0].Y);
                    conPointsTemp[1] = new OpenCvSharp.Point(gfencePnts[0][2].X, gfencePnts[0][2].Y);
                    conPointsTemp[2] = new OpenCvSharp.Point(gfencePnts[igfInde][1].X, gfencePnts[igfInde][1].Y);
                    conPointsTemp[3] = new OpenCvSharp.Point(gfencePnts[igfInde][3].X, gfencePnts[igfInde][3].Y);
                    OpenCvSharp.Rect rect0 = Cv2.BoundingRect(conPointsTemp);
                    conPoints0[0] = rect0.TopLeft;
                    conPoints0[1] = new OpenCvSharp.Point(rect0.X + rect0.Width, rect0.Y);
                    conPoints0[2] = new OpenCvSharp.Point(rect0.X, rect0.Y + rect0.Height);
                    conPoints0[3] = new OpenCvSharp.Point(rect0.X + rect0.Width, rect0.Y + rect0.Height);

                    int minWidth = conPoints0[0].X;
                    int maxWidth = conPoints0[3].X;
                    int minHeigh = conPoints0[0].Y;
                    int maxHeigh = conPoints0[3].Y;
                    isInTrangle = new bool[1280][];
                    for (int i = 0; i < 1280; i++)
                    {
                        isInTrangle[i] = new bool[720];
                    }

                    for (int i = minWidth; i <= maxWidth; i++)
                    {
                        for (int j = minHeigh + 1; j < maxHeigh; j++)
                        {
                            //按顺序
                            if (PointHelper.Instance.InQuadrangle(conPointsTemp[0], conPointsTemp[1], conPointsTemp[3], conPointsTemp[2], new OpenCvSharp.Point(i, j)))
                            {
                                isInTrangle[i][j] = true;

                            }
                        }
                    }

                }
            }
            else if (flag1 == 1)
            {
                //gfencePnts
                gfencePnts1.Clear();
                gfencePntsDisValue1.Clear();
                List<TargetPoint> TopList = targetPoints1.FindAll(a => a.status);
                List<TargetPoint> BottomList = targetPoints1.FindAll(a => !a.status);
                TopListSort1.Clear();
                BottomListSort1.Clear();
                TopListSort1 = TopList.OrderBy(a => a.x).ToList();
                BottomListSort1 = BottomList.OrderBy(a => a.x).ToList();

                int min = TopListSort1.Count > BottomListSort1.Count ? BottomListSort1.Count : TopListSort1.Count;
                int max = TopListSort1.Count > BottomListSort1.Count ? TopListSort1.Count : BottomListSort1.Count;
                for (int i = 0; i < min; i++)
                {
                    if (i <= min - 2)
                    {
                        System.Drawing.Point[] fpnts = new System.Drawing.Point[4];
                        int[] DisValue = new int[2];
                        System.Drawing.Point p = new System.Drawing.Point(TopListSort1[i].x, TopListSort1[i].y);
                        System.Drawing.Point p1 = new System.Drawing.Point(TopListSort1[i + 1].x, TopListSort1[i + 1].y);
                        System.Drawing.Point p2 = new System.Drawing.Point(BottomListSort1[i].x, BottomListSort1[i].y);
                        System.Drawing.Point p3 = new System.Drawing.Point(BottomListSort1[i + 1].x, BottomListSort1[i + 1].y);
                        int maxv = 0;
                        int minv = 0;
                        minv = Math.Min(BottomListSort1[i].dis, BottomListSort1[i + 1].dis);
                        maxv = Math.Max(BottomListSort1[i].dis, BottomListSort1[i + 1].dis);
                        DisValue[0] = minv;
                        DisValue[1] = maxv;
                        fpnts[0] = p;
                        fpnts[1] = p1;
                        fpnts[2] = p2;
                        fpnts[3] = p3;
                        gfencePnts1.Add(fpnts);
                        gfencePntsDisValue1.Add(DisValue);
                    }
                }
                if (gfencePnts1.Count > 0 && gfencePnts1[0].Length > 2)
                {
                    System.Drawing.Point bottomStartP = gfencePnts1[0][0];
                    System.Drawing.Point bottomEndP = gfencePnts1[0][2];
                    int x = (bottomStartP.X + bottomEndP.X) / 2;
                    int y = (bottomStartP.Y + bottomEndP.Y) / 2;
                    cenMarkPoint1 = new System.Drawing.Point(x, y);
                    Console.WriteLine();
                }
                if (flag)
                {
                    //gfencePnts
                    int igfInde = gfencePnts1.Count - 1;
                    conPoints1[0] = new OpenCvSharp.Point(gfencePnts1[0][0].X, gfencePnts1[0][0].Y);
                    conPoints1[1] = new OpenCvSharp.Point(gfencePnts1[0][2].X, gfencePnts1[0][2].Y);
                    conPoints1[2] = new OpenCvSharp.Point(gfencePnts1[igfInde][1].X, gfencePnts1[igfInde][1].Y);
                    conPoints1[3] = new OpenCvSharp.Point(gfencePnts1[igfInde][3].X, gfencePnts1[igfInde][3].Y);
                    //OpenCvSharp.Rect rect = Cv2.BoundingRect(contours[i]);
                    OpenCvSharp.Rect rect0 = Cv2.BoundingRect(conPoints1);
                    conPoints1[0] = rect0.TopLeft;
                    conPoints1[1] = new OpenCvSharp.Point(rect0.X + rect0.Width, rect0.Y);
                    conPoints1[2] = new OpenCvSharp.Point(rect0.X, rect0.Y + rect0.Height);
                    conPoints1[3] = new OpenCvSharp.Point(rect0.X + rect0.Width, rect0.Y + rect0.Height);
                }
            }


        }
        /// <summary>
        /// 
        /// </summary>
        private void CameraInit()
        {
            _width = 1280;
            _height = 720;
            openCameraSetting();
            recTimeTxt.SelectedIndex = 19;       
        }
        public List<string> ExistMonikerStrings = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool LoadCamera0(int index)
        {
            bool flag = false;
            try
            {
                if (rgbVideoSource.IsRunning)
                {
                    rgbVideoSource.SignalToStop();
                    //rgbVideoSource.Hide();
                }
                Boolean findIt = false;
                string MonikerString = string.Empty;
                if (index < filterInfoCollection.Count)
                {
                    FilterInfo device = filterInfoCollection[index];
                    rgbDeviceVideo = new VideoCaptureDevice(device.MonikerString);

                    for (int i = 0; i < rgbDeviceVideo.VideoCapabilities.Length; i++)
                    {
                        if (rgbDeviceVideo.VideoCapabilities[i].FrameSize.Width == _width
                            && rgbDeviceVideo.VideoCapabilities[i].FrameSize.Height == _height)
                        {
                            rgbDeviceVideo.VideoResolution = rgbDeviceVideo.VideoCapabilities[i];
                            findIt = true;
                            break;
                        }
                    }
                    if (!findIt)
                    {
                        rgbDeviceVideo = new VideoCaptureDevice(device.MonikerString);
                    }
                }

                if (findIt)
                {
                    rgbVideoSource.VideoSource = rgbDeviceVideo;
                    rgbVideoSource.Start();
                    //rgbVideoSource.Show();
                    rgbVideoSource.SendToBack();
                    flag = true;
                    ExistMonikerStrings.Add(rgbVideoSource.VideoSource.Source);
                }
            }
            catch (Exception ex)
            {
                flag = false;
                LoggerHelper.Debug(ex);
            }
            if (rgbVideoSource.IsRunning)
            { flag = true; }
            else
            { flag = false; }
            if (!flag)
            {
                openCameraBtn.Text = "打开摄像头";
                openCameraBtn.BackColor = Color.White;
                serialConnectStripStatusLabel2.Text = "摄像头未开启";
                serialConnectStripStatusLabel2.ForeColor = Color.Red;
                FrmTips.ShowTipsError(this, "打开摄像头失败!");
            }
            else
            {
                serialConnectStripStatusLabel2.Text = "摄像头开启中";
                serialConnectStripStatusLabel2.ForeColor = Color.Green;
                openCameraBtn.Text = "关闭摄像头";
                openCameraBtn.BackColor = Color.Red;
            }

            return flag;
        }
        /// <summary>
        /// 
        /// </summary>
        public void CloseCamera()
        {
            try
            {
                if (!rgbVideoSource.IsRunning) return;
                if (rgbVideoSource != null && rgbVideoSource.IsRunning)
                {
                    rgbVideoSource.SignalToStop();
                    //rgbVideoSource.Hide();
                }
                ExistMonikerStrings.RemoveAll(a => a == rgbVideoSource.VideoSource.Source);
                openCameraBtn.Text = "打开摄像头";
                serialConnectStripStatusLabel2.Text = "摄像头未开启";
                serialConnectStripStatusLabel2.ForeColor = Color.Red;
                openCameraBtn.BackColor = Color.White;
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool LoadCamera1(int index)
        {
            bool flag = false;
            try
            {
                if (videoSourcePlayer1.IsRunning)
                {
                    videoSourcePlayer1.SignalToStop();
                    //rgbVideoSource.Hide();
                }
                Boolean findIt = false;
                if (index < filterInfoCollection.Count)
                {
                    FilterInfo device = filterInfoCollection[index];
                    rgbDeviceVideo = new VideoCaptureDevice(device.MonikerString);
                    for (int i = 0; i < rgbDeviceVideo.VideoCapabilities.Length; i++)
                    {
                        if (rgbDeviceVideo.VideoCapabilities[i].FrameSize.Width == _width
                            && rgbDeviceVideo.VideoCapabilities[i].FrameSize.Height == _height)
                        {
                            rgbDeviceVideo.VideoResolution = rgbDeviceVideo.VideoCapabilities[i];
                            findIt = true;
                            break;
                        }
                    }
                }

                if (findIt)
                {
                    videoSourcePlayer1.VideoSource = rgbDeviceVideo;
                    videoSourcePlayer1.Start();
                    //rgbVideoSource.Show();
                    videoSourcePlayer1.SendToBack();
                    flag = true;
                    ExistMonikerStrings.Add(videoSourcePlayer1.VideoSource.Source);
                }
            }
            catch (Exception ex)
            {
                flag = false;
                LoggerHelper.Debug(ex);
            }
            if (videoSourcePlayer1.IsRunning)
            { flag = true; }
            else
            { flag = false; }
            if (!flag)
            {
                openCameraBtn1.Text = "打开摄像头";
                openCameraBtn1.BackColor = Color.White;
                serialConnectStripStatusLabel1.Text = "摄像头未开启";
                serialConnectStripStatusLabel1.ForeColor = Color.Red;
                FrmTips.ShowTipsError(this, "打开摄像头失败!");
            }
            else
            {
                serialConnectStripStatusLabel1.Text = "摄像头开启中";
                serialConnectStripStatusLabel1.ForeColor = Color.Green;
                openCameraBtn1.Text = "关闭摄像头";
                openCameraBtn1.BackColor = Color.Red;
            }
            return flag;
        }
        /// <summary>
        /// 
        /// </summary>
        public void CloseCamera1()
        {
            try
            {
                if (!videoSourcePlayer1.IsRunning) return;
                if (videoSourcePlayer1 != null && videoSourcePlayer1.IsRunning)
                {

                    videoSourcePlayer1.SignalToStop();
                    //rgbVideoSource.Hide();
                }

                ExistMonikerStrings.RemoveAll(a => a == videoSourcePlayer1.VideoSource.Source);
                openCameraBtn1.Text = "打开摄像头";
                serialConnectStripStatusLabel1.Text = "摄像头未开启";
                serialConnectStripStatusLabel1.ForeColor = Color.Red;
                openCameraBtn1.BackColor = Color.White;
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool LoadCamera(int index)
        {
            bool flag = false;
            if (distanceMode == 0)
            {
                //落点摄像头
                LoadCamera0(index);
            }
            else if (distanceMode == 1)
            {

                LoadCamera1(index);
            }

            return flag;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool rgbVideoSourceStart()
        {
            bool flag = false;

            if (rgbVideoSource != null)
            {
                rgbVideoSource.Start();
                //rgbVideoSource.Show();
                return true;
            }
            if (string.IsNullOrEmpty(cameraName))
            {
                FrmTips.ShowTipsError(this, "请选择摄像头!");
                return false;
            }
            //flag = openCamearaFun(cameraName);
            flag = LoadCamera0(cameraIndex);
            return flag;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool rgbVideoSourceStart1()
        {
            bool flag = false;

            if (videoSourcePlayer1 != null)
            {
                videoSourcePlayer1.Start();
                //rgbVideoSource.Show();
                return true;
            }
            if (string.IsNullOrEmpty(cameraName1))
            {
                FrmTips.ShowTipsError(this, "请选择摄像头!");
                return false;
            }
            //flag = openCamearaFun(cameraName);
            flag = LoadCamera1(cameraIndex1);
            return flag;
        }
        /// <summary>
        /// 
        /// </summary>
        public void rgbVideoSourceStop()
        {
            CloseCamera();

        }
        /// <summary>
        /// 
        /// </summary>
        public void rgbVideoSourceStop1()
        {
            CloseCamera1();
        }
        /// <summary>
        /// 调用写入视频流模式 0:opencvsharp 1:aforge.video.ffmpeg
        /// </summary>
        int videoWriteMode = 0;
        // VideoFileWriter vfw = null;
        /// <summary>
        /// 打开写入本地流
        /// </summary>
        /// <param name="outPath"></param>
        public void OpenVideoOutPut(string outPath)
        {
            try
            {
                ReleaseVideoOutPut();
                if (imgMs.Count == 0) return;
                if (videoWriteMode == 0)
                {
                    ///opencvsharp 写入视频
                    VideoOutPut = new OpenCvSharp.VideoWriter(outPath, OpenCvSharp.FourCC.MP42, 60, new OpenCvSharp.Size(_width, _height));
                    //VideoOutPut = new OpenCvSharp.VideoWriter(outPath, VideoWriter.FourCC("@XVID"), 30, new OpenCvSharp.Size(_width, _height));
                    bool flag = VideoOutPut.IsOpened();
                    Console.WriteLine();
                }
                else if (videoWriteMode == 1)
                {
                    /* outPath = outPath.Replace("mp4", "avi");
                     //aforge.video.ffmpeg写入视频
                     vfw = new VideoFileWriter();
                     vfw.Open(outPath, _width, _height, 30, VideoCodec.MPEG4);*/

                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="outPath"></param>
        public void OpenVideoOutPut1(string outPath)
        {
            try
            {

                if (VideoOutPut1 != null && VideoOutPut1.IsOpened())
                {
                    VideoOutPut1.Release();
                }
                if (imgMs1.Count == 0) return;
                VideoOutPut1 = new OpenCvSharp.VideoWriter(outPath, OpenCvSharp.FourCC.MP42, 60, new OpenCvSharp.Size(_width, _height));
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool isVideoOutPutOpen()
        {
            bool flag = false;
            if (VideoOutPut != null)// && VideoOutPut.IsOpened()
            {
                flag = true;
            }

            return flag;
        }
        /// <summary>
        /// 写入视频
        /// </summary>
        /// <param name="bitmap"></param>
        public void VideoWriteFrame(Bitmap bitmap)
        {
            if (videoWriteMode == 0)
            {
                OpenCvSharp.Mat mat = ImageHelper.Bitmap2Mat(bitmap);
                VideoOutPut.Write(mat);
            }
            else if (videoWriteMode == 1)
            {
                //vfw.WriteVideoFrame(bitmap);
            }
            //bitmap.Dispose();

        }
        /// <summary>
        /// 释放写入本地流
        /// </summary>
        public void ReleaseVideoOutPut()
        {
            try
            {
                if (videoWriteMode == 0)
                {
                    if (VideoOutPut != null && VideoOutPut.IsOpened())
                    {
                        VideoOutPut.Release();
                    }
                }
                else if (videoWriteMode == 1)
                {
                    /* if (vfw != null && vfw.IsOpen)
                     {
                         vfw.Close();
                         vfw.Dispose();
                     }*/
                }

            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void ReleaseVideoOutPut1()
        {
            try
            {
                if (VideoOutPut1 != null && VideoOutPut1.IsOpened())
                {
                    VideoOutPut1.Release();
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cameraIndex"></param>
        /// <returns></returns>
        private bool openCamearaFun(int cameraIndex)
        {
            bool flag = LoadCamera(cameraIndex);

            return flag;
        }
        /// <summary>
        /// 
        /// </summary>
        private void saveMarkSetting()
        {
            if (distanceMode == 0)
            {
                StringBuilder builder = new StringBuilder();
                foreach (var t in targetPoints)
                {
                    builder.Append(t.name + ";");
                    builder.Append(t.x + ",");
                    builder.Append(t.y + ";");
                    builder.Append(t.dis + ";");
                    if (t.status)
                    {
                        builder.Append("1\r\n");
                    }
                    else
                    {
                        builder.Append("0\r\n");
                    }
                }
                TextFileHelper.Instance.Write(strMainModule + markPointFile, builder.ToString());
                TextFileHelper.Instance.Write(strMainModule + markDirectionFile, Directionmode.ToString());
            }
            else if (distanceMode == 1)
            {
                StringBuilder builder = new StringBuilder();
                foreach (var t in targetPoints1)
                {
                    builder.Append(t.name + ";");
                    builder.Append(t.x + ",");
                    builder.Append(t.y + ";");
                    builder.Append(t.dis + ";");
                    if (t.status)
                    {
                        builder.Append("1\r\n");
                    }
                    else
                    {
                        builder.Append("0\r\n");
                    }
                }
                TextFileHelper.Instance.Write(strMainModule + markPointFile1, builder.ToString());
                TextFileHelper.Instance.Write(strMainModule + markDirectionFile1, Directionmode1.ToString());
            }

        }
        /// <summary>
        /// 刷新视频显示
        /// </summary>
        private void rgbVideoSourceRePaint()
        {
            if (distanceMode == 0)
            {
                if (!rgbVideoSource.IsRunning)
                    rgbVideoSource.Refresh();

            }
            else if (distanceMode == 1)
            {
                if (!videoSourcePlayer1.IsRunning)
                    videoSourcePlayer1.Refresh();
            }
            RefreshPictureBox();
        }

        int tolerance = 60;
        private bool rgbVideoSourcePaintFlag = false;
        FuseBitmap fb = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="image"></param>
        private void rgbVideoSource_NewFrame(object sender, ref Bitmap image)
        {
            FrameFlash0++;
            skipFrameDispR0++;

            if (rgbVideoSourcePaintFlag) return;
            rgbVideoSourcePaintFlag = true;
            try
            {
                if (skipFrameDispR0 < cameraSkip)
                {
                    return;
                }
                skipFrameDispR0 = 0;
                if (image != null)
                {
                    //image = (Bitmap)ImageUtil.ScaleImage(image, _width, _height);
                    //得到当前RGB摄像头下的图片
                    Bitmap bmp = ImageHelper.DeepCopyBitmap(image);
                    if (bmp == null) return;
                    ///处理录像数据
                    backBp = ImageHelper.DeepCopyBitmap(bmp);
                    Bitmap dstBitmap = null;
                    if (isSitting)
                    {
                        try
                        {
                            if (FuseBitmap.GetBackGroundBmp() == null || updateBackImgFlag)
                            {
                                FuseBitmap.setBackGround(backBp, tolerance);
                                updateBackImgFlag = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerHelper.Debug(ex);
                        }
                        finally
                        {
                            updateBackImgFlag = false;
                        }

                        fb = new FuseBitmap(bmp);
                        fb.SetRect(conPoints0, isInTrangle);
                        fb.FuseColorImg(tolerance, isShowHandFlag);
                        fb.Dispose();
                        dstBitmap = fb.GetDstBitmap();
                    }
                    if (recTimeR0 > 0)
                    {
                        if (isBallCheckBoxb)
                        {
                            Bitmap bp = ImageHelper.DeepCopyBitmap(bmp);
                            // Bitmap dstcopy1 = (Bitmap)ImageHelper.ScaleImage(bp, 320, 180);
                            Bitmap dstcopy1 = (Bitmap)ImageHelper.ResizeImage(bp, 640, 360);

                            ImageAndIndex iai = new ImageAndIndex();
                            iai.index = frameSum;
                            iai.image = dstcopy1;
                            lock (ImageQueuesLock)
                            {
                                ImageQueues.Enqueue(iai);
                            }
                        }
                        ImgMsS mss = new ImgMsS();
                        mss.dt = DateTime.Now;
                        mss.name = "img" + imgMs.Count;
                        //坐位体前屈
                        if (isSitting && fb != null && dstBitmap != null)
                        {
                            mss.img = ImageHelper.Bitmap2MemoryStream(dstBitmap);
                            mss.isHand = fb.GetIsHand();
                        }
                        else
                        {
                            Bitmap bitmap = ImageHelper.DeepCopyBitmap(bmp);
                            mss.img = ImageHelper.Bitmap2MemoryStream(bitmap);
                        }
                        imgMs.Add(mss);
                        frameSum++;
                    }
                    if (isSitting && fb != null && dstBitmap != null)
                    {
                        bmp = ImageHelper.DeepCopyBitmap(dstBitmap);
                    }
                    frameRecSum++;//计算帧速用
                    if (distanceMode == 0)
                    {
                        //显示图片
                        pictureBox1.Image = bmp;
                    }
                    if (FrameFlash0 > FrameFlashCount0)
                    {
                        FrameFlash0 = 0;

                    }

                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
            finally
            {
                GCCounta++;
                if (GCCounta > 10)
                {
                    GCCounta = 0;
                    clearMemory();
                }
                rgbVideoSourcePaintFlag = false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            markerTmp.X = e.X;
            markerTmp.Y = e.Y;
            mouseMovePoint.X = e.X;
            mouseMovePoint.Y = e.Y;
            if (formShowStatus > 0 && distanceMode == 0)
            {
                bool flag = false;
                //添加定标
                targetPoints.Add(new TargetPoint()
                {
                    x = markerTmp.X,
                    y = markerTmp.Y,
                    name = jpName,
                    dis = PointHelper.Instance.Str2int(jpDis),//cm
                    status = jpStatus
                });
                if (targetPoints.Count == colum * 2)
                {
                    ///定标结束
                    formShowStatus = 0;
                    pf1.Hide();
                    saveMarkSetting();
                    clearMemory();
                    flag = true;
                }
                else
                {
                    pf1.updateFlp(++nowColum);
                    jpName = nowColum + "";
                    if (nowColum % 2 != 0)
                    {
                        initDis += distance;
                    }
                    jpDis = initDis + "";
                    jpStatus = !jpStatus;
                    System.Drawing.Point ptc = this.PointToScreen(new System.Drawing.Point(e.X, e.Y - 100));
                    pf1.Location = ptc;
                }
                updateTargetListView(flag);
            }
            dispJumpLength1(e.X, e.Y);
            //rgbVideoSourceRePaint();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            mouseMovePoint.X = e.X;
            mouseMovePoint.Y = e.Y;
            if (formShowStatus > 0 && distanceMode == 0)
            {
                Task.Run(() =>
                {
                    System.Drawing.Point ptc = this.PointToScreen(new System.Drawing.Point(e.X, e.Y - 100));
                    pf1.Location = ptc;
                });
            }
            else
            {
                if (Measure)
                    dispJumpLength1(e.X, e.Y);
            }
            if (Measure)
            {
                rgbVideoSourceRePaint();
            }

        }
        [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        //Graphics vGraphics; IntPtr vHandle;
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            //if (recTimeR0 > 0) return;

            flashCount++;
            #region 画图
            Graphics vGraphics = e.Graphics;
            Pen pen = new Pen(Color.MediumSpringGreen, 3);
            vGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Font drawFont = new Font("Arial", 14);
            SolidBrush drawBrush = new SolidBrush(Color.MediumSpringGreen);
            List<TargetPoint> TopList = targetPoints.FindAll(a => a.status);
            List<TargetPoint> BottomList = targetPoints.FindAll(a => !a.status);
            List<TargetPoint> TopListSort = TopList.OrderBy(a => a.x).ToList();
            List<TargetPoint> BottomListSort = BottomList.OrderBy(a => a.x).ToList();
            //框点十字
            int left = 0;
            foreach (var mark in TopListSort)
            {
                System.Drawing.Point p = new System.Drawing.Point(mark.x, mark.y);
                if (left <= TopListSort.Count - 2)
                {
                    System.Drawing.Point p1 = new System.Drawing.Point(TopListSort[left + 1].x, TopListSort[left + 1].y);
                    vGraphics.DrawLine(pen, p, p1);
                    left++;
                }
                PointHelper.Instance. DrawPointCross(vGraphics, p, pen);
                PointHelper.Instance. DrawPointText(vGraphics, $"({mark.name}){mark.dis}cm", p, drawFont, drawBrush, 0, 20, 0, 30);
            }
            left = 0;
            foreach (var mark in BottomListSort)
            {
                System.Drawing.Point p = new System.Drawing.Point(mark.x, mark.y);

                if (left <= BottomListSort.Count - 2)
                {
                    System.Drawing.Point p1 = new System.Drawing.Point(BottomListSort[left + 1].x, BottomListSort[left + 1].y);
                    vGraphics.DrawLine(pen, p, p1);
                    left++;
                }
                 PointHelper.Instance. DrawPointCross(vGraphics, p, pen);
                  PointHelper.Instance.DrawPointText(vGraphics, $"({mark.name}){mark.dis}cm", p, drawFont, drawBrush, 0, 20, 1, 10);
            }
            //中间框点连线
            int min = TopListSort.Count > BottomListSort.Count ? BottomListSort.Count : TopListSort.Count;
            for (int i = 0; i < min; i++)
            {
                if (i != 0 && i != min - 1)
                {
                    continue;
                }
                TargetPoint mark = TopListSort[i];
                TargetPoint mark1 = BottomListSort[i];
                System.Drawing.Point p = new System.Drawing.Point(mark.x, mark.y);
                System.Drawing.Point p1 = new System.Drawing.Point(mark1.x, mark1.y);
                vGraphics.DrawLine(pen, p, p1);
            }
            //鼠标画十字
            Pen pen1 = new Pen(Color.Red, 1);
            PointHelper.Instance. DrawPointCross(vGraphics, mouseMovePoint, pen1);
            double fenmu = 1000;
            if (isSitting)
            {
                fenmu = 10;
            }
            string LenX = (MeasureLenX / fenmu).ToString(ReservedDigitsTxt);
            string LenY = (MeasureLenY / fenmu).ToString(ReservedDigitsTxt);
            string Len = (MeasureLen / fenmu).ToString(ReservedDigitsTxt);

            pen.Color = Color.Red;
            vGraphics.DrawLine(pen, markerTopJump, markerBottomJump);
              PointHelper.Instance. DrawPointCross(vGraphics, mousePoint, pen1);
            drawFont = new Font("微软雅黑", 28, FontStyle.Bold);
            drawBrush = new SolidBrush(Color.Red);// Create point for upper-left corner of drawing.
              PointHelper.Instance.DrawPointText(vGraphics, $"成绩:{Len}{dangwei}", new System.Drawing.Point(10, 10), drawFont, drawBrush, 1, 10, 1, 0);
            vGraphics.FillRectangle(new SolidBrush(Color.White), new Rectangle(new System.Drawing.Point(350, 0), new System.Drawing.Size(1000, 50)));
            //时间
            vGraphics.DrawString($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}", new Font("微软雅黑", 14), drawBrush, 350, 15);
            //考生姓名和学号
            if (isHaveStudent(false))
            {
                vGraphics.DrawString($"组号:{nowRaceStudentData.groupName} 考号:{nowRaceStudentData.idNumber} 姓名:{nowRaceStudentData.name}", new Font("微软雅黑", 14), drawBrush, 550, 15);
            }

            vGraphics.DrawString($"X:{LenX},Y:{LenY}", new Font("微软雅黑", 14, FontStyle.Bold), new SolidBrush(Color.Blue), 1100, 15);
            if (isShotPut)
            {
                pen.DashPattern = new float[] { 5f, 10f };
                vGraphics.DrawLine(pen, cenMarkPoint, mousePoint);
            }

            #region 方框
            #endregion 方框
            #endregion 画图
        }
        private int sleepCount = 0;
        private int RecordEnd = 0;
            //录像结束剩余图片总数
        private int remainderImgSum = 0;
            //录像结束 未处理图片数
        private int remainderImgCount = 0;
        /// <summary>
        /// 处理实心球合成图
        /// </summary>
        /// <param name="obj"></param>
        private void ImagePredictLabelQueues2ThreadFun(object obj)
        {
            while (true)
            {
                if (ImageQueues.Count == 0)
                {
                    Thread.Sleep(10);
                    sleepCount++;
                    if (sleepCount > 10)
                    {
                        Thread.Sleep(100);
                    }
                    if (RecordEnd == 1)
                    {
                        //剩余图片处理结束
                        RecordEnd = 2;
                        FuseImage.Dispose();
                        Bitmap dstcopy0 = FuseImage.DeepCopyBitmap(FuseImage.GetDstBitmap());
                        Bitmap dstcopy1 = (Bitmap)ImageHelper.ScaleImage(dstcopy0, 1280, 720);
                        ImgMsS mss = new ImgMsS();
                        mss.img = ImageHelper.Bitmap2MemoryStream(dstcopy1);
                        mss.dt = DateTime.Now;
                        mss.name = "img" + imgMs.Count;
                        imgMs.Add(mss);
                        if (!isSitting)
                        {
                            //setHScrollBarValue(imgMs.Count - 2);
                            pictureBox1.Image = ImageHelper.MemoryStream2Bitmap(imgMs[imgMs.Count - 1].img);
                            //setHScrollBarValue(imgMs.Count - 1);
                        }
                    }
                }
                else
                {
                    sleepCount = 0;
                    //处理剩余图片
                    if (RecordEnd == 1)
                    {
                        remainderImgSum = ImageQueues.Count;
                        remainderImgCount = 0;
                        bool flag = false;
                        ImageAndIndex iplr = new ImageAndIndex();
                        lock (ImageQueuesLock)
                        {
                            flag = ImageQueues.TryDequeue(out iplr);
                        }
                        while (flag)
                        {
                            remainderImgCount++;
                            FuseImage.FuseColorImg1(iplr.image);
                            lock (ImageQueuesLock)
                            {
                                flag = ImageQueues.TryDequeue(out iplr);
                            }
                        }
                        //剩余图片处理结束
                        RecordEnd = 2;
                        FuseImage.Dispose();
                        Bitmap dstcopy0 = FuseImage.DeepCopyBitmap(FuseImage.GetDstBitmap());
                        Bitmap dstcopy1 = (Bitmap)ImageHelper.ScaleImage(dstcopy0, 1280, 720);
                        ImgMsS mss = new ImgMsS();
                        mss.img = ImageHelper.Bitmap2MemoryStream(dstcopy1);
                        mss.dt = DateTime.Now;
                        mss.name = "img" + imgMs.Count;
                        imgMs.Add(mss);
                        FuseImage = null;
                        //setHScrollBarValue(imgMs.Count - 2);
                        pictureBox1.Image = ImageHelper.MemoryStream2Bitmap(imgMs[imgMs.Count - 1].img);
                        //setHScrollBarValue(imgMs.Count - 1);
                        startMeasure();
                    }
                    else
                    {
                        bool flag = false;
                        ImageAndIndex iplr = new ImageAndIndex();
                        lock (ImageQueuesLock)
                        {
                            flag = ImageQueues.TryDequeue(out iplr);
                        }
                        if (flag)
                        {
                            FuseImage.FuseColorImg1(iplr.image);
                        }
                    }
                }
                Thread.Sleep(10);
            }
        }
        /// <summary>
        /// 打开界面选择录像图片
        /// </summary>
        private void openImgList()
        {
            try
            {
                if (!isHaveStudent())
                {
                    tabControl1.SelectedIndex = 0;
                    return;
                }
                int sp = 0;
                FindImageWindow fi = new FindImageWindow();
                List<ImgMsS> imgMsTemp = imgMs;
                if (distanceMode == 1) imgMsTemp = imgMs1;
                fi.imgMs = imgMsTemp;
                //fi.nowTestDir = nowTestDir;
                if (fi.ShowDialog() == DialogResult.OK)
                {
                    nowFileName = fi.fileName;
                    //string fileName = nowTestDir + "\\" + fi.fileName + ".jpg";
                    sp = PointHelper.Instance.Str2int(nowFileName);
                    if (sp < imgMsTemp.Count - 1)
                    {
                        if (distanceMode == 0)
                            setHScrollBarValue(sp);
                        else if (distanceMode == 1)
                            setHScrollBarValue1(sp);
                    }
                }
                else
                {
                    sp = imgMsTemp.Count - 5;
                    nowFileName = sp + "";
                    if (sp >= imgMsTemp.Count)
                    {
                        sp = imgMsTemp.Count - 1;
                        nowFileName = sp + "";
                    }
                }
                if (sp < imgMsTemp.Count - 1)
                {
                    if (imgMsTemp[sp].img != null)
                    {
                        if (distanceMode == 0)
                            pictureBox1.Image = ImageHelper.MemoryStream2Bitmap(imgMsTemp[sp].img);
                        else if (distanceMode == 0)
                            pictureBox3.Image = ImageHelper.MemoryStream2Bitmap(imgMsTemp[sp].img);
                    }
                    else
                    {
                        if (distanceMode == 0)
                            pictureBox1.Image = ImageHelper.MemoryStream2Bitmap(imgMsTemp[imgMsTemp.Count - 1].img);
                        else if (distanceMode == 0)
                            pictureBox3.Image = ImageHelper.MemoryStream2Bitmap(imgMsTemp[imgMsTemp.Count - 1].img);
                    }
                }
                recImgIndex.Text = $"图片索引:{nowFileName}";
                startMeasure();
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }

        }
        int BeginRecTimeOut = 0;
        /// <summary>
        /// 开始录像
        /// </summary>
        private void BeginRec()
        {
            if (BeginRecTimeOut > 0) return;
            try
            {
                if (recTimeR0 > 0)//停止录像
                {
                    recTimeR0 = 1;
                    _endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    return;
                }
                if (label18.Text != "未测试")
                {
                    MessageBox.Show("该考生本轮已测试");
                    return;
                }
                bool flag = true;
                flag = beginTest();
                if (MeasureLen != 0 || recTimeR0 != 0)
                {
                    //flag = beginTest();
                }
                if (flag)
                {
                    startRec();
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
            finally
            {
                BeginRecTimeOut = 2;
            }


        }
        private string _beginTime = "";
        private string _endTime = "";
        /// <summary>
        /// 开始计时
        /// </summary>
        private void startRec()
        {
            _beginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _endTime = "";
            updateBackImgFlag = true;
            if (!isHaveStudent())
            {
                return;
            }
            recTimeR0 = 0;
            RecordEnd = 0;
            Bitmap backBpCopy = ImageHelper.DeepCopyBitmap(backBp);
            Bitmap dstcopy1 = (Bitmap)ImageHelper.ResizeImage(backBpCopy, 640, 360);
            FuseImage = new FuseImage(dstcopy1);
            ControlHelper.ThreadInvokerControl(this, () =>
            {
                button12.Text = "录像中...";
                button12.BackColor = Color.Red;
                //pictureBox2.Image = peilinInfraredEquipment.Properties.Resources.录制中_红;
            });

            stopMeasure();
            nowTestDir = $"\\{_ProjectName}\\{_GroupName}\\{nowRaceStudentData.idNumber}_{nowRaceStudentData.name}\\第{nowRaceStudentData.RoundId}轮\\";
            nowTestDir = ScoreDir + nowTestDir;
            if (!Directory.Exists(nowTestDir))
            {
                DirectoryInfo dir = new DirectoryInfo(nowTestDir);
                dir.Create();//自行判断一下是否存在。
            }
            /* string avipath = Path.Combine(nowTestDir,
                 $"{nowRaceStudentData.idNumber}_{nowRaceStudentData.name}_第{nowRaceStudentData.RoundId}轮摄像头1.mp4");
             if (File.Exists(avipath))
             {
                File.Delete(avipath);
             }
             OpenVideoOutPut(avipath);
             avipath = Path.Combine(nowTestDir,
                 $"{nowRaceStudentData.idNumber}_{nowRaceStudentData.name}_第{nowRaceStudentData.RoundId}轮摄像头2.mp4");
             if (File.Exists(avipath))
             {
                File.Delete(avipath);
             }
             OpenVideoOutPut1(avipath);*/
            //recFileSp = 1;//录像文件顺序号
            frameSum = 0;
            frameSum1 = 0;
            recTimeR0 = PointHelper.Instance.Str2int(recTimeTxt.Text) * 10;
            //Task.Run(() => voiceOut0($"{nowRaceStudentData.name}开始考试"));
            Task.Run(() => voiceOut0($"{nowRaceStudentData.name}第{nowRaceStudentData.RoundId}轮开始考试"));
            SendScore(nowRaceStudentData.name, "开始考试", "");
        }
        /// <summary>
        /// 刷新图片显示
        /// </summary>

        public void RefreshPictureBox()
        {
            if (distanceMode == 0)
            {
                pictureBox1.Refresh();
            }
            else if (distanceMode == 1)
            {
                pictureBox3.Refresh();
            }

        }
        #endregion 图像显示

        #region 控制面板
        private void voiceOut0(string str, int rate = 3)
        {
            Task.Run(() =>
            {
                SpVoice voice = new SpVoice();
                ISpeechObjectTokens obj = voice.GetVoices();
                voice.Voice = obj.Item(0);
                voice.Rate = rate;
                voice.Speak(str, SpeechVoiceSpeakFlags.SVSFIsXML | SpeechVoiceSpeakFlags.SVSFDefault);
            });
        }
        /// <summary>
        /// 写入成绩
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnWriteScore_Click(object sender, EventArgs e)
        {
            WriteScore2Db();
        }
        int writeCount = 0;
        bool _IsFoul = false;
        /// <summary>
        /// 写入成绩
        /// </summary>
        private void WriteScore2Db()
        {
            try
            {
                stopMeasure();
                _IsFoul = false;
                if (recTimeR0 > 0)
                {
                    FrmTips.ShowTipsError(this, "考试中请勿进行此操作");
                    return;
                }
                //updateRaceStudentDataListsScore();
                if (!isHaveStudent())
                {
                    FrmTips.ShowTipsError(this, "数据异常");
                    return;
                }
                //写入成绩
                if (imgMs.Count > 0)
                {
                    //弹窗显示成绩是否写入
                    DetermineGradesWindow dmg = new DetermineGradesWindow();
                    //米单位
                    double fenmu = 1000;
                    if (isSitting)
                    {
                        //坐位体前屈 写入单位为cm
                        fenmu = 10;
                    }

                    dmg.dangwei = dangwei;
                    dmg.TopMost = true;
                    dmg.ReservedDigitsTxt = ReservedDigitsTxt;
                    if (distanceMode == 0) dmg.setScore(MeasureLen / fenmu);
                    else if (distanceMode == 1) dmg.setScore(m_MeasureLen / fenmu);

                    var ds = sQLiteHelper.ExecuteReaderList($"SELECT Id,Name,IdNumber,Sex FROM DbPersonInfos WHERE Id='{nowRaceStudentData.id}'");
                    string Sex = "0";
                    foreach (var item in ds)
                    {
                        Sex = item["Sex"];
                    }
                    double UnusualScoreMin = 0;
                    double UnusualScoreMax = 0;
                    if (Sex == "0")
                    {
                        //男
                        UnusualScoreMin = bUnusualScoreMin;
                        UnusualScoreMax = bUnusualScoreMax;
                    }
                    else if (Sex == "1")
                    {
                        //女
                        UnusualScoreMin = gUnusualScoreMin;
                        UnusualScoreMax = gUnusualScoreMax;
                    }
                    if (dmg.score0 >= UnusualScoreMax || dmg.score0 <= UnusualScoreMin)
                    {
                        DialogResult dialogResult = MessageBox.Show($"本次成绩异常为{dmg.score0},是否写入?", "异常成绩", MessageBoxButtons.YesNo);
                        if (dialogResult == DialogResult.No)
                        {
                            return;
                        }
                    }
                    //滚动图示
                    TxProcessRollForm txProcess = new TxProcessRollForm();
                    txProcess.TopMost = true;
                    try
                    {
                        DialogResult dialogResult = dmg.ShowDialog();
                        if (dialogResult == DialogResult.OK)
                        {
                            new Thread((ThreadStart)delegate
                            {
                                txProcess.ShowDialog();
                            }).Start();

                            string txt_GNametxt = nowRaceStudentData.name;
                            //测试项目
                            string projectTypeCbxtxt = typeName;
                            string txt_Grouptxt = _GroupName;
                            if (dmg.isFoul)
                            {
                                nowRaceStudentData.state = ResultState.ResultState2Int("犯规");
                                MeasureLen = 0;
                                m_MeasureLen = 0;
                                _IsFoul = true;
                                string scoreContent = string.Format("时间:{0},项目:{1},组别:{2},准考证号:{3},姓名:{4},第{5}次成绩:修改成绩{6}为{7}, 状态:{8}",
                                         DateTime.Now.ToString("yyyy年MM月dd日HH:mm:ss"),
                                         projectTypeCbxtxt,
                                         txt_Grouptxt,
                                         nowRaceStudentData.idNumber,
                                         txt_GNametxt,
                                         RoundCount0,
                                         MeasureLen.ToString(ReservedDigitsTxt),
                                         dmg.checkScore,
                                         "犯规");
                                File.AppendAllText(@"./成绩日志.txt", scoreContent + "\n");
                                Task.Run(() => voiceOut0($"{txt_GNametxt}第{RoundCount0}轮成绩:犯规"));
                                bool sendScoreReturn = SendScore(nowRaceStudentData.name, "犯规", "");
                            }
                            else
                            {
                                _IsFoul = false;
                                ///成绩修改写入日志
                                if (dmg.checkScore != 0)
                                {
                                    MeasureLen = dmg.checkScore * fenmu;

                                    double score1 = dmg.score;
                                    string scoreContent = string.Format("时间:{0},项目:{1},组别:{2},准考证号:{3},姓名{4},第{5}次成绩:修改成绩{6}为{7}, 状态:{8}",
                                          DateTime.Now.ToString("yyyy年MM月dd日HH:mm:ss"),
                                          projectTypeCbxtxt,
                                          txt_Grouptxt,
                                          nowRaceStudentData.idNumber,
                                          txt_GNametxt,
                                          RoundCount0,
                                          score1.ToString(ReservedDigitsTxt),
                                          dmg.checkScore,
                                          "已测试");
                                    File.AppendAllText(@"./成绩日志.txt", scoreContent + "\n");
                                }

                                string score = (MeasureLen / fenmu).ToString(ReservedDigitsTxt);
                                if (distanceMode == 1) score = (m_MeasureLen / fenmu).ToString(ReservedDigitsTxt);

                                string score_str = score.Replace('-', '负');
                                if (isSpeakScoreVoice)
                                {
                                    Task.Run(() => voiceOut0($"{txt_GNametxt}第{RoundCount0}轮成绩{score_str}{dangwei}"));
                                }

                                bool sendScoreReturn = SendScore(nowRaceStudentData.name, score, dangwei);
                            }

                            //写入成绩
                            input2Result();
                            if (checkBox3.Checked)
                            {
                                if (_TestMethod == 0)
                                {
                                    writeCount++;
                                    //自动下一个
                                    nextPerson();
                                }
                                else
                                {
                                    //自动下一轮
                                    nextRound();
                                }
                            }
                            beginTest();
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Debug(ex);
                    }
                    finally
                    {

                        try
                        {
                            if (txProcess.isLoad)
                            {
                                //importpath=string.Empty;
                                txProcess.Invoke((EventHandler)delegate { txProcess.Close(); });
                                txProcess.Dispose();
                            }
                        }
                        catch (Exception)
                        { }

                    }


                }
                else
                {
                    FrmTips.ShowTipsError(this, "未录像");
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
            finally
            {

            }


        }

        #region 右键列表选择处理
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 缺考ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetErrorState("缺考");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 中退ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetErrorState("中退");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 犯规ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetErrorState("犯规");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 弃权ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetErrorState("弃权");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 成绩查询ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (stuView.SelectedRows.Count == 0)
                {
                    FrmTips.ShowTipsError(this, "未选择考生");
                    return;
                }
                string idNumber = stuView.SelectedRows[0].Cells[1].Value.ToString();
                string name = stuView.SelectedRows[0].Cells[2].Value.ToString();
                string nowTestDir1 = $"\\{_ProjectName}\\{_GroupName}\\{idNumber}_{name}\\第1轮";
                nowTestDir1 = ScoreDir + nowTestDir1;
                if (Directory.Exists(nowTestDir1))
                {
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
                    psi.Arguments = "/e,/select," + nowTestDir1;
                    System.Diagnostics.Process.Start(psi);
                }
                else
                {
                    FrmTips.ShowTipsError(this, "未找到文件夹");
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
        }

        /// <summary>
        /// 设置异常状态
        /// </summary>
        /// <param name="state"></param>
        private void SetErrorState(string state)
        {
            if (stuView.SelectedRows.Count == 0)
            {
                FrmTips.ShowTipsError(this, "未选择考生");
                return;
            }
            int state0 = ResultState.ResultState2Int(state);
            string idNumber = stuView.SelectedRows[0].Cells[1].Value.ToString();
            string name = stuView.SelectedRows[0].Cells[2].Value.ToString();
            string id = stuView.SelectedRows[0].Cells[6 + ((_RoundCount - 1) * 3)].Value.ToString();
            string sql = $"UPDATE DbPersonInfos SET State=1,FinalScore=1 WHERE Id='{id}'";
            int result = sQLiteHelper.ExecuteNonQuery(sql);
            sql = $"UPDATE ResultInfos SET State={state0} WHERE PersonId='{id}' AND RoundId={RoundCount0} AND IsRemoved=0";
            result = sQLiteHelper.ExecuteNonQuery(sql);
            //更新没有该成绩 插入
            if (result == 0)
            {
                var sortid = sQLiteHelper.ExecuteScalar($"SELECT MAX(SortId) + 1 FROM ResultInfos");
                string sortid0 = "1";
                if (sortid != null && sortid.ToString() != "") sortid0 = sortid.ToString();

                sql = $"INSERT INTO ResultInfos(CreateTime,SortId,IsRemoved,PersonId,SportItemType,PersonName,PersonIdNumber,RoundId,Result,State) " +
                         $"VALUES('{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', {sortid0}, 0, '{id}',0,'{name}','{idNumber}',{RoundCount0},{0},{state0})";
                //处理写入数据库
                sQLiteHelper.ExecuteNonQuery(sql);
            }
            if (RoundCount0 > 0)
            {
                bool sendScoreReturn = SendScore(name, state, "");
                UpdateListView(_ProjectId, _GroupName, RoundCount0);
            }
        }

        #endregion 右键列表选择处理
        /// <summary>
        /// 当前是否有考生
        /// </summary>
        /// <returns></returns>
        private bool isHaveStudent(bool flag = true)
        {
            if (nowRaceStudentData == null || string.IsNullOrEmpty(nowRaceStudentData.id))
            {
                if (flag)
                    FrmTips.ShowTipsError(this, "请选择考生");
                return false;
            }
            return true;
        }
        /// <summary>
        /// 定标设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button27_Click(object sender, EventArgs e)
        {
            if (recTimeR0 > 0)
            {
                FrmTips.ShowTipsError(this, "考试中请勿进行此操作");
                return;
            }
            SelectPunctuationRuleWindow sptr = new SelectPunctuationRuleWindow();
            if (typeName.Trim() == "坐位体前屈")
            {
                sptr.type = "坐位体前屈";
            }
            DialogResult dr = sptr.ShowDialog();
            if (dr == DialogResult.OK)
            {
                if (distanceMode == 0)
                {
                    targetPoints.Clear();
                }
                else if (distanceMode == 1)
                {
                    targetPoints1.Clear();
                }

                colum = sptr.colum;
                initDis = sptr.initDis;
                distance = sptr.distance;
                pf1.columSum = colum;
                nowColum = 1;
                jpName = nowColum + "";
                jpDis = initDis + "";
                jpStatus = true;
                if (distanceMode == 0)
                {
                    Directionmode = sptr.Directionmode;
                }
                else if (distanceMode == 1)
                {
                    Directionmode1 = sptr.Directionmode;

                }


                pf1.Directionmode = sptr.Directionmode;
                pf1.updateFlp(nowColum);
                formShowStatus = 1;
                pf1.Show();
            }
            else if (dr == DialogResult.Yes)
            {
                try
                {
                    colum = sptr.colum;
                    initDis = sptr.initDis;
                    distance = sptr.distance;
                    nowColum = 1;
                    jpDis = initDis + "";
                    int tarnlen = targetPoints.Count;
                    if (distanceMode == 0)
                    {
                        Directionmode = sptr.Directionmode;
                        tarnlen = targetPoints.Count;
                    }
                    else if (distanceMode == 1)
                    {
                        Directionmode1 = sptr.Directionmode;
                        tarnlen = targetPoints1.Count;
                    }

                    for (int i = 0; i < tarnlen; i++)
                    {
                        TargetPoint tp = null;
                        if (distanceMode == 0)
                        {
                            tp = targetPoints[i];
                        }
                        else if (distanceMode == 1)
                        {
                            tp = targetPoints1[i];
                        }
                        tp.dis = PointHelper.Instance.Str2int(jpDis);
                        nowColum++;
                        if (nowColum % 2 != 0)
                        {
                            initDis += distance;
                        }
                        jpDis = initDis + "";
                    }
                    updateTargetListView(true);
                    saveMarkSetting();
                    clearMemory();
                }
                catch (Exception ex)
                {
                    LoggerHelper.Debug(ex);
                }
            }
        }
        /// <summary>
        /// 浏览图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button10_Click(object sender, EventArgs e)
        {
            if (recTimeR0 > 0)
            {
                FrmTips.ShowTipsError(this, "考试中请勿进行此操作");
                return;
            }
            openImgList();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            isPrintFlag = true;
            OutPutScore1();
        }
        bool isPrintFlag = false;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        private bool OutPutScore(bool flag = false)
        {
            bool result = false;
            try
            {
                string path = Application.StartupPath + $"\\data\\excel\\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path += $"output{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                Dictionary<string, string> dict0 = sQLiteHelper.ExecuteReaderOne($"SELECT RoundCount FROM SportProjectInfos WHERE Id='{_ProjectId}' ");
                if (dict0.Count == 0)
                {
                    FrmTips.ShowTipsError(this, "数据错误");
                    return false;
                }
                int.TryParse(dict0["RoundCount"], out int RoundCount);
                List<Dictionary<string, string>> ldic = new List<Dictionary<string, string>>();
                //序号 项目名称    组别名称 姓名  准考证号 考试状态    第1轮 第2轮 最好成绩
                string sql = $"SELECT BeginTime, Id, GroupName, Name, IdNumber,State,Sex FROM DbPersonInfos WHERE ProjectId='{_ProjectId}' ";
                if (!flag)
                {
                    sql += $" AND GroupName = '{_GroupName}'";
                }
                //List<outPutPrintExcelStudentPojo> students = new List<outPutPrintExcelStudentPojo>();
                List<Dictionary<string, string>> list1 = sQLiteHelper.ExecuteReaderList(sql);
                int step = 1;
                foreach (var item1 in list1)
                {
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic.Add("序号", step + "");
                    dic.Add("项目名称", _ProjectName);
                    dic.Add("组别名称", item1["GroupName"]);
                    dic.Add("姓名", item1["Name"]);
                    dic.Add("准考证号", item1["IdNumber"]);
                    dic.Add("性别", item1["Sex"] == "0" ? "男" : "女");
                    string state0 = ResultState.ResultState2Str(item1["State"]);
                    dic.Add("考试状态", state0);
                    List<Dictionary<string, string>> list2 = sQLiteHelper.ExecuteReaderList(
                        $"SELECT * FROM ResultInfos WHERE PersonId='{item1["Id"]}' And IsRemoved=0 ORDER BY RoundId ASC LIMIT {RoundCount}");
                    int step2 = 1;
                    double maxScore = 0;
                    foreach (var item2 in list2)
                    {
                        double.TryParse(item2["Result"], out double result0);
                        int.TryParse(item2["State"], out int state1);
                        if (result0 > maxScore) maxScore = result0;
                        if (state1 == 1)
                        {
                            dic.Add($"第{step2}轮", result0 + "");
                        }
                        else
                        {
                            dic.Add($"第{step2}轮", ResultState.ResultState2Str(state1));
                        }
                        step2++;
                    }
                    for (int i = step2; i <= RoundCount; i++)
                    {
                        dic.Add($"第{i}轮", "");
                    }
                    if (step2 > 1)
                    {
                        dic.Add($"最好成绩", maxScore + "");
                    }
                    else
                    {
                        dic.Add($"最好成绩", "");
                    }
                    dic.Add($"签字", "");
                    ldic.Add(dic);
                    step++;
                }
                result = ExcelUtils.MiniExcel_OutPutExcel(path, ldic);
                //result = ExcelUtils.OutPutExcel(ldic, path);
                if (result)
                {


                    Workbook wb = new Workbook();
                    wb.LoadFromFile(path);
                    Worksheet sheet = wb.Worksheets[0];
                    /*sheet.AllocatedRange.AutoFitColumns();
                    sheet.AllocatedRange.AutoFitRows();*/
                    sheet = AutoResizeColumnWidth(sheet, 130);
                    sheet.PageSetup.Orientation = PageOrientationType.Landscape;
                    wb.SaveToFile(path, ExcelVersion.Version2013);
                    //System.Diagnostics.Process.Start(path);
                    /*//设置打印对话框属性
                    PrintDialog dialog = new PrintDialog();
                    dialog.AllowPrintToFile = true;
                    dialog.AllowCurrentPage = true;
                    dialog.AllowSomePages = true;
                    //设置单面打印
                    dialog.PrinterSettings.Duplex = Duplex.Simplex;
                    //设置打印份数
                    dialog.PrinterSettings.Copies = 1;
                    //打印文档
                    wb.PrintDialog = dialog;
                    PrintDocument pd = wb.PrintDocument;
                    //这是设置打印文档的方向为横向(若为纵向打印则两步都需要改)
                    pd.DefaultPageSettings.Landscape = true;
                    PrintController printController = new StandardPrintController();
                    pd.PrintController = printController;
                    pd.Print();*/

                    if (isPrintFlag)
                    {
                        System.Diagnostics.Process p = new System.Diagnostics.Process();
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        p.StartInfo.UseShellExecute = true;
                        p.StartInfo.FileName = path;
                        p.StartInfo.Verb = "print";
                        p.Start();
                    }
                    else
                    {
                        Process.Start(path);
                    }



                }
                return result;
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return false;
            }
        }

        #region 打印成绩
        /// <summary>
        /// 导入学生名单类
        /// </summary>
        public class outPutPrintExcelStudentPojo
        {
            //[ExcelColumnName("序号")]
            public int Id { get; set; }
            //[ExcelColumnName("准考证号")]
            public string IdNumber { get; set; }
            public string school { get; set; }
            //[ExcelColumnName("姓名")]
            public string Name { get; set; }
            //[ExcelColumnName("性别")]
            public string Sex { get; set; }
            public string classNumber { get; set; }
            public string groupName { get; set; }
            //[ExcelColumnName("项目1")]
            public string projectName { get; set; }
            //[ExcelColumnName("项目1成绩")]
            public string projectScore { get; set; }
            public string error { get; set; }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        private bool OutPutScore1(bool flag = false)
        {
            bool result = false;
            try
            {
                string path = Application.StartupPath + $"\\data\\excel\\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path += $"output{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";
                Dictionary<string, string> dict0 = sQLiteHelper.ExecuteReaderOne($"SELECT RoundCount FROM SportProjectInfos WHERE Id='{_ProjectId}' ");
                if (dict0.Count == 0)
                {
                    FrmTips.ShowTipsError(this, "数据错误");
                    return false;
                }
                int.TryParse(dict0["RoundCount"], out int RoundCount);
                List<Dictionary<string, string>> ldic = new List<Dictionary<string, string>>();
                //序号 项目名称    组别名称 姓名  准考证号 考试状态    第1轮 第2轮 最好成绩
                string sql = $"SELECT * FROM DbPersonInfos WHERE ProjectId='{_ProjectId}' ";
                if (!flag)
                {
                    sql += $" AND GroupName = '{_GroupName}'";
                }
                List<outPutPrintExcelStudentPojo> students = new List<outPutPrintExcelStudentPojo>();
                List<Dictionary<string, string>> list1 = sQLiteHelper.ExecuteReaderList(sql);
                int step = 1;
                foreach (var item1 in list1)
                {
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic.Add("序号", step + "");
                    dic.Add("项目名称", _ProjectName);
                    dic.Add("组别名称", item1["GroupName"]);
                    dic.Add("姓名", item1["Name"]);
                    dic.Add("准考证号", item1["IdNumber"]);
                    dic.Add("性别", item1["Sex"] == "0" ? "男" : "女");
                    string state0 = ResultState.ResultState2Str(item1["State"]);
                    dic.Add("考试状态", state0);
                    List<Dictionary<string, string>> list2 = sQLiteHelper.ExecuteReaderList(
                        $"SELECT * FROM ResultInfos WHERE PersonId='{item1["Id"]}' And IsRemoved=0 ORDER BY RoundId ASC LIMIT {RoundCount}");
                    int step2 = 1;
                    double maxScore = 0;
                    List<string> StateList = new List<string>();
                    foreach (var item2 in list2)
                    {
                        double.TryParse(item2["Result"], out double result0);
                        int.TryParse(item2["State"], out int state1);
                        if (result0 > maxScore) maxScore = result0;
                        if (state1 == 1)
                        {
                            dic.Add($"第{step2}轮", result0 + "");
                        }
                        else
                        {
                            string stateStr0 = ResultState.ResultState2Str(state1);
                            dic.Add($"第{step2}轮", stateStr0);
                            StateList.Add(stateStr0);
                        }
                        step2++;
                    }
                    for (int i = step2; i <= RoundCount; i++)
                    {
                        dic.Add($"第{i}轮", "");
                    }
                    if (step2 > 1)
                    {
                        dic.Add($"最好成绩", maxScore + "");
                    }
                    else
                    {
                        dic.Add($"最好成绩", "");
                    }
                    dic.Add($"签字", "");
                    ldic.Add(dic);
                    string MaxScoreStr = maxScore.ToString();
                    if (maxScore == 0)
                    {
                        if (StateList.Count == 0)
                        {
                            MaxScoreStr = "未测试";
                        }
                        else
                        {
                            MaxScoreStr = StateList[0];
                        }
                    }
                    outPutPrintExcelStudentPojo outPutPrintExcelStudentPojo = new outPutPrintExcelStudentPojo()
                    {
                        Id = step,
                        IdNumber = item1["IdNumber"],
                        Name = item1["Name"],
                        Sex = item1["Sex"] == "0" ? "男" : "女",
                        school = item1["SchoolName"],
                        projectName = typeName,
                        projectScore = MaxScoreStr,
                        classNumber = item1["ClassNumber"],
                        groupName = item1["GroupName"],
                        error = ""
                    };
                    students.Add(outPutPrintExcelStudentPojo);
                    step++;
                }
                //result = ExcelUtils.MiniExcel_OutPutExcel(path, ldic);
                DateTime nowDt = DateTime.Now;
                var value = new
                {
                    s = students.ToArray(),
                    year = nowDt.Year.ToString(),
                    time = nowDt.ToString("yyyy-MM-dd HH:mm:ss")
                };
                string Templatepath = Application.StartupPath + "\\模板\\打印成绩模板.xlsx";
                //MiniExcel.SaveAs(path, outPutExcelDataList);
                MiniExcel.SaveAsByTemplate(path, Templatepath, value);
                result = File.Exists(path);
                if (result)
                {
                    if (isPrintFlag)
                    {
                        System.Diagnostics.Process p = new System.Diagnostics.Process();
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        p.StartInfo.UseShellExecute = true;
                        p.StartInfo.FileName = path;
                        p.StartInfo.Verb = "print";
                        p.Start();
                    }
                    else
                    {
                        Process.Start(path);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return false;
            }
        }
        #endregion
        /// <summary>
        /// Excel的页面自动列宽处理
        /// </summary>
        /// <param name="sheet">目标Sheet</param>
        /// <param name="nPageWidthChars">Sheet页面设置后每行可容纳字符总宽(需自行测试后确认)</param>
        /// <param name="nRowStart">要自动处理的起始行号</param>
        /// <param name="nColIgnore">要忽略的重分配的列集合</param>
        /// <returns>处理后的Sheet</returns>
        public Worksheet AutoResizeColumnWidth(Worksheet sheet, int nPageWidthChars, int nRowStart = 1, int[] nColIgnore = null)
        {
            //单、双字节列宽自适应(不包括隐藏列)处理
            int allWidth = 0;         //总列宽
            int nVisibleColumns = 0;  //可见列数
            for (int i = 1; i <= sheet.LastColumn; i++)
            {
                if (sheet.Range[nRowStart, i].ColumnWidth == 0)  //隐藏列(宽度为0)
                    continue;
                nVisibleColumns++;    //可见列计数
                int columnWidth = 0;  //初始化列宽
                for (int j = nRowStart; j <= sheet.LastRow; j++)
                {
                    var cell = sheet.Range[j, i];
                    if (!string.IsNullOrWhiteSpace(cell.DisplayedText))
                    {
                        int length = Encoding.Default.GetBytes(cell.DisplayedText).Length;  //单双字节
                        if (columnWidth < length)
                            columnWidth = length;
                    }
                }
                sheet.SetColumnWidth(i, columnWidth);
                allWidth += columnWidth;
            }
            //页宽多余宽度重新分配
            int nAddWidth = (nPageWidthChars - allWidth) / (nVisibleColumns - ((nColIgnore != null) ? nColIgnore.Length : 0));
            for (int i = 1; (i <= sheet.LastColumn && nAddWidth >= 1); i++)
            {
                if (sheet.Range[nRowStart, i].ColumnWidth == 0)  //若表格中的列为隐藏列(宽度为0)
                    continue;
                if (nColIgnore != null && ((IList)nColIgnore).Contains(i))
                    continue;
                sheet.SetColumnWidth(i, sheet.GetColumnWidth(i) + nAddWidth);
            }
            return sheet;
        }

        /// <summary>
        /// 测量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button13_Click(object sender, EventArgs e)
        {
            MeasureFun();
        }
        /// <summary>
        /// 上一张
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            if (recTimeR0 > 0)
            {
                FrmTips.ShowTipsError(this, "考试中请勿进行此操作");
                return;
            }
            dispDecPic();
        }
        /// <summary>
        /// 
        /// </summary>
        private void dispDecPic()
        {
            if (!isHaveStudent())
            {
                tabControl1.SelectedIndex = 0;
                return;
            }
            List<ImgMsS> imgMsTemp = imgMs;
            if (distanceMode == 1) imgMsTemp = imgMs1;
            if (imgMsTemp.Count == 0)
            {
                MessageBox.Show("请录像");
                return;
            }
            if (distanceMode == 0)
            {
                CloseCamera();
            }
            else if (distanceMode == 1)
            {
                CloseCamera1();
            }
            int i = PointHelper.Instance.Str2int(nowFileName);

            if (i == 0)
            {
                MessageBox.Show("到尽头了");
                return;
            }
            i--;
            if (imgMsTemp.Count < i)
                i = 1;

            nowFileName = i + "";
            if (null != imgMsTemp[i])
            {
                //setHScrollBarValue(i);

                if (distanceMode == 0) pictureBox1.Image = ImageHelper.MemoryStream2Bitmap(imgMsTemp[i].img);
                else if (distanceMode == 1) pictureBox3.Image = ImageHelper.MemoryStream2Bitmap(imgMsTemp[i].img);
                recImgIndex.Text = $"图片索引:{nowFileName}";
            }
            rgbVideoSourceRePaint();
        }
        /// <summary>
        /// 下一张
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button9_Click(object sender, EventArgs e)
        {
            if (recTimeR0 > 0)
            {
                FrmTips.ShowTipsError(this, "考试中请勿进行此操作");
                return;
            }
            dispIncPic();
        }
        /// <summary>
        /// 
        /// </summary>
        private void dispIncPic()
        {
            if (!isHaveStudent())
            {
                tabControl1.SelectedIndex = 0;
                return;
            }
            if (imgMs.Count == 0)
            {
                MessageBox.Show("请录像");
                return;
            }
            if (distanceMode == 0)
            {
                CloseCamera();
            }
            else if (distanceMode == 1)
            {
                CloseCamera1();
            }
            List<ImgMsS> imgMsTemp = imgMs;
            if (distanceMode == 1) imgMsTemp = imgMs1;
            int i = PointHelper.Instance.Str2int(nowFileName);
            i++;

            if (i >= imgMsTemp.Count)
            {
                i = imgMsTemp.Count - 1;
                MessageBox.Show("到尽头了");
                return;
            }
            nowFileName = i + "";
            if (null != imgMsTemp[i])
            {
                //setHScrollBarValue(i);

                if (distanceMode == 0) pictureBox1.Image = ImageHelper.MemoryStream2Bitmap(imgMsTemp[i].img);
                else if (distanceMode == 1) pictureBox3.Image = ImageHelper.MemoryStream2Bitmap(imgMsTemp[i].img);
                recImgIndex.Text = $"图片索引:{nowFileName}";
            }

            rgbVideoSourceRePaint();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool beginTest()
        {
            //SendScore(nowRaceStudentData.name, "准备考试", "");
            //检查成绩要测第几次
            MeasureLen = 0;//测量长度
            m_MeasureLen = 0;//测量长度
            bool IhvaStu = isHaveStudent();
            if (!IhvaStu)
            {
                return IhvaStu;
            }
            nowRaceStudentData.state = 0;
            recTimeR0 = 0;
            imgMs.Clear();
            imgMs1.Clear();
            clearMemory();
            try
            {
                rgbVideoSourceStart();
                rgbVideoSourceStart1();
                return true;
            }
            catch (Exception ex)
            {
                FrmTips.ShowTipsError(this, "摄像头未开启");
                return false;
            }
        }
        private string AdminUserIdNumber = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stuView_CurrentCellChanged(object sender, EventArgs e)
        {
            nowRaceStudentData = new RaceStudentData();

            if (stuView.SelectedRows.Count == 0)
            {
                return;
            }
            try
            {
                DataGridViewRow dataGridViewRow = stuView.SelectedRows[0];

                //序号 考号 姓名 成绩 考试状态 上传状态 唯一编号
                nowRaceStudentData.id = dataGridViewRow.Cells[6 + ((_RoundCount - 1) * 3)].Value.ToString();
                nowRaceStudentData.name = dataGridViewRow.Cells[2].Value.ToString();
                nowRaceStudentData.idNumber = dataGridViewRow.Cells[1].Value.ToString();
                AdminUserIdNumber = nowRaceStudentData.idNumber;
                String stateStr = dataGridViewRow.Cells[4].Value.ToString();
                int stateInt = ResultState.ResultState2Int(stateStr);
                nowRaceStudentData.state = stateInt;
                nowRaceStudentData.groupName = _GroupName;

                StuViewPojo stuViewPojo = stuViewPojos.Find(a => a.IdNumber == AdminUserIdNumber);
                if (stuViewPojo != null)
                {
                    bool b = true;
                    for (int i = 1; i <= _RoundCount; i++)
                    {
                        int v = stuViewPojo.ScoreViewPojos.FindIndex(a => a.roundCound == i);
                        if (v == -1 && i <= RoundCountCbx.Items.Count)
                        {
                            RoundCountCbx.SelectedIndex = i - 1;
                            RoundCount0 = RoundCountCbx.SelectedIndex + 1;
                            b = false; break;
                        }
                    }
                    if (b)
                    {
                        RoundCountCbx.SelectedIndex = RoundCountCbx.Items.Count - 1;
                        RoundCount0 = RoundCountCbx.SelectedIndex + 1;
                    }
                }
                int inddx = dataGridViewRow.Index;
                if (inddx >= 0 && inddx < comboBox2.Items.Count)
                {
                    if (comboBox2.SelectedIndex != inddx)
                        comboBox2.SelectedIndex = inddx;
                }
                nowRaceStudentData.RoundId = RoundCount0;


                var ds1 = sQLiteHelper.ExecuteReaderOne($"SELECT PersonName,Result,State,uploadState FROM ResultInfos " +
                    $"WHERE PersonId='{nowRaceStudentData.id}' AND RoundId={RoundCount0}");

                if (ds1.Count == 0)
                {
                    label12.Text = nowRaceStudentData.name;
                    label14.Text = nowRaceStudentData.idNumber;
                    label18.Text = "未测试";
                    label20.Text = $"0.00{dangwei}";
                }
                else
                {
                    //序号 考号 姓名 成绩 考试状态 上传状态 唯一编号
                    string PersonName0 = ds1["PersonName"];
                    double.TryParse(ds1["Result"], out double Result0);
                    int.TryParse(ds1["State"], out int State0);
                    string sstate = ResultState.ResultState2Str(State0);
                    label12.Text = PersonName0;
                    label14.Text = nowRaceStudentData.idNumber;
                    label18.Text = sstate;
                    label20.Text = ds1["Result"] + dangwei;
                }




            }
            catch (Exception ex)
            {
                nowRaceStudentData = new RaceStudentData();
                LoggerHelper.Debug(ex);
            }
            finally
            {
                RefreshPictureBox();
            }
        }
        /// <summary>
        /// 开始录像按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button12_Click(object sender, EventArgs e)
        {
            BeginRec();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openCameraBtn_Click(object sender, EventArgs e)
        {
            if (recTimeR0 > 0)
            {
                FrmTips.ShowTipsError(this, "考试中请勿进行此操作");
                return;
            }
            if (openCameraBtn.Text == "关闭摄像头")
            {
                CloseCamera();
                //openCameraBtn.Text = "打开摄像头";
                return;
            }
            if (string.IsNullOrEmpty(cameraName))
            {
                FrmTips.ShowTipsError(this, "请选择摄像头!");
                return;
            }
            bool flag = openCamearaFun(cameraIndex);
        }
        /// <summary>
        /// 摄像头属性设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            if (recTimeR0 > 0)
            {
                FrmTips.ShowTipsError(this, "考试中请勿进行此操作");
                return;
            }

            try
            {
                openCameraSetting();
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
        }
        private string cameraName = String.Empty;
        public int cameraIndex = -1;
        private string cameraName1 = String.Empty;
        public int cameraIndex1 = -1;
        private int maxFps = 0;
        private int maxFps1 = 0;
        private int Fps = 0;
        private int Fps1 = 0;
        /// <summary>
        /// 打开摄像头设置
        /// </summary>
        public void openCameraSetting()
        {
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            CameraSettingWindow frmc = new CameraSettingWindow();
            frmc.filterInfoCollection = filterInfoCollection;
            frmc.ExistMonikerStrings = ExistMonikerStrings;
            if (frmc.ShowDialog() == DialogResult.OK)
            {
                cameraName = frmc.cameraName;
                cameraIndex = frmc.cameraIndex;
                maxFps = frmc.maxFps;
                Fps = frmc.Fps;
                if (Fps == 0)
                {
                    cameraSkip = maxFps;
                }
                else
                {
                    cameraSkip = maxFps / Fps;
                }
                FrameFlashCount0 = maxFps / 10;
                if (!string.IsNullOrEmpty(cameraName))
                {
                    bool flag = openCamearaFun(cameraIndex);
                }
                tabControl3.SelectedIndex = 1;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void openCameraSetting1()
        {
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            CameraSettingWindow frmc = new CameraSettingWindow();
            frmc.ExistMonikerStrings = ExistMonikerStrings;
            frmc.filterInfoCollection = filterInfoCollection;
            if (frmc.ShowDialog() == DialogResult.OK)
            {
                cameraName1 = frmc.cameraName;
                cameraIndex1 = frmc.cameraIndex;
                maxFps1 = frmc.maxFps;
                Fps1 = frmc.Fps;
                if (Fps1 == 0)
                {
                    cameraSkip1 = maxFps1;
                }
                else
                {
                    cameraSkip1 = maxFps1 / Fps1;
                }
                FrameFlashCount1 = maxFps1 / 10;
                if (!string.IsNullOrEmpty(cameraName1))
                {
                    bool flag = openCamearaFun(cameraIndex1);
                }

                tabControl3.SelectedIndex = 0;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            if (imgMs.Count == 0)
            {
                hScrollBar1.Value = 0;
                hScrollBar1.Maximum = 0;
                return;
            }
            int sp = hScrollBar1.Value;
            int imgcount = imgMs.Count - 1;
            hScrollBar1.Maximum = imgcount;

            if (sp > imgcount)
            {
                sp = imgcount;
            }
            if (null != imgMs[sp])
            {
                nowFileName = sp + "";
                Image image = ImageHelper.MemoryStream2Bitmap(imgMs[sp].img);
                if (image == null) image = ImageHelper.MemoryStream2Bitmap(imgMs[sp].img);
                pictureBox1.Image = image;
                recImgIndex.Text = $"图片索引:{nowFileName}";
                //rgbVideoSourceRePaint();
                Application.DoEvents();
                // RefreshPictureBox();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void RunTest_KeyDown(object sender, KeyEventArgs e)
        {
            if (tabControl1.SelectedIndex != 0) return;
            if (BarCodeText.Focused) return;
            if (e.KeyCode == Keys.Space)
            {
                BeginRec();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.A)
            {
                if (recTimeR0 > 0) return;
                //上一张
                dispDecPic();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.D)
            {
                //下一张
                dispIncPic();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.S)
            {
                if (recTimeR0 > 0) return;
                //测量
                MeasureFun();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.W)
            {
                WriteScore2Db();

                e.Handled = true;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stuView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (e.RowIndex >= 0)
                {
                    stuView.ClearSelection();
                    stuView.Rows[e.RowIndex].Selected = true;
                    stuView.CurrentCell = stuView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            if (SoftWareProperty.singleMode != "0")
            {
                MessageBox.Show("单机模式无法操作");
                return;
            }
            if (recTimeR0 > 0)
            {
                FrmTips.ShowTipsError(this, "考试中请勿进行此操作");
                return;
            }
            ParameterizedThreadStart method = new ParameterizedThreadStart((obj) =>
            {
                EquipMentSettingWindowSys .Instance.ShowPlatFormSettingWindow(sQLiteHelper);
            });
            Thread threadRead = new Thread(method);
            threadRead.IsBackground = true;
            threadRead.Start();
        }

        private bool isShowHandFlag = false;

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            isShowHandFlag = checkBox2.Checked;
        }

        /// <summary>
        /// 写入结果集
        /// </summary>
        private void input2Result()
        {
            if (!isHaveStudent())
            {
                tabControl1.SelectedIndex = 0;
                return;
            }
            if (imgMs.Count < 1)
            {
                FrmTips.ShowTipsError(this, "先录像");
                return;
            }
            makeEndResult();
            saveOkFileOut(new object());

            if (RoundCount0 > 0)
            {
                UpdateListView(_ProjectId, _GroupName, RoundCount0);
            }

            rgbVideoSourceStart();
            rgbVideoSourceStart1();
        }

        private void makeEndResult()
        {
            double fenmu = 1000;
            if (isSitting)
            {
                fenmu = 10;
            }
            string score = (MeasureLen / fenmu).ToString(ReservedDigitsTxt);
            if (distanceMode == 1) score = (m_MeasureLen / fenmu).ToString(ReservedDigitsTxt);
            nowRaceStudentData.score = Convert.ToDouble(score);
            string projectTypeCbxtxt = typeName;
            string txt_Grouptxt = _GroupName;
            string txt_GNametxt = nowRaceStudentData.name;
            string ScoreStatus = ResultState.ResultState2Str(nowRaceStudentData.state);
            /*  string scoreContent = $"时间:{DateTime.Now.ToString("yyyy年MM月dd日HH:mm:ss")} , " +
                  $"项目:{projectTypeCbxtxt} , 组别:{txt_Grouptxt} , 准考证号:{nowRaceStudentData.idNumber} ,  姓名：{txt_GNametxt} ,  " +
                  $"第{RoundCount0}次成绩:{score}, 状态:{ScoreStatus}";*/
            /*  string scoreContent = string.Format("时间:{0},项目:{1},组别:{2},准考证号:{3},姓名{4},第{5}次成绩:{6}, 状态:{7}",
                  DateTime.Now.ToString("yyyy年MM月dd日HH: mm:ss"),
                  projectTypeCbxtxt,
                  txt_Grouptxt,
                  nowRaceStudentData.idNumber,
                  txt_GNametxt,
                  RoundCount0,
                  score,
                  ScoreStatus);
              File.AppendAllText(@"./成绩日志.txt", scoreContent + "\n");*/
            //复制现有定标
            File.Copy(strMainModule + markPointFile, nowTestDir + markPointFile);
            File.Copy(strMainModule + markPointFile1, nowTestDir + markPointFile1);
            File.Copy(strMainModule + markDirectionFile, nowTestDir + markDirectionFile);
            File.Copy(strMainModule + markDirectionFile1, nowTestDir + markDirectionFile1);
            _MemoStr = richTextBox1.Text;
            updateJumpLen();
        }
        /// <summary>
        /// 写入数据库
        /// </summary>
        private void updateJumpLen()
        {
            try
            {
                string searchSql = $"select * from ResultInfos where PersonId='{nowRaceStudentData.id}' and RoundId='{RoundCount0}'";
                List<Dictionary<string, string>> list = sQLiteHelper.ExecuteReaderList(searchSql);
                if (list.Count > 0)
                {
                    MessageBox.Show("该学生本轮成绩已写入请勿重复写入成绩!");
                    return;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }


            System.Data.SQLite.SQLiteTransaction sQLiteTransaction = sQLiteHelper.BeginTransaction();
            try
            {

                var sortid = sQLiteHelper.ExecuteScalar($"SELECT MAX(SortId) + 1 FROM ResultInfos");
                string sortid0 = "1";
                if (sortid != null && sortid.ToString() != "") sortid0 = sortid.ToString();
                int state0 = nowRaceStudentData.state == 0 ? 1 : nowRaceStudentData.state;

                string selectSql = $"select * from ResultInfos where PersonId='{nowRaceStudentData.id}'";
                List<Dictionary<string, string>> list = sQLiteHelper.ExecuteReaderList(selectSql);
                List<ScoreViewPojo> stuViews = new List<ScoreViewPojo>();
                foreach (var item in list)
                {
                    ScoreViewPojo svp = new ScoreViewPojo();
                    svp.Score = item["Result"];
                    int.TryParse(item["RoundId"], out int RoundId);
                    svp.roundCound = RoundId;
                    stuViews.Add(svp);
                }
                int round0 = -1;
                for (int i = 1; i <= _RoundCount; i++)
                {
                    int v = stuViews.FindIndex(a => a.roundCound == i);
                    if (v == -1) { round0 = i; break; }
                }
                if (_IsFoul)
                {
                    nowRaceStudentData.score = 0;
                }
                if (round0 != -1)
                {
                    string sql = $"INSERT INTO ResultInfos(CreateTime,SortId,IsRemoved,PersonId,SportItemType,PersonName,PersonIdNumber,RoundId,Result,State) " +
                    $"VALUES('{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', {sortid0}, 0, '{nowRaceStudentData.id}',0,'{nowRaceStudentData.name}'," +
                    $"'{nowRaceStudentData.idNumber}',{RoundCount0},{nowRaceStudentData.score},{state0})";
                    //处理写入数据库
                    sQLiteHelper.ExecuteNonQuery(sql);
                }
                //更新状态为已考
                sQLiteHelper.ExecuteNonQuery($"UPDATE DbPersonInfos SET State = 1, FinalScore = 1 WHERE Id = '{nowRaceStudentData.id}'");
                FrmTips.ShowTipsSuccess(this, "写入成功");
            }
            catch (Exception ex)
            {
                FrmTips.ShowTipsError(this, ex.Message);
                LoggerHelper.Debug(ex);
            }
            sQLiteHelper.CommitTransaction(ref sQLiteTransaction);

        }
        /// <summary>
        /// 保存文件
        /// </summary>
        private void saveOkFileOut(object obj)
        {
            Image saveImg0 = null;
            Image saveImg1 = null;
            if (distanceMode == 0) saveImg0 = pictureBox1.Image;
            else if (distanceMode == 1) saveImg0 = pictureBox3.Image;
            saveImg1 = ImageHelper.DeepCopyBitmap((Bitmap)saveImg0);

            //nowTestDir
            string savePath = Path.Combine(nowTestDir, $"落地_{nowRaceStudentData.idNumber}.jpg");
            string avipath = Path.Combine(nowTestDir,
               $"{nowRaceStudentData.idNumber}_{nowRaceStudentData.name}_第{nowRaceStudentData.RoundId}轮落点摄像头1.mp4");
            if (File.Exists(avipath))
            {
                File.Delete(avipath);
            }
            OpenVideoOutPut(avipath);
            if (isVideoOutPutOpen() && imgMs.Count > 0)
            {
                for (int i = 0; i < imgMs.Count; i++)
                {
                    try
                    {
                        Bitmap bitmap = ImageHelper.DeepCopyBitmap((Bitmap)ImageHelper.MemoryStream2Bitmap(imgMs[i].img));
                        VideoWriteFrame(bitmap);
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Debug(ex);
                    }
                }
            }
            ReleaseVideoOutPut();

            avipath = Path.Combine(nowTestDir,
               $"{nowRaceStudentData.idNumber}_{nowRaceStudentData.name}_第{nowRaceStudentData.RoundId}轮起点摄像头2.mp4");
            if (File.Exists(avipath))
            {
                File.Delete(avipath);
            }
            OpenVideoOutPut1(avipath);

            //是否写入
            if (VideoOutPut1 != null && VideoOutPut1.IsOpened() && imgMs1.Count > 0)
            {
                for (int i = 0; i < imgMs1.Count; i++)
                {
                    try
                    {
                        Bitmap bitmap = ImageHelper.DeepCopyBitmap((Bitmap)ImageHelper.MemoryStream2Bitmap(imgMs1[i].img));
                        OpenCvSharp.Mat mat = ImageHelper.Bitmap2Mat(bitmap);
                        VideoOutPut1.Write(mat);
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Debug(ex);
                    }
                }
            }
            ReleaseVideoOutPut1();


            if (saveImg1 != null)
            {
                imgJpgSave(saveImg1, savePath);
                Image newImage = saveImg1;
                {
                    if (null != newImage)
                    {
                        using (var graphic = Graphics.FromImage(newImage))
                        {
                            // 核心参数啊，感觉相当于PS保存时间的质量设置参数
                            Int64 qualityLevel = 80L;
                            Pen pen = new Pen(Color.MediumSpringGreen, 1);
                            Font drawFont = new Font("Arial", 14);
                            SolidBrush drawBrush = new SolidBrush(Color.MediumSpringGreen);
                            System.Drawing.Point markerTopJumpT = new System.Drawing.Point(0, 0);
                            System.Drawing.Point markerBottomJumpT = new System.Drawing.Point(0, 0);
                            List<TargetPoint> TopList = new List<TargetPoint>();
                            List<TargetPoint> BottomList = new List<TargetPoint>();
                            markerTopJumpT = markerTopJump;
                            if (distanceMode == 1) markerTopJumpT = m_markerTopJump;
                            markerBottomJumpT = markerBottomJump;
                            if (distanceMode == 1) markerBottomJumpT = m_markerBottomJump;
                            if (distanceMode == 0)
                            {
                                TopList = targetPoints.FindAll(a => a.status);
                                BottomList = targetPoints.FindAll(a => !a.status);
                            }
                            else if (distanceMode == 1)
                            {
                                TopList = targetPoints1.FindAll(a => a.status);
                                BottomList = targetPoints1.FindAll(a => !a.status);
                            }
                            List<TargetPoint> TopListSort = TopList.OrderBy(a => a.x).ToList();
                            List<TargetPoint> BottomListSort = BottomList.OrderBy(a => a.x).ToList();
                            // 高质量
                            graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                            graphic.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                            qualityLevel = 100L;
                            System.Drawing.Imaging.ImageCodecInfo codec = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()[1];
                            System.Drawing.Imaging.EncoderParameters eParams = new System.Drawing.Imaging.EncoderParameters(1);
                            eParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qualityLevel);
                            {
                                //框点十字
                                int left = 0;
                                //画顶标
                                foreach (var mark in TopListSort)
                                {
                                    System.Drawing.Point p = new System.Drawing.Point(mark.x, mark.y);

                                    if (left <= TopListSort.Count - 2)
                                    {
                                        System.Drawing.Point p1 = new System.Drawing.Point(TopListSort[left + 1].x, TopListSort[left + 1].y);
                                        graphic.DrawLine(pen, p, p1);
                                        left++;
                                    }
                                    PointHelper.Instance.DrawPointCross(graphic, p, pen);
                                    PointHelper.Instance.DrawPointText(graphic, $"({mark.name}){mark.dis}cm", p, drawFont, drawBrush, 0, 60, 0, 30);
                                }
                                left = 0;
                                //画底标
                                foreach (var mark in BottomListSort)
                                {
                                    System.Drawing.Point p = new System.Drawing.Point(mark.x, mark.y);

                                    if (left <= BottomListSort.Count - 2)
                                    {
                                        System.Drawing.Point p1 = new System.Drawing.Point(BottomListSort[left + 1].x, BottomListSort[left + 1].y);
                                        graphic.DrawLine(pen, p, p1);
                                        left++;
                                    }
                                    PointHelper.Instance.DrawPointCross(graphic, p, pen);
                                    PointHelper.Instance.DrawPointText(graphic, $"({mark.name}){mark.dis}m", p, drawFont, drawBrush, 0, 60, 1, 10);
                                }
                                //中间框点连线
                                int min = TopListSort.Count > BottomListSort.Count ? BottomListSort.Count : TopListSort.Count;
                                for (int i = 0; i < min; i++)
                                {
                                    TargetPoint mark = TopListSort[i];
                                    TargetPoint mark1 = BottomListSort[i];
                                    System.Drawing.Point p = new System.Drawing.Point(mark.x, mark.y);
                                    System.Drawing.Point p1 = new System.Drawing.Point(mark1.x, mark1.y);
                                    graphic.DrawLine(pen, p, p1);
                                }
                            }

                            pen.Color = Color.Red;
                            drawFont = new Font("Arial", 32);
                            graphic.DrawLine(pen, markerTopJumpT, markerBottomJumpT);
                            drawBrush = new SolidBrush(Color.Red);// Create point for upper-left corner of drawing.
                            double MeasureLenTemp = 0;
                            if (distanceMode == 0)
                            {
                                MeasureLenTemp = MeasureLen;
                            }
                            else if (distanceMode == 1)
                            {
                                MeasureLenTemp = m_MeasureLen;
                            }

                            if (_IsFoul)
                            {
                                //犯规
                                graphic.DrawString("犯规", drawFont, drawBrush, markerBottomJumpT.X - 70, markerBottomJumpT.Y);
                            }
                            else
                            {
                                if (isSitting)
                                {
                                    graphic.DrawString((MeasureLenTemp / 10).ToString(ReservedDigitsTxt) + dangwei, drawFont, drawBrush, markerBottomJumpT.X - 70, markerBottomJumpT.Y);
                                }
                                else
                                {
                                    graphic.DrawString((MeasureLenTemp / 1000).ToString(ReservedDigitsTxt) + dangwei, drawFont, drawBrush, markerBottomJumpT.X - 70, markerBottomJumpT.Y);
                                }
                            }
                            //时间
                            graphic.FillRectangle(new SolidBrush(Color.White), new Rectangle(new System.Drawing.Point(350, 0), new System.Drawing.Size(300, 50)));
                            graphic.DrawString($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}", new Font("Arial", 20), drawBrush, 360, 10);

                            //考生姓名和学号
                            graphic.FillRectangle(new SolidBrush(Color.White), new Rectangle(new System.Drawing.Point(700, 0), new System.Drawing.Size(540, 50)));
                            graphic.DrawString($"组号:{nowRaceStudentData.groupName} 考号:{nowRaceStudentData.idNumber} 姓名:{nowRaceStudentData.name}", new Font("Arial", 18), drawBrush, 710, 10);
                            if (isShotPut)
                            {
                                pen.DashPattern = new float[] { 5f, 10f };
                                graphic.DrawLine(pen, cenMarkPoint, mousePoint);
                            }
                            string imgOkFileName = Path.Combine(nowTestDir, $"{nowRaceStudentData.idNumber}.jpg");// = 0;//第几次考试
                            if (File.Exists(imgOkFileName))
                                File.Delete(imgOkFileName);

                            newImage.Save(imgOkFileName, codec, eParams);
                        }
                    }
                }
            }
            try
            {
                double MeasureLenTemp = 0;
                if (distanceMode == 0)
                {
                    MeasureLenTemp = MeasureLen;
                }
                else if (distanceMode == 1)
                {
                    MeasureLenTemp = m_MeasureLen;
                }
                string MeasureLenTempStr = "";
                if (isSitting)
                {
                    MeasureLenTempStr = (MeasureLenTemp / 10).ToString(ReservedDigitsTxt) + dangwei;
                }
                else
                {
                    MeasureLenTempStr = (MeasureLenTemp / 1000).ToString(ReservedDigitsTxt) + dangwei;
                }
                string scoreContent = string.Format("开始时间:{0},结束时间:{1},组别:{2},准考证号:{3},姓名:{4},第{5}次成绩:{6},状态:{7},备注信息:{8}",
                                    _beginTime,
                                    _endTime,
                                    nowRaceStudentData.groupName,
                                    nowRaceStudentData.idNumber,
                                    nowRaceStudentData.name,
                                    RoundCount0,
                                    MeasureLenTempStr,
                                    nowRaceStudentData.state <= 1 ? "正常" : ResultState.ResultState2Str(nowRaceStudentData.state),
                                    _MemoStr
                                    );
                string studentLogSavePath = Path.Combine(nowTestDir, $"{nowRaceStudentData.idNumber}_{nowRaceStudentData.name}.log");
                File.AppendAllText(studentLogSavePath, scoreContent);
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }


            List<ImgMsS> imgMsTemp = imgMs;
            if (distanceMode == 1) imgMsTemp = imgMs1;
            //当前图片索引
            int nowFileName1 = PointHelper.Instance.Str2int(nowFileName);
            if (isBallCheckBoxb)
            {
                if (nowFileName1 == 0 || nowFileName1 > imgMsTemp.Count - 1)
                    return;
            }
            /* //保存前面第5帧
             int sum = 5;
             for (int i = 0; i < sum; i++)
             {
                 if (i < imgMsTemp.Count)
                 {
                     savePath = Path.Combine(nowTestDir, $"{nowRaceStudentData.idNumber}_开始{i}.jpg");
                     imgJpgSave(ImageHelper.MemoryStream2Bitmap(imgMsTemp[i].img), savePath);
                 }
             }
             //落地前5帧
             int sp = 5;
             int fsum = nowFileName1 - sp;
             if (fsum < 0)
             {
                 fsum = 0;
             }
             for (int i = fsum; i < nowFileName1; i++)
             {
                 if (i < imgMsTemp.Count)
                 {
                     savePath = Path.Combine(nowTestDir, $"{nowRaceStudentData.idNumber}_落地前{i}.jpg");
                     imgJpgSave(ImageHelper.MemoryStream2Bitmap(imgMsTemp[i].img), savePath);
                 }
             }
             //落地后
             int backsum = nowFileName1 + sp;
             if (backsum >= imgMsTemp.Count)
             {
                 backsum = imgMsTemp.Count;
             }
             for (int i = nowFileName1; i < backsum; i++)
             {
                 if (i < imgMsTemp.Count)
                 {
                     savePath = Path.Combine(nowTestDir, $"{nowRaceStudentData.idNumber}_落地后{i}.jpg");
                     imgJpgSave(ImageHelper.MemoryStream2Bitmap(imgMsTemp[i].img), savePath);
                 }
             }*/
        }
        private void imgJpgSave(Image img, string path)
        {
            if (File.Exists(path))
                File.Delete(path);
            img.Save(path);
        }
        /// <summary>
        /// 上一轮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button14_Click(object sender, EventArgs e)
        {
            preRound();
        }
        /// <summary>
        /// 
        /// </summary>
        private void preRound()
        {
            int selectIndex = RoundCountCbx.SelectedIndex;
            int round = RoundCountCbx.Items.Count;
            selectIndex--;
            if (selectIndex >= 0 && selectIndex < round)
            {
                RoundCount0 = selectIndex + 1;
                RoundCountCbx.SelectedIndex = selectIndex;
            }
            else
            {
                dicPerson();
            }
        }
        /// <summary>
        /// 下一轮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button15_Click(object sender, EventArgs e)
        {
            nextRound();
        }
        private void nextRound()
        {
            int GroupselectIndex = groupCbx.SelectedIndex;
            int Groupround = groupCbx.Items.Count;
            int selectIndex = RoundCountCbx.SelectedIndex;
            int round = RoundCountCbx.Items.Count;

            StuViewPojo svp = stuViewPojos.Find(a => a.IdNumber == AdminUserIdNumber);
            if (svp.isMaxScore)
            {
                nextPerson();
                return;
            }
            while (selectIndex < round)
            {
                if (svp.ScoreViewPojos.FindIndex(a => a.roundCound == (selectIndex + 1)) == -1)
                {
                    break;
                }
                selectIndex++;
            }
            if (selectIndex == round)
            {
                if (!string.IsNullOrEmpty(AdminUserIdNumber))
                {
                    nextPerson();
                }
            }
            else
            {
                RoundCount0 = selectIndex + 1;
                RoundCountCbx.SelectedIndex = selectIndex;
            }
        }

        /// <summary>
        /// 上一人
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button17_Click(object sender, EventArgs e)
        {
            writeCount++;
            dicPerson();
        }
        private void dicPerson()
        {
            /*int GroupselectIndex = groupCbx.SelectedIndex;
            int Groupround = groupCbx.Items.Count;
            int ncount = stuView.Rows.Count;
            int groupIndex = 0;
            if (!string.IsNullOrEmpty(AdminUserIdNumber))
            {
               for (int i = 0; i < ncount; i++)
               {
                  try
                  {
                     string idnumber0 = stuView.Rows[i].Cells[1].Value.ToString();
                     if (idnumber0 == AdminUserIdNumber)
                     {
                        groupIndex = i;
                        break;
                     }
                  }
                  catch (Exception ex)
                  {
                     groupIndex = 0;
                     LoggerHelper.Debug(ex);
                  }
               }
               groupIndex--;
               if (groupIndex >= 0)
               {
                  //不是本组最后一个人
                  //索引到下一个人
                  //AdminUserIdNumber = stuView.Rows[groupIndex].Cells[1].Value.ToString();
                  stuView.CurrentCell = stuView.Rows[groupIndex].Cells[1];
               }
               else
               {
                  //本组最后一个人就下一组
                  GroupselectIndex--;
                  if (GroupselectIndex >= 0 && GroupselectIndex < Groupround)
                  {
                     groupCbx.SelectedIndex = GroupselectIndex;
                  }
               }
               if (writeCount == 0)
               {
                  ///轮次满了就下一个人
                  RoundCountCbx.SelectedIndex = 0;
               }
               writeCount = 0;
            }*/

            try
            {
                int GroupselectIndex = groupCbx.SelectedIndex;
                int Groupround = groupCbx.Items.Count;
                int ncount = stuView.Rows.Count;
                int groupIndex = 0;
                if (!string.IsNullOrEmpty(AdminUserIdNumber))
                {
                    int index = stuViewPojos.FindIndex(a => a.IdNumber == AdminUserIdNumber);
                    if (index == -1) return;
                    if (index == 0)
                    {
                        //第一个人
                        GroupselectIndex--;
                        if (GroupselectIndex >= 0 && GroupselectIndex < Groupround)
                        {
                            groupCbx.SelectedIndex = GroupselectIndex;
                        }
                        return;
                    }
                    index--;
                    //选择下一个考生
                    comboBox2.SelectedIndex = index;

                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
            finally
            {

            }

        }
        /// <summary>
        /// 下一人
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button16_Click(object sender, EventArgs e)
        {
            writeCount++;
            nextPerson();
        }
        /// <summary>
        /// 
        /// </summary>
        private void nextPerson()
        {
            #region 旧

            /* int GroupselectIndex = groupCbx.SelectedIndex;
             int Groupround = groupCbx.Items.Count;
             int ncount = stuView.Rows.Count;
             int groupIndex = 0;
             if (!string.IsNullOrEmpty(AdminUserIdNumber))
             {
                for (int i = 0; i < ncount; i++)
                {
                   try
                   {
                      string idnumber0 = stuView.Rows[i].Cells[1].Value.ToString();
                      if (idnumber0 == AdminUserIdNumber)
                      {
                         groupIndex = i;
                         break;
                      }
                   }
                   catch (Exception ex)
                   {
                      groupIndex = 0;
                      LoggerHelper.Debug(ex);
                   }
                }
                groupIndex++;
                if (groupIndex < ncount)
                {
                   //不是本组最后一个人
                   //索引到下一个人
                   //AdminUserIdNumber = stuView.Rows[groupIndex].Cells[1].Value.ToString();
                   stuView.CurrentCell = stuView.Rows[groupIndex].Cells[1];
                }
                else
                {
                   //本组最后一个人就下一组
                   GroupselectIndex++;
                   if (GroupselectIndex >= 0 && GroupselectIndex < Groupround)
                   {
                      groupCbx.SelectedIndex = GroupselectIndex;
                   }
                }
                //if (writeCount == 0)
                {
                   ///轮次满了就下一个人
                   RoundCountCbx.SelectedIndex = 0;
                }
                writeCount = 0;

             }*/

            #endregion


            try
            {
                int GroupselectIndex = groupCbx.SelectedIndex;
                int Groupround = groupCbx.Items.Count;
                int ncount = stuView.Rows.Count;
                if (!string.IsNullOrEmpty(AdminUserIdNumber))
                {
                    int index = stuViewPojos.FindIndex(a => a.IdNumber == AdminUserIdNumber);
                    if (index == -1) return;
                    if (index == stuViewPojos.Count - 1)
                    {
                        //最后一人
                        GroupselectIndex++;
                        if (GroupselectIndex >= 0 && GroupselectIndex < Groupround)
                        {
                            groupCbx.SelectedIndex = GroupselectIndex;
                        }
                        return;
                    }
                    index++;
                    //满分自动下一个
                    while (index <= comboBox2.Items.Count - 2)
                    {
                        if (stuViewPojos[index].isMaxScore || stuViewPojos[index].ScoreViewPojos.Count == _RoundCount)
                        {
                            //选择下一个考生
                            index++;

                        }
                        else
                        {
                            break;
                        }
                    }
                    comboBox2.SelectedIndex = index;

                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
            finally
            {

            }

        }
        /// <summary>
        /// 比赛模式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex < 0) return;
            _TestMethod = comboBox1.SelectedIndex;
            try
            {
                if (_TestMethod == 0)
                {
                    //0:自动下一位 1:自动下一轮
                    checkBox3.Text = "自动下一位";
                }
                else if (_TestMethod == 1)
                {
                    checkBox3.Text = "自动下一轮";
                }
                string sql = $"UPDATE SportProjectInfos SET TestMethod={_TestMethod} where Id='{_ProjectId}'";
                int result = sQLiteHelper.ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = comboBox2.SelectedIndex;
            if (index == -1) index = 0;
            if (index >= 0 && index < stuView.Rows.Count)
            {
                if (stuView.CurrentCell.RowIndex != index)
                    stuView.CurrentCell = stuView.Rows[index].Cells[1];
            }
            /* StuViewPojo stuViewPojo = stuViewPojos.Find(a => a.IdNumber == AdminUserIdNumber);
             if (stuViewPojo != null)
             {
                bool b = true;
                for (int i = 1; i <= _RoundCount; i++)
                {
                   int v = stuViewPojo.ScoreViewPojos.FindIndex(a => a.roundCound == i);
                   if (v == -1&& i<= RoundCountCbx.Items.Count)
                   {
                      RoundCountCbx.SelectedIndex = i - 1;
                      b = false; break;
                   }
                }
                if (b)
                {
                   RoundCountCbx.SelectedIndex = RoundCountCbx.Items.Count - 1;
                }
             }*/

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel3_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (stuView.SelectedRows.Count == 0) return;

                contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toleranceTxt_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(toleranceTxt.Text, out tolerance);
            if (tolerance == 0)
            {
                tolerance = 30;
                toleranceTxt.Text = tolerance + "";
            }

            File.WriteAllText(Path.Combine(strMainModule, "tolerance.dat"), tolerance + "");
        }
        bool updateBackImgFlag = false;
        private void button4_Click(object sender, EventArgs e)
        {
            updateBackImgFlag = true;
        }
        private void button5_Click(object sender, EventArgs e)
        {
            //W Task.Run(() => voiceOut0($"{nowRaceStudentData.name}准备考试"));
            Task.Run(() => voiceOut0($"{nowRaceStudentData.name}第{nowRaceStudentData.RoundId}轮准备考试"));
            SendScore(nowRaceStudentData.name, "准备考试", "");

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            BeginRec();
        }
        #endregion 控制面板
        #region 单人显示屏
        private SerialPortHelper sReader = null;
        private void AnalyData(byte[] btData)
        {
            string v = Encoding.UTF8.GetString(btData, 0, btData.Length);
            WriteLog(lrtxtLog, v, 0);
            if (v.Contains("ch="))
            {
                string v1 = v.Replace("ch=", "");
                v1 = v1.Trim();
                textBox1.Text = v1;

            }
        }

        private void ReceiveData(byte[] btData)
        {
        }

        private void SendData(byte[] btData)
        {
        }
        /// <summary>
        /// 初始化串口
        /// </summary>
        /// <returns></returns>
        private bool serialInit()
        {
            bool flag = true;
            try
            {
                sReader = new SerialPortHelper();
                sReader.AnalyCallback = AnalyData;
                sReader.ReceiveCallback = ReceiveData;
                sReader.SendCallback = SendData;

                SerialTool.init();
                ConnectPort();
            }
            catch (Exception)
            {
                flag = false;
            }
            return flag;
        }
        private void openSerialPortBtn_Click(object sender, EventArgs e)
        {
            ConnectPort();
        }
        private void pidtxt_TextChanged(object sender, EventArgs e)
        {
            File.WriteAllText(Path.Combine(strMainModule, "pidtxt.dat"), pidtxt.Text);
        }
        private void vidtxt_TextChanged(object sender, EventArgs e)
        {
            File.WriteAllText(Path.Combine(strMainModule, "vidtxt.dat"), vidtxt.Text);
        }
        private void groupCbx_MouseLeave(object sender, EventArgs e)
        {
            label5.Focus();
        }
        /// <summary>
        /// 读取计分牌id
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                string code = "readCh?";
                byte[] readCh = Encoding.UTF8.GetBytes(code);
                sReader.SendMessage(readCh);
            });
        }
        /// <summary>
        /// 设置计分牌id
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button11_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                string id = textBox1.Text;
                string code = "setCh" + id;
                byte[] readCh = Encoding.UTF8.GetBytes(code);
                sReader.SendMessage(readCh);
            });
        }
        /// <summary>
        /// 串口连接
        /// </summary>
        private void ConnectPort()
        {

            if (openSerialPortBtn.Text == "断开显示屏")
            {
                if (sReader != null && sReader.IsComOpen())
                {
                    //处理串口断开连接读写器
                    sReader.CloseCom();
                }
                ScreenState.Text = "单人显示屏未连接";
                ScreenState.ForeColor = Color.Red;
                openSerialPortBtn.Text = "连接显示屏";
            }
            else if (openSerialPortBtn.Text == "连接显示屏")
            {
                try
                {
                    string pid = pidtxt.Text;
                    string vid = vidtxt.Text;
                    string portn = portNameSearch.Text;
                    List<string> strPorts = new List<string>();
                    bool flag = true;
                    if (portn == "USB Serial Port")
                    {
                        //无线
                        string[] portNames = getPortDeviceName(portn);
                        if (portNames.Length == 0)
                        {
                            portn = "USB-SERIAL";
                            portNames = getPortDeviceName(portn);
                        }
                        if (portNames.Length == 0)
                        {
                            portn = "USB-to-Serial";
                            portNames = getPortDeviceName(portn);
                        }
                        strPorts = portNames.ToList();
                        tb_nBaudrate.Text = "115200";
                    }
                    if (strPorts.Count == 0)
                    {
                        portn = "USB 串行设备";
                        ///有线
                        strPorts = getScreenCom(pid, vid);
                        if (strPorts.Count > 0)
                        {
                            flag = false;
                            portNameSearch.Text = portn;
                            tb_nBaudrate.Text = "57600";
                        }
                    }


                    string strPort = string.Empty;
                    if (strPorts.Count > 0)
                    {
                        strPort = strPorts[0];
                    }
                    //string strPort = SerialTool.PortDeviceName2PortName(portNamesList.Text);
                    if (string.IsNullOrEmpty(strPort))
                    {
                        //FrmTips.ShowTipsError(this, "请连接单人显示屏串口");
                        ScreenState.Text = "单人显示屏未连接";
                        ScreenState.ForeColor = Color.Red;
                        return;
                    }

                    int.TryParse(tb_nBaudrate.Text, out int nBaudrate);
                    string strException = string.Empty;
                    int nRet = sReader.OpenCom(strPort, nBaudrate, out strException);
                    if (nRet == -1)
                    {
                        ScreenState.Text = "单人显示屏未连接";
                        ScreenState.ForeColor = Color.Red;
                        openSerialPortBtn.Text = "连接显示屏";
                        //FrmTips.ShowTipsError(this, "打开单人显示屏失败");
                    }
                    else
                    {
                        HideFun1(flag);
                        ScreenState.Text = "单人显示屏已连接连接";
                        ScreenState.ForeColor = Color.Green;
                        openSerialPortBtn.Text = "断开显示屏";
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Debug(ex);
                }
            }
        }
        public void HideFun1(bool flag)
        {
            button7.Visible = flag;
            button11.Visible = flag;
            label28.Visible = flag;
            textBox1.Visible = flag;

        }
        /// <summary>
        /// 获取串口
        /// </summary>
        /// <param name="portName"></param>
        /// <returns></returns>
        private string[] getPortDeviceName(string portName = null)
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
        /// <param name="pid0"></param>
        /// <param name="vid0"></param>
        /// <returns></returns>
        private List<string> getScreenCom(string pid0 = "1000", string vid0 = "1122")
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
                        string str1 = current["Caption"].ToString();// "(COM173"
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
                            if (strPid == pid0 && strVid == vid0)
                            {
                                commData = current["Caption"] + "";
                                commData = SerialTool.PortDeviceName2PortName(commData);
                                coms.Add(commData);
                            }
                        }
                    }
                }
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

        /// <summary>
        /// 发送成绩至小屏
        /// </summary>
        /// <param name="name"></param>
        /// <param name="Score"></param>
        private bool SendScore(string name, string Score, string txt3 = "米")
        {
            if (sReader == null || !sReader.IsComOpen()) return false;
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
                byte[] result = SerialTool.PushText_BL2(name, c1, Score, c2, txt3, c3);
                Task.Run(() =>
                {
                    for (int i = 0; i < 3; i++)
                    {
                        sReader.SendMessage(result);
                        Thread.Sleep(300);
                    }
                });
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
                return false;
            }
            return true;
        }
        private void lrtxtLog_DoubleClick(object sender, EventArgs e)
        {
            lrtxtLog.BeginInvoke(new ThreadStart((MethodInvoker)delegate ()
            {
                lrtxtLog.Clear();
            }));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logRichTxt"></param>
        /// <param name="strLog"></param>
        /// <param name="nType"></param>
        public void WriteLog(CustomControl.LogRichTextBox logRichTxt, string strLog, int nType)
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
        #endregion 单人显示屏
        #region 远距离摄像头相关函数
        private bool rgbVideoSourcePaintFlag1 = false;
        private int skipFrameDispR1 = 0;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hScrollBar2_ValueChanged(object sender, EventArgs e)
        {
            if (imgMs1.Count == 0)
            {
                hScrollBar2.Value = 0;
                hScrollBar2.Maximum = 0;
                return;
            }
            int sp = hScrollBar2.Value;
            int imgcount = imgMs1.Count - 1;
            hScrollBar2.Maximum = imgcount;

            if (sp > imgcount)
            {
                sp = imgcount;
            }
            if (null != imgMs1[sp])
            {
                nowFileName = sp + "";
                pictureBox3.Image = ImageHelper.MemoryStream2Bitmap(imgMs1[sp].img);
                recImgIndex.Text = $"图片索引:{nowFileName}";
                rgbVideoSourceRePaint();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="image"></param>
        private void videoSourcePlayer1_NewFrame(object sender, ref Bitmap image)
        {
            skipFrameDispR1++;
            FrameFlash1++;
            if (rgbVideoSourcePaintFlag1) return;
            rgbVideoSourcePaintFlag1 = true;
            //float offsetX = pBox1Width * 1f / bmp.Width;
            //float offsetY = pBox1Height * 1f / bmp.Height;
            try
            {
                if (skipFrameDispR1 < cameraSkip1)
                {
                    return;
                }
                skipFrameDispR1 = 0;
                if (image != null)
                {
                    //image = (Bitmap)ImageUtil.ScaleImage(image, _width, _height);
                    //得到当前RGB摄像头下的图片
                    Bitmap bmp = ImageHelper.DeepCopyBitmap(image);
                    if (bmp == null)
                    {
                        return;
                    }

                    if (recTimeR0 > 0)
                    {
                        ImgMsS mss = new ImgMsS();
                        mss.dt = DateTime.Now;
                        mss.name = "img" + imgMs1.Count;
                        Bitmap bitmap = ImageHelper.DeepCopyBitmap(bmp);
                        mss.img = ImageHelper.Bitmap2MemoryStream(bitmap);
                        imgMs1.Add(mss);
                        frameSum1++;
                    }
                    //frameRecSum++;//计算帧速用
                    if (FrameFlash1 > FrameFlashCount1)
                    {
                        FrameFlash1 = 0;
                        //显示图片
                        pictureBox3.Image = bmp;
                    }

                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Debug(ex);
            }
            finally
            {
                GCCounta++;
                if (GCCounta > 10)
                {
                    GCCounta = 0;
                    clearMemory();
                }
                rgbVideoSourcePaintFlag1 = false;
            }
        }
        /// <summary>
        /// 男生最大值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            double.TryParse(textBox2.Text, out boy_MaxScore);
            boy_MaxScore = boy_MaxScore > 0 ? boy_MaxScore : 3.6;
        }
        /// <summary>
        /// 女生最大值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            double.TryParse(textBox3.Text, out girl_MaxScore);
            girl_MaxScore = girl_MaxScore > 0 ? girl_MaxScore : 3.6;
        }

        int FrameFlash0 = 0;
        int FrameFlash1 = 0;

        /// <summary>
        /// 跳帧数
        /// </summary>
        int FrameFlashCount0 = 10;
        int FrameFlashCount1 = 10;
        /// <summary>
        /// 界面刷新率0
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex >= 0)
            {
                FrameFlashCount0 = 10 + comboBox3.SelectedIndex * 5;
                FrameFlashCount0 = maxFps / FrameFlashCount0;

            }

        }
        /// <summary>
        /// 界面刷新率1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox4.SelectedIndex >= 0)
            {
                FrameFlashCount1 = 10 + comboBox4.SelectedIndex * 5;
                FrameFlashCount1 = maxFps1 / FrameFlashCount1;
            }
        }

        /// <summary>
        /// 摄像头远近模式,0近,1远
        /// </summary>
        int distanceMode = 0;
        private void tabControl3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl3.SelectedIndex == 1)
            {
                distanceMode = 0;
            }
            else if (tabControl3.SelectedIndex == 0)
            {
                if (isSitting)
                {
                    tabControl3.SelectedIndex = 1;
                    distanceMode = 0;
                }
                else
                {
                    distanceMode = 1;
                }
            }
        }
        /// <summary>
        /// 远距离摄像头设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button18_Click(object sender, EventArgs e)
        {
            if (recTimeR0 > 0)
            {
                FrmTips.ShowTipsError(this, "考试中请勿进行此操作");
                return;
            }
            openCameraSetting1();
        }
        /// <summary>
        /// 远距离摄像头打开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button19_Click(object sender, EventArgs e)
        {
            if (recTimeR0 > 0)
            {
                FrmTips.ShowTipsError(this, "考试中请勿进行此操作");
                return;
            }
            if (openCameraBtn1.Text == "关闭摄像头")
            {
                CloseCamera1();
                //openCameraBtn.Text = "打开摄像头";
                return;
            }
            if (string.IsNullOrEmpty(cameraName1))
            {
                FrmTips.ShowTipsError(this, "请选择摄像头!");
                return;
            }
            bool flag = openCamearaFun(cameraIndex1);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox3_Paint(object sender, PaintEventArgs e)
        {
            if (recTimeR0 > 0) return;
            #region 画图

            Pen pen = new Pen(Color.MediumSpringGreen, 1);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Font drawFont = new Font("Arial", 14);
            SolidBrush drawBrush = new SolidBrush(Color.MediumSpringGreen);
            List<TargetPoint> TopList = targetPoints1.FindAll(a => a.status);
            List<TargetPoint> BottomList = targetPoints1.FindAll(a => !a.status);
            List<TargetPoint> TopListSort = TopList.OrderBy(a => a.x).ToList();
            List<TargetPoint> BottomListSort = BottomList.OrderBy(a => a.x).ToList();
            //框点十字
            int left = 0;
            foreach (var mark in TopListSort)
            {
                System.Drawing.Point p = new System.Drawing.Point(mark.x, mark.y);
                if (left <= TopListSort.Count - 2)
                {
                    System.Drawing.Point p1 = new System.Drawing.Point(TopListSort[left + 1].x, TopListSort[left + 1].y);
                    e.Graphics.DrawLine(pen, p, p1);
                    left++;
                }
                PointHelper.Instance.DrawPointCross(e.Graphics, p, pen);
                PointHelper.Instance.DrawPointText(e.Graphics, $"({mark.name}){mark.dis}cm", p, drawFont, drawBrush, 0, 20, 0, 30);
            }
            left = 0;
            foreach (var mark in BottomListSort)
            {
                System.Drawing.Point p = new System.Drawing.Point(mark.x, mark.y);

                if (left <= BottomListSort.Count - 2)
                {
                    System.Drawing.Point p1 = new System.Drawing.Point(BottomListSort[left + 1].x, BottomListSort[left + 1].y);
                    e.Graphics.DrawLine(pen, p, p1);
                    left++;
                }
                PointHelper.Instance.DrawPointCross(e.Graphics, p, pen);
                PointHelper.Instance.DrawPointText(e.Graphics, $"({mark.name}){mark.dis}cm", p, drawFont, drawBrush, 0, 20, 1, 10);
            }
            //中间框点连线
            int min = TopListSort.Count > BottomListSort.Count ? BottomListSort.Count : TopListSort.Count;
            for (int i = 0; i < min; i++)
            {
                if (i != 0 && i != min - 1)
                {
                    continue;
                }
                TargetPoint mark = TopListSort[i];
                TargetPoint mark1 = BottomListSort[i];
                System.Drawing.Point p = new System.Drawing.Point(mark.x, mark.y);
                System.Drawing.Point p1 = new System.Drawing.Point(mark1.x, mark1.y);
                e.Graphics.DrawLine(pen, p, p1);
            }
            //鼠标画十字
            Pen pen1 = new Pen(Color.Red, 1);
            PointHelper.Instance.DrawPointCross(e.Graphics, mouseMovePoint, pen1);
            double fenmu = 1000;
            if (isSitting)
            {
                fenmu = 10;
            }
            string LenX = (m_MeasureLenX / fenmu).ToString(ReservedDigitsTxt);
            string LenY = (m_MeasureLenY / fenmu).ToString(ReservedDigitsTxt);
            string Len = (m_MeasureLen / fenmu).ToString(ReservedDigitsTxt);

            pen.Color = Color.Red;
            e.Graphics.DrawLine(pen, m_markerTopJump, m_markerBottomJump);
            PointHelper.Instance.DrawPointCross(e.Graphics, mousePoint, pen1);
            drawFont = new Font("微软雅黑", 28, FontStyle.Bold);
            drawBrush = new SolidBrush(Color.Red);// Create point for upper-left corner of drawing.
            PointHelper.Instance.DrawPointText(e.Graphics, $"成绩:{Len}{dangwei}", new System.Drawing.Point(10, 10), drawFont, drawBrush, 1, 10, 1, 0);
            e.Graphics.FillRectangle(new SolidBrush(Color.White), new Rectangle(new System.Drawing.Point(350, 0), new System.Drawing.Size(1000, 50)));
            //时间
            e.Graphics.DrawString($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}", new Font("微软雅黑", 14), drawBrush, 350, 15);
            //考生姓名和学号
            if (isHaveStudent(false))
            {
                e.Graphics.DrawString($"组号:{nowRaceStudentData.groupName} 考号:{nowRaceStudentData.idNumber} 姓名:{nowRaceStudentData.name}", new Font("微软雅黑", 14), drawBrush, 550, 15);
            }

            e.Graphics.DrawString($"X:{LenX},Y:{LenY}", new Font("微软雅黑", 14, FontStyle.Bold), new SolidBrush(Color.Blue), 1100, 15);
            if (isShotPut)
            {
                pen.DashPattern = new float[] { 5f, 10f };
                e.Graphics.DrawLine(pen, cenMarkPoint1, mousePoint);
            }
            #endregion 画图
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="reason"></param>
        private void rgbVideoSource_PlayingFinished(object sender, ReasonToFinishPlaying reason)
        {
            //pictureBox1.Image = null;
        }
        private void videoSourcePlayer1_PlayingFinished(object sender, ReasonToFinishPlaying reason)
        {
            //pictureBox3.Image = null;
        }
        bool isSpeakScoreVoice = true;
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            isSpeakScoreVoice = checkBox4.Checked;
        }


        #region 异常成绩
        double bUnusualScoreMin = 0;
        double bUnusualScoreMax = 3;
        double gUnusualScoreMin = 0;
        double gUnusualScoreMax = 3;
        private void bUnusualScoreMinTb_TextChanged(object sender, EventArgs e)
        {
            double.TryParse(bUnusualScoreMinTb.Text, out bUnusualScoreMin);
        }
        private void bUnusualScoreMaxTb_TextChanged(object sender, EventArgs e)
        {
            double.TryParse(bUnusualScoreMaxTb.Text, out bUnusualScoreMax);
            if (bUnusualScoreMax == 0) bUnusualScoreMax = 3;
        }
        private void gUnusualScoreMinTb_TextChanged(object sender, EventArgs e)
        {
            double.TryParse(gUnusualScoreMinTb.Text, out gUnusualScoreMin);
        }
        private void gUnusualScoreMaxTb_TextChanged(object sender, EventArgs e)
        {
            double.TryParse(gUnusualScoreMaxTb.Text, out gUnusualScoreMax);
            if (gUnusualScoreMax == 0) gUnusualScoreMax = 3;
        }
        #endregion
        private void tabPage7_Click(object sender, EventArgs e)
        {
            BarCodeTextFoucs(false);
        }
        private void serialConnectStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }
        private void button20_Click(object sender, EventArgs e)
        {
            isPrintFlag = false;
            OutPutScore1();
        }

        public int _VerifyRound = 0;
        public string _nowTestDir = String.Empty;

        private void 成绩审核ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //ReadVideo0();
            return;
            bool flag = verifyPassword();
            if (flag)
            {
                ScoreReviewFun();
            }
        }

        /// <summary>
        /// 成绩审核
        /// </summary>
        private void ScoreReviewFun()
        {
            CloseCamera();
            CloseCamera1();
            MessageBox.Show("成绩审核后请重启软件!");
            showChooseRoundDialog showChooseRoundDialog = new showChooseRoundDialog();
            if (showChooseRoundDialog.ShowDialog() == DialogResult.OK)
            {
                _VerifyRound = showChooseRoundDialog.selectRound;
                try
                {
                    if (stuView.SelectedRows.Count == 0)
                    {
                        FrmTips.ShowTipsError(this, "未选择考生");
                        return;
                    }
                    string idNumber = stuView.SelectedRows[0].Cells[1].Value.ToString();
                    string name = stuView.SelectedRows[0].Cells[2].Value.ToString();
                    string nowTestDir1 = $"\\{_ProjectName}\\{_GroupName}\\{idNumber}_{name}\\第{_VerifyRound}轮";
                    _nowTestDir = ScoreDir + nowTestDir1;
                    if (Directory.Exists(_nowTestDir))
                    {
                        string video0Path = Path.Combine(_nowTestDir, $"{idNumber}_{name}_第{_VerifyRound}轮起点摄像头2");
                        string video1Path = Path.Combine(_nowTestDir, $"{idNumber}_{name}_第{_VerifyRound}轮落点摄像头1");

                    }
                    else
                    {
                        FrmTips.ShowTipsError(this, "未找到文件夹");
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Debug(ex);
                }
            }
        }
        /// <summary>
        /// 起点摄像头
        /// </summary>
        /// <param name="path"></param>
        void ReadVideo0(string path)
        {
            string rtspStr = "E:\\wjxSportProjects\\2023-05-04\\画线测距\\bin\\Debug\\data\\Score\\揭西县第二华侨中学\\7\\23522203500031_黄腾\\第1轮\\23522203500031_黄腾_第1轮起点摄像头2.mp4";
            rtspStr = path;
            var capture = new VideoCapture();
            capture.Open(rtspStr);
            //此处参考网上的读取方法
            int sleepTime = (int)Math.Round(1000 / capture.Fps);
            // 声明实例 Mat类
            Mat image = new Mat();
            imgMs.Clear();
            // 进入读取视频每镇的循环
            while (true)
            {
                capture.Read(image);
                //判断是否还有没有视频图像 
                if (image.Empty())
                    break;
                // 在picturebox中播放视频， 需要先转换成bitmap格式
                Bitmap bmp = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);
                ImgMsS mss = new ImgMsS();
                mss.dt = DateTime.Now;
                mss.name = "img" + imgMs1.Count;
                Bitmap bitmap = ImageHelper.DeepCopyBitmap(bmp);
                mss.img = ImageHelper.Bitmap2MemoryStream(bitmap);
                imgMs1.Add(mss);
            }
            MessageBox.Show("读取完成");
        }
        /// <summary>
        /// 落点摄像头
        /// </summary>
        /// <param name="path"></param>
        void ReadVideo1(string path)
        {
            string rtspStr = "E:\\wjxSportProjects\\2023-05-04\\画线测距\\bin\\Debug\\data\\Score\\揭西县第二华侨中学\\7\\23522203500031_黄腾\\第1轮\\23522203500031_黄腾_第1轮落点摄像头1.mp4";
            rtspStr = path;
            var capture = new VideoCapture();
            capture.Open(rtspStr);
            //此处参考网上的读取方法
            int sleepTime = (int)Math.Round(1000 / capture.Fps);
            // 声明实例 Mat类
            Mat image = new Mat();
            imgMs.Clear();
            // 进入读取视频每镇的循环
            while (true)
            {
                capture.Read(image);
                //判断是否还有没有视频图像 
                if (image.Empty())
                    break;
                // 在picturebox中播放视频， 需要先转换成bitmap格式
                Bitmap bmp = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image);
                ImgMsS mss = new ImgMsS();
                mss.dt = DateTime.Now;
                mss.name = "img" + imgMs.Count;
                Bitmap bitmap = ImageHelper.DeepCopyBitmap(bmp);
                mss.img = ImageHelper.Bitmap2MemoryStream(bitmap);
                imgMs.Add(mss);
            }
            MessageBox.Show("读取完成");
        }
        string _password = "bl8879";

        public bool verifyPassword()
        {
            bool flag = false;

            try
            {
                InputPassword ip = new InputPassword();
                DialogResult dialogResult = ip.ShowDialog();
                if (dialogResult == DialogResult.OK)
                {
                    if (ip.passWord != _password)
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                }
                else
                {
                    flag = false;
                }

            }
            catch (Exception ex)
            {
                flag = false;
                LoggerHelper.Debug(ex);
            }

            return flag;
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox3_MouseDown(object sender, MouseEventArgs e)
        {
            markerTmp.X = e.X;
            markerTmp.Y = e.Y;
            mouseMovePoint.X = e.X;
            mouseMovePoint.Y = e.Y;
            if (formShowStatus > 0 && distanceMode == 1)
            {
                bool flag = false;
                //添加定标
                targetPoints1.Add(new TargetPoint()
                {
                    x = markerTmp.X,
                    y = markerTmp.Y,
                    name = jpName,
                    dis = PointHelper.Instance.Str2int(jpDis),//cm
                    status = jpStatus
                });
                if (targetPoints1.Count == colum * 2)
                {
                    ///定标结束
                    formShowStatus = 0;
                    pf1.Hide();
                    saveMarkSetting();
                    clearMemory();
                    flag = true;
                }
                else
                {
                    pf1.updateFlp(++nowColum);
                    jpName = nowColum + "";
                    if (nowColum % 2 != 0)
                    {
                        initDis += distance;
                    }
                    jpDis = initDis + "";
                    jpStatus = !jpStatus;
                    System.Drawing.Point ptc = this.PointToScreen(new System.Drawing.Point(e.X, e.Y - 100));
                    pf1.Location = ptc;
                }
                updateTargetListView(flag);
            }
            dispJumpLength1(e.X, e.Y);
            RefreshPictureBox();

        }
        private void pictureBox3_MouseMove(object sender, MouseEventArgs e)
        {
            mouseMovePoint.X = e.X;
            mouseMovePoint.Y = e.Y;
            if (formShowStatus > 0 && distanceMode == 1)
            {
                Task.Run(() =>
                {
                    System.Drawing.Point ptc = this.PointToScreen(new System.Drawing.Point(e.X, e.Y - 100));
                    pf1.Location = ptc;
                });
            }
            else
            {
                if (Measure)
                    dispJumpLength1(e.X, e.Y);
            }
            if (Measure)
            {
                rgbVideoSourceRePaint();
            }
        }

        #endregion
        #region 扫码枪
        Stopwatch sw = new Stopwatch();
        private long lastdt = 0;//记录上次输入时间值
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox1.ForeColor = Color.Red;
                BarCodeText.Text = "";
                lastdt = 0;
                BarCodeText.Focus();
            }
            else
            {
                checkBox1.ForeColor = Color.Black;
                label10.Focus();
            }
        }
        void BarCodeTextFoucs(bool flag)
        {
            ControlHelper.ThreadInvokerControl(this, () =>
            {
                checkBox1.Checked = flag;
            });

        }
        private void BarCodeText_TextChanged(object sender, EventArgs e)
        {

        }
        private void button19_Click_1(object sender, EventArgs e)
            {
                SearchStudentByIdNumber(BarCodeText.Text);
            }
        private void BarCodeText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Console.WriteLine($"查询{BarCodeText.Text}");
                SearchStudentByIdNumber(BarCodeText.Text);
                //label10.Focus();
                BarCodeTextFoucs(false);
            }
            else
            {
                /* if (lastdt != 0)//判断是不是第一次输入
                 {
                     if ((sw.ElapsedMilliseconds - lastdt) > 50)//判断时间间隔，如果时间间隔大于50毫秒则判定人为输入
                     {
                         BarCodeText.Text = "";
                         sw.Stop();
                         lastdt = 0;
                         e.Handled = true;
                     }
                     else
                     {
                         lastdt = sw.ElapsedMilliseconds;
                     }
                 }
                 else
                 {
                     sw.Start();
                     lastdt = sw.ElapsedMilliseconds;
                 }
                 BarCodeTextFoucs(true);*/
            }
        }
        void SearchStudentByIdNumber(string idnumber)
        {
            string sql = $"SELECT Id,Name,IdNumber,Sex,GroupName FROM DbPersonInfos WHERE ProjectId='{_ProjectId}' and IdNumber='{idnumber}'";
            var ds = sQLiteHelper.ExecuteReaderList(sql);
            if (ds.Count == 0)
            {
                MessageBox.Show($"未找到考号{idnumber}");
                return;
            }

            /* int index = comboBox2.SelectedIndex;
             if (index == -1) index = 0;
             if (index >= 0 && index < stuView.Rows.Count)
             {
                 if (stuView.CurrentCell.RowIndex != index)
                     stuView.CurrentCell = stuView.Rows[index].Cells[1];
             }*/
            foreach (var dic in ds)
            {
                int k = stuView.Rows.Add(new DataGridViewRow());
                string stuId = dic["Id"];
                string stuName = dic["Name"];
                string idNumber = dic["IdNumber"];
                string groupName = dic["GroupName"];
                int v = groupCbx.Items.IndexOf(groupName);
                if (v == -1)
                {
                    MessageBox.Show($"未找到组号:{groupName}");
                    return;
                }
                UpdateListView(_ProjectId, _GroupName, RoundCount0);
                groupCbx.SelectedIndex = v;
                int v1 = stuViewPojos.FindIndex(a => a.IdNumber == idNumber);
                stuView.CurrentCell = stuView.Rows[v1].Cells[1];
                Task.Run(() => voiceOut0($"{nowRaceStudentData.name}第{nowRaceStudentData.RoundId}轮准备考试"));
                SendScore(nowRaceStudentData.name, "准备考试", "");
                break;
            }

            //stuViewPojos
        }



        #endregion


        
    }
}


