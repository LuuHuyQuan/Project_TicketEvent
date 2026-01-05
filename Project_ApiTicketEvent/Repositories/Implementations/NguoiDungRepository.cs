using Data;
using Microsoft.Data.SqlClient;
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
    public class NguoiDungRepository : INguoiDungRepository
    {
        private readonly IDbConnectionFactory _factory;

        public NguoiDungRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public List<NguoiDung> GetAll()
        {
            const string sql = @"
            SELECT NguoiDungId, HoTen, Email, MatKhauHash, VaiTroId, NgayTao, TrangThai, TenDangNhap, SoDienThoai
            FROM dbo.NguoiDung
            WHERE TrangThai = 1 OR TrangThai IS NULL
            ORDER BY NguoiDungId DESC;";

            var list = new List<NguoiDung>();

            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(Map(reader));
            }

            return list;
        }

        public NguoiDung? GetById(int id)
        {
            const string sql = @"
            SELECT TOP 1 NguoiDungId, HoTen, Email, MatKhauHash, VaiTroId, NgayTao, TrangThai, TenDangNhap, SoDienThoai
            FROM dbo.NguoiDung
            WHERE NguoiDungId = @Id;";

            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            AddParam(cmd, "@Id", id);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return Map(reader);
        }

        public NguoiDung? GetByEmail(string email)
        {
            const string sql = @"
            SELECT TOP 1 NguoiDungId, HoTen, Email, MatKhauHash, VaiTroId, NgayTao, TrangThai, TenDangNhap, SoDienThoai
            FROM dbo.NguoiDung
            WHERE Email = @Email;";

            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            AddParam(cmd, "@Email", email);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return Map(reader);
        }

        public int Create(NguoiDung user)
        {
            user.NgayTao ??= DateTime.Now;
            user.TrangThai ??= true;

            const string sql = @"
            INSERT INTO dbo.NguoiDung (HoTen, Email, MatKhauHash, VaiTroId, NgayTao, TrangThai, TenDangNhap, SoDienThoai)
            VALUES (@HoTen, @Email, @MatKhauHash, @VaiTroId, @NgayTao, @TrangThai, @TenDangNhap, @SoDienThoai);
            SELECT CAST(SCOPE_IDENTITY() AS int);";

            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            AddParam(cmd, "@HoTen", user.HoTen);
            AddParam(cmd, "@Email", user.Email);
            AddParam(cmd, "@MatKhauHash", user.MatKhauHash);
            AddParam(cmd, "@VaiTroId", user.VaiTroId);
            AddParam(cmd, "@NgayTao", user.NgayTao);
            AddParam(cmd, "@TrangThai", user.TrangThai);
            AddParam(cmd, "@TenDangNhap", user.TenDangNhap);
            AddParam(cmd, "@SoDienThoai", user.SoDienThoai);

            var newIdObj = cmd.ExecuteScalar();
            return newIdObj == null ? 0 : Convert.ToInt32(newIdObj);
        }

        public bool Update(NguoiDung user)
        {
            const string sql = @"
            UPDATE dbo.NguoiDung
            SET HoTen = @HoTen,
                Email = @Email,
                VaiTroId = @VaiTroId,
                TrangThai = @TrangThai,
                TenDangNhap = @TenDangNhap,
                SoDienThoai = @SoDienThoai
            WHERE NguoiDungId = @Id;";

            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            AddParam(cmd, "@Id", user.NguoiDungId);
            AddParam(cmd, "@HoTen", user.HoTen);
            AddParam(cmd, "@Email", user.Email);
            AddParam(cmd, "@VaiTroId", user.VaiTroId);
            AddParam(cmd, "@TrangThai", user.TrangThai);
            AddParam(cmd, "@TenDangNhap", user.TenDangNhap);
            AddParam(cmd, "@SoDienThoai", user.SoDienThoai);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool SoftDelete(int id)
        {
            const string sql = @"
        UPDATE dbo.NguoiDung
        SET TrangThai = 0
        WHERE NguoiDungId = @Id;";

            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            AddParam(cmd, "@Id", id);

            return cmd.ExecuteNonQuery() > 0;
        }

        private static void AddParam(IDbCommand cmd, string name, object? value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        private static NguoiDung Map(IDataRecord r)
        {
            return new NguoiDung
            {
                NguoiDungId = Convert.ToInt32(r["NguoiDungId"]),
                HoTen = r["HoTen"]?.ToString() ?? "",
                Email = r["Email"]?.ToString() ?? "",
                MatKhauHash = r["MatKhauHash"]?.ToString() ?? "",

                VaiTroId = r["VaiTroId"] == DBNull.Value ? null : (int?)Convert.ToInt32(r["VaiTroId"]),
                NgayTao = r["NgayTao"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(r["NgayTao"]),
                TrangThai = r["TrangThai"] == DBNull.Value ? null : (bool?)Convert.ToBoolean(r["TrangThai"]),

                TenDangNhap = r["TenDangNhap"]?.ToString() ?? "",
                SoDienThoai = r["SoDienThoai"] == DBNull.Value ? null : r["SoDienThoai"]?.ToString()
            };
        }

        public List<NguoiDung> GetByMaVaiTro(string maVaiTro)
        {
            const string sql = @"
            SELECT nd.NguoiDungId, nd.HoTen, nd.Email, nd.MatKhauHash, nd.VaiTroId,
                   nd.NgayTao, nd.TrangThai, nd.TenDangNhap, nd.SoDienThoai
            FROM dbo.NguoiDung nd
            INNER JOIN dbo.VaiTro vt ON vt.VaiTroId = nd.VaiTroId
            WHERE vt.MaVaiTro = @MaVaiTro
              AND (nd.TrangThai = 1 OR nd.TrangThai IS NULL)
            ORDER BY nd.NguoiDungId DESC;";

              var list = new List<NguoiDung>();

         using var conn = _factory.CreateConnection();
             conn.Open();

    using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
                        AddParam(cmd, "@MaVaiTro", maVaiTro);

                        using var reader = cmd.ExecuteReader();
                        while (reader.Read())
                            list.Add(Map(reader));

                        return list;
        }
    }
}
