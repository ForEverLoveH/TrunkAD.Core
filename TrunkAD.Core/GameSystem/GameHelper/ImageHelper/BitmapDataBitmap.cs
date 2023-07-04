using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace TrunkAD.Core.GameSystem.GameHelper
{
    public class BitmapDataBitmap
    {
        Bitmap source = null;
        IntPtr ptr = IntPtr.Zero;
        BitmapData bitmapData = null;

        public int Depth { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int size { get; private set; }
        public byte[] srcArray { get; private set; }
        public BitmapDataBitmap(Bitmap bmp)
        {
            Width = bmp.Width;
            Height = bmp.Height;
            source = bmp;
            size = Width * Height * 3;
            //缓冲区数组
            srcArray = new byte[size];
        }

        public void LockBits()
        {
            try
            {
                bitmapData = source.LockBits(
                    new Rectangle(0, 0, Width, Height),
                    ImageLockMode.ReadWrite,
                    PixelFormat.Format24bppRgb);
                unsafe
                {
                    ptr = bitmapData.Scan0;
                    //把像素值复制到缓冲区
                    Marshal.Copy(ptr, srcArray, 0, size);
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void UnlockBits()
        {
            try
            {
                //从缓冲区复制回BitmapData
                Marshal.Copy(srcArray, 0, ptr, size);
                //从内存中解锁
                source.UnlockBits(bitmapData);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}