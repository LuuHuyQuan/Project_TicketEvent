using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface ILoaiVeRepository
    {
        Task<List<LoaiVe>> GetAllAsync(bool? trangThai = true);
        Task<List<LoaiVe>> GetByNameAsync(string tenLoaiVe, bool? trangThai = true);
        Task<List<LoaiVe>> GetByTenSuKienAsync(string tenSuKien, bool? trangThai = true);
    }
}
