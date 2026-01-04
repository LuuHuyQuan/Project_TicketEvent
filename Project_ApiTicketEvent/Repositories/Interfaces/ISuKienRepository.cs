using Models;
using Models.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface ISuKienRepository
    {
        Task<IEnumerable<SuKien>> GetAllAsync();
        Task<List<SuKienRequest>> GetByNameAsync(string tenSuKien, bool? trangThai = true);
        Task<List<SuKienRequest>> GetByDanhMucNameAsync(string tenDanhMuc, bool? trangThai = true);
        Task<SuKien?> GetByIdAsync(int id);
        Task<int> CreateAsync(SuKien suKien);
        Task<bool> UpdateAsync(SuKien suKien);
        Task<bool> UpdateTrangThaiAsync(int id, byte trangThai);
        Task<IEnumerable<SuKien>> GetExpiredEventsAsync();
    }
}
