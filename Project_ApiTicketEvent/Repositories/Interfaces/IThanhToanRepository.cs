using Microsoft.Data.SqlClient;
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
        int Insert(ThanhToan tt, SqlConnection conn, SqlTransaction tran);
        Task<List<ThanhToan>> GetHistoryAsync(int nguoiMuaId);
        Task<List<ThanhToan>> GetHistoryByDonHangAsync(int nguoiMuaId, int donHangId);
    }
}
