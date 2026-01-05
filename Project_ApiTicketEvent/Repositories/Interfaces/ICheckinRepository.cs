using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface ICheckinRepository
    {
        Task<IEnumerable<NhatKyCheckIn>> GetAllAsync();
        Task<NhatKyCheckIn?> GetByIdAsync(int id);
        Task<IEnumerable<NhatKyCheckIn>> GetBySuKienAsync(int suKienId);
        Task<IEnumerable<NhatKyCheckIn>> GetByVeAsync(int veId);
        Task<int> CreateAsync(NhatKyCheckIn checkin);
        Task<bool> UpdateAsync(NhatKyCheckIn checkin);
        Task<int> GetTotalCheckinBySuKienAsync(int suKienId);
    }
}
