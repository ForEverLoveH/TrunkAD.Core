using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrunkAD.Core.GameSystem.GameHelper;

namespace TrunkAD.Core.GameSystem.GameWindow
{
    public partial class LoginWindowForm : Form
    {
        public LoginWindowForm()
        {
            InitializeComponent();
        }
        private NFC_Helper USBWatcher = new NFC_Helper();
        private void LoginWindowForm_Load(object sender, EventArgs e)
        {
            USBWatcher.AddUSBEventWatcher(USBEventHandler, USBEventHandler, new TimeSpan(0, 0, 1));
            serialInit();
        }
        List<string> portNamesList = new List<string>();
        // ScreenSerialReader reader;
        public static string strMainModule = System.AppDomain.CurrentDomain.BaseDirectory + "data\\";

        /// <summary>
        /// 刷新串口
        /// </summary>
        /// <returns></returns>
        public bool RefreshComPorts()
        {
            bool flag = false;
            try
            {
                loadSuccess = false;
                string portFind = "USB 串行设备";
                string[] portNames = getPortDeviceName(portFind);
                string pid = string.Empty;
                string vid = string.Empty;

                string[] strg = TextFileHelper.Instance.Read(Path.Combine(strMainModule, "pidtxt.dat"));
                if (strg.Length > 0)
                {
                    pid = strg[0];
                }
                strg = TextFileHelper.Instance.Read(Path.Combine(strMainModule, "vidtxt.dat"));
                if (strg.Length > 0)
                {
                    vid = strg[0];
                }
                List<string> strPorts = GetScreenCom(pid, vid);

                portNamesList.Clear();
                foreach (var item in portNames)
                {
                    //显示屏跳过
                    if (strPorts.Contains(item)) continue;
                    portNamesList.Add(item);
                }
                if (portNames.Length > 0)
                {
                    flag = true;
                }
            }
            catch (Exception)
            {
                flag = false;
            }

            return flag;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pid0"></param>
        /// <param name="vid0"></param>
        /// <returns></returns>
        private List<string> GetScreenCom(string pid0 = "1000", string vid0 = "1122")
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
        /// 串口连接
        /// </summary>
        void ConnectPort()
        {


        }
        List<SerialPortHelper> screenSerialReaders = new List<SerialPortHelper>();
        /// <summary>
        /// 初始化串口
        /// </summary>
        /// <returns></returns>
        bool serialInit()
        {
            bool flag = false;

            try
            {
                foreach (var item in screenSerialReaders)
                {
                    if (item != null && item.IsComOpen())
                    {
                        item.CloseCom();
                    }
                }
                screenSerialReaders.Clear();

                RefreshComPorts();
                int nlen = portNamesList.Count;

                for (int i = 0; i < nlen; i++)
                {

                    try
                    {
                        SerialPortHelper reader = new SerialPortHelper();
                        reader.AnalyCallback = AnalyData;

                        string strPort = PortDeviceName2PortName(portNamesList[i]);
                        if (string.IsNullOrWhiteSpace(strPort))
                        {
                            continue;
                        }
                        int nBaudrate = 9600;
                        string strException = string.Empty;
                        int nRet = reader.OpenCom(strPort, nBaudrate, out strException);
                        if (nRet == 0)
                        {
                            screenSerialReaders.Add(reader);
                        }

                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Debug(ex);
                    }

                }
                unSuccesssum = 0;
                if (nlen > 0)
                {
                    beginVer = true;
                    SendVerData();
                }

            }
            catch (Exception)
            {

                flag = false;
            }
            return flag;
        }
        /// <summary>
        /// 获取串口信息
        /// </summary>
        /// <returns></returns>
        public static string[] getPortDeviceName(string comName)
        {
            List<string> strs = new List<string>();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_PnPEntity where Name like '%(COM%'"))
            {
                var hardInfos = searcher.Get();
                foreach (var hardInfo in hardInfos)
                {
                    if (hardInfo.Properties["Name"].Value != null)
                    {
                        string deviceName = hardInfo.Properties["Name"].Value.ToString();
                        if (deviceName.Contains(comName))
                        {
                            strs.Add(deviceName);
                        }
                    }
                }
            }
            return strs.ToArray();
        }

        public static string PortDeviceName2PortName(string deviceName)
        {
            try
            {
                int a = deviceName.IndexOf("(COM") + 1;//a会等于1
                string str = deviceName.Substring(a, deviceName.Length - a);
                a = str.IndexOf(")");//a会等于1
                str = str.Substring(0, a);

                return str;
            }
            catch (Exception)
            {

                return "";
            }

        }

        bool beginVer = false;
        bool loadSuccess = false;
        bool loadSuccess1 = false;
        private void AnalyData(byte[] btData)
        {
            if (beginVer)
            {
                string code = CCommondMethod.ByteArrayToString(btData, 0, btData.Length);
                string strLog = "AnalyData:" + hexTextSpace(code);
                Console.WriteLine(strLog);
                bool flag = checkByte1(btData);
                if (!flag)
                {
                    label1.Text = "验证失败";
                    beginVer = true;
                }
                else
                {
                    loadSuccess = true;
                }
            }
        }

        /// <summary>
        /// 数字字符串每两位加空格
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private string hexTextSpace(string code)
        {
            StringBuilder sb = new StringBuilder();//sb为可变字符串

            char s = ' ';//需要加入的字符
            string str = code.Replace(s.ToString(), "");//用空格替换掉"-"
            for (int i = 1; i <= str.Length; i++)
            {
                sb.Append(str[i - 1]);//末尾加入前一个字符
                if ((i != 0 && i % 2 == 0))//i不为零且为偶数时
                {
                    if (i == str.Length) continue;//i为结尾时退出循环，到比较条件
                    sb.Append(s);//添加"-"
                }
            }
            return sb.ToString();
        }
        void SendVerData()
        {
            foreach (var reader in screenSerialReaders)
            {
                if (reader != null && reader.IsComOpen())
                {
                    label1.Text = "验证摄像头";
                    byte[] bytes = test03();
                    reader.SendMessage(bytes);
                    beginVer = true;
                }
            }

        }


        private byte[] test03()
        {
            // opencamera1234567890
            string opencamera = $"opencamera{DateTime.Now.ToString("ddHHmmssff")}";
            byte[] opencameraByte = Encoding.UTF8.GetBytes(opencamera);
            int nlen = opencameraByte.Length;
            Random random = new Random();
            int sp = 0;
            byte[] temp = new byte[100];
            byte[] vlaueTemp = new byte[4];
            vlaueTemp[sp++] = 0x27;
            vlaueTemp[sp++] = (byte)random.Next(255);
            vlaueTemp[sp++] = (byte)random.Next(255);
            vlaueTemp[sp++] = (byte)(vlaueTemp[1] | vlaueTemp[2]);
            byte[] vlaueTemp0 = new byte[4 + nlen];
            Array.Copy(vlaueTemp, vlaueTemp0, 0);
            Array.Copy(opencameraByte, 0, vlaueTemp0, 4, nlen);
            Array.Copy(vlaueTemp, 0, temp, 0, 4);
            sp = 4;
            for (int i = 0; i < 10; i++)
            {
                temp[sp++] = (byte)(vlaueTemp0[i + 4] ^ vlaueTemp0[i + 4 + 10]);
            }
            for (int i = 0; i < 10; i++)
            {
                temp[sp++] = vlaueTemp0[i + 14];
            }
            byte cheack = 0xaa;
            for (int i = 0; i < opencameraByte.Length; i++)
            {
                cheack ^= opencameraByte[i];
            }
            temp[sp++] = cheack;
            byte[] dst = new byte[sp];
            Array.Copy(temp, dst, sp);
            return dst;
        }
        bool checkByte1(byte[] btData)
        {
            beginVer = false;
            int sp = 0;
            bool flag = true;
            bool flag2 = false;
            while (flag)
            {
                int[] result = findHeadIndex(btData, sp);
                if (result[0] == -1)
                {
                    flag = false;
                    continue;
                }
                byte cheack = 0xaa;
                for (int i = result[0]; i < result[1]; i++)
                {
                    cheack = (byte)(cheack ^ btData[i]);
                }
                if (cheack == btData[result[1]])
                {
                    flag = false;
                    flag2 = true;
                    break;
                }
            }

            return flag2;
        }

        int[] findHeadIndex(byte[] btData, int begin)
        {
            int[] result = new int[2];
            int sp = -1;
            int nlen = btData.Length;
            int templegth = 0;
            for (int i = begin; i < nlen; i++)
            {
                if (btData[i] == 0x27)
                {
                    sp = i; break;
                }
            }
            if (sp + 4 > nlen - 1)
            {
                sp = -1;
            }
            else
            {
                byte PCcheack = btData[sp + 4];
                if (PCcheack % 5 == 0)
                {
                    templegth = 15;
                }
                else if (PCcheack % 3 == 0)
                {
                    templegth = 20;
                }
                else if (PCcheack % 2 == 0)
                {
                    templegth = 22;
                }
                else
                {
                    templegth = 24;
                }
                if (sp + templegth > nlen - 1)
                {
                    sp = -1;
                }
            }
            result[0] = sp;
            result[1] = sp + templegth;

            return result;
        }


        private void label1_DoubleClick(object sender, EventArgs e)
        {
            verComData.Clear();
            if (RefreshComPorts())
            {
                ConnectPort();
                SendVerData();
            }
            else
            {
                label1.Text = "请连接摄像头";
            }
        }


        int unSuccesssum = 0;
        List<string> verComData = new List<string>();
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (loadSuccess)
            {
                //成功
                unSuccesssum = 0;
                loadSuccess = false;
                loadSuccess1 = true;
                timer1.Stop();
                foreach (var item in screenSerialReaders)
                {
                    if (item != null && item.IsComOpen())
                    {
                        item.CloseCom();
                    }
                }
                this.Hide();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }

            if (beginVer)
            {
                unSuccesssum++;
                if (unSuccesssum > 3)
                {
                    //超过三次未收到串口数据切换下一个串口
                    unSuccesssum = 0;
                    /* beginVer = false;
                     if (beginVer) return;
                     if (loadSuccess1) return;*/
                }
                else
                {
                    SendVerData();
                }

            }
            else
            {
                label1.Text = "请连接摄像头";
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
                //设备插入
                serialInit();
                //RefreshComPorts();
            }
            else if (e.NewEvent.ClassPath.ClassName == "__InstanceDeletionEvent")
            {
                //检测断开,断开提示
                //MessageBox.Show("设备断开请检查");

            }
            watcher.Start();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoginWindowForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Stop();
            foreach (var item in screenSerialReaders)
            {
                if (item != null && item.IsComOpen())
                {
                    item.CloseCom();
                }
            }
        }
    }

        
}
