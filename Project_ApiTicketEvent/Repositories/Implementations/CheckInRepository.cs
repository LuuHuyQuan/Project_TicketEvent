using Data;
using Microsoft.Data.SqlClient;
using Models.DTOs.Reponses;
using Models.DTOs.Requests;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implementations
{
    public class CheckInRepository:ICheckInRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public CheckInRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public CheckInResponse? Scan(CheckInRequest req, int banToChucId, int? suKienId = null)
        {
            var qrToken = (req.QrToken ?? "").Trim();
            var maVe = (req.MaVe ?? "").Trim();

            if (string.IsNullOrWhiteSpace(qrToken) && string.IsNullOrWhiteSpace(maVe))
                throw new ArgumentException("Thiếu QrToken hoặc MaVe.");

            // Lấy vé theo QrToken (ưu tiên) hoặc MaVe, join đúng như VeRepository đang làm
            const string sqlFind = @"
SELECT TOP 1
    v.VeID, v.MaVe, v.QrToken, v.TrangThai,
    v.LoaiVeID,
    lv.TenLoaiVe,
    sk.SuKienID, sk.TenSuKien
FROM dbo.Ve v
JOIN dbo.LoaiVe lv ON lv.LoaiVeID = v.LoaiVeID
JOIN dbo.SuKien sk ON sk.SuKienID = lv.SuKienID
WHERE (@QrToken IS NULL OR v.QrToken = @QrToken)
  AND (@MaVe IS NULL OR v.MaVe = @MaVe);";

            using var conn = (SqlConnection)_connectionFactory.CreateConnection();
            if (conn.State != ConnectionState.Open) conn.Open();

            int veId, loaiVeId, suKienIdVe;
            string maVeDb, qrTokenDb, tenLoaiVe, tenSuKien;
            byte trangThai;

            using (var cmd = new SqlCommand(sqlFind, conn))
            {
                cmd.Parameters.AddWithValue("@QrToken", string.IsNullOrWhiteSpace(qrToken) ? (object)DBNull.Value : qrToken);
                cmd.Parameters.AddWithValue("@MaVe", string.IsNullOrWhiteSpace(maVe) ? (object)DBNull.Value : maVe);

                using var r = cmd.ExecuteReader();
                if (!r.Read()) return null;

                veId = r.GetInt32(r.GetOrdinal("VeID"));
                maVeDb = r.GetString(r.GetOrdinal("MaVe"));
                qrTokenDb = r.GetString(r.GetOrdinal("QrToken"));
                trangThai = Convert.ToByte(r["TrangThai"]);

                loaiVeId = r.GetInt32(r.GetOrdinal("LoaiVeID"));
                tenLoaiVe = r.GetString(r.GetOrdinal("TenLoaiVe"));

                suKienIdVe = r.GetInt32(r.GetOrdinal("SuKienID"));
                tenSuKien = r.GetString(r.GetOrdinal("TenSuKien"));
            }

            if (suKienId.HasValue && suKienId.Value > 0 && suKienIdVe != suKienId.Value)
                throw new InvalidOperationException($"Vé không thuộc sự kiện này (Ve thuộc SuKienID={suKienIdVe}).");

            // Trạng thái vé
            if (trangThai == 1)
            {
                return new CheckInResponse
                {
                    VeID = veId,
                    MaVe = maVeDb,
                    QrToken = qrTokenDb,
                    SuKienID = suKienIdVe,
                    TenSuKien = tenSuKien,
                    LoaiVeID = loaiVeId,
                    TenLoaiVe = tenLoaiVe,
                    TrangThaiTruoc = 1,
                    TrangThaiSau = 1,
                    ThoiGianCheckIn = DateTime.Now,
                    BanToChucID = banToChucId
                };
            }

            if (trangThai == 2)
                throw new InvalidOperationException("Vé đã bị hủy/hoàn, không thể check-in.");

            // Update check-in: 0 -> 1 
            const string sqlUpdate = @"
UPDATE dbo.Ve
SET TrangThai = 1
WHERE VeID = @VeID AND TrangThai = 0;";

            using (var cmdUp = new SqlCommand(sqlUpdate, conn))
            {
                cmdUp.Parameters.AddWithValue("@VeID", veId);
                var affected = cmdUp.ExecuteNonQuery();
                if (affected == 0)
                    throw new InvalidOperationException("Không thể check-in (vé có thể vừa đổi trạng thái).");
            }

            return new CheckInResponse
            {
                VeID = veId,
                MaVe = maVeDb,
                QrToken = qrTokenDb,
                SuKienID = suKienIdVe,
                TenSuKien = tenSuKien,
                LoaiVeID = loaiVeId,
                TenLoaiVe = tenLoaiVe,
                TrangThaiTruoc = 0,
                TrangThaiSau = 1,
                ThoiGianCheckIn = DateTime.Now,
                BanToChucID = banToChucId
            };
        }
    }
}
