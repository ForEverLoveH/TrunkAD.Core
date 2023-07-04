using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrunkAD.Core.GameSystem.GameModel
{
    public  class ProjectModel
    {
        public string projectName { get; set; }
        public  List<GroupModel> Groups { get; set; }
    }

    public class GroupModel
    {
        public string GroupName { get; set; }
        public int IsAllTested { get;set; }
    }
}
