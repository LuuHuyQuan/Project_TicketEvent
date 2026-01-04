using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IDanhMucSuKienRepository
    {
        Task<List<DanhMucSuKien>> GetAllAsync(bool? trangThai = true);
        Task<DanhMucSuKien?> GetByNameAsync(string tenDanhMuc, bool? trangThai = true);
    }
}
