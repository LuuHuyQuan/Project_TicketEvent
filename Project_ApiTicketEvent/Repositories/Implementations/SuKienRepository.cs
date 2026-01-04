using Data;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Repositories.Interfaces;

namespace Repositories.Implementations
{
    public class SuKienRepository : ISuKienRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public SuKienRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<SuKien>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT SuKienID, DanhMucID, DiaDiemID, ToChucID, TenSuKien, 
                              MoTa, ThoiGianBatDau, ThoiGianKetThuc, AnhBiaUrl, 
                              TrangThai, NgayTao 
                       FROM SuKien 
                       ORDER BY NgayTao DESC";

            return await connection.QueryAsync<SuKien>(sql);
        }

        public async Task<SuKien?> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT SuKienID, DanhMucID, DiaDiemID, ToChucID, TenSuKien, 
                              MoTa, ThoiGianBatDau, ThoiGianKetThuc, AnhBiaUrl, 
                              TrangThai, NgayTao 
                       FROM SuKien 
                       WHERE SuKienID = @Id";

            return await connection.QueryFirstOrDefaultAsync<SuKien>(sql, new { Id = id });
        }

        public async Task<int> CreateAsync(SuKien suKien)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"INSERT INTO SuKien (DanhMucID, DiaDiemID, ToChucID, TenSuKien, 
                                           MoTa, ThoiGianBatDau, ThoiGianKetThuc, 
                                           AnhBiaUrl, TrangThai, NgayTao)
                       VALUES (@DanhMucID, @DiaDiemID, @ToChucID, @TenSuKien, 
                               @MoTa, @ThoiGianBatDau, @ThoiGianKetThuc, 
                               @AnhBiaUrl, @TrangThai, @NgayTao);
                       SELECT CAST(SCOPE_IDENTITY() as int)";

            return await connection.QuerySingleAsync<int>(sql, suKien);
        }

        public async Task<bool> UpdateAsync(SuKien suKien)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"UPDATE SuKien 
                       SET DanhMucID = @DanhMucID,
                           DiaDiemID = @DiaDiemID,
                           ToChucID = @ToChucID,
                           TenSuKien = @TenSuKien,
                           MoTa = @MoTa,
                           ThoiGianBatDau = @ThoiGianBatDau,
                           ThoiGianKetThuc = @ThoiGianKetThuc,
                           AnhBiaUrl = @AnhBiaUrl,
                           TrangThai = @TrangThai
                       WHERE SuKienID = @SuKienID";

            var rowsAffected = await connection.ExecuteAsync(sql, suKien);
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateTrangThaiAsync(int id, byte trangThai)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"UPDATE SuKien 
                       SET TrangThai = @TrangThai
                       WHERE SuKienID = @Id";

            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, TrangThai = trangThai });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<SuKien>> GetExpiredEventsAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT SuKienID, DanhMucID, DiaDiemID, ToChucID, TenSuKien, 
                              MoTa, ThoiGianBatDau, ThoiGianKetThuc, AnhBiaUrl, 
                              TrangThai, NgayTao 
                       FROM SuKien 
                       WHERE ThoiGianKetThuc <= @Now 
                       AND TrangThai NOT IN (3, 4)";

            return await connection.QueryAsync<SuKien>(sql, new { Now = DateTime.Now });
        }
    }
}
