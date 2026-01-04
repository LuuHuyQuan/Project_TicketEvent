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
        Task<List<DiaDiem>> GetAllAsync();
        Task<DiaDiem?> GetByNameAsync(string tenDiaDiem);
        int Create(DiaDiem entity);
        bool Update(DiaDiem entity);
        bool Delete(int id);
    }
}
