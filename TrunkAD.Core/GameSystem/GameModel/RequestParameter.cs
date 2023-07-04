using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrunkAD.Core.GameSystem.GameModel
{
    public class RequestParameter
    {
        /// <summary>
        /// 注册软件生成的机器码
        /// </summary>
        public string MachineCode { get; set; }
        /// <summary>
        /// 管理员账号
        /// </summary>
        public string AdminUserName { get; set; }
        /// <summary>
        /// 裁判员账号
        /// </summary>
        public string TestManUserName { get; set; }
        /// <summary>
        /// 裁判员密码
        /// </summary>
        public string TestManPassword { get; set; }
        /// <summary>
        /// 考试id
        /// </summary>
        public string ExamId { get; set; }

        /// <summary>
        /// 组id
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// 准考证号
        /// </summary>
        public string IdNumber { get; set; }

        /// <summary>
        /// 要下载的组数
        /// </summary>
        public string GroupNums { get; set; }
    }
}
