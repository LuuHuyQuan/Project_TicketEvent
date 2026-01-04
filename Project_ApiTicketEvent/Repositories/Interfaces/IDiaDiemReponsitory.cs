using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IDiaDiemReponsitory
    {
        Task<List<DiaDiem>> GetAllAsync(bool? trangThai = true);
        Task<DiaDiem?> GetByNameAsync(string tenDiaDiem, bool? trangThai = true);
        int Create(DiaDiem entity);
        bool Update(DiaDiem entity);
        bool Delete(int id);
    }
}
