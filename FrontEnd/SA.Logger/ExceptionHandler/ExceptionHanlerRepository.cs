using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SA.LA
{
    public class ExceptionHanlerRepository : IExceptionHanlerRepository<ExceptionLogDetails>
    {

        //IConnectionFactory _ConnectionFactory;

        public ExceptionHanlerRepository()
        {
           // _ConnectionFactory = new DbConnectionFactory("DbContext");
        }

        public void WriteExceptionLogs(string exceptionData)
        {
        //    _ConnectionFactory = new DbConnectionFactory("DbContext");
        //    using (DbContext _context = new DbContext(_ConnectionFactory))
        //    {
        //        using (var command = _context.CreateCommand())
        //        {
        //            command.CommandType = CommandType.StoredProcedure;
        //            command.CommandText = "uspCreateExceptionLogDetails";
        //            command.Parameters.Add(new SqlParameter("@ExceptionDetails",exceptionData));
        //            command.Parameters.Add(new SqlParameter("@CreatedOn", DateTime.Now));
        //            command.Parameters.Add(new SqlParameter("@Createdby", 1));
        //            var obj= command.ExecuteNonQuery();
        //        }
        //    }
        //    //_context.Dispose();
        }
    }
}
