using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrunkAD.Core.GameSystem.GameModel
{
    public class UpLoadResult
    {
        public string Error { get; set; }
        public List<Dictionary<string, int>> Results { get; set; }
    }
    public class UpLoadResults
    {
        public static int success = 1;// 成功
        public static int error1 = -2;// 学生数据有误
        public static int error2 = -3; // 报项数据有误
        public static int error3 = -4;// 轮次数据有误
        public static String Match(int index)
        {
            string res = "";
            switch (index)
            {
                case 1:
                    res = "成功";
                    break;
                case -1:
                    res = "学生数据有误";
                    break;
                case -2:
                    res = "报项数据有误";
                    break;
                case -3:
                    res = "轮次数据有误";
                    break;
                case -4:
                    res = "轮次已经上报过了";
                    break;
                default:
                    res = "未解析错误";
                    break;
            }

            return res;
        }

    }
}
