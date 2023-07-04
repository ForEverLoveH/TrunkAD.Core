using System;
using System.IO.Ports;
using System.Timers;
using Org.BouncyCastle.Asn1.Cms;

namespace TrunkAD.Core.GameSystem.GameHelper
{
    public delegate void RecieveDataCallBack(byte[] data);

    public delegate void SendDataCallBack(byte[] data);

    public delegate void AnalyDataDataCallBack(byte[] data);
    public class SerialPortHelper
    {
        private SerialPort iSerialPort;
        private int m_nType = -1;
        public RecieveDataCallBack ReceiveCallback;
        public SendDataCallBack SendCallback;
        public AnalyDataDataCallBack AnalyCallback;

        private System.Timers.Timer waitTimer;

        /// <summary>
        /// 缓存数据
        /// </summary>
        byte[] s232Buffer = new byte[2048];
        int s232Buffersp = 0;


        public SerialPortHelper()
        {
            iSerialPort = new SerialPort();
            iSerialPort.DataReceived += new SerialDataReceivedEventHandler(ReceivedComData);
        }
        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="strPort"></param>
        /// <param name="nBaudrate"></param>
        /// <param name="strException"></param>
        /// <returns></returns>
        public int OpenCom(string strPort, int nBaudrate, out string strException)
        {
            strException = string.Empty;
            if (iSerialPort.IsOpen)
            {
                iSerialPort.Close();
            }
            try
            {
                iSerialPort.PortName = strPort;
                iSerialPort.BaudRate = nBaudrate;
                iSerialPort.StopBits = StopBits.One;
                iSerialPort.Parity = Parity.None;
                iSerialPort.ReadTimeout = 10;
                iSerialPort.WriteTimeout = 1000;
                iSerialPort.ReadBufferSize = 4096 * 10;
                //iSerialPort.ReceivedBytesThreshold = 8;
                iSerialPort.Open();
                if (waitTimer == null)
                {
                    //建立定时器处理数据
                    waitTimer = new System.Timers.Timer(100);//实例化Timer类，设置间隔时间为10000毫秒；
                    waitTimer.Elapsed += new System.Timers.ElapsedEventHandler(AnalyReceivedData);//到达时间的时候执行事件；
                    waitTimer.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
                    waitTimer.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
                    waitTimer.Start(); //启动定时器
                }

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                strException = ex.Message;
                return -1;
            }
            m_nType = 0;
            return 0;
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        public void CloseCom()
        {
            try
            {
                if (iSerialPort.IsOpen)
                {
                    iSerialPort.Close();
                }
                if (waitTimer != null)
                {
                    waitTimer.Stop();
                }

            }
            catch (Exception ex)
            {

                LoggerHelper.Debug(ex);
            }
            m_nType = -1;
        }


        /// <summary>
        /// 串口是否开启
        /// </summary>
        /// <returns></returns>
        public bool IsComOpen()
        {
            try
            {
                return iSerialPort.IsOpen;
            }
            catch (Exception)
            {

                return false;
            }

        }

        /// <summary>
        /// 串口接收数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReceivedComData(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int nCount = iSerialPort.BytesToRead;

                if (nCount == 0)
                {
                    return;
                }
                byte[] btAryBuffer = new byte[nCount];
                iSerialPort.Read(btAryBuffer, 0, nCount);
                for (int i = 0; i < nCount; i++)
                {
                    s232Buffer[s232Buffersp] = btAryBuffer[i];
                    if (s232Buffersp < (s232Buffer.Length - 2))
                        s232Buffersp++;
                }

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 定时器处理数据
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void AnalyReceivedData(object source, System.Timers.ElapsedEventArgs e)
        {
            if (waitTimer != null)
                waitTimer.Stop();
            if (s232Buffersp != 0)
            {
                byte[] btAryBuffer = new byte[s232Buffersp];
                Array.Copy(s232Buffer, 0, btAryBuffer, 0, s232Buffersp);
                Array.Clear(s232Buffer, 0, s232Buffersp);
                s232Buffersp = 0;
                RunReceiveDataCallback(btAryBuffer);
                //string code = CCommondMethod.ByteArrayToString(btAryBuffer, 0, btAryBuffer.Length);
                //Console.WriteLine($"------------------------receiveCount:{btAryBuffer.Length}   recv:{code}");
            }
            if (waitTimer != null)
                waitTimer.Start();
        }

        /// <summary>
        /// 接收处理函数
        /// </summary>
        /// <param name="btAryBuffer"></param>
        private void RunReceiveDataCallback(byte[] btAryBuffer)
        {
            try
            {
                if (ReceiveCallback != null)
                {
                    ReceiveCallback(btAryBuffer);
                }

                int nCount = btAryBuffer.Length;

                if (AnalyCallback != null)
                {
                    AnalyCallback(btAryBuffer);
                }

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public int SendMessage(byte[] btArySenderData)
        {
            //串口连接方式
            if (m_nType == 0)
            {
                if (!iSerialPort.IsOpen)
                {
                    return -1;
                }

                iSerialPort.Write(btArySenderData, 0, btArySenderData.Length);

                if (SendCallback != null)
                {
                    SendCallback(btArySenderData);
                }

                return 0;
            }
            return -1;
        }


    }
}