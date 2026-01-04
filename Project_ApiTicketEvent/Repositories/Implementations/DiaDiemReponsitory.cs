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
