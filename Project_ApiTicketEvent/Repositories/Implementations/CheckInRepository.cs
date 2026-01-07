using Data;
using Microsoft.Data.SqlClient;
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

        public object Checkin(CheckInRequest req)
        {
            var qrToken = (req.QrToken ?? "").Trim();
            var maVe = (req.MaVe ?? "").Trim();

            if (string.IsNullOrWhiteSpace(qrToken) && string.IsNullOrWhiteSpace(maVe))
                return new { success = false, message = "Thiếu QrToken hoặc MaVe." };

            if (req.NhanVienID <= 0)
                return new { success = false, message = "nhanVienID không hợp lệ." };

            using var raw = _connectionFactory.CreateConnection();
            var conn = (SqlConnection)raw;
            if (conn.State != ConnectionState.Open) conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                // 1) Tìm vé theo QrToken hoặc MaVe + join để lấy SuKienID + kiểm tra đơn hàng đã thanh toán
                // Lưu ý: tên cột/bảng có thể khác nhẹ tùy SQL của bạn -> nếu khác thì đổi cho khớp.
                const string sqlFind = @"
SELECT TOP 1
    v.VeID, v.MaVe, v.QrToken, v.TrangThai AS VeTrangThai,
    v.DonHangID, dh.TrangThai AS DonHangTrangThai,
    lv.LoaiVeID, lv.TenLoaiVe,
    sk.SuKienID, sk.TenSuKien,
    sk.ToChucID
FROM dbo.Ve v
JOIN dbo.DonHang dh ON dh.DonHangID = v.DonHangID
JOIN dbo.LoaiVe lv ON lv.LoaiVeID = v.LoaiVeID
JOIN dbo.SuKien sk ON sk.SuKienID = lv.SuKienID
WHERE (@QrToken IS NULL OR v.QrToken = @QrToken)
  AND (@MaVe IS NULL OR v.MaVe = @MaVe);";

                int veId, donHangId, suKienId, toChucId;
                string maVeDb, qrTokenDb, tenLoaiVe, tenSuKien;
                byte veTrangThai, donHangTrangThai;

                using (var cmd = new SqlCommand(sqlFind, conn, tx))
                {
                    cmd.Parameters.AddWithValue("@QrToken", string.IsNullOrWhiteSpace(qrToken) ? (object)DBNull.Value : qrToken);
                    cmd.Parameters.AddWithValue("@MaVe", string.IsNullOrWhiteSpace(maVe) ? (object)DBNull.Value : maVe);

                    using var r = cmd.ExecuteReader();
                    if (!r.Read())
                    {
                        tx.Rollback();
                        return new { success = false, message = "Không tìm thấy vé theo QrToken/MaVe." };
                    }

                    veId = r.GetInt32(r.GetOrdinal("VeID"));
                    donHangId = r.GetInt32(r.GetOrdinal("DonHangID"));
                    suKienId = r.GetInt32(r.GetOrdinal("SuKienID"));
                    toChucId = r.GetInt32(r.GetOrdinal("ToChucID"));

                    maVeDb = r.GetString(r.GetOrdinal("MaVe"));
                    qrTokenDb = r.GetString(r.GetOrdinal("QrToken"));
                    tenLoaiVe = r.GetString(r.GetOrdinal("TenLoaiVe"));
                    tenSuKien = r.GetString(r.GetOrdinal("TenSuKien"));

                    veTrangThai = Convert.ToByte(r["VeTrangThai"]);
                    donHangTrangThai = Convert.ToByte(r["DonHangTrangThai"]);
                }

                if (req.NhanVienID != toChucId)
                {
                    InsertLog(conn, tx, veId, suKienId, req.NhanVienID, false, "Không thuộc quyền ban tổ chức. " + req.GhiChu);
                    tx.Commit();
                    return new { success = false, message = "Bạn không có quyền check-in vé của sự kiện này." };
                }

                // 3) Đơn hàng phải đã thanh toán
                if (donHangTrangThai != 1)
                {
                    InsertLog(conn, tx, veId, suKienId, req.NhanVienID, false, "Đơn hàng chưa thanh toán. " + req.GhiChu);
                    tx.Commit();
                    return new { success = false, message = "Đơn hàng chưa thanh toán nên không thể check-in." };
                }

                // 4) Vé phải ở trạng thái 0 mới được check-in
                if (veTrangThai != 0)
                {
                    InsertLog(conn, tx, veId, suKienId, req.NhanVienID, false, $"Vé không hợp lệ (TrangThai={veTrangThai}). " + req.GhiChu);
                    tx.Commit();
                    return new { success = false, message = $"Vé không thể check-in (TrangThai={veTrangThai})." };
                }

                // 5) Update ve TrangThai: 0 -> 1 (chống quét 2 lần)
                const string sqlUpdate = @"
UPDATE dbo.Ve
SET TrangThai = 1
WHERE VeID = @VeID AND TrangThai = 0;";

                int affected;
                using (var cmdUp = new SqlCommand(sqlUpdate, conn, tx))
                {
                    cmdUp.Parameters.AddWithValue("@VeID", veId);
                    affected = cmdUp.ExecuteNonQuery();
                }

                if (affected == 0)
                {
                    InsertLog(conn, tx, veId, suKienId, req.NhanVienID, false, "Race condition: vé vừa đổi trạng thái. " + req.GhiChu);
                    tx.Commit();
                    return new { success = false, message = "Không thể check-in (vé có thể vừa đổi trạng thái)." };
                }

                // 6) Log thành công
                InsertLog(conn, tx, veId, suKienId, req.NhanVienID, true, "Check-in thành công. " + req.GhiChu);

                tx.Commit();

                return new
                {
                    success = true,
                    message = "Check-in thành công.",
                    data = new
                    {
                        veId,
                        maVe = maVeDb,
                        qrToken = qrTokenDb,
                        donHangId,
                        suKienId,
                        tenSuKien,
                        tenLoaiVe,
                        trangThaiTruoc = 0,
                        trangThaiSau = 1
                    }
                };
            }
            catch (Exception ex)
            {
                tx.Rollback();
                return new { success = false, message = "Lỗi check-in: " + ex.Message };
            }
        }

        private static void InsertLog(SqlConnection conn, SqlTransaction tx, int veId, int suKienId, int nhanVienId, bool ketQua, string? ghiChu)
        {
            // Nếu DB bạn khác tên cột, đổi lại cho khớp
            const string sqlLog = @"
INSERT INTO dbo.NhatKyCheckin (VeID, SuKienID, NhanVienID, KetQua, GhiChu)
VALUES (@VeID, @SuKienID, @NhanVienID, @KetQua, @GhiChu);";

            using var cmd = new SqlCommand(sqlLog, conn, tx);
            cmd.Parameters.AddWithValue("@VeID", veId);
            cmd.Parameters.AddWithValue("@SuKienID", suKienId);
            cmd.Parameters.AddWithValue("@NhanVienID", nhanVienId);
            cmd.Parameters.AddWithValue("@KetQua", ketQua);
            cmd.Parameters.AddWithValue("@GhiChu", (object?)ghiChu ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }
    }
}
