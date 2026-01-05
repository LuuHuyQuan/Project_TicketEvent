using Models;
using Models.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Models.DTOs.Requests.ThanhToanRequest;

namespace Repositories.Interfaces
{
    public interface IThanhToanRepository
    {
        Task<List<ThanhToan>> GetByDonHangAsync(int nguoiMuaId, int donHangId);
        Task<int> TaoThanhToanAsync(TaoThanhToanRequest req);                 // return ThanhToanID
        Task<object> XacNhanThanhToanAsync(XacNhanThanhToanRequest req);      // trả kết quả + vé
    }
}
