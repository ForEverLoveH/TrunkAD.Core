using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrunkAD.Core.GameSystem.GameModel
{
   
        public class GetExamList
        {
            public List<GetExamListResults> Results { get; set; }

            public String Error { get; set; }

        }
        public class GetExamListResults
        {
            public String exam_id;

            public String title;
        }

    
}
