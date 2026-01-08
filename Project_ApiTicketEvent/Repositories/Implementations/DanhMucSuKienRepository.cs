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
        public DanhMucSuKien? GetById(int id)
        {
            using var conn = _connectionFactory.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM DanhMucSuKien WHERE DanhMucID = @id";
            AddParam(cmd, "@id", id);

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? Map(reader) : null;
        }
        public int Create(DanhMucSuKien entity)
        {
            using var conn = _connectionFactory.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO DanhMucSuKien (TenDanhMuc, MoTa, ThuTuHienThi, TrangThai)
                OUTPUT INSERTED.DanhMucID
                VALUES (@Ten, @MoTa, @ThuTu, @TrangThai)";

            AddParam(cmd, "@Ten", entity.TenDanhMuc);
            AddParam(cmd, "@MoTa", entity.MoTa);
            AddParam(cmd, "@ThuTu", entity.ThuTuHienThi);
            AddParam(cmd, "@TrangThai", entity.TrangThai);

            return (int)cmd.ExecuteScalar()!;
        }

        public bool Delete(int id)
        {
            using var conn = _connectionFactory.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE DanhMucSuKien SET TrangThai = 0 WHERE DanhMucID = @id";
            AddParam(cmd, "@id", id);

            return cmd.ExecuteNonQuery() > 0;
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

        public bool Update(DanhMucSuKien entity)
        {
             using var conn = _connectionFactory.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE DanhMucSuKien
                SET TenDanhMuc=@Ten, MoTa=@MoTa, ThuTuHienThi=@ThuTu, TrangThai=@TrangThai
                WHERE DanhMucID=@Id";

            AddParam(cmd, "@Id", entity.DanhMucID);
            AddParam(cmd, "@Ten", entity.TenDanhMuc);
            AddParam(cmd, "@MoTa", entity.MoTa);
            AddParam(cmd, "@ThuTu", entity.ThuTuHienThi);
            AddParam(cmd, "@TrangThai", entity.TrangThai);

            return cmd.ExecuteNonQuery() > 0;
        }
        private void AddParam(IDbCommand cmd, string name, object? value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }
        private DanhMucSuKien Map(IDataReader r)
        {
            return new DanhMucSuKien
            {
                DanhMucID = (int)r["DanhMucID"],
                TenDanhMuc = r["TenDanhMuc"].ToString()!,
                MoTa = r["MoTa"] as string,
                ThuTuHienThi = r["ThuTuHienThi"] as int?,
                TrangThai = (bool)r["TrangThai"]
            };
        }
    }
}
