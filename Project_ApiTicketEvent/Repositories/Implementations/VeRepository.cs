using Data;
using Microsoft.Data.SqlClient;
using Models.DTOs.Reponses;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class VeRepository:IVeRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public VeRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        // GET /api/Ve/me
        public List<VeResponse> GetMyTickets(int nguoiSoHuuId)
        {
            const string sql = @"
            SELECT
                v.VeID, v.DonHangID, v.LoaiVeID, v.MaVe, v.QrToken, v.TrangThai,
                lv.TenLoaiVe, lv.DonGia,
                sk.SuKienID, sk.TenSuKien
            FROM dbo.Ve v
            JOIN dbo.LoaiVe lv ON lv.LoaiVeID = v.LoaiVeID
            JOIN dbo.SuKien sk ON sk.SuKienID = lv.SuKienID
            WHERE v.NguoiSoHuuID = @NguoiSoHuuID
            ORDER BY v.VeID DESC;";

            var list = new List<VeResponse>();

            using var conn = (SqlConnection)_connectionFactory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@NguoiSoHuuID", nguoiSoHuuId);

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(Map(r));
            }

            return list;
        }

        // GET /api/Ve/{maVe}
        public VeResponse? GetMyTicketByMaVe(int nguoiSoHuuId, string maVe)
        {
            if (string.IsNullOrWhiteSpace(maVe)) return null;

            const string sql = @"
            SELECT TOP 1
                v.VeID, v.DonHangID, v.LoaiVeID, v.MaVe, v.QrToken, v.TrangThai,
                lv.TenLoaiVe, lv.DonGia,
                sk.SuKienID, sk.TenSuKien
            FROM dbo.Ve v
            JOIN dbo.LoaiVe lv ON lv.LoaiVeID = v.LoaiVeID
            JOIN dbo.SuKien sk ON sk.SuKienID = lv.SuKienID
            WHERE v.NguoiSoHuuID = @NguoiSoHuuID
              AND v.MaVe = @MaVe;";

            using var conn = (SqlConnection)_connectionFactory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@NguoiSoHuuID", nguoiSoHuuId);
            cmd.Parameters.AddWithValue("@MaVe", maVe);

            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            return Map(r);
        }

        private static VeResponse Map(SqlDataReader r)
        {
            return new VeResponse
            {
                VeID = r.GetInt32(r.GetOrdinal("VeID")),
                DonHangID = r.GetInt32(r.GetOrdinal("DonHangID")),
                LoaiVeID = r.GetInt32(r.GetOrdinal("LoaiVeID")),
                MaVe = r.GetString(r.GetOrdinal("MaVe")),
                QrToken = r.GetString(r.GetOrdinal("QrToken")),
                TrangThai = Convert.ToByte(r["TrangThai"]),
                TenLoaiVe = r.GetString(r.GetOrdinal("TenLoaiVe")),
                DonGia = r.GetDecimal(r.GetOrdinal("DonGia")),
                SuKienID = r.GetInt32(r.GetOrdinal("SuKienID")),
                TenSuKien = r.GetString(r.GetOrdinal("TenSuKien"))
            };
        }
    }
}
