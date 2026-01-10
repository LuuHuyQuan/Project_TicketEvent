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
    public class VeRepository : IVeRepository
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
        public async Task<bool> HuyVeAsync(int nguoiSoHuuId, string maVe, string? lyDo)
        {
            if (nguoiSoHuuId <= 0) return false;
            if (string.IsNullOrWhiteSpace(maVe)) return false;

            // Chỉ cho hủy khi vé đang ở trạng thái 0 (hợp lệ, chưa dùng)
            const string sql = @"
UPDATE dbo.Ve
SET TrangThai = 2
WHERE MaVe = @MaVe
  AND NguoiSoHuuID = @NguoiSoHuuID
  AND TrangThai = 0;";

            using var conn = (SqlConnection)_connectionFactory.CreateConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@MaVe", maVe);
            cmd.Parameters.AddWithValue("@NguoiSoHuuID", nguoiSoHuuId);

            var affected = await cmd.ExecuteNonQueryAsync();
            return affected > 0;
        }

        public async Task<HoanVeResponse> HoanVeAsync(
            int nguoiSoHuuId,
            string maVe,
            string? lyDo,
            string? phuongThuc,
            string? rawResponse)
        {
            var res = new HoanVeResponse { Success = false };

            if (nguoiSoHuuId <= 0 || string.IsNullOrWhiteSpace(maVe))
            {
                res.Message = "nguoiSoHuuId/maVe invalid";
                return res;
            }

            using var conn = (SqlConnection)_connectionFactory.CreateConnection();
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();

            using var tran = conn.BeginTransaction();

            try
            {
                // 1) Lấy Vé + Đơn hàng để kiểm tra trạng thái
                const string sqlGet = @"
SELECT TOP 1
    v.VeID, v.DonHangID, v.LoaiVeID, v.TrangThai AS VeTrangThai,
    dh.TrangThai AS DonTrangThai
FROM dbo.Ve v
JOIN dbo.DonHang dh ON dh.DonHangID = v.DonHangID
WHERE v.MaVe = @MaVe
  AND v.NguoiSoHuuID = @NguoiSoHuuID;";

                int veId, donHangId, loaiVeId;
                byte veTrangThai, donTrangThai;

                using (var cmd = new SqlCommand(sqlGet, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@MaVe", maVe);
                    cmd.Parameters.AddWithValue("@NguoiSoHuuID", nguoiSoHuuId);

                    using var r = await cmd.ExecuteReaderAsync();
                    if (!await r.ReadAsync())
                    {
                        res.Message = "Không tìm thấy vé hoặc vé không thuộc về bạn.";
                        return res;
                    }

                    veId = r.GetInt32(r.GetOrdinal("VeID"));
                    donHangId = r.GetInt32(r.GetOrdinal("DonHangID"));
                    loaiVeId = r.GetInt32(r.GetOrdinal("LoaiVeID"));
                    veTrangThai = Convert.ToByte(r["VeTrangThai"]);
                    donTrangThai = Convert.ToByte(r["DonTrangThai"]);
                }

                // 2) Chặn hoàn nếu vé không còn hợp lệ
                if (veTrangThai != 0)
                {
                    res.Message = $"Không thể hoàn vé vì vé không ở trạng thái hợp lệ (TrangThai={veTrangThai}).";
                    return res;
                }

                // 3) Chặn hoàn nếu đơn hàng chưa thanh toán
                // Trong hệ thống của bạn: DonHang.TrangThai=0 là chờ, =1 là đã thanh toán (đang dùng ở mock pay):contentReference[oaicite:7]{index=7}.
                if (donTrangThai != 1)
                {
                    res.Message = $"Không thể hoàn vì đơn hàng chưa thanh toán (DonHang.TrangThai={donTrangThai}).";
                    return res;
                }

                // 4) Lấy đơn giá theo ChiTietDonHang (ưu tiên) để khớp dữ liệu lúc mua
                // DonHangRepository đang insert ChiTietDonHang có DonGia:contentReference[oaicite:8]{index=8}.
                decimal donGia;

                const string sqlGia1 = @"
SELECT TOP 1 DonGia
FROM dbo.ChiTietDonHang
WHERE DonHangID = @DonHangID AND LoaiVeID = @LoaiVeID;";

                using (var cmdGia = new SqlCommand(sqlGia1, conn, tran))
                {
                    cmdGia.Parameters.AddWithValue("@DonHangID", donHangId);
                    cmdGia.Parameters.AddWithValue("@LoaiVeID", loaiVeId);

                    var obj = await cmdGia.ExecuteScalarAsync();
                    if (obj != null && obj != DBNull.Value)
                    {
                        donGia = Convert.ToDecimal(obj);
                    }
                    else
                    {
                        // fallback: lấy DonGia từ LoaiVe (vì bạn đang query DonGia từ LoaiVe trong Vé):contentReference[oaicite:9]{index=9}
                        const string sqlGia2 = @"SELECT DonGia FROM dbo.LoaiVe WHERE LoaiVeID = @LoaiVeID;";
                        using var cmdGia2 = new SqlCommand(sqlGia2, conn, tran);
                        cmdGia2.Parameters.AddWithValue("@LoaiVeID", loaiVeId);

                        var obj2 = await cmdGia2.ExecuteScalarAsync();
                        if (obj2 == null || obj2 == DBNull.Value)
                        {
                            res.Message = "Không lấy được đơn giá để hoàn.";
                            return res;
                        }

                        donGia = Convert.ToDecimal(obj2);
                    }
                }

                // 5) Insert lịch sử hoàn tiền vào ThanhToan (ghi số tiền âm để phân biệt)
                var maGiaoDich = "REFUND_" + Guid.NewGuid().ToString("N");
                var pt = string.IsNullOrWhiteSpace(phuongThuc) ? "REFUND_MOCK" : phuongThuc;

                var raw = (rawResponse ?? "");
                raw = $"REFUND maVe={maVe}; lyDo={lyDo}; {raw}".Trim();

                const string sqlInsertTT = @"
INSERT INTO dbo.ThanhToan (DonHangID, MaGiaoDich, PhuongThuc, SoTien, TrangThai, ThoiGianThanhToan, RawResponse)
VALUES (@DonHangID, @MaGiaoDich, @PhuongThuc, @SoTien, @TrangThai, SYSUTCDATETIME(), @RawResponse);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                int thanhToanId;
                using (var cmdIns = new SqlCommand(sqlInsertTT, conn, tran))
                {
                    cmdIns.Parameters.AddWithValue("@DonHangID", donHangId);
                    cmdIns.Parameters.AddWithValue("@MaGiaoDich", maGiaoDich);
                    cmdIns.Parameters.AddWithValue("@PhuongThuc", pt);
                    var soTienHoan = Math.Abs(donGia);

                    // nếu constraint là > 0 thì chặn luôn trường hợp 0
                    if (soTienHoan <= 0)
                    {
                        res.Message = "Không thể hoàn vì số tiền hoàn không hợp lệ (<=0). Kiểm tra DonGia/ChiTietDonHang.";
                        return res;
                    }

                    cmdIns.Parameters.AddWithValue("@SoTien", soTienHoan);
                    cmdIns.Parameters.AddWithValue("@TrangThai", 1);   // 1 = thành công (đang dùng trong hệ thống thanh toán mock)
                    cmdIns.Parameters.AddWithValue("@RawResponse", raw);

                    thanhToanId = (int)(await cmdIns.ExecuteScalarAsync() ?? 0);
                }

                // 6) Update vé sang trạng thái Hoàn (3)
                const string sqlUpVe = @"
UPDATE dbo.Ve
SET TrangThai = 3
WHERE VeID = @VeID AND TrangThai = 0;";

                using (var cmdUp = new SqlCommand(sqlUpVe, conn, tran))
                {
                    cmdUp.Parameters.AddWithValue("@VeID", veId);
                    var affected = await cmdUp.ExecuteNonQueryAsync();
                    if (affected <= 0)
                    {
                        res.Message = "Hoàn thất bại do vé đã bị thay đổi trạng thái trước đó.";
                        return res;
                    }
                }

                tran.Commit();

                res.Success = true;
                res.Message = "Hoàn vé thành công.";
                res.VeID = veId;
                res.DonHangID = donHangId;
                res.RefundAmount = donGia;     // trả về dương cho dễ hiểu
                res.ThanhToanID = thanhToanId;
                res.MaGiaoDich = maGiaoDich;

                return res;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
    }
}
