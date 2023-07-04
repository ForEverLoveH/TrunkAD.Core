using System;
using System.Drawing;
using System.Threading.Tasks;

namespace TrunkAD.Core.GameSystem.GameHelper
{
    public class FuseBitmap
    {
        BitmapDataBitmap dstPb = null;
      private Bitmap dstBitmap = null;
      private bool[][] isHand;
      private int awidth = 0;
      private int aheight = 0;
      private int minWidth = 0;
      private int maxWidth = 0;
      private int minHeigh = 0;
      private int maxHeigh = 0;
      private static BitmapDataBitmap BackGroundPb = null;
      private static Bitmap backGroundBmp = null;
      private static bool BackGroundBmplock = false;
      private static int tolerance = 30;
      /// <summary>
      /// 是否在多边形内
      /// </summary>
      public bool[][] isInTrangle;

      public static Bitmap GetBackGroundBmp()
      {
         return backGroundBmp;
      }

      public Bitmap GetDstBitmap()
      {
         return dstBitmap;
      }

      public void SetDstBitmap(Bitmap value)
      {
         dstBitmap = value;
      }

      public bool[][] GetIsHand()
      {
         return isHand;
      }

      public void SetIsHand(bool[][] value)
      {
         isHand = value;
      }

      public int GetMinWidth()
      {
         return minWidth;
      }

      public void SetMinWidth(int value)
      {
         minWidth = value;
      }

      public int GetMaxWidth()
      {
         return maxWidth;
      }

      public void SetMaxWidth(int value)
      {
         maxWidth = value;
      }

      public int GetMinHeigh()
      {
         return minHeigh;
      }

      public void SetMinHeigh(int value)
      {
         minHeigh = value;
      }

      public int GetMaxHeigh()
      {
         return maxHeigh;
      }

      public void SetMaxHeigh(int value)
      {
         maxHeigh = value;
      }

      public int GetAheight()
      {
         return aheight;
      }

      public void SetAheight(int value)
      {
         aheight = value;
      }

      public int GetAwidth()
      {
         return awidth;
      }

      public void SetAwidth(int value)
      {
         awidth = value;
      }

      public static void setBackGround(Bitmap backgroup, int tol)
      {
         if (BackGroundBmplock)
         {
            BackGroundPb.UnlockBits();
            BackGroundPb = null;
         }
         tolerance = tol;
         
         backGroundBmp = DeepCopyBitmap(backgroup);
         BackGroundPb = new BitmapDataBitmap(backGroundBmp);
         BackGroundPb.LockBits();
         BackGroundBmplock = true;

      }


      public FuseBitmap(Bitmap back)
      {
         SetAwidth(back.Width);
         SetAheight(back.Height);
         isHand = new bool[GetAwidth()][];
         for (int i = 0; i < GetAwidth(); i++)
         {
            isHand[i] = new bool[GetAheight()];
         }
         dstBitmap = DeepCopyBitmap(back);
         dstPb = new BitmapDataBitmap(dstBitmap);
         dstPb.LockBits();
      }
      public void SetRect(OpenCvSharp.Point[] conPoints0, bool[][] isInTrangle)
      {
         this.isInTrangle = isInTrangle;
         SetMinWidth(conPoints0[0].X);
         SetMaxWidth(conPoints0[3].X);
         SetMinHeigh(conPoints0[0].Y);
         SetMaxHeigh(conPoints0[3].Y);
      }
      public void Dispose()
      {
         dstPb.UnlockBits();
      }

      public void FuseColorImg(int tol, bool flag = false)
      {
         tolerance = tol;
         Parallel.For(GetMinWidth(), GetMaxWidth(), new ParallelOptions { MaxDegreeOfParallelism = 3 }, (i) =>
         //Parallel.For(0, awidth,(j) =>
         {
            //R>95 && G>40 && B>20 && R>G && R>B && Max(R,G,B)-Min(R,G,B)>15 && Abs(R-G)>15
            //R [p + 2]
            //G [p + 1]
            //B [p] 
            for (int j = GetMinHeigh(); j < GetMaxHeigh(); j++)
            {
               if (!isInTrangle[i][j]) continue;
               //定位像素点位置
               int p = j * GetAwidth() * 3 + i * 3;

               int[] list = new int[3];
               list[0] = (dstPb.srcArray[p + 2] - BackGroundPb.srcArray[p + 2]);
               list[1] = (dstPb.srcArray[p + 1] - BackGroundPb.srcArray[p + 1]);
               list[2] = (dstPb.srcArray[p] - BackGroundPb.srcArray[p]);
               Array.Sort(list);
               if (list[list.Length - 1] - list[0] > tolerance)
               {
                  if (flag)
                  {
                     dstPb.srcArray[p] = 0;
                     dstPb.srcArray[p + 1] = 0;
                     dstPb.srcArray[p + 2] = 255;
                  }
                  isHand[i][j] = true;
               }
            }
         });
      }

      int Max(int R, int G, int B)
      {
         int Max = R > G ? R : G;
         if (Max < B)
         {
            Max = B;
         }
         return Max;
      }

      int Min(int R, int G, int B)
      {
         int Min = R < G ? R : G;
         if (Min > B)
         {
            Min = B;
         }
         return Min;
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

    }
}