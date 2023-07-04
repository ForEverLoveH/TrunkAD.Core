using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrunkAD.Core.GameSystem.GameModel
{
    public class StuViewPojo
    {
        //序号
        public string Number { get; set; }
        //姓名
        public string Name { get; set; }
        //考号
        public string IdNumber { get; set; }
        //轮次成绩
        public List<ScoreViewPojo> ScoreViewPojos { get; set; }
        //唯一编号
        public string Id { get; set; }

        public bool isMaxScore { get; set; }

        public StuViewPojo()
        {
            isMaxScore = false;
            Number = string.Empty;
            Name = string.Empty;
            IdNumber = string.Empty;
            Id = string.Empty;
            ScoreViewPojos = new List<ScoreViewPojo>();
        }
    }

    public class ScoreViewPojo
    {
        //成绩
        public string Score { get; set; }
        //是否上传
        public bool IsUpload { get; set; }
        /// <summary>
        /// 轮次 -1为错误数据
        /// </summary>
        public int roundCound { get; set; }

        public ScoreViewPojo()
        {
            Score = string.Empty;
            IsUpload = false;
            roundCound = -1;
        }
    }
}

