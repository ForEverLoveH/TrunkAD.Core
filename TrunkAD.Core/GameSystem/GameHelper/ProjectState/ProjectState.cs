using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrunkAD.Core.GameSystem.GameHelper 
{
    public class ProjectState
    {
        /*立定跳远
           投掷实心球
           坐位体前屈
           投掷铅球*/
        public static int Type1 = 0;//立定跳远
        public static int Type2 = 1;//投掷实心球
        public static int Type3 = 2;//坐位体前屈
        public static int Type4 = 3;//投掷铅球

        public static int ProjectStateType2Int(string state)
        {
            switch (state)
            {
                case "立定跳远":
                    return Type1;
                case "投掷实心球":
                    return Type2;
                case "坐位体前屈":
                    return Type3;
                case "投掷铅球":
                    return Type4;
                default:
                    return 0;
            }
        }
        public static string ProjectState2Str(int state)
        {
            switch (state)
            {
                case 0:
                    return "立定跳远";
                case 1:
                    return "投掷实心球";
                case 2:
                    return "坐位体前屈";
                case 3:
                    return "投掷铅球";
                default:
                    return "立定跳远";
            }
        }
        public static string ProjectState2Str(string state0)
        {
            int.TryParse(state0, out int state);
            return ProjectState2Str(state);
        }
    }
}
