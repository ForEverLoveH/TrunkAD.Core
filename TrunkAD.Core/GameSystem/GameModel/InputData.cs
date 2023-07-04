using MiniExcelLibs.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrunkAD.Core.GameSystem.GameModel
{
    public class InputData
    {

        [ExcelColumnName("序号")]
        public int Id { get; set; }
        [ExcelColumnName("日期")]
        public string examTime { get; set; }
        [ExcelColumnName("学校")]
        public string School { get; set; }
        [ExcelColumnName("年级")]
        public string GradeName { get; set; }
        [ExcelColumnName("班级")]
        public string ClassName { get; set; }
        [ExcelColumnName("姓名")]
        public string Name { get; set; }
        [ExcelColumnName("性别")]
        public string Sex { get; set; }
        [ExcelColumnName("准考证号")]
        public string IdNumber { get; set; }
        [ExcelColumnName("组别名称")]
        public string GroupName { get; set; }
    }
}
