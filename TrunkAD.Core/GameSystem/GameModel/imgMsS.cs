using System;
using System.Drawing;
using System.IO;

namespace TrunkAD.Core.GameSystem.GameModel
{
    public class ImgMsS
    {
        public int imgIndex = -1;
        //public Bitmap img;
        public MemoryStream img;
        public DateTime dt;
        public bool isDown;
        public string name;
        public bool[][] isHand;
    }
    public struct ImageAndIndex
    {
        //图片顺序索引
        public int index;
        public Bitmap image;
    }
}