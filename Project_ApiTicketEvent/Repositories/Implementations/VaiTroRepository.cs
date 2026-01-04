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
    public class VaiTroRepository : IVaiTroRepository
    {
        private readonly IDbConnectionFactory _factory;

        public VaiTroRepository(IDbConnectionFactory factory)
        {
            _factory = factory;
        }

        public List<VaiTro> GetAll()
        {
            var list = new List<VaiTro>();

            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            SELECT VaiTroId AS VaiTroID, MaVaiTro, TenVaiTro, NgayTao
            FROM dbo.VaiTro
            ORDER BY VaiTroId DESC;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(Map(reader));
            }

            return list;
        }
        public VaiTro? GetById(int id)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            SELECT TOP 1 VaiTroId AS VaiTroID, MaVaiTro, TenVaiTro, NgayTao
            FROM dbo.VaiTro
            WHERE VaiTroId = @Id;";

            AddParam(cmd, "@Id", id);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return Map(reader);
        }
        public VaiTro? GetByMa(string maVaiTro)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            SELECT TOP 1 VaiTroId AS VaiTroID, MaVaiTro, TenVaiTro, NgayTao
            FROM dbo.VaiTro
            WHERE MaVaiTro = @Ma;";

            AddParam(cmd, "@Ma", maVaiTro);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return Map(reader);
        }

        private static void AddParam(IDbCommand cmd, string name, object? value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        private static VaiTro Map(IDataRecord r)
        {
            return new VaiTro
            {
                VaiTroID = Convert.ToInt32(r["VaiTroID"]),
                MaVaiTro = r["MaVaiTro"]?.ToString() ?? "",
                TenVaiTro = r["TenVaiTro"]?.ToString() ?? "",
                NgayTao = r["NgayTao"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["NgayTao"])
            };
        }
    }
}
