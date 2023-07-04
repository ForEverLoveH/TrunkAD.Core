using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrunkAD.Core.GameSystem.GameModel
{
    public class UploadResultsRequestParameter
    {
        public string ExamId { get; set; }
        public string MachineCode { get; set; }
        public string AdminUserName { get; set; }
        public string TestManUserName { get; set; }
        public string TestManPassword { get; set; }
        public List<SudentsItem> Sudents { get; set; }

    }
    public class SudentsItem
    {
        public string SchoolName { get; set; }
        public string GradeName { get; set; }
        public string ClassNumber { get; set; }
        public string Name { get; set; }
        public string IdNumber { get; set; }
        public List<RoundsItem> Rounds { get; set; }
    }

    public class RoundsItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int RoundId { get; set; }
        /// <summary>
        /// 正常
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double Result { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string GroupNo { get; set; }
        public string Memo { get; set; }

        public Dictionary<string, string> Text { get; set; }
        public Dictionary<string, string> Images { get; set; }
        public Dictionary<string, string> Videos { get; set; }
    }

}
