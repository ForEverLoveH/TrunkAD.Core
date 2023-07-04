using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AForge.Video.DirectShow;
using TrunkAD.Core.GameSystem.GameHelper;

namespace TrunkAD.Core.GameSystem.GameWindow
{
    public partial class CameraSettingWindow : Form
    {
        public CameraSettingWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 视频输入设备信息
        /// </summary>
        public FilterInfoCollection filterInfoCollection;

        public string cameraName = string.Empty;

        public int maxFps = 0;

        public int Fps = 0;

        public int _width = 1280;
        public int _height = 720;
        public int cameraIndex = 0;
        
        public List<string> ExistMonikerStrings = new List<string>();
        private void CameraSettingWindow_Load(object sender, System.EventArgs e)
        {
            this.Text = "摄像头参数设置";
            this.comboBox2.SelectedIndex = 0;
             
            ReloadCameraList();
        }
        private void ReloadCameraList()
        {
            comboBox_camera.Items.Clear();
            //设置视频来源
            try
            {
                // 枚举所有视频输入设备
                filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (filterInfoCollection.Count == 0)
                    throw new ApplicationException();   //没有找到摄像头设备
                foreach (FilterInfo device in filterInfoCollection)
                {
                    /* if (device.Name.Contains("Web"))
                     {
                        continue;
                     }*/

                    comboBox_camera.Items.Add(device.Name);
                }
                if (comboBox_camera.Items.Count > 0)
                {
                    comboBox_camera.SelectedIndex = comboBox_camera.Items.Count - 1;
                }
                else
                {
                    DialogResult = DialogResult.Cancel;
                }
            }
            catch (ApplicationException ex)
            {
                filterInfoCollection = null;
                comboBox_camera.Items.Clear();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_camera_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(comboBox_camera.Text))
            {
                cameraName = comboBox_camera.Text;
                cameraIndex = comboBox_camera.SelectedIndex;                
                ChooseCamera(cameraName);
            }
        }
        public List<string> FpsList = new List<string>();
        bool _isCameraUse = false;
        private void ChooseCamera(string name)
        {
             FpsList.Clear();
             _isCameraUse = false;
            foreach (FilterInfo device in filterInfoCollection)
            {
                if (device.Name == name)
                {
                    if (ExistMonikerStrings.Contains(device.MonikerString)) _isCameraUse = true;
                    VideoCaptureDevice rgbDeviceVideo = new VideoCaptureDevice(device.MonikerString);
                    for (int i = 0; i < rgbDeviceVideo.VideoCapabilities.Length; i++)
                    {
                        if (rgbDeviceVideo.VideoCapabilities[i].FrameSize.Width == _width
                            && rgbDeviceVideo.VideoCapabilities[i].FrameSize.Height == _height)
                        {
                            //rgbDeviceVideo.VideoResolution = rgbDeviceVideo.VideoCapabilities[i];
                            string fps = rgbDeviceVideo.VideoCapabilities[i].AverageFrameRate + "";
                            if (!FpsList.Contains(fps))
                                FpsList.Add(fps);
                            break;
                        }
                    }
                    break;
                }
            }
            if (FpsList.Count == 0)
            {
                foreach (FilterInfo device in filterInfoCollection)
                {
                    if (device.Name == name)
                    {
                        VideoCaptureDevice rgbDeviceVideo = new VideoCaptureDevice(device.MonikerString);
                        for (int i = 0; i < rgbDeviceVideo.VideoCapabilities.Length; i++)
                        {
                            if (rgbDeviceVideo.VideoCapabilities[i].FrameSize.Width == 1920
                                && rgbDeviceVideo.VideoCapabilities[i].FrameSize.Height == 1080)
                            {
                                //rgbDeviceVideo.VideoResolution = rgbDeviceVideo.VideoCapabilities[i];
                                string fps = rgbDeviceVideo.VideoCapabilities[i].AverageFrameRate + "";
                                if (!FpsList.Contains(fps))
                                    FpsList.Add(fps);
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            comboBox1.Items.Clear();
            foreach (var item in FpsList)
            {
                int.TryParse(item, out int fps);
                maxFps = fps;
                fps /= 2;
                while (fps >= 30)
                {
                    if (fps >= 30)
                        comboBox1.Items.Add(fps + "fps");
                    fps /= 2;
                }
                comboBox1.Items.Add(maxFps + "fps");
                break;
            }
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(comboBox1.Text))
            {
                string cb1 = comboBox1.Text;
                string cb2 = cb1.Substring(0, cb1.IndexOf("fps"));
                int.TryParse(cb2, out Fps);
            }
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cameraName))
                DialogResult = DialogResult.OK;
            else
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void uiButton2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}