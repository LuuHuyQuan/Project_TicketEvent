using Dapper;
using Data;
using Models;
using Repositories.Interfaces;
using Models.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public async Task<List<SuKienRequest>> GetByNameAsync(string tenSuKien, bool? trangThai = true)
        {
            tenSuKien = (tenSuKien ?? string.Empty).Trim();
            if (tenSuKien.Length == 0) return new List<SuKienRequest>();

            const string sql = @"
            SELECT 
                sk.SuKienID, sk.TenSuKien, sk.DanhMucID,
                dm.TenDanhMuc,
                sk.DiaDiemID, dd.TenDiaDiem,
                sk.ThoiGianBatDau, sk.ThoiGianKetThuc,
                sk.MoTa,sk.AnhBiaUrl, sk.TrangThai
            FROM dbo.SuKien sk
            JOIN dbo.DanhMucSuKien dm ON dm.DanhMucID = sk.DanhMucID
            LEFT JOIN dbo.DiaDiem dd ON dd.DiaDiemID = sk.DiaDiemID
            WHERE (@TrangThai IS NULL OR sk.TrangThai = @TrangThai)
              AND (sk.TenSuKien = @Ten OR sk.TenSuKien LIKE N'%' + @Ten + N'%')
            ORDER BY
                CASE WHEN sk.TenSuKien = @Ten THEN 0 ELSE 1 END,
                sk.ThoiGianBatDau DESC;";

            return await QueryListAsync(sql,
                ("@Ten", tenSuKien),
                ("@TrangThai", (object?)trangThai ?? DBNull.Value));
        }

        public async Task<List<SuKienRequest>> GetByDanhMucNameAsync(string tenDanhMuc, bool? trangThai = true)
        {
            tenDanhMuc = (tenDanhMuc ?? string.Empty).Trim();
            if (tenDanhMuc.Length == 0) return new List<SuKienRequest>();

            const string sql = @"
            SELECT 
                sk.SuKienID, sk.TenSuKien, sk.DanhMucID,
                dm.TenDanhMuc,
                sk.DiaDiemID, dd.TenDiaDiem,
                sk.ThoiGianBatDau, sk.ThoiGianKetThuc,
                sk.MoTa,sk.AnhBiaUrl, sk.TrangThai
            FROM dbo.SuKien sk
            JOIN dbo.DanhMucSuKien dm ON dm.DanhMucID = sk.DanhMucID
            LEFT JOIN dbo.DiaDiem dd ON dd.DiaDiemID = sk.DiaDiemID
            WHERE (@TrangThai IS NULL OR sk.TrangThai = @TrangThai)
              AND (dm.TenDanhMuc = @TenDM OR dm.TenDanhMuc LIKE N'%' + @TenDM + N'%')
            ORDER BY sk.ThoiGianBatDau DESC;";

            return await QueryListAsync(sql,
                ("@TenDM", tenDanhMuc),
                ("@TrangThai", (object?)trangThai ?? DBNull.Value));
        }

        private async Task<List<SuKienRequest>> QueryListAsync(string sql, params (string Name, object Value)[] parameters)
        {
            var result = new List<SuKienRequest>();

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
                result.Add(new SuKienRequest
                {
                    SuKienID = reader.GetInt32(reader.GetOrdinal("SuKienID")),
                    TenSuKien = reader.GetString(reader.GetOrdinal("TenSuKien")),
                    DanhMucID = reader.GetInt32(reader.GetOrdinal("DanhMucID")),
                    TenDanhMuc = reader.GetString(reader.GetOrdinal("TenDanhMuc")),
                    DiaDiemID = reader.IsDBNull(reader.GetOrdinal("DiaDiemID")) ? null : reader.GetInt32(reader.GetOrdinal("DiaDiemID")),
                    TenDiaDiem = reader.IsDBNull(reader.GetOrdinal("TenDiaDiem")) ? null : reader.GetString(reader.GetOrdinal("TenDiaDiem")),
                    ThoiGianBatDau = reader.IsDBNull(reader.GetOrdinal("ThoiGianBatDau")) ? null : reader.GetDateTime(reader.GetOrdinal("ThoiGianBatDau")),
                    ThoiGianKetThuc = reader.IsDBNull(reader.GetOrdinal("ThoiGianKetThuc")) ? null : reader.GetDateTime(reader.GetOrdinal("ThoiGianKetThuc")),
                    MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? null : reader.GetString(reader.GetOrdinal("MoTa")),
                    AnhBiaUrl = reader.IsDBNull(reader.GetOrdinal("AnhBiaUrl")) ? null : reader.GetString(reader.GetOrdinal("AnhBiaUrl")),
                    TrangThai = Convert.ToInt32(reader["TrangThai"]) == 1
                });
            }

            return await Task.FromResult(result);
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
