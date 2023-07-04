using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

namespace TrunkAD.Core.GameSystem.GameHelper
{
    public class SerialTool
    {
        public static string[] GetPortDeviceName(string portName)
        {
            List<string> sts = new List<string>();
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
                            sts.Add(deviceName);
                        }
                    }
                }

            }

            return sts.ToArray();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        public static string PortDeviceName2PortName(string deviceName)
        {
            try
            {
                int a = deviceName.IndexOf("(COM") + 1; //a会等于1
                string str = deviceName.Substring(a, deviceName.Length - a);
                a = str.IndexOf(")"); //a会等于1
                str = str.Substring(0, a);

                return str;
            }
            catch (Exception exception)
            {
                LoggerHelper.Debug(exception);
                return null;
            }

        }
        
        public static bx_5k_area_header heard;
        public static bx_5k_sound sound;
        /// <summary>
        /// 
        /// </summary>
        public static void init()
        {
            heard.AreaType = 0;
            heard.AreaX = 0 / 8;
            heard.AreaY = 0;
            heard.AreaWidth = 64 / 8;
            heard.AreaHeight = 32;
            heard.DynamicAreaLoc = 0;
            heard.Lines_sizes = 0;
            heard.RunMode = 0;
            heard.Timeout = 2;
            heard.Reserved1 = 0;
            heard.Reserved2 = 0;
            heard.Reserved3 = 0;

            heard.SingleLine = 2;
            //是否自动换行 1不自动 2w 自动
            heard.NewLine = 1;
            //调整显示模式 1静止显示，2快速打出，3向左移动，4向右移动，5向上移动，6向下移动
            heard.DisplayMode = 1;
            heard.ExitMode = 0;
            //0至24中间值，值越大速度越慢
            heard.Speed = 18;
            //0为不停留，1代表0.5s
            heard.StayTime = 2;
            sound.StoreFlag = 0;
            sound.SoundPerson = 0;
            sound.SoundVolum = 5;
            sound.SoundSpeed = 5;
        }
        /// <summary>
        /// 往大屏幕发送数据
        /// </summary>
        /// <param name="type"></param>
        /// <param name="text"></param>
        /// <param name="area_header"></param>
        /// <returns></returns>
         public static byte[] SendInfo(int type, string text, bx_5k_area_header area_header)
        {
            //帧头
            byte[] frameHead = new byte[] { 0xA5, 0xA5, 0xA5, 0xA5, 0xA5, 0xA5, 0xA5, 0xA5 };
            //包头
            byte[] bagHead = new byte[0];// { 0x01, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFE, 0x02, 0x32, 0x00 };
                                         //包头数据  共14位
                                         //屏地址: 在 PHY层，广播地址定义如下： 0xFFFF 为广播地址 1，此种模式下，控制器不返回 数据，其可用于广播校时等命令。 0xFFFE 为广播地址 2，此种模式主要用于广播设置 屏参，控制器需返回数据。在返回的数据帧中，地 址也应为 0XFFFE。 0x8000~0xDFFF地址为保留地址，对于物理层类型 为 TCP/IP 或 GPRS这种不需要处理 DstAddr 的，可 将其目标地址设置为这个范围中的一个，默认设置 地址为 0x8000。 
            byte[] DstAddr = { 0x01, 0x00 };
            //源地址 几个特殊地址定义如下： PC 客户端软件从 0x8000 开始，范围为 0x8000~0xDFFF，用来代表不同客户端软件； 0xE000~0xFFFE 为保留地
            byte[] SrcAddr = { 0x00, 0x80 };
            //保留 
            byte[] Reserved = { 0x00, 0x00, 0x00 };
            //选项   当该字节的 BIT0 为 1 时，需要发送接下来的 16字 节 BarCode，这么做是为了便于在线设置控制器 IP. 反之，当该字节的 BIT0 为 0 时，不需要发送接下来 的 16字节 BarCode. 注意： 1. 只有设置 IP 命令需要将该字节的 BIT0 设置为 1 2. 上位机需要通过网络搜索命令来获取当前局域网 内所有控制卡的 BarCode 
            byte[] Option = { 0x00 };
            /*
            //校验模式   校验值共两个字节 
             * 当该字节为 0 时，采用 CRC16 方式 
             * 当该字节为 1 时，采用和校验的方式，仅保留最低 位两个字节，采用小端模式 
             * 当该字节为 2 时，无校验，校验字节可以为任意值             
             */
            byte[] CheckMode = new byte[1] { 0x00 };
            //显示模式 0x00：普通模式，动态区与节目可同时显示，但各 区域不可重叠。 0x01：动态模式，优先显示动态区，无动态区则显 示节目，动态区与节目区可重叠。 注：特殊动态区不支持动态模式。 
            byte[] DisplayMode = { 0x00 };
            //设备类型   用于区分网络中不同的设备类型，
            /*
            定义如下： 
             * 0x51——BX-5K1
             * 0x58——BX-5K2 
             * 0x53——BX-5MK2 
             * 0x54——BX-5MK1 
             * 0x61——BX-6K1      与 5K1 (0x51)兼容 
             * 0x62——BX-6K2      与 5MK1 (0x54)  兼容
             * 0x63——BX-6K3      与 5K2,5MK2 (0x53/0x58)兼容
             * 0x64——BX-6K1-YY      
             * 0x65——BX-6K2-YY       
             * 0x66——BX-6K3-YY 
             * 0xFE    通配
             */
            byte[] LedType = new byte[] { 0x51, 0x58, 0x53, 0x54, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0xFE };
            byte[] DeviceType = new byte[1] { LedType[type] };
            //协议版本号  协议版本号，用于区分控制卡使用的协议 
            byte[] ProtocolVersion = { 0x02 };

            //数据域长度   数据域的长度（不包括帧头、帧尾、帧校验和包 头） 
            byte[] DataLen = new byte[2];
            //发送实时显示区域数据 
            byte[] AreaDate = new byte[] { 0xA3, 0x06, 0x01, 0x00, 0x00, 0x00, 0x01, 0x29, 0x00 };
            byte[] AreaDate1 = new byte[Marshal.SizeOf(area_header)];
            Array.Copy(StructToBytes(area_header), 0, AreaDate1, 0, Marshal.SizeOf(area_header));
            byte[] param = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x18, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x02, 0x02, 0x03, 0x00, 0x04, 0x05, 0x0E, 0x00, 0x00, 0x00 };
            //节目内容
            byte[] data = System.Text.Encoding.Default.GetBytes(text);
            //AreaDataLen0           2  区域 0 数据长度 
            byte[] AreaDataLen0 = BitConverter.GetBytes(AreaDate1.Length + data.Length);
            AreaDate[AreaDate.Length - 2] = AreaDataLen0[0];
            AreaDate[AreaDate.Length - 1] = AreaDataLen0[1];
            //计算数据域长度
            byte[] buf8 = BitConverter.GetBytes(AreaDate.Length + AreaDate1.Length + data.Length);
            DataLen[0] = buf8[0];
            DataLen[1] = buf8[1];

            //计算显示内容长度
            byte[] buf9 = BitConverter.GetBytes(data.Length);
            AreaDate1[AreaDate1.Length - 4] = buf9[0];
            AreaDate1[AreaDate1.Length - 3] = buf9[1];
            //合并数据区域参数
            param = AreaDate.Concat(AreaDate1).ToArray();
            //合并包头各项参数
            bagHead = bagHead.Concat(DstAddr).ToArray();
            bagHead = bagHead.Concat(SrcAddr).ToArray();
            bagHead = bagHead.Concat(Reserved).ToArray();
            bagHead = bagHead.Concat(Option).ToArray();
            bagHead = bagHead.Concat(CheckMode).ToArray();
            bagHead = bagHead.Concat(DisplayMode).ToArray();
            bagHead = bagHead.Concat(DeviceType).ToArray();
            bagHead = bagHead.Concat(ProtocolVersion).ToArray();
            bagHead = bagHead.Concat(DataLen).ToArray();
            //计算校验值           
            byte[] crc;
            byte[] temp = bagHead.Concat(param).ToArray();
            temp = temp.Concat(data).ToArray();
            byte[] crc1 = GetCRC16(temp);
            crc = new byte[] { crc1[0], crc1[1] };
            temp = temp.Concat(crc).ToArray();

            /*转义
             * 封帧中遇到 0xA5，则将之转义为 0xA6，0x02；如遇到 0xA6，则将之转义为 0xA6，0x01 。 
             * 封帧中遇到 0x5A，则将之转义为 0x5B，0x02；如遇到 0x5B，则将之转义为 0x5B，0x01 。 
             * 解帧过程如果遇到连续两个字节为 0xA6, 0x02 ,则反转义为 0xA5 。 
             * 解帧过程如果遇到连续两个字节为 0xA6, 0x01 ,则反转义为 0xA6 。
             * 解帧过程如果遇到连续两个字节为 0x5B, 0x02 ,则反转义为 0x5A。
             * 解帧过程如果遇到连续两个字节为 0x5B, 0x01 ,则反转义为 0x5B
             封帧过程中，所涉及校验的数据皆是转义之前的数据，所涉及的数据长度皆是转义之前的数据长度
             */
            List<byte> list = new List<byte>();
            for (int i = 0; i < temp.Length; i++)
            {
                if (temp[i] == 0xA6)
                {
                    list.Add(0xA6);
                    list.Add(0x01);
                }
                else if (temp[i] == 0xA5)
                {
                    list.Add(0xA6);
                    list.Add(0x02);
                }
                else if (temp[i] == 0x5B)
                {
                    list.Add(0x5B);
                    list.Add(0x01);
                }
                else if (temp[i] == 0x5A)
                {
                    list.Add(0x5B);
                    list.Add(0x02);
                }
                else
                {
                    list.Add(temp[i]);
                }
            }

            temp = new byte[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                temp[i] = list[i];
            }
            //帧尾
            byte[] frameEnd = new byte[] { 0x5A };

            //合并所有指令内容
            byte[] returnData = new byte[0];
            returnData = returnData.Concat(frameHead).ToArray();//帧头
            returnData = returnData.Concat(temp).ToArray();
            returnData = returnData.Concat(frameEnd).ToArray();//帧头
            return returnData;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="txt1"></param>
        /// <param name="c1"></param>
        /// <param name="txt2"></param>
        /// <param name="c2"></param>
        /// <param name="txt3"></param>
        /// <param name="c3"></param>
        /// <returns></returns>
        public static byte[] PushText_BL2(string txt1, int c1, string txt2, int c2, string txt3, int c3)
        {
            string[] colors = new string[]
            {
                "C1","C2","C3"
            };
            txt1 = getCenter(txt1, 8);
            txt2 = getCenter(txt2, 6);
            string txt = $"\\{colors[c1]}{txt1}\\n\\{colors[c2]}{txt2}\\{colors[c3]}{txt3}";
            byte[] data = System.Text.Encoding.Default.GetBytes(txt);
            heard.DataLen = data.Length;

            byte[] buf = SendInfo(10, txt, heard);


            return buf;
        }
        public static string getCenter(string data, int len)
        {
            byte[] namebyteArray1 = System.Text.Encoding.Default.GetBytes(data);
            int yuNum = len - namebyteArray1.Length;
            int rightNum = yuNum / 2;
            int leftNum = yuNum - yuNum / 2;
            //补全空格居中
            for (int i = 0; i < leftNum; i++)
            {
                data = " " + data;
            }
            for (int i = 0; i < rightNum; i++)
            {
                data = data + " ";
            }
            return data;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static byte[] GetCRC16(byte[] data)
        {
            int CheckValue;
            string str = String.Empty;
            CheckValue = crc16(data);
            byte[] toByte = BitConverter.GetBytes(CheckValue);
            return toByte;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static int crc16(byte[] data)
        {
            int size = data.Length;
            int crc = 0x0;
            byte data_t;
            int i = 0;
            int j = 0;
            if (data == null)
            {
                return 0;
            }
            for (j = 0; j < size; j++)
            {
                data_t = data[j];
                crc = (data_t ^ (crc));
                for (i = 0; i < 8; i++)
                {
                    if ((crc & 0x1) == 1)
                    {
                        crc = (crc >> 1) ^ 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            return crc;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="structObj"></param>
        /// <returns></returns>
        public static byte[] StructToBytes(object structObj)
        {
            //得到结构体的大小
            int size = Marshal.SizeOf(structObj);
            //创建byte数组
            byte[] bytes = new byte[size];
            //分配结构体大小的内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将结构体拷到分配好的内存空间
            Marshal.StructureToPtr(structObj, structPtr, false);
            //从内存空间拷到byte数组
            Marshal.Copy(structPtr, bytes, 0, size);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            //返回byte数组
            return bytes;
        }
    }
}