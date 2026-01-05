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
        public Task<List<LoaiVe>> GetAllAsync(byte? trangThai = 1)
        {
            const string sql = @"
SELECT LoaiVeID, SuKienID, TenLoaiVe, GiaVe, SoLuong, MoTa, TrangThai
FROM dbo.LoaiVe
WHERE (@TrangThai IS NULL OR TrangThai = @TrangThai)
ORDER BY SuKienID, GiaVe;";

            return QueryListAsync(sql,
                ("@TrangThai", (object?)trangThai ?? DBNull.Value));
        }

        // 2) GET BY NAME
        public Task<List<LoaiVe>> GetByNameAsync(string tenLoaiVe, byte? trangThai = 1)
        {
            tenLoaiVe = (tenLoaiVe ?? string.Empty).Trim();
            if (tenLoaiVe.Length == 0) return Task.FromResult(new List<LoaiVe>());

            const string sql = @"
SELECT LoaiVeID, SuKienID, TenLoaiVe, GiaVe, SoLuong, MoTa, TrangThai
FROM dbo.LoaiVe
WHERE (@TrangThai IS NULL OR TrangThai = @TrangThai)
  AND (TenLoaiVe = @Ten OR TenLoaiVe LIKE N'%' + @Ten + N'%')
ORDER BY
  CASE WHEN TenLoaiVe = @Ten THEN 0 ELSE 1 END,
  SuKienID, GiaVe;";

            return QueryListAsync(sql,
                ("@Ten", tenLoaiVe),
                ("@TrangThai", (object?)trangThai ?? DBNull.Value));
        }

        // 3) GET BY EVENT (SuKienID)
        public Task<List<LoaiVe>> GetBySuKienIdAsync(int suKienId, byte? trangThai = 1)
        {
            const string sql = @"
SELECT LoaiVeID, SuKienID, TenLoaiVe, GiaVe, SoLuong, MoTa, TrangThai
FROM dbo.LoaiVe
WHERE (@TrangThai IS NULL OR TrangThai = @TrangThai)
  AND SuKienID = @SuKienID
ORDER BY GiaVe;";

            return QueryListAsync(sql,
                ("@SuKienID", suKienId),
                ("@TrangThai", (object?)trangThai ?? DBNull.Value));
        }

        // Common mapper
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
                    GiaVe = reader.GetDecimal(reader.GetOrdinal("GiaVe")),
                    SoLuong = reader.IsDBNull(reader.GetOrdinal("SoLuong")) ? null : reader.GetInt32(reader.GetOrdinal("SoLuong")),
                    MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? null : reader.GetString(reader.GetOrdinal("MoTa")),
                    TrangThai = Convert.ToByte(reader["TrangThai"]) // tinyint-safe
                });
            }

            return Task.FromResult(result);
        }
    }
}
