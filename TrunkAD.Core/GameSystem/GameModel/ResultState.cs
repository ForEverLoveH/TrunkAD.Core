using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrunkAD.Core.GameSystem.GameModel
{
    public class ResultState
    {
        public static int NoTest = 0;//未测试
        public static int Test = 1;//已测试
        public static int Withdrawal = 2;//中退
        public static int MissTest = 3;//缺考
        public static int Foul = 4;//犯规 
        public static int Waiver = 5;//弃权 
        public static int ResultState2Int(string state)
        {
            switch (state)
            {
                case "未测试":
                    return ResultState.NoTest;
                case "已测试":
                    return ResultState.Test;
                case "中退":
                    return ResultState.Withdrawal;
                case "缺考":
                    return ResultState.MissTest;
                case "犯规":
                    return ResultState.Foul;
                case "弃权":
                    return ResultState.Waiver;
                default:
                    return 0;
            }
        }
        public static string ResultState2Str(int state)
        {
            switch (state)
            {
                case 0:
                    return "未测试";
                case 1:
                    return "已测试";
                case 2:
                    return "中退";
                case 3:
                    return "缺考";
                case 4:
                    return "犯规";
                case 5:
                    return "弃权";
                default:
                    return "";
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state0"></param>
        /// <returns></returns>
        public static string ResultState2Str(string state0)
        {
            int.TryParse(state0, out int state);
            return ResultState2Str(state);
        }
    }
}
