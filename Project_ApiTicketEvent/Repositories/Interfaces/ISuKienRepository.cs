using Models;
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
        Task<SuKien?> GetByIdAsync(int id);
        Task<int> CreateAsync(SuKien suKien);
        Task<bool> UpdateAsync(SuKien suKien);
        Task<bool> UpdateTrangThaiAsync(int id, byte trangThai);
        Task<IEnumerable<SuKien>> GetExpiredEventsAsync();
        List<SuKien> GetPending();    
        byte? GetStatusById(int id);   

        bool Approve(int id);       
        bool Cancel(int id);
    }
}
