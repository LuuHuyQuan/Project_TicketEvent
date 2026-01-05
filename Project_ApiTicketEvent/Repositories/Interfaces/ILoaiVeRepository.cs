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
        Task<List<LoaiVe>> GetAllAsync(byte? trangThai = 1);
        Task<List<LoaiVe>> GetByNameAsync(string tenLoaiVe, byte? trangThai = 1);
        Task<List<LoaiVe>> GetBySuKienIdAsync(int suKienId, byte? trangThai = 1);
    }
}
