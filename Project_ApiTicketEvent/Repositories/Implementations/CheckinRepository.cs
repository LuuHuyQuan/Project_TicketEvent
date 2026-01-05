using Data;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.Interfaces;
using Models;
using Dapper;

namespace Repositories.Implementations
{
    public class CheckinRepository : ICheckinRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public CheckinRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<NhatKyCheckIn>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT CheckinID, VeID, SuKienID, NhanVienID, 
                              ThoiGianCheckin, KetQua, GhiChu 
                       FROM Checkin 
                       ORDER BY ThoiGianCheckin DESC";

            return await connection.QueryAsync<NhatKyCheckIn>(sql);
        }

        public async Task<NhatKyCheckIn?> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT CheckinID, VeID, SuKienID, NhanVienID, 
                              ThoiGianCheckin, KetQua, GhiChu 
                       FROM Checkin 
                       WHERE CheckinID = @Id";

            return await connection.QueryFirstOrDefaultAsync<NhatKyCheckIn>(sql, new { Id = id });
        }

        public async Task<IEnumerable<NhatKyCheckIn>> GetBySuKienAsync(int suKienId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT CheckinID, VeID, SuKienID, NhanVienID, 
                              ThoiGianCheckin, KetQua, GhiChu 
                       FROM Checkin 
                       WHERE SuKienID = @SuKienId
                       ORDER BY ThoiGianCheckin DESC";

            return await connection.QueryAsync<NhatKyCheckIn>(sql, new { SuKienId = suKienId });
        }

        public async Task<IEnumerable<NhatKyCheckIn>> GetByVeAsync(int veId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT CheckinID, VeID, SuKienID, NhanVienID, 
                              ThoiGianCheckin, KetQua, GhiChu 
                       FROM Checkin 
                       WHERE VeID = @VeId
                       ORDER BY ThoiGianCheckin DESC";

            return await connection.QueryAsync<NhatKyCheckIn>(sql, new { VeId = veId });
        }

        public async Task<int> CreateAsync(NhatKyCheckIn checkin)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"INSERT INTO Checkin (VeID, SuKienID, NhanVienID, 
                                            ThoiGianCheckin, KetQua, GhiChu)
                       VALUES (@VeID, @SuKienID, @NhanVienID, 
                               @ThoiGianCheckin, @KetQua, @GhiChu);
                       SELECT CAST(SCOPE_IDENTITY() as int)";

            return await connection.QuerySingleAsync<int>(sql, checkin);
        }

        public async Task<bool> UpdateAsync(NhatKyCheckIn checkin)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"UPDATE Checkin 
                       SET VeID = @VeID,
                           SuKienID = @SuKienID,
                           NhanVienID = @NhanVienID,
                           ThoiGianCheckin = @ThoiGianCheckin,
                           KetQua = @KetQua,
                           GhiChu = @GhiChu
                       WHERE CheckinID = @CheckinID";

            var rowsAffected = await connection.ExecuteAsync(sql, checkin);
            return rowsAffected > 0;
        }

        public async Task<int> GetTotalCheckinBySuKienAsync(int suKienId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT COUNT(*) 
                       FROM Checkin 
                       WHERE SuKienID = @SuKienId AND KetQua = 1";

            return await connection.ExecuteScalarAsync<int>(sql, new { SuKienId = suKienId });
        }
    }
}
