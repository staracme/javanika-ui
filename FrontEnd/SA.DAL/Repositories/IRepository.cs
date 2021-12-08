using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace SA.DAL.Repositories
{
    public interface IRepository<T>
        where T : class       
    {
        //IEnumerable<T> ToList(IDbCommand command);
         //T Map<T>(IDataRecord record);

    }
}
