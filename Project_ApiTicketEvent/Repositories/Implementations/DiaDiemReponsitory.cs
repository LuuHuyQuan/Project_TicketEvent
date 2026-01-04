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
    public class DiaDiemReponsitory : IDiaDiemReponsitory
    {
        private readonly IDbConnectionFactory _connectionFactory;
        public DiaDiemReponsitory(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public async Task<List<DiaDiem>> GetAllAsync(bool? trangThai = true)
        {
            var result = new List<DiaDiem>();

            const string sql = @"
            SELECT DiaDiemID, TenDiaDiem, DiaChi, SucChua, MoTa, TrangThai
            FROM dbo.DiaDiem
            WHERE (@TrangThai IS NULL OR TrangThai = @TrangThai)
            ORDER BY TenDiaDiem;";

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
                result.Add(new DiaDiem
                {
                    DiaDiemID = reader.GetInt32(reader.GetOrdinal("DiaDiemID")),
                    TenDiaDiem = reader.GetString(reader.GetOrdinal("TenDiaDiem")),
                    DiaChi = reader.GetString(reader.GetOrdinal("DiaChi")),
                    SucChua = reader.IsDBNull(reader.GetOrdinal("SucChua")) ? null : reader.GetInt32(reader.GetOrdinal("SucChua")),
                    MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? null : reader.GetString(reader.GetOrdinal("MoTa")),
                    TrangThai = reader.GetBoolean(reader.GetOrdinal("TrangThai"))
                });
            }

            return await Task.FromResult(result);
        }

        public async Task<DiaDiem?> GetByNameAsync(string tenDiaDiem, bool? trangThai = true)
        {
            tenDiaDiem = (tenDiaDiem ?? string.Empty).Trim();
            if (tenDiaDiem.Length == 0) return null;

            const string sql = @"
            SELECT TOP 1 DiaDiemID, TenDiaDiem, DiaChi, SucChua, MoTa, TrangThai
            FROM dbo.DiaDiem
            WHERE (@TrangThai IS NULL OR TrangThai = @TrangThai)
              AND (TenDiaDiem = @Ten OR TenDiaDiem LIKE N'%' + @Ten + N'%')
            ORDER BY CASE WHEN TenDiaDiem = @Ten THEN 0 ELSE 1 END, TenDiaDiem;";

            using var conn = _connectionFactory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            var pTen = cmd.CreateParameter();
            pTen.ParameterName = "@Ten";
            pTen.Value = tenDiaDiem;
            cmd.Parameters.Add(pTen);

            var pTrangThai = cmd.CreateParameter();
            pTrangThai.ParameterName = "@TrangThai";
            pTrangThai.Value = (object?)trangThai ?? DBNull.Value;
            cmd.Parameters.Add(pTrangThai);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            var dto = new DiaDiem
            {
                DiaDiemID = reader.GetInt32(reader.GetOrdinal("DiaDiemID")),
                TenDiaDiem = reader.GetString(reader.GetOrdinal("TenDiaDiem")),
                DiaChi = reader.GetString(reader.GetOrdinal("DiaChi")),
                SucChua = reader.IsDBNull(reader.GetOrdinal("SucChua")) ? null : reader.GetInt32(reader.GetOrdinal("SucChua")),
                MoTa = reader.IsDBNull(reader.GetOrdinal("MoTa")) ? null : reader.GetString(reader.GetOrdinal("MoTa")),
                TrangThai = reader.GetBoolean(reader.GetOrdinal("TrangThai"))
            };

            return await Task.FromResult(dto);
        }
        public int Create(DiaDiem entity)
        {
                using var conn = _connectionFactory.CreateConnection();
                conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                INSERT INTO dbo.DiaDiem (TenDiaDiem, DiaChi, SucChua, MoTa, TrangThai)
                OUTPUT INSERTED.DiaDiemID
                VALUES (@TenDiaDiem, @DiaChi, @SucChua, @MoTa, @TrangThai);";

                AddParam(cmd, "@TenDiaDiem", entity.TenDiaDiem);
                AddParam(cmd, "@DiaChi", entity.DiaChi);
                AddParam(cmd, "@SucChua", entity.SucChua);
                AddParam(cmd, "@MoTa", entity.MoTa);
                AddParam(cmd, "@TrangThai", entity.TrangThai);

                var obj = cmd.ExecuteScalar();
                return Convert.ToInt32(obj);
        }

        public bool Delete(int id)
        {
            using var conn = _connectionFactory.CreateConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE dbo.DiaDiem SET TrangThai = 0 WHERE DiaDiemID = @Id;";
            AddParam(cmd, "@Id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Update(DiaDiem entity)
        {
            using var conn = _connectionFactory.CreateConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            UPDATE dbo.DiaDiem
            SET TenDiaDiem = @TenDiaDiem,
                DiaChi     = @DiaChi,
                SucChua    = @SucChua,
                MoTa       = @MoTa,
                TrangThai  = @TrangThai
            WHERE DiaDiemID = @Id;";

            AddParam(cmd, "@Id", entity.DiaDiemID);
            AddParam(cmd, "@TenDiaDiem", entity.TenDiaDiem);
            AddParam(cmd, "@DiaChi", entity.DiaChi);
            AddParam(cmd, "@SucChua", entity.SucChua);
            AddParam(cmd, "@MoTa", entity.MoTa);
            AddParam(cmd, "@TrangThai", entity.TrangThai);

            return cmd.ExecuteNonQuery() > 0;
        }
        private static void AddParam(IDbCommand cmd, string name, object? value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }
    }
}
