using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IDiaDiemService
    {
        int Create(DiaDiem entity);
        bool Update(DiaDiem entity);
        bool Delete(int id);
    }
}
