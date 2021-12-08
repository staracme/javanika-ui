using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SA.Models.ExceptionHandler
{
    public class ExceptionLogDetails: BaseModel
    {
        int LogID { get; set; }
        string ExceptionDetails { get; set; }
    }
}
