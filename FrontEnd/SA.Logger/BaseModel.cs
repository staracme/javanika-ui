using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SA.LA
{
    public class BaseModel
    {
        public string errMsg { get; set; }
        public ErrorModel errInfo { get; set; }
    }

    public class ErrorModel
    {
        public string errCode { get; set; }
        public string errMsg { get; set; }
    }

}
