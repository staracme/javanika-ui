using SA.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SA.Models.ExceptionHandler;

namespace SA.DAL.ExceptionHanler
{
    public interface IExceptionHanlerRepository<T>: IRepository< ExceptionLogDetails>
    {
        void WriteExceptionLogs(string exception);
    }
}
