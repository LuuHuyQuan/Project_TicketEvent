using Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;
using Repositories.Interfaces;
namespace Repositories.Implementations
{
    public class DanhMucSuKienRepository:IDanhMucSuKienRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DanhMucSuKienRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<DanhMucSuKien>> GetAllAsync(bool? trangThai = true)
        {
            var result = new List<DanhMucSuKien>();

            const string sql = @"
SELECT DanhMucID, TenDanhMuc, MoTa, ThuTuHienThi, TrangThai
FROM dbo.DanhMucSuKien
WHERE (@TrangThai IS NULL OR TrangThai = @TrangThai)
ORDER BY ISNULL(ThuTuHienThi, 999999), TenDanhMuc;";

            using var conn = _connectionFactory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            var pTrangThai = cmd.CreateParameter();
            pTrangThai.ParameterName = "@TrangThai";
            pTrangThai.Value = (object?)trangThai ?? DBNull.Value;
            cmd.Parameters.Add(pTrangThai);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new DanhMucSuKien
                {
                    DanhMucID = reader.GetInt32(reader.GetOrdinal("DanhMucID")),
                    TenDanhMuc = reader.GetString(reader.GetOrdinal("TenDanhMuc")),
                    MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? null : reader.GetString(reader.GetOrdinal("MoTa")),
                    ThuTuHienThi = reader.IsDBNull(reader.GetOrdinal("ThuTuHienThi")) ? null : reader.GetInt32(reader.GetOrdinal("ThuTuHienThi")),
                    TrangThai = reader.GetBoolean(reader.GetOrdinal("TrangThai"))
                });
            }

            return await Task.FromResult(result);
        }

        public async Task<DanhMucSuKien?> GetByNameAsync(string tenDanhMuc, bool? trangThai = true)
        {
            tenDanhMuc = (tenDanhMuc ?? string.Empty).Trim();
            if (tenDanhMuc.Length == 0) return null;

            const string sql = @"
SELECT TOP 1 DanhMucID, TenDanhMuc, MoTa, ThuTuHienThi, TrangThai
FROM dbo.DanhMucSuKien
WHERE
    (@TrangThai IS NULL OR TrangThai = @TrangThai)
    AND (
        TenDanhMuc = @TenDanhMuc
        OR TenDanhMuc LIKE N'%' + @TenDanhMuc + N'%'
    )
ORDER BY
    CASE WHEN TenDanhMuc = @TenDanhMuc THEN 0 ELSE 1 END,
    TenDanhMuc;";

            using var conn = _connectionFactory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            var pTen = cmd.CreateParameter();
            pTen.ParameterName = "@TenDanhMuc";
            pTen.Value = tenDanhMuc;
            cmd.Parameters.Add(pTen);

            var pTrangThai = cmd.CreateParameter();
            pTrangThai.ParameterName = "@TrangThai";
            pTrangThai.Value = (object?)trangThai ?? DBNull.Value;
            cmd.Parameters.Add(pTrangThai);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            var dto = new DanhMucSuKien
            {
                DanhMucID = reader.GetInt32(reader.GetOrdinal("DanhMucID")),
                TenDanhMuc = reader.GetString(reader.GetOrdinal("TenDanhMuc")),
                MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? null : reader.GetString(reader.GetOrdinal("MoTa")),
                ThuTuHienThi = reader.IsDBNull(reader.GetOrdinal("ThuTuHienThi")) ? null : reader.GetInt32(reader.GetOrdinal("ThuTuHienThi")),
                TrangThai = reader.GetBoolean(reader.GetOrdinal("TrangThai"))
            };

            return await Task.FromResult(dto);
        }
    }
}
