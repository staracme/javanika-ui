using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SA.LA
{
    public interface IExceptionHanlerRepository<T> //: IRepository< ExceptionLogDetails>
    {
        void WriteExceptionLogs(string exception);
    }
}
