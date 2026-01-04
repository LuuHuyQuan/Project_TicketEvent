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
        Task<IEnumerable<LoaiVe>> GetAllAsync();
        Task<LoaiVe?> GetByIdAsync(int id);
        Task<IEnumerable<LoaiVe>> GetBySuKienAsync(int suKienId);
        Task<int> CreateAsync(LoaiVe loaiVe);
        Task<bool> UpdateAsync(LoaiVe loaiVe);
    }
}
