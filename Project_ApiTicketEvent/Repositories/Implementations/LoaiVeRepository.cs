using Data;
using Models;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class LoaiVeRepository:ILoaiVeRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public LoaiVeRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        // 1) GET ALL
        public Task<List<LoaiVe>> GetAllAsync(bool? trangThai = true)
        {
            const string sql = @"
SELECT LoaiVeID, SuKienID, TenLoaiVe, MoTa, DonGia,
       SoLuongToiDa, SoLuongDaBan, GioiHanMoiKhach,
       ThoiGianMoBan, ThoiGianDongBan, TrangThai
FROM dbo.LoaiVe
WHERE (@TrangThai IS NULL OR TrangThai = @TrangThai)
ORDER BY SuKienID, DonGia;";

            return QueryListAsync(sql, ("@TrangThai", (object?)trangThai ?? DBNull.Value));
        }

        // 2) GET BY NAME
        public Task<List<LoaiVe>> GetByNameAsync(string tenLoaiVe, bool? trangThai = true)
        {
            tenLoaiVe = (tenLoaiVe ?? string.Empty).Trim();
            if (tenLoaiVe.Length == 0) return Task.FromResult(new List<LoaiVe>());

            const string sql = @"
SELECT LoaiVeID, SuKienID, TenLoaiVe, MoTa, DonGia,
       SoLuongToiDa, SoLuongDaBan, GioiHanMoiKhach,
       ThoiGianMoBan, ThoiGianDongBan, TrangThai
FROM dbo.LoaiVe
WHERE (@TrangThai IS NULL OR TrangThai = @TrangThai)
  AND (TenLoaiVe = @Ten OR TenLoaiVe LIKE N'%' + @Ten + N'%')
ORDER BY
  CASE WHEN TenLoaiVe = @Ten THEN 0 ELSE 1 END,
  SuKienID, DonGia;";

            return QueryListAsync(sql,
                ("@Ten", tenLoaiVe),
                ("@TrangThai", (object?)trangThai ?? DBNull.Value));
        }

        public Task<List<LoaiVe>> GetByTenSuKienAsync(string tenSuKien, bool? trangThai = true)
        {
            tenSuKien = (tenSuKien ?? string.Empty).Trim();
            if (tenSuKien.Length == 0) return Task.FromResult(new List<LoaiVe>());

            const string sql = @"
SELECT 
    lv.LoaiVeID, lv.SuKienID, lv.TenLoaiVe, lv.MoTa, lv.DonGia,
    lv.SoLuongToiDa, lv.SoLuongDaBan, lv.GioiHanMoiKhach,
    lv.ThoiGianMoBan, lv.ThoiGianDongBan, lv.TrangThai
FROM dbo.LoaiVe lv
JOIN dbo.SuKien sk ON sk.SuKienID = lv.SuKienID
WHERE (@TrangThai IS NULL OR lv.TrangThai = @TrangThai)
  AND (sk.TenSuKien = @Ten OR sk.TenSuKien LIKE N'%' + @Ten + N'%')
ORDER BY 
  CASE WHEN sk.TenSuKien = @Ten THEN 0 ELSE 1 END,
  lv.DonGia;";

            return QueryListAsync(sql,
                ("@Ten", tenSuKien),
                ("@TrangThai", (object?)trangThai ?? DBNull.Value));
        }

        private Task<List<LoaiVe>> QueryListAsync(string sql, params (string Name, object Value)[] parameters)
        {
            var result = new List<LoaiVe>();

            using var conn = _connectionFactory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            foreach (var (name, value) in parameters)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = name;
                p.Value = value;
                cmd.Parameters.Add(p);
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new LoaiVe
                {
                    LoaiVeID = reader.GetInt32(reader.GetOrdinal("LoaiVeID")),
                    SuKienID = reader.GetInt32(reader.GetOrdinal("SuKienID")),
                    TenLoaiVe = reader.GetString(reader.GetOrdinal("TenLoaiVe")),
                    MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? null : reader.GetString(reader.GetOrdinal("MoTa")),
                    DonGia = reader.GetDecimal(reader.GetOrdinal("DonGia")),
                    SoLuongToiDa = reader.GetInt32(reader.GetOrdinal("SoLuongToiDa")),
                    SoLuongDaBan = reader.GetInt32(reader.GetOrdinal("SoLuongDaBan")),
                    GioiHanMoiKhach = reader.IsDBNull(reader.GetOrdinal("GioiHanMoiKhach")) ? null : reader.GetInt32(reader.GetOrdinal("GioiHanMoiKhach")),
                    ThoiGianMoBan = reader.IsDBNull(reader.GetOrdinal("ThoiGianMoBan")) ? null : reader.GetDateTime(reader.GetOrdinal("ThoiGianMoBan")),
                    ThoiGianDongBan = reader.IsDBNull(reader.GetOrdinal("ThoiGianDongBan")) ? null : reader.GetDateTime(reader.GetOrdinal("ThoiGianDongBan")),
                    TrangThai = reader.GetBoolean(reader.GetOrdinal("TrangThai")) // BIT -> bool
                });
            }

            return Task.FromResult(result);
        }
    }
}
