using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrunkAD.Core.GameSystem.GameModel
{
    
        public class GetMachineCodeList
        {
            public List<GetMachineCodeListResults> Results;

            public String Error;
        }
        public class GetMachineCodeListResults
        {
            public String title;

            public String MachineCode;
        }
    
}
