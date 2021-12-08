using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SA.DAL
{
    public interface IUnitOfWork
    {
        void Dispose();

        void SaveChanges();
    }
}
