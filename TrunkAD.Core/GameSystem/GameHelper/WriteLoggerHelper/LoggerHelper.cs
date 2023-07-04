using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrunkAD.Core.GameSystem.GameHelper 
{
    public class LoggerHelper
    {
        private static readonly log4net.ILog LogInfo = log4net.LogManager.GetLogger("LogInfo");

        private static readonly log4net.ILog LogError = log4net.LogManager.GetLogger("LogError");

        private static readonly log4net.ILog LogMonitor = log4net.LogManager.GetLogger("LogMonitor");
        public static void Error(string errorMsg, Exception ex = null)
        {
            if (ex != null)
            {
                LogError.Error(errorMsg, ex);
            }
            else
            {
                LogError.Error(errorMsg);
            }
        }
        public static void Debug(Exception ex)
        {
            if (ex != null)
            {
                string message = GetExceptionMsg(ex);
                Log.Debug(message);

            }

        }

        private static string GetExceptionMsg(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(" ");
            sb.AppendLine("****************************异常文本****************************");
            sb.AppendLine("【出现时间】：" + DateTime.Now.ToString());
            if (ex != null)
            {
                sb.AppendLine("【异常类型】：" + ex.GetType().Name);
                sb.AppendLine("【异常信息】：" + ex.Message);
                sb.AppendLine("【堆栈调用】：" + ex.StackTrace);

                sb.AppendLine("【异常方法】：" + ex.TargetSite);
            }
            else
            {
                sb.AppendLine("【未处理异常】：" + ex.Message);
            }
            sb.AppendLine("***************************************************************");
            return sb.ToString();
        }

        /// 

        /// 记录Info日志
        /// 

        /// 
        /// 
        public static void Info(string msg, Exception ex = null)
        {
            if (ex != null)
            {
                LogInfo.Info(msg, ex);
            }
            else
            {
                LogInfo.Info(msg);
            }
        }

        /// 

        /// 记录Monitor日志
        /// 

        /// 
        public static void Monitor(string msg)
        {
            LogMonitor.Info(msg);
        }

        public static void Fatal(string msg)
        {
            try
            {
                Log.Fatal(msg);
            }
            catch (Exception ex)
            {
                Debug(ex);
            }
        }

    }
}
