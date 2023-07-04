using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace TrunkAD.Core.GameSystem.GameHelper
{
    public class MeasureDLL
    {
         private Type type;
      private bool isUse = false;
      private const string Dll_PATH = "ClassLibrary2.dll";

      public bool IsUse { get => isUse; internal set => isUse = value; }

      public MeasureDLL(string v1, string v2)
      {
         Assembly dll = Assembly.LoadFrom(Dll_PATH);
         Type[] types = dll.GetTypes();
         //获取类名
         type = types.Where(arg => arg.Name.Equals("MeasureDLL")).FirstOrDefault();
         IsUse = verifyTime(v1, v2);
      }
      public bool verifyTime(string v1, string v2)
      {
         MethodInfo methodInfo = type.GetMethod("verifyTime");
         object[] parameters = new object[] { v1, v2 };
         //方法无参数的话
         object obj = methodInfo.Invoke(null, parameters);
         bool value = (bool)obj;
         return value;
      }
      public static string getCode1()
      {
         Assembly dll = Assembly.LoadFrom(Dll_PATH);
         Type[] types = dll.GetTypes();
         Type type = types.Where(arg => arg.Name.Equals("MeasureDLL")).FirstOrDefault();
         MethodInfo methodInfo = type.GetMethod("getCode1");
         object obj = methodInfo.Invoke(null, null);
         string value = (string)obj;
         return value;
      }
      public static string getCode2()
      {
         Assembly dll = Assembly.LoadFrom(Dll_PATH);
         Type[] types = dll.GetTypes();
         Type type = types.Where(arg => arg.Name.Equals("MeasureDLL")).FirstOrDefault();
         MethodInfo methodInfo = type.GetMethod("getCode2");
         object obj = methodInfo.Invoke(null, null);
         string value = (string)obj;
         return value;
      }

      public static string getECode()
      {
         Assembly dll = Assembly.LoadFrom(Dll_PATH);
         Type[] types = dll.GetTypes();
         Type type = types.Where(arg => arg.Name.Equals("MeasureDLL")).FirstOrDefault();
         MethodInfo methodInfo = type.GetMethod("getECode");
         object obj = methodInfo.Invoke(null, null);
         string value = (string)obj;
         return value;
      }
      public  void dispJumpLength1(int x3, int y3,
        List<System.Drawing.Point[]> gfencePnts,
        List<int[]> gfencePntsDisValue,
        int distanceMode,
        int Directionmode,
        Point mousePoint,
        List<System.Drawing.Point[]> gfencePnts1,
        List<int[]> gfencePntsDisValue1,
        int Directionmode1,
        ref Point markerTopJump,
        ref Point markerBottomJump,
        ref double MeasureLenX,
        ref Point m_markerTopJump,
        ref Point m_markerBottomJump,
        ref double m_MeasureLenX,
        ref Point markerTopJumpY,
        ref Point markerBottomJumpY,
        ref double MeasureLenY,
        ref Point m_markerTopJumpY,
        ref Point m_markerBottomJumpY,
        ref double m_MeasureLenY)
      {
         if (!IsUse) return;
         MethodInfo methodInfo = type.GetMethod("dispJumpLength1");
         object[] parameters = new object[22];
         int step = 0;
         parameters[step] = x3;
         step++;
         parameters[step] = y3;
         step++;
         parameters[step] = gfencePnts;
         step++;
         parameters[step] = gfencePntsDisValue;
         step++;
         parameters[step] = distanceMode;
         step++;
         parameters[step] = Directionmode;
         step++;
         parameters[step] = mousePoint;
         step++;
         parameters[step] = gfencePnts1;
         step++;
         parameters[step] = gfencePntsDisValue1;
         step++;
         parameters[step] = Directionmode1;
         methodInfo.Invoke(null, parameters);
         Console.WriteLine();
         step = 10;
         markerTopJump = (Point)parameters[step]; step++;
         markerBottomJump = (Point)parameters[step]; step++;
         MeasureLenX = (double)parameters[step]; step++;
         m_markerTopJump = (Point)parameters[step]; step++;
         m_markerBottomJump = (Point)parameters[step]; step++;
         m_MeasureLenX = (double)parameters[step]; step++;
         markerTopJumpY = (Point)parameters[step]; step++;
         markerBottomJumpY = (Point)parameters[step]; step++;
         MeasureLenY = (double)parameters[step]; step++;
         m_markerTopJumpY = (Point)parameters[step]; step++;
         m_markerBottomJumpY = (Point)parameters[step]; step++;
         m_MeasureLenY = (double)parameters[step]; step++;

         /* object[] parameters = new object[] {
      x3,y3,
      gfencePnts,
      gfencePntsDisValue,
      distanceMode,
      Directionmode,
      mousePoint,
      gfencePnts1,
      gfencePntsDisValue1,
      Directionmode1,
      markerTopJump,
      markerBottomJump,
      MeasureLenX,
      m_markerTopJump,
      m_markerBottomJump,
      m_MeasureLenX,
      markerTopJumpY,
      markerBottomJumpY,
      MeasureLenY,
      m_markerTopJumpY,
      m_markerBottomJumpY,
      m_MeasureLenY };*/
      }

      public static void dispJumpLength2(ref int x3, ref int y3)
      {
         Assembly dll = Assembly.LoadFrom(Dll_PATH);
         Type[] types = dll.GetTypes();
         Type type = types.Where(arg => arg.Name.Equals("MeasureDLL")).FirstOrDefault();
         MethodInfo methodInfo = type.GetMethod("dispJumpLength2");
         object[] parameters = new object[] { x3, y3 };
         parameters = new object[2];
         methodInfo.Invoke(null, parameters);
         Console.WriteLine();
      }

    }
}