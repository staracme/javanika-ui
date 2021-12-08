using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SA.DAL.Repositories
{

    public abstract class Repository<T> : IRepository<T>
       where T : class

    {

        DbContext _context;

        public Repository(DbContext context)
        {
            _context = context;
        }

        protected DbContext Context
        {
            get
            {
                return this._context;
            }
        }


      


    }






















    //////public abstract class Repository<TEntity> where TEntity : new() 
    //////{
    //////    DbContext _context;

    //////    public Repository(DbContext context)
    //////    {
    //////        _context = context;
    //////    }

    //////    protected DbContext Context 
    //////    { 
    //////        get
    //////        {
    //////        return this._context;
    //////        } 
    //////    }

    //////    protected IEnumerable<TEntity> ToList(IDbCommand command)
    //////    {
    //////        using (var record = command.ExecuteReader())
    //////        {
    //////            List<TEntity> items = new List<TEntity>();
    //////            while (record.Read())
    //////            {

    //////                items.Add(Map<TEntity>(record));
    //////            }
    //////            return items;
    //////        }
    //////    }





    //////    protected TEntity Map<TEntity>(IDataRecord record)
    //////    {
    //////        var objT = Activator.CreateInstance<TEntity>();
    //////        foreach (var property in typeof(TEntity).GetProperties())
    //////        {
    //////            if (record.HasColumn(property.Name) && !record.IsDBNull(record.GetOrdinal(property.Name)))
    //////                property.SetValue(objT, record[property.Name]);


    //////        }
    //////        return objT;
    //////    }


    //////    //protected abstract TEntity Map<TEntity>(IDataRecord record);
    //////}


}
