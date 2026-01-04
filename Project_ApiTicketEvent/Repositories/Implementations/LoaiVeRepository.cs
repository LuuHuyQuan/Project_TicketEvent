using Data;
using Models;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Repositories.Implementations
{
    public class LoaiVeRepository : ILoaiVeRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public LoaiVeRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<LoaiVe>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT LoaiVeID, SuKienID, TenLoaiVe, MoTa, DonGia, 
                              SoLuongToiDa, SoLuongDaBan, GioiHanMoiKhach, 
                              ThoiGianMoBan, ThoiGianDongBan, TrangThai 
                       FROM LoaiVe 
                       ORDER BY SuKienID, TenLoaiVe";

            return await connection.QueryAsync<LoaiVe>(sql);
        }

        public async Task<LoaiVe?> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT LoaiVeID, SuKienID, TenLoaiVe, MoTa, DonGia, 
                              SoLuongToiDa, SoLuongDaBan, GioiHanMoiKhach, 
                              ThoiGianMoBan, ThoiGianDongBan, TrangThai 
                       FROM LoaiVe 
                       WHERE LoaiVeID = @Id";

            return await connection.QueryFirstOrDefaultAsync<LoaiVe>(sql, new { Id = id });
        }

        public async Task<IEnumerable<LoaiVe>> GetBySuKienAsync(int suKienId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"SELECT LoaiVeID, SuKienID, TenLoaiVe, MoTa, DonGia, 
                              SoLuongToiDa, SoLuongDaBan, GioiHanMoiKhach, 
                              ThoiGianMoBan, ThoiGianDongBan, TrangThai 
                       FROM LoaiVe 
                       WHERE SuKienID = @SuKienId
                       ORDER BY DonGia";

            return await connection.QueryAsync<LoaiVe>(sql, new { SuKienId = suKienId });
        }

        public async Task<int> CreateAsync(LoaiVe loaiVe)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"INSERT INTO LoaiVe (SuKienID, TenLoaiVe, MoTa, DonGia, 
                                           SoLuongToiDa, SoLuongDaBan, GioiHanMoiKhach, 
                                           ThoiGianMoBan, ThoiGianDongBan, TrangThai)
                       VALUES (@SuKienID, @TenLoaiVe, @MoTa, @DonGia, 
                               @SoLuongToiDa, @SoLuongDaBan, @GioiHanMoiKhach, 
                               @ThoiGianMoBan, @ThoiGianDongBan, @TrangThai);
                       SELECT CAST(SCOPE_IDENTITY() as int)";

            return await connection.QuerySingleAsync<int>(sql, loaiVe);
        }

        public async Task<bool> UpdateAsync(LoaiVe loaiVe)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"UPDATE LoaiVe 
                       SET SuKienID = @SuKienID,
                           TenLoaiVe = @TenLoaiVe,
                           MoTa = @MoTa,
                           DonGia = @DonGia,
                           SoLuongToiDa = @SoLuongToiDa,
                           SoLuongDaBan = @SoLuongDaBan,
                           GioiHanMoiKhach = @GioiHanMoiKhach,
                           ThoiGianMoBan = @ThoiGianMoBan,
                           ThoiGianDongBan = @ThoiGianDongBan,
                           TrangThai = @TrangThai
                       WHERE LoaiVeID = @LoaiVeID";

            var rowsAffected = await connection.ExecuteAsync(sql, loaiVe);
            return rowsAffected > 0;
        }
    }
}
