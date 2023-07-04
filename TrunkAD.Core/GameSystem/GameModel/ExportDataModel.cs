using MiniExcelLibs.Attributes;

namespace TrunkAD.Core.GameSystem.GameModel
{
    public class ExportDataModel
    {
        /// <summary>
        /// 序号
        /// </summary>
        [ExcelColumnName("序号")]  public string id { get; set; }
        /// <summary>
        /// 学校名
        /// </summary>
        [ExcelColumnName("学校名")] public string schoolName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [ExcelColumnName("组号")]   public  string groupName { get; set; }
        /// <summary>
        /// 准考证
        /// </summary>
        [ExcelColumnName("准考证号码")]  public  string ExamIDNumber { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        [ExcelColumnName("学生姓名")]  public  string name { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        [ExcelColumnName("性别")]  public  string sex { get; set; } 
        /// <summary>
        /// 项目名称
        /// </summary>
        [ExcelColumnName("项目名称")]    public string projectName { get; set; }
        /// <summary>
        /// 第一轮成绩
        /// </summary>
        [ExcelColumnName("第一轮")]    public  string FristRound { get; set; }
        /// <summary>
        /// 第二轮成绩
        /// </summary>
        [ExcelColumnName("第二轮")]    public  string  TwoRound { get; set; }
        /// <summary>
        /// 签名
        /// </summary>
        [ExcelColumnName("学生确认签名")]    public  string Remark { get; set; }
    }
}