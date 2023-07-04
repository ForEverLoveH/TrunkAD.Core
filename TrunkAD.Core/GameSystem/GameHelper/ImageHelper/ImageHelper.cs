using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.XFeatures2D;
using Point = OpenCvSharp.Point;


namespace TrunkAD.Core.GameSystem.GameHelper
{
   public class ImageHelper
   {
      public static OpenCvSharp.Mat Bitmap2Mat(Bitmap bmp)
      {
         OpenCvSharp.Mat mat = null;
         try
         {
            mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmp); //用bitmap转换为mat

         }
         catch (Exception ex)
         {
            mat = null;
            Console.WriteLine(ex.Message);
         }

         return mat;
      }

      public static Bitmap Mat2Bitmap(OpenCvSharp.Mat mat)
      {
         Bitmap bmp = null;
         try
         {
            bmp = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat); //用bitmap转换为mat

         }
         catch (Exception ex)
         {
            bmp = null;
            Console.WriteLine(ex.Message);
         }
         return bmp;
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

      public static System.Drawing.Point FindPicFromImage(Bitmap imgSrc, Bitmap imgSub, double threshold = 0.9)
      {
         OpenCvSharp.Mat srcMat = null;
         OpenCvSharp.Mat dstMat = null;
         OpenCvSharp.OutputArray outArray = null;
         try
         {
            srcMat = imgSrc.ToMat();
            dstMat = imgSub.ToMat();
            outArray = OpenCvSharp.OutputArray.Create(srcMat);
            OpenCvSharp.Cv2.MatchTemplate(srcMat, dstMat, outArray, TemplateMatchModes.SqDiffNormed);
            double minValue, maxValue;
            OpenCvSharp.Point location, point;
            OpenCvSharp.Cv2.MinMaxLoc(OpenCvSharp.InputArray.Create(outArray.GetMat()), out minValue, out maxValue,
               out location, out point);
            Console.WriteLine(maxValue);
            if (maxValue >= threshold)
               return new System.Drawing.Point(point.X, point.Y);
            return System.Drawing.Point.Empty;
         }
         catch (Exception ex)
         {
            return System.Drawing.Point.Empty;
         }
         finally
         {
            if (srcMat != null)
               srcMat.Dispose();
            if (dstMat != null)
               dstMat.Dispose();
            if (outArray != null)
               outArray.Dispose();
         }
      }


      public static Bitmap findImage(Mat OrgMat, Mat ImageROI)
      {
         Bitmap bmp = null;
         try
         {
            Mat RoiClone = ImageROI.Clone();
            Mat OrgMatClone = OrgMat.Clone();

            Mat mat3 = new Mat();
            //创建result的模板，就是MatchTemplate里的第三个参数
            //mat3.Create(mat1.Cols - mat2.Cols + 1, mat1.Rows - mat2.Rows + 1, MatType.CV_32FC1);

            //进行匹配(1母图,2模版子图,3返回的result，4匹配模式)
            Cv2.MatchTemplate(OrgMat, RoiClone, mat3, TemplateMatchModes.SqDiff);

            //对结果进行归一化(这里我测试的时候没有发现有什么用,但在opencv的书里有这个操作，应该有什么神秘加成，这里也加上)
            Cv2.Normalize(mat3, mat3, 1, 0, NormTypes.MinMax, -1);

            double minValue, maxValue;
            Point minLocation, maxLocation;

            /// 通过函数 minMaxLoc 定位最匹配的位置
            /// (这个方法在opencv里有5个参数，这里我写的时候发现在有3个重载，看了下可以直接写成拿到起始坐标就不取最大值和最小值了)
            /// minLocation和maxLocation根据匹配调用的模式取不同的点
            Cv2.MinMaxLoc(mat3, out minValue, out maxValue, out minLocation, out maxLocation);
            /* Mat OrgMatClone = OrgMat.Clone();
             CircleSegment[] cs = Cv2.HoughCircles(RoiClone, HoughModes.Gradient, 1, 80, 70, 100, 100, 200);
             for (int i = 0; i < cs.Count(); i++)
             {
                 //画圆
                 Cv2.Circle(OrgMatClone, (int)(cs[i].Center.X + minLocation.X), (int)(cs[i].Center.Y + minLocation.Y), (int)cs[i].Radius, new Scalar(0, 0, 255), 2, LineTypes.AntiAlias);
                 //加强圆心显示
                 Cv2.Circle(OrgMatClone, (int)cs[i].Center.X, (int)cs[i].Center.Y, 3, new Scalar(0, 0, 255), 2, LineTypes.AntiAlias);

             }*/
            Point point = new Point(minLocation.X + ImageROI.Width, minLocation.Y + ImageROI.Height);
            OrgMatClone.Rectangle(minLocation, point, new Scalar(0, 0, 255), 3);

            bmp = BitmapConverter.ToBitmap(OrgMatClone);

         }
         catch (Exception ex)
         {
            bmp = null;

         }

         return bmp;
      }


      public static Bitmap HoughCircles(Mat src, double i1 = 30, int i2 = 10)
      {
         Bitmap bmp = null;
         try
         {
            //using (Mat src = new Mat(path, ImreadModes.AnyColor | ImreadModes.AnyDepth))
            using (Mat dst = new Mat())
            {

               //1:因为霍夫圆检测对噪声比较敏感，所以首先对图像做一个中值滤波或高斯滤波(噪声如果没有可以不做)
               Mat m1 = new Mat();
               Cv2.MedianBlur(src, m1, 3); //  ksize必须大于1且是奇数

               //2：转为灰度图像
               Mat m2 = new Mat();
               Cv2.CvtColor(m1, m2, ColorConversionCodes.BGR2GRAY);

               //3：霍夫圆检测：使用霍夫变换查找灰度图像中的圆。
               /*
                * 参数：
                *      1：输入参数： 8位、单通道、灰度输入图像
                *      2：实现方法：目前，唯一的实现方法是HoughCirclesMethod.Gradient
                *      3: dp      :累加器分辨率与图像分辨率的反比。默认=1
                *      4：minDist: 检测到的圆的中心之间的最小距离。(最短距离-可以分辨是两个圆的，否则认为是同心圆-                            src_gray.rows/8)
                *      5:param1:   第一个方法特定的参数。[默认值是100] canny边缘检测阈值低
                *      6:param2:   第二个方法特定于参数。[默认值是100] 中心点累加器阈值 – 候选圆心
                *      7:minRadius: 最小半径
                *      8:maxRadius: 最大半径
                * 
                */
               //CircleSegment[] cs = Cv2.HoughCircles(m2, HoughModes.Gradient, 1, 80, i1, i2, 10, 60);
               CircleSegment[] cs = Cv2.HoughCircles(m2, HoughModes.Gradient, 1, 80, 70, i1, i2, 60);
               src.CopyTo(dst);
               // Vec3d vec = new Vec3d();
               for (int i = 0; i < cs.Count(); i++)
               {
                  //画圆
                  Cv2.Circle(dst, (int)cs[i].Center.X, (int)cs[i].Center.Y, (int)cs[i].Radius, new Scalar(0, 0, 255), 2,
                     LineTypes.AntiAlias);
                  //加强圆心显示
                  Cv2.Circle(dst, (int)cs[i].Center.X, (int)cs[i].Center.Y, 3, new Scalar(0, 0, 255), 2,
                     LineTypes.AntiAlias);
               }

               bmp = BitmapConverter.ToBitmap(dst);

            }
         }
         catch (Exception ex)
         {

            bmp = null;
         }

         return bmp;
      }

      /// <summary>
      /// SURF 特征匹配
      /// </summary>
      /// <returns></returns>
      public static Mat MatchBySURF1(Mat src2, Mat src1)
      {

         var gray1 = new Mat();
         var gray2 = new Mat();

         Cv2.CvtColor(src1, gray1, ColorConversionCodes.BGR2GRAY);
         Cv2.CvtColor(src2, gray2, ColorConversionCodes.BGR2GRAY);

         var surf = SURF.Create(1000, 4, 2, true);

         var descriptors1 = new Mat();
         var descriptors2 = new Mat();
         surf.DetectAndCompute(gray1, null, out KeyPoint[] keypoints1, descriptors1);
         surf.DetectAndCompute(gray2, null, out KeyPoint[] keypoints2, descriptors2);

         var matcher = new BFMatcher(NormTypes.L2, false);
         DMatch[] matches = matcher.Match(descriptors1, descriptors2);

         matches = FindGoodMatchs(descriptors1, matches);

         Mat img = new Mat();
         Cv2.DrawMatches(gray1, keypoints1, gray2, keypoints2, matches, img, null, null, null,
            DrawMatchesFlags.NotDrawSinglePoints);

         return img;
      }

      /// <summary>
      /// FindGoodMatchs
      /// </summary>
      /// <param name="src"></param>
      /// <param name="matches"></param>
      /// <returns></returns>
      private static DMatch[] FindGoodMatchs(Mat src, DMatch[] matches)
      {
         double max = 0, min = 1000;
         for (int i = 0; i < src.Rows; i++)
         {
            double dist = matches[i].Distance;
            if (dist > max)
            {
               max = dist;
            }

            if (dist < min)
            {
               min = dist;
            }
         }

         var good = new List<DMatch>();
         for (int i = 0; i < src.Rows; i++)
         {
            double dist = matches[i].Distance;
            if (dist < Math.Max(3 * min, 0.02))
            {
               good.Add(matches[i]);
            }
         }

         return good.ToArray();
      }

      /// <summary>
      /// SURF 特征匹配
      /// </summary>
      /// <returns></returns>
      private static Mat MatchBySURF2(Mat src2, Mat src1)
      {

         var gray1 = new Mat();
         var gray2 = new Mat();

         Cv2.CvtColor(src1, gray1, ColorConversionCodes.BGR2GRAY);
         Cv2.CvtColor(src2, gray2, ColorConversionCodes.BGR2GRAY);

         var surf = SURF.Create(1000, 4, 2, true);

         var descriptors1 = new Mat();
         var descriptors2 = new Mat();
         surf.DetectAndCompute(gray1, null, out KeyPoint[] keypoints1, descriptors1);
         surf.DetectAndCompute(gray2, null, out KeyPoint[] keypoints2, descriptors2);

         var matcher = new FlannBasedMatcher();
         DMatch[] matches = matcher.Match(descriptors1, descriptors2);

         var img = new Mat();
         Cv2.DrawMatches(gray1, keypoints1, gray2, keypoints2, matches, img, new Scalar(0, 255, 0),
            new Scalar(0, 0, 255), null, DrawMatchesFlags.NotDrawSinglePoints);

         return img;
      }

      /// <summary>
      /// bitmap转stream
      /// </summary>
      /// <param name="bmp"></param>
      /// <returns></returns>
      public static MemoryStream Bitmap2MemoryStream(Bitmap bmp)
      {
         System.IO.MemoryStream ms = new System.IO.MemoryStream();
         try
         {
            bmp.Save(ms, ImageFormat.Bmp);
         }
         catch (Exception ex)
         {
            ms = null;
            LoggerHelper.Debug(ex);
         }

         return ms;
      }

      public static Image MemoryStream2Bitmap(MemoryStream ms)
      {
         Image bmp = null;
         try
         {
            bmp = System.Drawing.Bitmap.FromStream(ms);

         }
         catch (Exception ex)
         {
            bmp = null;
            LoggerHelper.Debug(ex);
         }

         return bmp;
      }

      /// <summary>
      /// 获取图片缩放比例
      /// </summary>
      /// <param name="oldWidth">原图片宽</param>
      /// <param name="oldHeigt">原图片高</param>
      /// <param name="newWidth">目标图片宽</param>
      /// <param name="newHeight">目标图片高</param>
      /// <returns></returns>
      public static float getWidthAndHeight(int oldWidth, int oldHeigt, int newWidth, int newHeight)
      {
         //按比例缩放           
         float scaleRate = 0.0f;
         if (oldWidth >= newWidth && oldHeigt >= newHeight)
         {
            int widthDis = oldWidth - newWidth;
            int heightDis = oldHeigt - newHeight;
            if (widthDis > heightDis)
            {
               scaleRate = newWidth * 1f / oldWidth;
            }
            else
            {
               scaleRate = newHeight * 1f / oldHeigt;
            }
         }
         else if (oldWidth >= newWidth && oldHeigt < newHeight)
         {
            scaleRate = newWidth * 1f / oldWidth;
         }
         else if (oldWidth < newWidth && oldHeigt >= newHeight)
         {
            scaleRate = newHeight * 1f / oldHeigt;
         }
         else
         {
            int widthDis = newWidth - oldWidth;
            int heightDis = newHeight - oldHeigt;
            if (widthDis > heightDis)
            {
               scaleRate = newHeight * 1f / oldHeigt;
            }
            else
            {
               scaleRate = newWidth * 1f / oldWidth;
            }
         }

         return scaleRate;
      }

      /// <summary>
      /// 按指定宽高缩放图片
      /// </summary>
      /// <param name="image">原图片</param>
      /// <param name="dstWidth">目标图片宽</param>
      /// <param name="dstHeight">目标图片高</param>
      /// <returns></returns>
      public static Image ScaleImage(Image image, int dstWidth, int dstHeight)
      {
         Graphics g = null;
         try
         {
            //按比例缩放           
            float scaleRate = getWidthAndHeight(image.Width, image.Height, dstWidth, dstHeight);
            int width = (int)(image.Width * scaleRate);
            int height = (int)(image.Height * scaleRate);

            //将宽度调整为4的整数倍
            if (width % 4 != 0)
            {
               width = width - width % 4;
            }

            Bitmap destBitmap = new Bitmap(width, height);
            g = Graphics.FromImage(destBitmap);
            g.Clear(Color.Transparent);

            //设置画布的描绘质量         
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, new Rectangle((width - width) / 2, (height - height) / 2, width, height), 0, 0,
               image.Width, image.Height, GraphicsUnit.Pixel);

            return destBitmap;
         }
         catch (Exception e)
         {
            Console.WriteLine(e.Message);
         }
         finally
         {
            if (g != null)
            {
               g.Dispose();
            }
         }

         return null;
      }



      public static Bitmap ResizeImage(Image image, int Width, int Height)
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