using System;
using System.Drawing;
using System.Threading.Tasks;
using NPOI.SS.Formula.Functions;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace TrunkAD.Core.GameSystem.GameHelper 
{
   public class FuseImage
   {
      // PointBitmap unback = null;
      BitmapDataBitmap unback = null;
      BitmapDataBitmap dstPb = null;
      private Bitmap dstBitmap = null;
      private Bitmap backBitmap = null;
      private int awidth = 0;
      private int aheight = 0;

      public Bitmap GetDstBitmap()
      {
         return dstBitmap;
      }

      public void SetDstBitmap(Bitmap value)
      {
         dstBitmap = value;
      }

      public Bitmap GetBackBitmap()
      {
         return backBitmap;
      }

      public void SetBackBitmap(Bitmap value)
      {
         backBitmap = value;
      }

      public FuseImage(Bitmap back)
      {
         awidth = back.Width;
         aheight = back.Height;
         SetDstBitmap(DeepCopyBitmap(back));
         SetBackBitmap(DeepCopyBitmap(back));
         dstPb = new BitmapDataBitmap(GetDstBitmap());
         unback = new BitmapDataBitmap(GetBackBitmap());
         unback.LockBits();
         dstPb.LockBits();
      }

      public void Dispose()
      {
         unback.UnlockBits();
         dstPb.UnlockBits();
      }

      public void FuseColorImg1(Bitmap b)
      {
         BitmapDataBitmap unb2 = new BitmapDataBitmap(b);
         unb2.LockBits();
         Parallel.For(0, awidth, new ParallelOptions { MaxDegreeOfParallelism = 4 }, (i) =>
            //Parallel.For(0, awidth,(j) =>
         {
            for (int j = 0; j < aheight; j++)
            {
               //定位像素点位置
               int p = j * awidth * 3 + i * 3;
               int[] list = new int[3];
               list[0] = (unback.srcArray[p] - unb2.srcArray[p]);
               list[1] = (unback.srcArray[p + 1] - unb2.srcArray[p + 1]);
               list[2] = (unback.srcArray[p + 2] - unb2.srcArray[p + 2]);
               Array.Sort(list);
               if (list[list.Length - 1] - list[0] > 20)
               {
                  dstPb.srcArray[p] = unb2.srcArray[p];
                  dstPb.srcArray[p + 1] = unb2.srcArray[p + 1];
                  dstPb.srcArray[p + 2] = unb2.srcArray[p + 2];
               }
            }
         });


         unb2.UnlockBits();
      }

      /// <summary>
      /// 深度复制bitmap
      /// </summary>
      /// <param name="bitmap"></param>
      /// <returns></returns>
      public static Bitmap DeepCopyBitmap(Bitmap bitmap)
      {
         try
         {
            if (bitmap == null) return null;
            Bitmap dstBitmap = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat);
            return dstBitmap;
         }
         catch (Exception ex)
         {
            Console.WriteLine("Error : {0}", ex.Message);
            return null;
         }
      }

      private static Bitmap ResizeImage(Image image, int Width, int Height)
      {
         int fillWidth = 0, fillHeight = 0;
         int fillWidth2 = 0, fillHeight2 = 0;

         if (image.Width < Width && image.Height < Height)
         {
            fillWidth = (Width - image.Width) / 2;
            fillWidth2 = Width - image.Width - fillWidth;

            fillHeight = (Height - image.Height) / 2;
            fillHeight2 = Height - image.Height - fillHeight;
            using (Mat source = BitmapConverter.ToMat((Bitmap)image))
            using (Mat dest = new Mat(new OpenCvSharp.Size(Width, Height), source.Type()))
            {
               Cv2.CopyMakeBorder(source, dest, fillHeight, fillHeight2, fillWidth, fillWidth2, BorderTypes.Constant,
                  null);
               return dest.ToBitmap(image.PixelFormat);
            }
         }

         bool horizontalFill = Height > Width;
         int resizedWidth, resizedHeight;
         // ┌┬┬┐
         // ││││
         // └┴┴┘
         if (horizontalFill)
         {
            resizedWidth = image.Width * Height / image.Height;
            resizedHeight = Height;
            fillWidth = (Width - resizedWidth) / 2;
            fillWidth2 = Width - resizedWidth - fillWidth;
         }
         // ┌─┐
         // ├─┤
         // ├─┤
         // └─┘
         else
         {
            resizedWidth = Width;
            resizedHeight = image.Height * Width / image.Width;
            fillHeight = (Height - resizedHeight) / 2;
            fillHeight2 = Height - resizedHeight - fillHeight;
         }

         using (Mat resizedMat = new Mat())
         using (Mat source = BitmapConverter.ToMat((Bitmap)image))
         using (Mat dest = new Mat(new OpenCvSharp.Size(Width, Height), source.Type()))
         {
            Cv2.Resize(source, resizedMat, new OpenCvSharp.Size(resizedWidth, resizedHeight));
            Cv2.CopyMakeBorder(resizedMat, dest, fillHeight, fillHeight2, fillWidth, fillWidth2, BorderTypes.Constant,
               null);
            return dest.ToBitmap(image.PixelFormat);
         }
      }
   }
   
}