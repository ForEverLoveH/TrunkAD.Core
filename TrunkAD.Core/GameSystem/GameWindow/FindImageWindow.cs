using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Spire.Pdf.Exporting.XPS.Schema;
using TrunkAD.Core.GameSystem.GameHelper;
using TrunkAD.Core.GameSystem.GameModel;

namespace TrunkAD.Core.GameSystem.GameWindow
{
    public partial class FindImageWindow : Form
    {
       public string fileName = "";
        public string nowTestDir = "";
        FlowLayoutPanel flp = new FlowLayoutPanel();
        //public List<Bitmap> BmpGroup;
       // public Bitmap[] BmpGroup;
        public List<ImgMsS> imgMs = new List<ImgMsS>();
        public FindImageWindow()
        {
            InitializeComponent();
        }

        private void FindImageWindow_Load(object sender, EventArgs e)
        {

            if(imgMs.Count<1)//if (BmpGroup.Length<1)
            {
                MessageBox.Show("录不到相片");
                return;
            }
           
            Controls.Add(flp);
            flp.Dock = DockStyle.Fill;
            flp.AutoScroll = true;
            //if (nowTestDir.Length < 1) return;
            //nowTestDir = @"E:\培林体育\视角项目\跳远\跳远\bin\Debug\img\2021年05月13日\16时14分51秒\";
            //nowTestDir = @"E:\培林体育\视角项目\跳远\跳远\bin\Debug\img\2021年05月15日\21时19分02秒\";
            //string[] files = Directory.GetFiles(nowTestDir, "*.jpg", SearchOption.TopDirectoryOnly);
            int len = 0;
            if (imgMs.Count > 0)
            { 
                flp.SuspendLayout();
                int width = flp.Width / 5-10;
                int height = flp.Height / 5-10;
                int n = imgMs.Count;
                int interval = n / 20;
                PictureBox[] pics = new PictureBox[imgMs.Count];
                int count = 0;
                for (int i = pics.Length-1; i >0; i--)
                {
                    if (i % interval != 0) continue;
                    pics[i] = new PictureBox();
                    pics[i].Image = ImageHelper.MemoryStream2Bitmap(imgMs[i].img); //Image.FromHbitmap(BmpGroup[i].GetHbitmap()); //global::SkipExec.Properties.Resources.face; //BmpGroup[i];
                    pics[i].Size = new System.Drawing.Size(width, height);
                    pics[i].SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;

                    pics[i].Name = i+"";
                    pics[i].Click += new System.EventHandler(this.pictureBox2_Click);
                    count++;
                    if (count >= 20) break;
                   
                }

                flp.Controls.AddRange(pics);
                flp.ResumeLayout();
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            fileName = (sender as PictureBox).Name;
            this.DialogResult = DialogResult.OK;
            
        }
    }
}